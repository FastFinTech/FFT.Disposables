// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Threading.Tasks;

namespace FFT.Disposables;

public static class IDisposableExtensions
{
  public static ValueTask DisposeAsyncIfNotNull(this IDisposable? disposable)
  {
    disposable?.Dispose();
    return default;
  }

  public static ValueTask DisposeIfNotNullAsync(this IAsyncDisposable? disposable)
  {
    return disposable?.DisposeAsync() ?? default;
  }
}
