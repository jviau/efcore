// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class KeysWithConvertersTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : KeysWithConvertersTestBase<TFixture>.KeysWithConvertersFixtureBase, new()
    {
        protected KeysWithConvertersTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected DbContext CreateContext() => Fixture.CreateContext();

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_struct_key()
        {
            using (var context = CreateContext())
            {
                context.Set<StructKeyPrincipal>().AddRange(
                    new StructKeyPrincipal { Id = new StructKey { Id = 1 }, Foo = "X1" },
                    new StructKeyPrincipal { Id = new StructKey { Id = 2 }, Foo = "X3" },
                    new StructKeyPrincipal { Id = new StructKey { Id = 3 }, Foo = "X2" });

                context.Set<StructKeyOptionalDependent>().AddRange(
                    new StructKeyOptionalDependent { Id = new StructKey { Id = 11 }, PrincipalId = new StructKey { Id = 1 } },
                    new StructKeyOptionalDependent { Id = new StructKey { Id = 12 }, PrincipalId = new StructKey { Id = 2 } },
                    new StructKeyOptionalDependent { Id = new StructKey { Id = 13 }, PrincipalId = new StructKey { Id = 3 } },
                    new StructKeyOptionalDependent { Id = new StructKey { Id = 14 }, PrincipalId = null });

                Assert.Equal(7, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity1 = QueryByStructKey(context, new StructKey { Id = 1 });
                Assert.Equal(new StructKey { Id = 1 }, entity1.Id);
                Assert.Equal(1, entity1.OptionalDependents.Count);

                var entity2 = QueryByStructKey(context, new StructKey { Id = 2 });
                Assert.Equal(new StructKey { Id = 2 }, entity2.Id);
                Assert.Equal(1, entity2.OptionalDependents.Count);

                var entity3 = QueryByStructKey(context, new StructKey { Id = 3 });
                Assert.Equal(new StructKey { Id = 3 }, entity3.Id);
                Assert.Equal(1, entity3.OptionalDependents.Count);

                var orphan = context.Set<StructKeyOptionalDependent>().Single(e => e.PrincipalId == null);
                Assert.Equal(new StructKey { Id = 14 }, orphan.Id);
                Assert.Null(orphan.PrincipalId);
                Assert.Null(orphan.Principal);

                entity3.Foo = "Xx1";
                entity2.Foo = "Xx3";
                entity1.Foo = "Xx7";

                entity1.OptionalDependents.Single().PrincipalId = new StructKey { Id = 3 };
                entity2.OptionalDependents.Single().PrincipalId = null;
                orphan.PrincipalId = new StructKey { Id = 3 };

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var entity1 = QueryByStructKey(context, new StructKey { Id = 1 });
                Assert.Equal("Xx7", entity1.Foo);
                Assert.Equal(0, entity1.OptionalDependents.Count);

                var entity2 = QueryByStructKey(context, new StructKey { Id = 2 });
                Assert.Equal("Xx3", entity2.Foo);
                Assert.Equal(0, entity2.OptionalDependents.Count);

                var entity3 = QueryByStructKey(context, new StructKey { Id = 3 });
                Assert.Equal("Xx1", entity3.Foo);
                Assert.Equal(3, entity3.OptionalDependents.Count);
            }

            StructKeyPrincipal QueryByStructKey(DbContext context, StructKey id)
                => context
                    .Set<StructKeyPrincipal>()
                    .Include(e => e.OptionalDependents)
                    .Where(e => e.Id.Equals(id))
                    .ToList().Single();
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_comparable_struct_key()
        {
            using (var context = CreateContext())
            {
                context.Set<ComparableIntStructKeyPrincipal>().AddRange(
                    new ComparableIntStructKeyPrincipal { Id = new ComparableIntStructKey { Id = 1 }, Foo = "X1" },
                    new ComparableIntStructKeyPrincipal { Id = new ComparableIntStructKey { Id = 2 }, Foo = "X3" },
                    new ComparableIntStructKeyPrincipal { Id = new ComparableIntStructKey { Id = 3 }, Foo = "X2" });

                context.Set<ComparableIntStructKeyOptionalDependent>().AddRange(
                    new ComparableIntStructKeyOptionalDependent
                    {
                        Id = new ComparableIntStructKey { Id = 11 }, PrincipalId = new ComparableIntStructKey { Id = 1 }
                    },
                    new ComparableIntStructKeyOptionalDependent
                    {
                        Id = new ComparableIntStructKey { Id = 12 }, PrincipalId = new ComparableIntStructKey { Id = 2 }
                    },
                    new ComparableIntStructKeyOptionalDependent
                    {
                        Id = new ComparableIntStructKey { Id = 13 }, PrincipalId = new ComparableIntStructKey { Id = 3 }
                    },
                    new ComparableIntStructKeyOptionalDependent { Id = new ComparableIntStructKey { Id = 14 }, PrincipalId = null });

                Assert.Equal(7, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity1 = QueryByStructKey(context, new ComparableIntStructKey { Id = 1 });
                Assert.Equal(new ComparableIntStructKey { Id = 1 }, entity1.Id);
                Assert.Equal(1, entity1.OptionalDependents.Count);

                var entity2 = QueryByStructKey(context, new ComparableIntStructKey { Id = 2 });
                Assert.Equal(new ComparableIntStructKey { Id = 2 }, entity2.Id);
                Assert.Equal(1, entity2.OptionalDependents.Count);

                var entity3 = QueryByStructKey(context, new ComparableIntStructKey { Id = 3 });
                Assert.Equal(new ComparableIntStructKey { Id = 3 }, entity3.Id);
                Assert.Equal(1, entity3.OptionalDependents.Count);

                var orphan = context.Set<ComparableIntStructKeyOptionalDependent>().Single(e => e.PrincipalId == null);
                Assert.Equal(new ComparableIntStructKey { Id = 14 }, orphan.Id);
                Assert.Null(orphan.PrincipalId);
                Assert.Null(orphan.Principal);

                entity3.Foo = "Xx1";
                entity2.Foo = "Xx3";
                entity1.Foo = "Xx7";

                entity1.OptionalDependents.Single().PrincipalId = new ComparableIntStructKey { Id = 3 };
                entity2.OptionalDependents.Single().PrincipalId = null;
                orphan.PrincipalId = new ComparableIntStructKey { Id = 3 };

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var entity1 = QueryByStructKey(context, new ComparableIntStructKey { Id = 1 });
                Assert.Equal("Xx7", entity1.Foo);
                Assert.Equal(0, entity1.OptionalDependents.Count);

                var entity2 = QueryByStructKey(context, new ComparableIntStructKey { Id = 2 });
                Assert.Equal("Xx3", entity2.Foo);
                Assert.Equal(0, entity2.OptionalDependents.Count);

                var entity3 = QueryByStructKey(context, new ComparableIntStructKey { Id = 3 });
                Assert.Equal("Xx1", entity3.Foo);
                Assert.Equal(3, entity3.OptionalDependents.Count);
            }

            ComparableIntStructKeyPrincipal QueryByStructKey(DbContext context, ComparableIntStructKey id)
                => context
                    .Set<ComparableIntStructKeyPrincipal>()
                    .Include(e => e.OptionalDependents)
                    .Where(e => e.Id.Equals(id))
                    .ToList().Single();
        }

        private void ValidateOptional(
            int keySeed,
            IList<IIntPrincipal> principals,
            IList<IIntOptionalDependent> dependents,
            IList<(int, int[])> expectedPrincipalToDependents,
            IList<(int, int?)> expectedDependentToPrincipals,
            Func<IIntPrincipal, IList<IIntOptionalDependent>> getDependents,
            Func<IIntOptionalDependent, IIntPrincipal> getPrincipal)
        {
                Assert.Equal(4, principals.Count);
                for (var i = 0; i < 4; i++)
                {
                    Assert.Equal(keySeed + i, principals[i].BackingId);
                }

                Assert.Equal(6, dependents.Count);
                for (var i = 0; i < 6; i++)
                {
                    Assert.Equal(keySeed + 10 + i, dependents[i].BackingId);
                }

                foreach (var (dependentIndex, principalIndex) in expectedDependentToPrincipals)
                {
                    if (principalIndex.HasValue)
                    {
                        Assert.Same(principals[principalIndex.Value], getPrincipal(dependents[dependentIndex]));
                        Assert.Equal(principals[principalIndex.Value].BackingId, dependents[dependentIndex].BackingPrincipalId);
                    }
                    else
                    {
                        Assert.Null(getPrincipal(dependents[dependentIndex]));
                        Assert.Null(dependents[dependentIndex].BackingPrincipalId);
                    }
                }

                foreach (var (principalIndex, dependentIndexes) in expectedPrincipalToDependents)
                {
                    Assert.Equal(dependentIndexes.Length, getDependents(principals[principalIndex]).Count);
                    foreach (var dependentIndex in dependentIndexes)
                    {
                        Assert.Same(principals[principalIndex], getPrincipal(dependents[dependentIndex]));
                        Assert.Equal(principals[principalIndex].BackingId, dependents[dependentIndex].BackingPrincipalId);
                    }
                }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_class_key()
        {
            InsertOptionalGraph<ClassKeyPrincipal, ClassKeyOptionalDependent>(1);

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0 }), (1, new[] { 1 }), (2, new[] { 2, 2, 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, 1), (2, 2), (3, 2), (4, 2), (5, null) });

                foreach (var principal in principals)
                {
                    principal.Foo = "Mutant!";
                }

                dependents[5].Principal = principals[0];
                dependents[4].PrincipalId = null;
                dependents[3].PrincipalId = principals[0].Id;
                principals[1].OptionalDependents.Clear();

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                RunQueries(context, out var principals, out var dependents);

                Validate(
                    principals,
                    dependents,
                    new[] { (0, new[] { 0, 3, 5 }), (1, new int[0]), (2, new[] { 2 }), (3, new int[0]) },
                    new (int, int?)[] { (0, 0), (1, null), (2, 2), (3, 0), (4, null), (5, 0) });
            }

            void RunQueries(
                DbContext context,
                out ClassKeyPrincipal[] principals,
                out ClassKeyOptionalDependent[] dependents)
            {
                var two = 2;
                var three = new ClassKey { Id = 3 };

                principals = new[]
                {
                    context.Set<ClassKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ClassKey { Id = 1 })),
                    context.Set<ClassKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ClassKey { Id = two })),
                    context.Set<ClassKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(three)),
                    context.Set<ClassKeyPrincipal>().Include(e => e.OptionalDependents).Single(e => e.Id.Equals(new ClassKey { Id = 4 }))
                };

                var twelve = 12;
                var thirteen = new ClassKey { Id = 13 };
                var fifteen = 15;
                var sixteen = new ClassKey { Id = 16 };

                dependents = new[]
                {
                    context.Set<ClassKeyOptionalDependent>().Single(e => e.Id.Equals(new ClassKey { Id = 11 })),
                    context.Set<ClassKeyOptionalDependent>().Single(e => e.Id.Equals(new ClassKey { Id = twelve })),
                    context.Set<ClassKeyOptionalDependent>().Single(e => e.Id.Equals(thirteen)),
                    context.Set<ClassKeyOptionalDependent>().Single(e => e.Id == new ClassKey { Id = 14 }),
                    context.Set<ClassKeyOptionalDependent>().Single(e => e.Id == new ClassKey { Id = fifteen }),
                    context.Set<ClassKeyOptionalDependent>().Single(e => e.Id == sixteen)
                };
           }

            void Validate(
                ClassKeyPrincipal[] principals,
                ClassKeyOptionalDependent[] dependents,
                (int, int[])[] expectedPrincipalToDependents,
                (int, int?)[] expectedDependentToPrincipals)
            {
                ValidateOptional(
                    1,
                    principals,
                    dependents,
                    expectedPrincipalToDependents,
                    expectedDependentToPrincipals,
                    p => ((ClassKeyPrincipal)p).OptionalDependents.Select(d => (IIntOptionalDependent)d).ToList(),
                    d => ((ClassKeyOptionalDependent)d).Principal);
            }
        }

        public abstract class KeysWithConvertersFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "KeysWithConverters";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<StructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructKey.Converter);
                    });

                modelBuilder.Entity<StructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(StructKey.Converter);
                    });

                modelBuilder.Entity<StructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(StructKey.Converter);
                    });

                modelBuilder.Entity<ClassKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ClassKey.Converter);
                    });

                modelBuilder.Entity<ClassKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ClassKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ClassKey.Converter);
                    });

                modelBuilder.Entity<ClassKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ClassKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ClassKey.Converter);
                    });

                modelBuilder.Entity<ComparableIntStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                    });

                modelBuilder.Entity<ComparableIntStructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ComparableIntStructKey.Converter);
                    });

                modelBuilder.Entity<ComparableIntStructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(ComparableIntStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(ComparableIntStructKey.Converter);
                    });

                modelBuilder.Entity<GenericComparableStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(GenericComparableStructKey.Converter);
                    });

                modelBuilder.Entity<GenericComparableStructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(GenericComparableStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(GenericComparableStructKey.Converter);
                    });

                modelBuilder.Entity<GenericComparableStructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(GenericComparableStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(GenericComparableStructKey.Converter);
                    });

                modelBuilder.Entity<StructuralComparableStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructuralComparableStructKey.Converter);
                    });

                modelBuilder.Entity<StructuralComparableStructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructuralComparableStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(StructuralComparableStructKey.Converter);
                    });

                modelBuilder.Entity<StructuralComparableStructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(StructuralComparableStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(StructuralComparableStructKey.Converter);
                    });

                modelBuilder.Entity<BytesStructKeyPrincipal>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                    });

                modelBuilder.Entity<BytesStructKeyOptionalDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(BytesStructKey.Converter);
                    });

                modelBuilder.Entity<BytesStructKeyRequiredDependent>(
                    b =>
                    {
                        b.Property(e => e.Id).HasConversion(BytesStructKey.Converter);
                        b.Property(e => e.PrincipalId).HasConversion(BytesStructKey.Converter);
                    });
            }
        }

        private void InsertOptionalGraph<TPrincipal, TDependent>(int keySeed)
            where TPrincipal : class, IIntPrincipal, new()
            where TDependent : class, IIntOptionalDependent, new()
        {
            using (var context = CreateContext())
            {
                context.Set<TPrincipal>().AddRange(
                    new TPrincipal { BackingId = keySeed, Foo = "X1" },
                    new TPrincipal { BackingId = keySeed + 1, Foo = "X2" },
                    new TPrincipal { BackingId = keySeed + 2, Foo = "X3" },
                    new TPrincipal { BackingId = keySeed + 3, Foo = "X4" });

                context.Set<TDependent>().AddRange(
                    new TDependent { BackingId = keySeed + 10, BackingPrincipalId = keySeed },
                    new TDependent { BackingId = keySeed + 11, BackingPrincipalId = keySeed + 1 },
                    new TDependent { BackingId = keySeed + 12, BackingPrincipalId = keySeed + 2 },
                    new TDependent { BackingId = keySeed + 13, BackingPrincipalId = keySeed + 2 },
                    new TDependent { BackingId = keySeed + 14, BackingPrincipalId = keySeed + 2 },
                    new TDependent { BackingId = keySeed + 15 });

                Assert.Equal(10, context.SaveChanges());
            }
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct StructKey
#pragma warning restore 660,661
        {
            public static ValueConverter<StructKey, int> Converter
                = new ValueConverter<StructKey, int>(v => v.Id, v => new StructKey { Id = v });

            public int Id { get; set; }

            public static bool operator ==(StructKey left, StructKey right)
                => left.Id == right.Id;

            public static bool operator !=(StructKey left, StructKey right)
                => left.Id != right.Id;
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct BytesStructKey
#pragma warning restore 660,661
        {
            public static ValueConverter<BytesStructKey, byte[]> Converter
                = new ValueConverter<BytesStructKey, byte[]>(v => v.Id, v => new BytesStructKey { Id = v });

            public byte[] Id { get; set; }

            public static bool operator ==(BytesStructKey left, BytesStructKey right)
                => left.Id.SequenceEqual(right.Id);

            public static bool operator !=(BytesStructKey left, BytesStructKey right)
                => !(left == right);
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct ComparableIntStructKey : IComparable
#pragma warning restore 660,661
        {
            public static ValueConverter<ComparableIntStructKey, int> Converter
                = new ValueConverter<ComparableIntStructKey, int>(v => v.Id, v => new ComparableIntStructKey { Id = v });

            public int Id { get; set; }

            public static bool operator ==(ComparableIntStructKey left, ComparableIntStructKey right)
                => left.Id == right.Id;

            public static bool operator !=(ComparableIntStructKey left, ComparableIntStructKey right)
                => left.Id != right.Id;

            public int CompareTo(object other)
                => Id - ((ComparableIntStructKey)other).Id;
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct GenericComparableStructKey : IComparable<GenericComparableStructKey>
#pragma warning restore 660,661
        {
            public static ValueConverter<GenericComparableStructKey, int> Converter
                = new ValueConverter<GenericComparableStructKey, int>(v => v.Id, v => new GenericComparableStructKey { Id = v });

            public int Id { get; set; }

            public static bool operator ==(GenericComparableStructKey left, GenericComparableStructKey right)
                => left.Id == right.Id;

            public static bool operator !=(GenericComparableStructKey left, GenericComparableStructKey right)
                => left.Id != right.Id;

            public int CompareTo(GenericComparableStructKey other)
                => Id - other.Id;
        }

#pragma warning disable 660,661 // Issue #19407
        protected struct StructuralComparableStructKey : IStructuralComparable
#pragma warning restore 660,661
        {
            public static ValueConverter<StructuralComparableStructKey, byte[]> Converter
                = new ValueConverter<StructuralComparableStructKey, byte[]>(v => v.Id, v => new StructuralComparableStructKey { Id = v });

            public byte[] Id { get; set; }

            public static bool operator ==(StructuralComparableStructKey left, StructuralComparableStructKey right)
                => left.Id == right.Id;

            public static bool operator !=(StructuralComparableStructKey left, StructuralComparableStructKey right)
                => left.Id != right.Id;

            public int CompareTo(object other, IComparer comparer)
            {
                var typedOther = ((StructuralComparableStructKey)other);

                var i = -1;
                var result = Id.Length - typedOther.Id.Length;

                while (result == 0
                    && ++i < Id.Length)
                {
                    result = comparer.Compare(Id[i], typedOther.Id[i]);
                }

                return result;
            }
        }

        protected class ClassKey
        {
            public static ValueConverter<ClassKey, int> Converter
                = new ValueConverter<ClassKey, int>(v => v.Id, v => new ClassKey { Id = v });

            protected bool Equals(ClassKey other)
                => other != null && Id == other.Id;

            public override bool Equals(object obj)
                => obj == this
                    || obj?.GetType() == GetType()
                    && Equals((ClassKey)obj);

            public override int GetHashCode() => Id;

            public int Id { get; set; }
        }

        protected interface IBinaryPrincipal
        {
            byte[] BackingId { get; set; }
        }

        protected interface IBinaryOptionalDependent
        {
            byte[] BackingId { get; set; }
            byte[] BackingPrincipalId { get; set; }
        }

        protected interface IBinaryRequiredDependent
        {
            byte[] BackingId { get; set; }
            byte[] BackingPrincipalId { get; set; }
        }

        protected interface IIntPrincipal
        {
            int BackingId { get; set; }
            string Foo { get; set; }
            // ICollection<IIntOptionalDependent> BackingOptionalDependents { get; }
            // ICollection<IIntRequiredDependent> BackingRequiredDependents { get; }
        }

        protected interface IIntRequiredDependent
        {
            int BackingId { get; set; }
            int BackingPrincipalId { get; set; }
            // IIntPrincipal Principal { get; set; }
        }

        protected interface IIntOptionalDependent
        {
            int BackingId { get; set; }
            int? BackingPrincipalId { get; set; }
            // IIntPrincipal BackingPrincipal { get; set; }
        }

        protected class StructKeyPrincipal : IIntPrincipal
        {
            public StructKey Id { get; set; }
            public string Foo { get; set; }
            public ICollection<StructKeyRequiredDependent> RequiredDependents { get; set; }
            public ICollection<StructKeyOptionalDependent> OptionalDependents { get; set; }

            [NotMapped]
            public int BackingId
            {
                get => Id.Id;
                set => Id = new StructKey { Id = value };
            }

            // [NotMapped]
            // ICollection<IIntRequiredDependent> IIntPrincipal.BackingRequiredDependents
            //     => (ICollection<IIntRequiredDependent>)RequiredDependents;
            //
            // [NotMapped]
            // ICollection<IIntOptionalDependent> IIntPrincipal.BackingOptionalDependents
            //     => (ICollection<IIntOptionalDependent>)OptionalDependents;
        }

        protected class StructKeyRequiredDependent : IIntRequiredDependent
        {
            public StructKey Id { get; set; }
            public StructKey PrincipalId { get; set; }
            public StructKeyPrincipal Principal { get; set; }

            [NotMapped]
            public int BackingId
            {
                get => Id.Id;
                set => Id = new StructKey { Id = value };
            }

            [NotMapped]
            public int BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new StructKey { Id = value };
            }

            // [NotMapped]
            // IIntPrincipal IIntRequiredDependent.Principal
            // {
            //     get => Principal;
            //     set => Principal = (StructKeyPrincipal)value;
            // }
        }

        protected class StructKeyOptionalDependent : IIntOptionalDependent
        {
            public StructKey Id { get; set; }
            public StructKey? PrincipalId { get; set; }
            public StructKeyPrincipal Principal { get; set; }

            [NotMapped]
            public int BackingId
            {
                get => Id.Id;
                set => Id = new StructKey { Id = value };
            }

            [NotMapped]
            public int? BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value.HasValue ? new StructKey { Id = value.Value } : (StructKey?)null;
            }

            // [NotMapped]
            // IIntPrincipal IIntOptionalDependent.BackingPrincipal
            // {
            //     get => Principal;
            //     set => Principal = (StructKeyPrincipal)value;
            // }
        }

        protected class BytesStructKeyPrincipal
        {
            public BytesStructKey Id { get; set; }
            public string Foo { get; set; }
            public ICollection<BytesStructKeyOptionalDependent> OptionalDependents { get; set; }
            public ICollection<BytesStructKeyRequiredDependent> RequiredDependents { get; set; }
        }

        protected class BytesStructKeyOptionalDependent
        {
            public BytesStructKey Id { get; set; }
            public BytesStructKey? PrincipalId { get; set; }
            public BytesStructKeyPrincipal Principal { get; set; }
        }

        protected class BytesStructKeyRequiredDependent
        {
            public BytesStructKey Id { get; set; }
            public BytesStructKey PrincipalId { get; set; }
            public BytesStructKeyPrincipal Principal { get; set; }
        }

        protected class ComparableIntStructKeyPrincipal
        {
            public ComparableIntStructKey Id { get; set; }
            public string Foo { get; set; }
            public ICollection<ComparableIntStructKeyOptionalDependent> OptionalDependents { get; set; }
            public ICollection<ComparableIntStructKeyRequiredDependent> RequiredDependents { get; set; }
        }

        protected class ComparableIntStructKeyOptionalDependent
        {
            public ComparableIntStructKey Id { get; set; }
            public ComparableIntStructKey? PrincipalId { get; set; }
            public ComparableIntStructKeyPrincipal Principal { get; set; }
        }

        protected class ComparableIntStructKeyRequiredDependent
        {
            public ComparableIntStructKey Id { get; set; }
            public ComparableIntStructKey? PrincipalId { get; set; }
            public ComparableIntStructKeyPrincipal Principal { get; set; }
        }

        protected class GenericComparableStructKeyPrincipal
        {
            public GenericComparableStructKey Id { get; set; }
            public string Foo { get; set; }
            public ICollection<GenericComparableStructKeyOptionalDependent> OptionalDependents { get; set; }
            public ICollection<GenericComparableStructKeyRequiredDependent> RequiredDependents { get; set; }
        }

        protected class GenericComparableStructKeyOptionalDependent
        {
            public GenericComparableStructKey Id { get; set; }
            public GenericComparableStructKey? PrincipalId { get; set; }
            public GenericComparableStructKeyPrincipal Principal { get; set; }
        }

        protected class GenericComparableStructKeyRequiredDependent
        {
            public GenericComparableStructKey Id { get; set; }
            public GenericComparableStructKey PrincipalId { get; set; }
            public GenericComparableStructKeyPrincipal Principal { get; set; }
        }

        protected class StructuralComparableStructKeyPrincipal
        {
            public StructuralComparableStructKey Id { get; set; }
            public string Foo { get; set; }
            public ICollection<StructuralComparableStructKeyOptionalDependent> OptionalDependents { get; set; }
            public ICollection<StructuralComparableStructKeyRequiredDependent> RequiredDependents { get; set; }
        }

        protected class StructuralComparableStructKeyOptionalDependent
        {
            public StructuralComparableStructKey Id { get; set; }
            public StructuralComparableStructKey? PrincipalId { get; set; }
            public StructuralComparableStructKeyPrincipal Principal { get; set; }
        }

        protected class StructuralComparableStructKeyRequiredDependent
        {
            public StructuralComparableStructKey Id { get; set; }
            public StructuralComparableStructKey PrincipalId { get; set; }
            public StructuralComparableStructKeyPrincipal Principal { get; set; }
        }

        protected class ClassKeyPrincipal : IIntPrincipal
        {
            public ClassKey Id { get; set; }
            public string Foo { get; set; }
            public ICollection<ClassKeyOptionalDependent> OptionalDependents { get; set; }
            public ICollection<ClassKeyRequiredDependent> RequiredDependents { get; set; }

            [NotMapped]
            public int BackingId
            {
                get => Id.Id;
                set => Id = new ClassKey { Id = value };
            }

            // [NotMapped]
            // ICollection<IIntRequiredDependent> IIntPrincipal.BackingRequiredDependents
            //     => (ICollection<IIntRequiredDependent>)RequiredDependents;
            //
            // [NotMapped]
            // ICollection<IIntOptionalDependent> IIntPrincipal.BackingOptionalDependents
            //     => (ICollection<IIntOptionalDependent>)OptionalDependents;
        }

        protected class ClassKeyOptionalDependent : IIntOptionalDependent
        {
            public ClassKey Id { get; set; }
            public ClassKey PrincipalId { get; set; }
            public ClassKeyPrincipal Principal { get; set; }

            [NotMapped]
            public int BackingId
            {
                get => Id.Id;
                set => Id = new ClassKey { Id = value };
            }

            [NotMapped]
            public int? BackingPrincipalId
            {
                get => PrincipalId?.Id;
                set => PrincipalId = value.HasValue ? new ClassKey { Id = value.Value } : null;
            }

            // [NotMapped]
            // IIntPrincipal IIntOptionalDependent.BackingPrincipal
            // {
            //     get => Principal;
            //     set => Principal = (ClassKeyPrincipal)value;
            // }
        }

        protected class ClassKeyRequiredDependent : IIntRequiredDependent
        {
            public ClassKey Id { get; set; }
            public ClassKey PrincipalId { get; set; }
            public ClassKeyPrincipal Principal { get; set; }

            [NotMapped]
            public int BackingId
            {
                get => Id.Id;
                set => Id = new ClassKey { Id = value };
            }

            [NotMapped]
            public int BackingPrincipalId
            {
                get => PrincipalId.Id;
                set => PrincipalId = new ClassKey { Id = value };
            }

            // [NotMapped]
            // IIntPrincipal IIntRequiredDependent.Principal
            // {
            //     get => Principal;
            //     set => Principal = (ClassKeyPrincipal)value;
            // }
        }
    }
}
