﻿using System;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
// ReSharper disable All

namespace Tiger.Types.UnitTest
{
    /// <context>Tests related to acting upon <see cref="Either{TLeft,TRight}"/>.</context>
    public static partial class EitherTests
    {
        [Property(DisplayName = "A Left Either does not enumerate.")]
        static void GetEnumerator_Left(Guid left, Version before)
        {
            var actual = before;
            foreach (var v in Either.Left<Guid, Version>(left))
            {
                actual = v;
            }

            Assert.Equal(before, actual);
        }

        [Property(DisplayName = "A Right Either enumerates.")]
        static void GetEnumerator_Right(Version right, Version before)
        {
            var actual = before;
            foreach (var v in Either.Right<Guid, Version>(right))
            {
                actual = v;
            }

            Assert.Equal(right, actual);
        }

        [Property(DisplayName = "A Bottom Either does not enumerate.")]
        static void GetEnumerator_Bottom(Version before)
        {
            var actual = before;
            foreach (var v in default(Either<Guid, Version>))
            {
                actual = v;
            }

            Assert.Equal(before, actual);
        }
    }
}
