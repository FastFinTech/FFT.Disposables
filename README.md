# FFT.Disposables

[![NuGet package](https://img.shields.io/nuget/v/FFT.Disposables.svg)](https://nuget.org/packages/FFT.Disposables)

The `DisposeBase` class provides a number of features that make it easier to write code handling complex logic around the lifetimes of objects.

1. The `Dispose` method and the `CustomDispose` override method are guaranteed to run only once, regardless of how many times they are called.

1. The `DisposedToken` cancellation token property allows tasks to be aborted when the object is disposed.

1. The `DisposedTask` property allows user code to await disposal or be notified of disposal by adding a continuation to the task.

1. The `DisposalReason` exception property allows inspection of the reason an object was disposed.

The `AsyncDisposeBase` class is the same thing but implementing `IAsyncDisposable`. 

Neither of these base classes implements a finalization method. For this reason, the `CustomDispose` method does not have a `bool disposing` parameter because it is assumed that `Dispose` is being called by using code, not by a Garbage Collector's finalizer thread.

