# ESCd.Extensions.Cacing

[![Version](https://img.shields.io/nuget/vpre/ESCd.Extensions.Caching)](https://www.nuget.org/packages/ESCd.Extensions.Caching)

Common extensions and utilities for working with MemoryCache/DistributedCache.

## Commonly Used Types

- [`IAsyncCache`](https://github.com/ESCd/dotnet-extensions/tree/develop/src/Caching/Abstractions/IAsyncCache.cs): Provides a wrapper around `IMemoryCache` with support for `ValueTask` + cache stampede protection.