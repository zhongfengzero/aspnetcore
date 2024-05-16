// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.OpenApi;

internal static class ConcurrentDictionaryExtensions
{
    public static async ValueTask<TValue> GetOrAddAsync<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> d, TKey key, Func<TKey, ValueTask<TValue>> valueFactory)
        where TKey : notnull
        => d.TryGetValue(key, out TValue? value) ? value : d.GetOrAdd(key, await valueFactory(key));
}
