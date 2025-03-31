// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System.Threading.Tasks;

namespace FFT.Disposables;

public static class IDisposeBaseExtensions
{
  public static async ValueTask DisposeAsyncIfNotNull(this IDisposeBase? disposable)
  {
    if (disposable is DisposeBase synchronousDisposable)
      synchronousDisposable.Dispose();
    else if (disposable is AsyncDisposeBase asyncronousDisposable)
      await asyncronousDisposable.DisposeAsync();
  }
}
