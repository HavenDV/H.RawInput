using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.RawInput.UnitTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public async Task DelayTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}
