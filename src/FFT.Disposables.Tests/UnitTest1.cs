using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFT.Disposables.Tests
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public async Task TestMethod1()
    {
      using var x = new TestDisposable();
    }

    [TestMethod]
    public async Task TestMethod2()
    {
      await using var x = new AsyncTestDisposable();
    }
  }



  internal class TestDisposable : DisposeBase
  {
  }

  internal class AsyncTestDisposable : AsyncDisposeBase
  {
  }
}
