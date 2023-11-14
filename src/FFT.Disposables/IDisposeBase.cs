// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace FFT.Disposables;

using System;
using System.Threading;
using System.Threading.Tasks;

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
  /// Guranteed not-null when the <see cref="DisposedToken"/> is cancelled or the <see cref="DisposedTask"/> is completed.
  /// </summary>
  Exception? DisposalReason { get; }

  /// <summary>
  /// This cancellation token cancels itself when the object disposal has started.
  /// The disposal has not necessarily been fully completed when the token is canceled,
  /// but the <see cref="DisposalReason"/> is guaranteed to be set.
  /// </summary>
  CancellationToken DisposedToken { get; }

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
}
