// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFT.Disposables;
/// <summary>
/// Contains properties that are useful for acting upon notification of an object's disposal.
/// </summary>
public interface IDisposeBase
{
  /// <summary>
  /// This task completes without error when the object's disposal has been completed.
  /// </summary>
  Task DisposedTask { get; }

  /// <summary>
  /// At the time of disposal, just before the <see cref="CustomDisposeAsync()"/> method is called,
  /// this property will contain the exception that triggered disposal. If disposal is due to
  /// calling the <see cref="DisposeAsync()"/> method when finished with the object with no exception,
  /// this property will contain an <see cref="ObjectDisposedException"/>.
  /// Guranteed not-null when the <see cref="DisposingToken"/> is cancelled or the <see cref="DisposedTask"/> is completed.
  /// </summary>
  Exception? DisposalReason { get; }

  /// <summary>
  /// This cancellation token cancels itself when the object disposal has started.
  /// The disposal has not necessarily been fully completed when the token is canceled,
  /// but the <see cref="DisposalReason"/> is guaranteed to be set.
  /// </summary>
  CancellationToken DisposingToken { get; }

  /// <summary>
  /// Returns true if the Dispose method has been entered.
  /// </summary>
  bool IsDisposeStarted { get; }

  /// <summary>
  /// Starts disposal in a threadpool thread and returns without waiting for
  /// the disposal to be completed.
  /// Exceptions thrown by the disposal are observed but ignored so they are not thrown
  /// in the finalizer thread.
  /// </summary>
  void KickoffDispose(Exception? exception = null);

  /// <summary>
  /// Registers a callback to execute when this object is disposed, passing in
  /// the <see cref="DisposalReason"/> exception. Callback immediately executes
  /// if the object is already disposed.
  /// </summary>
  /// <param name="action">The callback to execute.</param>
  void OnDisposing(Action<Exception> action);

  /// <summary>
  /// Starts the given <paramref name="task"/> on a background thread, calling
  /// <see cref="KickoffDispose(Exception?)"/> if the task completes with an
  /// error that is NOT an operation canceled exception.
  /// </summary>
  void RunBackground(Func<Task> task);
}
