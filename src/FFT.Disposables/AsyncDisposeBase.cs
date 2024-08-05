// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Threading;
using System.Threading.Tasks;
using FFT.IgnoreTasks;
using static System.Threading.Tasks.TaskCreationOptions;

namespace FFT.Disposables;
/// <summary>
/// Inherit this class to use boiler plate code for disposable objects.
/// </summary>
public abstract class AsyncDisposeBase : IAsyncDisposable, IDisposeBase
{
  private readonly CancellationTokenSource _disposed;
  private readonly TaskCompletionSource<object?> _disposedTaskSource;

  private long _enteredDispose;

  /// <summary>
  /// Initializes a new instance of the <see cref="AsyncDisposeBase"/> class.
  /// </summary>
  protected AsyncDisposeBase()
  {
    _disposed = new();
    DisposingToken = _disposed.Token;
    // It's very important that the DisposedTask continuations run
    // asynchronously and do not block the progress of the Dispose method,
    // which should be able to complete immediately without waiting for this
    // tasks's children to complete. So don't modify the constructor
    // parameters.
    _disposedTaskSource = new(RunContinuationsAsynchronously);
    DisposedTask = _disposedTaskSource.Task;
  }

  /// <inheritdoc/>
  public CancellationToken DisposingToken { get; }

  /// <inheritdoc/>
  public Task DisposedTask { get; }

  /// <inheritdoc/>
  public bool IsDisposeStarted => Interlocked.Read(ref _enteredDispose) == 1;

  /// <inheritdoc/>
  public Exception? DisposalReason { get; private set; }

  /// <inheritdoc/>
  public void KickoffDispose(Exception? disposalReason = null)
    => DisposeAsync(disposalReason).Ignore();

  /// <inheritdoc/>
  public ValueTask DisposeAsync()
      => DisposeAsync(null);

  /// <inheritdoc/>
  public void OnDisposing(Action<Exception> action)
  {
    DisposingToken.Register(() => action(DisposalReason!));
  }

  /// <summary>
  /// Disposes the object, setting the <see cref="DisposalReason"/> property to the given <paramref name="disposalReason"/>.
  /// </summary>
  public async ValueTask DisposeAsync(Exception? disposalReason = null)
  {
    // Ensures that disposal only runs once.
    if (Interlocked.Exchange(ref _enteredDispose, 1) == 1)
      return;

    DisposalReason = disposalReason ?? new ObjectDisposedException("Object has been disposed.");
    _disposed.Cancel();
    _disposed.Dispose();
    await CustomDisposeAsync().ConfigureAwait(false);
    _disposedTaskSource.SetResult(null);
  }

  /// <summary>
  /// Override this method if you have some custom operations to perform at disposal.
  /// This method is guaranteed to be called only once.
  /// </summary>
  protected virtual ValueTask CustomDisposeAsync() => default;
}
