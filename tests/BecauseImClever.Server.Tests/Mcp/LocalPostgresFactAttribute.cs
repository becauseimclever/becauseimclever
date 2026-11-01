namespace BecauseImClever.Server.Tests.Mcp;

public sealed class LocalPostgresFactAttribute : FactAttribute
{
    public LocalPostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BIC_MCP_TEST_DB")))
        {
            this.Skip = "Requires a dedicated local PostgreSQL database via BIC_MCP_TEST_DB.";
        }
    }
}
