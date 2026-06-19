namespace BecauseImClever.Application.Tests.Interfaces;

using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using Xunit;

/// <summary>
/// Tests for the <see cref="ISpellCheckService"/> interface contract.
/// </summary>
public class SpellCheckServiceContractTests
{
    /// <summary>
    /// Verifies that CheckAsync is declared with the expected signature.
    /// </summary>
    [Fact]
    public void Interface_DeclaresCheckAsync_WithCorrectSignature()
    {
        // Arrange
        var method = typeof(ISpellCheckService).GetMethod(nameof(ISpellCheckService.CheckAsync));

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(Task<SpellCheckResponse>), method!.ReturnType);

        var parameters = method.GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(SpellCheckRequest), parameters[0].ParameterType);
    }
}
