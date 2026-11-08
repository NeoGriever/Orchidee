namespace Orchidee.Models;

/// <summary>IDs aus EXD; PlaySample erwartet die Orchestrion-Row-ID, nicht die sichtbare Nummer.</summary>
public sealed record OrchestrionEntry(
    ushort SampleId,
    string Name,
    string Category,
    int? DisplayNumber,
    bool IsMapped)
{
    public string NumberLabel => DisplayNumber is > 0 ? DisplayNumber.Value.ToString("D3") : "---";
}
