# FFT.Disposables

[![NuGet package](https://img.shields.io/nuget/v/FFT.Disposables.svg)](https://nuget.org/packages/FFT.Disposables)

The `DisposeBase` and `AsyncDisposeBase` classes provide a number of features that make it easier to write complex logic around the lifetimes of objects. They implement `IDisposable` and `IAsyncDisposable` respectively.

Override the `CustomDispose` or `CustomDisposeAsync` method to implement disposal behaviour that is guaranteed to execute only once.

1. The `DisposalReason` exception property allows inspection of the reason an object was disposed.

1. The `DisposedToken` cancellation token property allows tasks to be canceled when the object is disposed.

1. The `DisposedTask` property allows user code to await disposal or be notified of disposal by adding a continuation to the task.

1. The `KickoffDispose` method allows you to start disposal in a threadpool thread without waiting for it to complete. Exceptions thrown by the `CustomDispose` method (due to a bug in YOUR code) are observed so that they don't get rethrown by the finalizer thread.

1. The `IsDisposeStarted` property allows you to check whether disposal has started, in a general sense. It's not perfectly thread-safe, so it could return false after another thread has entered the disposal method but not yet "flicked the switch" on the flag used by this property.

Neither of these base classes have a finalizer method. For this reason, the `CustomDispose` methods do not have a `bool disposing` parameter because it is assumed that `Dispose` is being called by using code, not by a Garbage Collector's finalizer thread.
