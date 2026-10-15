using Nuxiba.Backend.Models;

namespace Nuxiba.Backend.Services;

public static class SessionPairing
{
    public static IReadOnlyList<(Login Start, Login End)> CompleteSessions(IEnumerable<Login> events)
    {
        var ordered = events.OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToList();
        var result = new List<(Login Start, Login End)>();
        for (var i = 0; i + 1 < ordered.Count; i++)
        {
            var current = ordered[i];
            var next = ordered[i + 1];
            if (current.TipoMov == 1 && next.TipoMov == 0 && next.Fecha >= current.Fecha)
                result.Add((current, next));
        }
        return result;
    }

    // strictEventId identifica el movimiento que se está intentando crear/editar.
    // Se tolera únicamente un logout huérfano histórico al principio del historial importado,
    // pero nunca uno que sea el movimiento nuevo/editado.
    public static string? ValidateSequence(IEnumerable<Login> events, int? strictEventId = null)
    {
        var ordered = events.OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToList();
        if (ordered.Any(x => x.TipoMov is not (0 or 1)))
            return "TipoMov solo puede ser 0 o 1.";
        if (ordered.Any(x => x.Fecha == default))
            return "La fecha es obligatoria y debe ser válida.";

        var open = false;
        var sawLogin = false;
        foreach (var item in ordered)
        {
            if (item.TipoMov == 1)
            {
                if (open) return "No se puede registrar un login mientras exista una sesión abierta.";
                open = true;
                sawLogin = true;
            }
            else
            {
                if (!open)
                {
                    if (!sawLogin && item.Id != strictEventId)
                        continue; // evento legado huérfano anterior al primer login disponible
                    return "No se puede registrar un logout sin un login previo.";
                }
                open = false;
            }
        }
        return null;
    }
}
