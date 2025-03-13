// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Threading;
using System.Threading.Tasks;
using FFT.IgnoreTasks;
using static System.Threading.Tasks.TaskCreationOptions;

namespace FFT.Disposables;
#pragma warning disable CA1063 // Implement IDisposable Correctly

/// <summary>
/// Inherit this class to use boiler plate code for disposable objects.
/// </summary>
public abstract class DisposeBase : IDisposable, IDisposeBase
{
  private readonly CancellationTokenSource _disposed;
  private readonly TaskCompletionSource<object?> _disposedTaskSource;

  private long _enteredDispose;

  /// <summary>
  /// Initializes a new instance of the <see cref="DisposeBase"/> class.
  /// </summary>
  protected DisposeBase()
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

  /// <inheritdoc cref="KickoffDispose(string)"/>
  public void KickoffDispose(string reason)
  {
    KickoffDispose(reason is null ? null : new Exception(reason));
  }

  /// <inheritdoc/>
  public void KickoffDispose(Exception? disposalReason = null)
    => Task.Run(() => Dispose(disposalReason)).Ignore();

  /// <summary>
  /// Disposes the object, setting the <see cref="DisposalReason"/> property to an instance of <see cref="ObjectDisposedException"/>.
  /// </summary>
  public void Dispose()
      => Dispose(null);

  /// <summary>
  /// Disposes the object, setting the <see cref="DisposalReason"/> property to the given <paramref name="disposalReason"/>.
  /// </summary>
  public void Dispose(Exception? disposalReason = null)
  {
    // Ensures that disposal only runs once.
    if (Interlocked.Exchange(ref _enteredDispose, 1) == 1)
      return;

    DisposalReason = disposalReason ?? new ObjectDisposedException("Object has been disposed.");
    _disposed.Cancel();
    _disposed.Dispose();
    CustomDispose();
    _disposedTaskSource.SetResult(null);
  }

  /// <inheritdoc/>
  public void OnDisposing(Action<Exception> action)
  {
    DisposingToken.Register(() => action(DisposalReason!));
  }

  /// <inheritdoc cref="IDisposeBase.RunBackground(Func{Task})"/>
  public Task RunBackground(Func<Task> task)
  {
    return Task.Run(async () =>
    {
      try
      {
        await task();
      }
      catch (Exception x)
      {
        KickoffDispose(x);
      }
    });
  }

  /// <summary>
  /// Override this method if you have some custom operations to perform at disposal.
  /// This method is guaranteed to be called only once.
  /// </summary>
  protected virtual void CustomDispose() { }
}
