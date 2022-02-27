﻿// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Disposables
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using FFT.IgnoreTasks;
  using static System.Threading.Tasks.TaskCreationOptions;

#pragma warning disable CA1063 // Implement IDisposable Correctly

  /// <summary>
  /// Inherit this class to use boiler plate code for disposable objects.
  /// </summary>
  public abstract class DisposeBase : IDisposable
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
      DisposedToken = _disposed.Token;
      // It's very important that the DisposedTask continuations run
      // asynchronously and do not block the progress of the Dispose method,
      // which should be able to complete immediately without waiting for this
      // tasks's children to complete. So don't modify the constructor
      // parameters.
      _disposedTaskSource = new(RunContinuationsAsynchronously);
      DisposedTask = _disposedTaskSource.Task;
    }

    /// <summary>
    /// This cancellation token is cancelled when <see cref="Dispose()"/> has been called.
    /// You can use it for cancelling operations that need to end when this class is disposed.
    /// </summary>
    public CancellationToken DisposedToken { get; }

    /// <summary>
    /// Gets a <see cref="Task"/> that completes (without exception) when this object has been disposed.
    /// The task is not completed until the entire Dispose method (including the call to <see cref="CustomDispose()"/>) has been completed.
    /// </summary>
    public Task DisposedTask { get; }

    /// <summary>
    /// Returns true if the Dispose method has been entered.
    /// </summary>
    public bool IsDisposeStarted => Interlocked.Read(ref _enteredDispose) == 1;

    /// <summary>
    /// At the time of disposal, just before the <see cref="CustomDispose()"/> method is called,
    /// this property will contain the exception that triggered disposal. If disposal is due to
    /// calling the <see cref="Dispose()"/> method when finished with the object with no exception,
    /// this property will contain an <see cref="ObjectDisposedException"/>.
    /// </summary>
    public Exception? DisposalReason { get; private set; }

    /// <summary>
    /// Starts disposal in a threadpool thread and returns without waiting for
    /// the disposal to be completed. Exceptions thrown by the disposal method
    /// (there should be none, unless you have a bug in the <see
    /// cref="CustomDispose"/> method) are observed so they are not thrown in
    /// the finalizer thread.
    /// </summary>
    public void KickoffDispose(Exception? disposalReason = null)
      => Task.Run(() => Dispose(disposalReason)).Ignore();

    /// <summary>
    /// Disposes the object, setting the <see cref="DisposalReason"/> property to an instance of <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void Dispose()
        => Dispose(new ObjectDisposedException("Object has been disposed."));

    /// <summary>
    /// Disposes the object, setting the <see cref="DisposalReason"/> property to the given <paramref name="disposalReason"/>.
    /// </summary>
    public void Dispose(Exception? disposalReason)
    {
      // Ensures that disposal only runs once.
      if (Interlocked.Exchange(ref _enteredDispose, 1) == 1)
        return;

      DisposalReason = disposalReason;
      _disposed.Cancel();
      _disposed.Dispose();
      CustomDispose();
      _disposedTaskSource.SetResult(null);
    }

    /// <summary>
    /// Override this method if you have some custom operations to perform at disposal.
    /// This method is guaranteed to be called only once.
    /// </summary>
    protected virtual void CustomDispose() { }
  }
}
