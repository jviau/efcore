// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class KeysWithConvertersSqlServerTest : KeysWithConvertersTestBase<KeysWithConvertersSqlServerTest.KeysWithConvertersSqlServerFixture>
    {
        public KeysWithConvertersSqlServerTest(KeysWithConvertersSqlServerFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public void Main()
        {

        }

        public class KeysWithConvertersSqlServerFixture : KeysWithConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            // protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            // {
            //     base.OnModelCreating(modelBuilder, context);
            // }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder.UseSqlServer(b => b.MinBatchSize(1));
        }
   }
}
