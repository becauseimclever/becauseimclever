namespace BecauseImClever.Infrastructure.Tests.Services;

public class CommissionPostgresSafetyTests
{
    [Theory]
    [InlineData("localhost", "commissions_disposable_ci")]
    [InlineData("127.0.0.1", "commissions_disposable_ci")]
    [InlineData("::1", "commissions_disposable_ci")]
    public void ValidateConnectionString_LoopbackAndDisposable_AllowsConnection(string host, string database)
    {
        var connectionString = $"Host={host};Database={database}";
        Assert.Equal(connectionString, CommissionPostgresTests.ValidateConnectionString(connectionString));
    }

    [Theory]
    [InlineData("example.invalid", "commissions_disposable_ci")]
    [InlineData("192.0.2.1", "commissions_disposable_ci")]
    [InlineData("localhost,192.0.2.1", "commissions_disposable_ci")]
    [InlineData("localhost", "application")]
    public void ValidateConnectionString_UnsafeHostOrDatabase_RejectsConnection(string host, string database)
    {
        Assert.Throws<InvalidOperationException>(() => CommissionPostgresTests.ValidateConnectionString($"Host={host};Database={database}"));
    }
}
