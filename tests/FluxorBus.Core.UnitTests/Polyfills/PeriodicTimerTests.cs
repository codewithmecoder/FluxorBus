namespace FluxorBus.Core.UnitTests.Polyfills;

[TestClass]
public class PeriodicTimerTests
{
    [TestMethod]
    public void Constructor_ValidPeriod_CreatesInstance()
    {
        // Arrange
        var period = TimeSpan.FromMilliseconds(100);

        // Act
        using var timer = new PeriodicTimer(period);

        // Assert
        Assert.IsNotNull(timer);
    }

    [TestMethod]
    public void Constructor_ZeroPeriod_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var period = TimeSpan.Zero;

        // Act & Assert
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PeriodicTimer(period));
        Assert.AreEqual("period", exception.ParamName);
    }

    [TestMethod]
    public void Constructor_NegativePeriod_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var period = TimeSpan.FromMilliseconds(-100);

        // Act & Assert
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PeriodicTimer(period));
        Assert.AreEqual("period", exception.ParamName);
    }

    [TestMethod]
    public async Task WaitForNextTickAsync_NoCancellation_ReturnsTrue()
    {
        // Arrange
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));

        // Act
        var result = await timer.WaitForNextTickAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task WaitForNextTickAsync_WithNonCanceledToken_ReturnsTrue()
    {
        // Arrange
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));
        using var cts = new CancellationTokenSource();

        // Act
        var result = await timer.WaitForNextTickAsync(cts.Token);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task WaitForNextTickAsync_AfterDispose_ReturnsFalse()
    {
        // Arrange
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));
        timer.Dispose();

        // Act
        var result = await timer.WaitForNextTickAsync();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task WaitForNextTickAsync_MultipleTicks_ReturnsTrue()
    {
        // Arrange
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));

        // Act
        var result1 = await timer.WaitForNextTickAsync();
        var result2 = await timer.WaitForNextTickAsync();
        var result3 = await timer.WaitForNextTickAsync();

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
        Assert.IsTrue(result3);
    }

    [TestMethod]
    public void Dispose_CalledOnce_SetsDisposedFlag()
    {
        // Arrange
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));

        // Act
        timer.Dispose();

        // Assert - verify by calling WaitForNextTickAsync
        var result = timer.WaitForNextTickAsync().AsTask().Result;
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));

        // Act & Assert
        timer.Dispose();
        timer.Dispose();
        timer.Dispose();
    }

    [TestMethod]
    public async Task WaitForNextTickAsync_DisposedBetweenCalls_ReturnsFalse()
    {
        // Arrange
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));
        await timer.WaitForNextTickAsync();

        // Act
        timer.Dispose();
        var result = await timer.WaitForNextTickAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }
}
