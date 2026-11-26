namespace BecauseImClever.Server.Tests.Mcp;

using BecauseImClever.Application.Commissions;
using BecauseImClever.Server.Mcp;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using Moq;

public class CommissionToolAuditTests
{
    [Fact]
    public async Task Create_Success_AuditsOperationWithoutProductContent()
    {
        var service = new Mock<IDraftCommissionService>();
        var logger = new Mock<ILogger<CommissionTools>>();
        var request = new CommissionWriteRequest("private-title", "private-slug", "private-body", 20, []);
        service.Setup(s => s.CreateAsync(request, default)).ReturnsAsync(new CommissionResponse(Guid.NewGuid(), request.Title, request.Slug, request.Description, 20, [], false, Guid.NewGuid(), []));
        var tools = new CommissionTools(service.Object, logger.Object);

        await tools.Create(request, default);

        var logged = Assert.Single(logger.Invocations, i => i.Method.Name == nameof(ILogger.Log));
        Assert.Equal(LogLevel.Information, logged.Arguments[0]);
        Assert.Equal("MCP create_commission_draft success", logged.Arguments[2]?.ToString());
        Assert.Null(logged.Arguments[3]);
    }

    [Fact]
    public async Task Get_UnexpectedFailure_LogsTypeAndOperationButReturnsSafeError()
    {
        var service = new Mock<IDraftCommissionService>();
        var logger = new Mock<ILogger<CommissionTools>>();
        service.Setup(s => s.GetAsync(It.IsAny<Guid>(), default)).ThrowsAsync(new InvalidOperationException("private-error-body"));
        var tools = new CommissionTools(service.Object, logger.Object);

        var error = await Assert.ThrowsAsync<McpException>(() => tools.Get(Guid.NewGuid(), default));

        Assert.Equal("operation_failed", error.Message);
        var logged = Assert.Single(logger.Invocations, i => i.Method.Name == nameof(ILogger.Log));
        Assert.Equal(LogLevel.Error, logged.Arguments[0]);
        Assert.Equal("MCP get_commission failed with InvalidOperationException", logged.Arguments[2]?.ToString());
        Assert.Null(logged.Arguments[3]);
    }
}
