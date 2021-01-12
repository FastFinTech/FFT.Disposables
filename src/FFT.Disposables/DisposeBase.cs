// Copyright (c) True Goodwill. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFT.Disposables
{

#pragma warning disable CA1063 // Implement IDisposable Correctly

    /// <summary>
    /// Inherit this class to use boiler plate code for disposable objects.
    /// </summary>
    public abstract class DisposeBase : IDisposable
    {
        private readonly CancellationTokenSource disposed;
        private readonly TaskCompletionSource<object?> disposedTaskSource;

        private long enteredDispose;

        /// <summary>
        /// This cancellation token is cancelled when <see cref="Dispose()"/> has been called.
        /// You can use it for cancelling operations that need to end when this class is disposed.
        /// </summary>
        public CancellationToken DisposedToken { get; }

        /// <summary>
        /// Gets a <see cref="Task"/> that completes (without exception) when this object has been disposed.
        /// The task is not completed until the entire Dispose method (including the call to <see cref="CustomDispose(bool)"/>) has been completed.
        /// </summary>
        public Task DisposedTask { get; }

        /// <summary>
        /// Returns true if the Dispose method has been entered.
        /// </summary>
        public bool IsDisposeStarted => Interlocked.Read(ref this.enteredDispose) == 1;

        /// <summary>
        /// At the time of disposal, just before the <see cref="CustomDispose(bool)"/> method is called,
        /// this property will contain the exception that triggered disposal. If disposal is due to 
        /// calling the <see cref="Dispose"/> method when finished with the object with no exception, 
        /// this property will contain an <see cref="ObjectDisposedException"/>.
        /// </summary>
        public Exception? DisposalReason { get; private set; }

        protected DisposeBase()
        {
            this.disposed = new();
            this.DisposedToken = this.disposed.Token;
            /// It's very important that the DisposedTask continuations run asynchronously and do not block the progress of the Dispose method, 
            /// which should be able to complete immediately without waiting for this tasks's children to complete.
            /// So don't modify the constructor parameters.
            this.disposedTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            this.DisposedTask = this.disposedTaskSource.Task;
        }

        /// <summary>
        /// Override this method if you have some custom operations to perform at disposal. 
        /// This method is guaranteed to be called only once.
        /// </summary>
        /// <param name="disposing">True during a disposal called by user code. False during a disposal called by the GC finalizer thread.</param>
        protected virtual void CustomDispose(bool disposing) { }

        /// <summary>
        /// Disposes the object, setting the <see cref="DisposalReason"/> property to an instance of <see cref="ObjectDisposedException"/>.
        /// If this method is called more than once, subsequent callers will have wait with a blocked 
        /// thread until the first caller's dispose has been completed.
        /// There is potential for a threadlock if you're not aware of this.
        /// </summary>
        public void Dispose()
            => this.Dispose(true, new ObjectDisposedException("Object has been disposed."));

        /// <summary>
        /// Disposes the object, setting the <see cref="DisposalReason"/> property to the given <paramref name="disposalReason"/>.
        /// If this method is called more than once, subsequent callers will have wait with a blocked 
        /// thread until the first caller's dispose has been completed.
        /// There is potential for a threadlock if you're not aware of this.
        /// </summary>
        public void Dispose(Exception? disposalReason)
            => this.Dispose(true, disposalReason);

        private void Dispose(bool disposing, Exception? x)
        {
            // Ensures that disposal only runs once.
            if (Interlocked.CompareExchange(ref this.enteredDispose, 1, 0) == 1)
                return;

            if (disposing)
            {
                this.DisposalReason = x;
                this.disposed.Cancel();
                this.disposed.Dispose();
            }

            this.CustomDispose(disposing);

            if (disposing)
            {
                this.disposedTaskSource.SetResult(null);
                GC.SuppressFinalize(this);
            }
        }

        ~DisposeBase()
            => this.Dispose(false, null);
    }
}
