// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class CurrentValueComparerFactory
    {
        public virtual IComparer<IUpdateEntry> Create([NotNull] IPropertyBase propertyBase)
        {
            var comparerType = propertyBase.ClrType;
            var nonNullableType = comparerType.UnwrapNullableType();
            if (IsGenericComparable())
            {
                return (IComparer<IUpdateEntry>)Activator.CreateInstance(
                    typeof(CurrentValueComparer<>).MakeGenericType(comparerType),
                    propertyBase);
            }

            if (typeof(IStructuralComparable).IsAssignableFrom(comparerType))
            {
                return new StructuralCurrentValueComparer(propertyBase);
            }

            if (typeof(IComparable).IsAssignableFrom(comparerType))
            {
                return new CurrentValueComparer(propertyBase);
            }

            if (propertyBase is IProperty property)
            {
                var converter = property.GetValueConverter()
                    ?? property.GetTypeMapping().Converter;

                if (converter != null)
                {
                    comparerType = converter.ProviderClrType;
                    nonNullableType = comparerType.UnwrapNullableType();
                    if (IsGenericComparable())
                    {
                        return (IComparer<IUpdateEntry>)Activator.CreateInstance(
                            typeof(ConvertedCurrentValueComparer<,>).MakeGenericType(
                                converter.ModelClrType, comparerType),
                            propertyBase,
                            converter);
                    }

                    if (typeof(IStructuralComparable).IsAssignableFrom(comparerType))
                    {
                        return new ConvertedStructuralCurrentValueComparer(propertyBase, converter);
                    }

                    if (typeof(IComparable).IsAssignableFrom(comparerType))
                    {
                        return new ConvertedCurrentValueComparer(propertyBase, converter);
                    }
                }
            }

            throw new InvalidOperationException($"Type not comparable: {propertyBase.ClrType}");

            bool IsGenericComparable()
                => typeof(IComparable<>).MakeGenericType(comparerType).IsAssignableFrom(comparerType)
                    || typeof(IComparable<>).MakeGenericType(nonNullableType).IsAssignableFrom(nonNullableType)
                    || comparerType.IsEnum;
        }
    }

    public class CurrentValueComparer<TProperty> : IComparer<IUpdateEntry>
    {
        private readonly IPropertyBase _property;
        private readonly IComparer<TProperty> _underlyingComparer;

        public CurrentValueComparer([NotNull] IPropertyBase property)
        {
            _property = property;
            _underlyingComparer = Comparer<TProperty>.Default;
        }

        public int Compare(IUpdateEntry x, IUpdateEntry y)
            => _underlyingComparer.Compare(
            x.GetCurrentValue<TProperty>(_property),
            y.GetCurrentValue<TProperty>(_property));
    }

    public class ConvertedCurrentValueComparer<TProperty, TProvider> : IComparer<IUpdateEntry>
    {
        private readonly IPropertyBase _property;
        private readonly IComparer<TProvider> _underlyingComparer;
        private readonly ValueConverter<TProperty, TProvider> _converter;

        public ConvertedCurrentValueComparer(
            [NotNull] IPropertyBase property,
            [NotNull] ValueConverter<TProperty, TProvider> converter)
        {
            _property = property;
            _converter = converter;
            _underlyingComparer = Comparer<TProvider>.Default;
        }

        public int Compare(IUpdateEntry x, IUpdateEntry y)
            => _underlyingComparer.Compare(
                _converter.ConvertToProviderTyped(x.GetCurrentValue<TProperty>(_property)),
                _converter.ConvertToProviderTyped(y.GetCurrentValue<TProperty>(_property)));
    }

    public class CurrentValueComparer : IComparer<IUpdateEntry>
    {
        private readonly IPropertyBase _property;
        private readonly IComparer _underlyingComparer;

        public CurrentValueComparer([NotNull] IPropertyBase property)
            : this(property, Comparer.Default)
        {
        }

        protected CurrentValueComparer([NotNull] IPropertyBase property, [NotNull] IComparer underlyingComparer)
        {
            _property = property;
            _underlyingComparer = underlyingComparer;
        }

        public virtual object GetCurrentValue(IUpdateEntry entry)
            => entry.GetCurrentValue(_property);

        public virtual int Compare(IUpdateEntry x, IUpdateEntry y)
            => Compare(x.GetCurrentValue(_property), y.GetCurrentValue(_property));

        protected virtual int Compare([CanBeNull] object x, [CanBeNull] object y)
            => _underlyingComparer.Compare(x, y);
    }

    public class ConvertedCurrentValueComparer : CurrentValueComparer
    {
        private readonly ValueConverter _converter;

        public ConvertedCurrentValueComparer(
            [NotNull] IPropertyBase property,
            [NotNull] ValueConverter converter)
            : base(property)
        {
            _converter = converter;
        }

        public override object GetCurrentValue(IUpdateEntry entry)
            => _converter.ConvertToProvider(base.GetCurrentValue(entry));
    }

    public class StructuralCurrentValueComparer : CurrentValueComparer
    {
        public StructuralCurrentValueComparer([NotNull] IPropertyBase property)
        : base(property, StructuralComparisons.StructuralComparer)
        {
        }

        public override int Compare(IUpdateEntry x, IUpdateEntry y)
        {
            var xValue = GetCurrentValue(x);
            var yValue = GetCurrentValue(y);

            return xValue is Array xArray
                && yValue is Array yArray
                && xArray.Length != yArray.Length
                    ? xArray.Length - yArray.Length
                    : base.Compare(xValue, yValue);
        }
    }

    public class ConvertedStructuralCurrentValueComparer : StructuralCurrentValueComparer
    {
        private readonly ValueConverter _converter;

        public ConvertedStructuralCurrentValueComparer(
            [NotNull] IPropertyBase property,
            [NotNull] ValueConverter converter)
            : base(property)
        {
            _converter = converter;
        }

        public override object GetCurrentValue(IUpdateEntry entry)
            => _converter.ConvertToProvider(base.GetCurrentValue(entry));
    }


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class StateManagerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<InternalEntityEntry> ToListForState(
            [NotNull] this IStateManager stateManager,
            bool added = false,
            bool modified = false,
            bool deleted = false,
            bool unchanged = false)
        {
            var list = new List<InternalEntityEntry>(
                stateManager.GetCountForState(added, modified, deleted, unchanged));

            foreach (var entry in stateManager.GetEntriesForState(added, modified, deleted, unchanged))
            {
                list.Add(entry);
            }

            return list;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<InternalEntityEntry> ToList(
            [NotNull] this IStateManager stateManager)
            => stateManager.ToListForState(added: true, modified: true, deleted: true, unchanged: true);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString([NotNull] this IStateManager stateManager, StateManagerDebugStringOptions options)
        {
            var builder = new StringBuilder();

            foreach (var entry in stateManager.Entries.OrderBy(e => e, EntityEntryComparer.Instance))
            {
                builder.AppendLine(entry.ToDebugString(options));
            }

            return builder.ToString();
        }

        private sealed class EntityEntryComparer : IComparer<InternalEntityEntry>
        {
            public static EntityEntryComparer Instance = new EntityEntryComparer();

            private EntityEntryComparer()
            {
            }

            public int Compare(InternalEntityEntry x, InternalEntityEntry y)
            {
                var result = StringComparer.InvariantCulture.Compare(x.EntityType.Name, y.EntityType.Name);
                if (result != 0)
                {
                    return result;
                }

                var primaryKey = x.EntityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    var keyProperties = primaryKey.Properties;
                    foreach (var keyProperty in keyProperties)
                    {
                        if (typeof(IComparable).IsAssignableFrom(keyProperty.ClrType))
                        {
                            result = Comparer.DefaultInvariant.Compare(
                                x.GetCurrentValue(keyProperty),
                                y.GetCurrentValue(keyProperty));

                            if (result != 0)
                            {
                                return result;
                            }
                        }
                    }
                }

                return 0;
            }
        }
    }
}
