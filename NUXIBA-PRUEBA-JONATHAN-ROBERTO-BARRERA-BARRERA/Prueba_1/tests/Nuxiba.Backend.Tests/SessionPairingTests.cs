using Xunit;
using Nuxiba.Backend.Models;
using Nuxiba.Backend.Services;

namespace Nuxiba.Backend.Tests;

public class SessionPairingTests
{
    [Fact]
    public void CompleteSessions_PairsLoginWithNextLogout()
    {
        var start = new DateTime(2023, 1, 1, 8, 0, 0);
        var events = new[]
        {
            new Login { Id = 1, UserId = 70, TipoMov = 1, Fecha = start },
            new Login { Id = 2, UserId = 70, TipoMov = 0, Fecha = start.AddHours(8) }
        };
        var result = SessionPairing.CompleteSessions(events);
        Assert.Single(result);
        Assert.Equal(TimeSpan.FromHours(8), result[0].End.Fecha - result[0].Start.Fecha);
    }

    [Fact]
    public void ValidateSequence_RejectsDuplicateLogin()
    {
        var time = new DateTime(2023, 1, 1);
        var error = SessionPairing.ValidateSequence(new[]
        {
            new Login { Id = 1, UserId = 1, TipoMov = 1, Fecha = time },
            new Login { Id = 2, UserId = 1, TipoMov = 1, Fecha = time.AddMinutes(1) }
        });
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidateSequence_RejectsLogoutWithoutLogin()
    {
        var error = SessionPairing.ValidateSequence(new[]
        {
            new Login { Id = 1, UserId = 1, TipoMov = 0, Fecha = new DateTime(2023, 1, 1) }
        });
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidateSequence_AcceptsLoginThenLogout()
    {
        var time = new DateTime(2023, 1, 1);
        var error = SessionPairing.ValidateSequence(new[]
        {
            new Login { Id = 1, UserId = 1, TipoMov = 1, Fecha = time },
            new Login { Id = 2, UserId = 1, TipoMov = 0, Fecha = time.AddHours(1) }
        });
        Assert.Null(error);
    }
}
