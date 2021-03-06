﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Simple.Common;

namespace Simple.Tests.Common
{
    public class SafeNullableFixture
    {
        [Test]
        public void CanGetValuesWithNoNullableValue()
        {
            SafeNullable.Get(() => 2).Should().Be(2);
        }

        [Test]
        public void CanGetValuesWithNullableValue()
        {
            object a = null;
            var safe = SafeNullable.Get(() => a.ToString());

            safe.Should().Be.OfType<SafeValue<string>>().And
                .ValueOf.Value.Should().Be.Null();

        }

        [Test]
        public void CanGetValuesWithSafe()
        {
            object a = null;
            var safe = a.WithSafe(x => x.ToString());

            safe.Should().Be.OfType<SafeValue<string>>().And
                .ValueOf.Value.Should().Be.Null();

        }

        [Test]
        public void CanGetValuesWithSafeAndDefault()
        {
            object a = null;
            var safe = a.WithSafe(x => x.ToString(), "asd");

            safe.Should().Be.OfType<SafeValue<string>>().And
                .ValueOf.Value.Should().Be("asd");

        }

        [Test]
        public void CanGetIntValueWithNullableValue()
        {
            object a = null;
            SafeNullable.Get(() => int.Parse(a.ToString())).Should().Be(0);
        }

        [Test]
        public void CanGetIntValueWithNullableValueAndDefValue()
        {
            object a = null;
            SafeNullable.Get(() => int.Parse(a.ToString()), 42).Should().Be(42);
        }

        [Test]
        public void CanGetStringValueWithNullableValueAndDefValue()
        {
            object a = null;
            SafeNullable.Get(() => a.ToString(), "42").Should().Be("42");
        }

        [Test]
        public void CanCallWithOnIntTypesAndReturnsZero()
        {
            IList<int> list = null;
            list.With(x => x.Count).Should().Be(0);
        }

        [Test]
        public void CanCallWithOnIntTypesAndReturnsDefaultValue()
        {
            IList<int> list = null;
            list.With(x => x.Count, 42).Should().Be(42);
        }

        [Test]
        public void CanCallWithNullableOnIntTypesThatCanBeNull()
        {
            IList<int> list = null;
            list.WithNullable(x => x.Count).Should().Be(null);
        }

        [Test]
        public void CanCallWithNullableOnIntTypesThatCanBeNullWithDefault()
        {
            IList<int> list = null;
            list.WithNullable(x => x.Count, null).Should().Be(null);
        }
    }
}
