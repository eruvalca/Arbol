using System.Diagnostics.CodeAnalysis;
using Arbol.Features.Account.Services;
using Shouldly;
using Xunit;

namespace Arbol.UnitTests.Features.Account;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "xUnit requires public test classes for discovery.")]
public sealed class IdentityCancellationTests
{
    [Fact]
    public void CompleteWriteClearsTheScopedTokenSoRequiredFollowUpCanFinish()
    {
        using var source = new CancellationTokenSource();
        var cancellation = new IdentityCancellation { Token = source.Token };

        cancellation.CompleteWrite();

        cancellation.Token.ShouldBe(CancellationToken.None);
    }
}
