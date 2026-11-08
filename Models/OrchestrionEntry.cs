namespace Orchidee.Models;

/// <summary>EXD IDs; PlaySample expects the Orchestrion row ID rather than its display number.</summary>
public sealed record OrchestrionEntry(
    ushort SampleId,
    string Name,
    string Category,
    int? DisplayNumber,
    bool IsMapped)
{
    public string NumberLabel => DisplayNumber is > 0 ? DisplayNumber.Value.ToString("D3") : "---";
}
