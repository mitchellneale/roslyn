﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Roslyn.Utilities
{
    internal static class LazyInitialization
    {
        internal static T InterlockedStore<T>([NotNull] ref T? target, T value) where T : class
            => Interlocked.CompareExchange(ref target, value, null) ?? value;

        /// <summary>
        /// Ensure that the given target value is initialized (not null) in a thread-safe manner.
        /// </summary>
        /// <typeparam name="T">The type of the target value. Must be a reference type.</typeparam>
        /// <param name="target">The target to initialize.</param>
        /// <param name="valueFactory">A factory delegate to create a new instance of the target value. Note that this delegate may be called
        /// more than once by multiple threads, but only one of those values will successfully be written to the target.</param>
        /// <returns>The target value.</returns>
        public static T EnsureInitialized<T>([NotNull] ref T? target, Func<T> valueFactory) where T : class
            => Volatile.Read(ref target!) ?? InterlockedStore(ref target, valueFactory());

        /// <summary>
        /// Ensure that the given target value is initialized (not null) in a thread-safe manner.
        /// </summary>
        /// <typeparam name="T">The type of the target value. Must be a reference type.</typeparam>
        /// <param name="target">The target to initialize.</param>
        /// <typeparam name="U">The type of the <paramref name="state"/> argument passed to the value factory.</typeparam>
        /// <param name="valueFactory">A factory delegate to create a new instance of the target value. Note that this delegate may be called
        /// more than once by multiple threads, but only one of those values will successfully be written to the target.</param>
        /// <param name="state">An argument passed to the value factory.</param>
        /// <returns>The target value.</returns>
        public static T EnsureInitialized<T, U>([NotNull] ref T? target, Func<U, T> valueFactory, U state)
            where T : class
        {
            return Volatile.Read(ref target!) ?? InterlockedStore(ref target, valueFactory(state));
        }

        /// <summary>
        /// Ensure that the given target value is initialized in a thread-safe manner. This overload supports the
        /// initialization of value types, and reference type fields where <see langword="null"/> is considered an
        /// initialized value.
        /// </summary>
        /// <typeparam name="T">The type of the target value.</typeparam>
        /// <param name="target">A target value box to initialize.</param>
        /// <param name="valueFactory">A factory delegate to create a new instance of the target value. Note that this delegate may be called
        /// more than once by multiple threads, but only one of those values will successfully be written to the target.</param>
        /// <returns>The target value.</returns>
        public static T? EnsureInitialized<T>([NotNull] ref StrongBox<T?>? target, Func<T?> valueFactory)
        {
            var box = Volatile.Read(ref target!) ?? InterlockedStore(ref target, new StrongBox<T?>(valueFactory()));
            return box.Value;
        }

        /// <summary>
        /// Ensure that the given target value is initialized in a thread-safe manner. This overload supports the
        /// initialization of value types, and reference type fields where <see langword="null"/> is considered an
        /// initialized value.
        /// </summary>
        /// <typeparam name="T">The type of the target value.</typeparam>
        /// <param name="target">A target value box to initialize.</param>
        /// <typeparam name="U">The type of the <paramref name="state"/> argument passed to the value factory.</typeparam>
        /// <param name="valueFactory">A factory delegate to create a new instance of the target value. Note that this delegate may be called
        /// more than once by multiple threads, but only one of those values will successfully be written to the target.</param>
        /// <param name="state">An argument passed to the value factory.</param>
        /// <returns>The target value.</returns>
        public static T? EnsureInitialized<T, U>([NotNull] ref StrongBox<T?>? target, Func<U, T?> valueFactory, U state)
        {
            var box = Volatile.Read(ref target!) ?? InterlockedStore(ref target, new StrongBox<T?>(valueFactory(state)));
            return box.Value;
        }
    }
}
