using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Orchidee.Models;
using Orchidee.Services;

namespace Orchidee.UI;

public sealed class MainWindow : Window
{
    private readonly OrchestrionPlaybackService playback;
    private readonly OrchestrionCatalogService catalog;
    private string search = string.Empty;
    private string category = "Alle";
    private bool diagnosticsVisible;
    private ushort? selectedId;

    public MainWindow(OrchestrionPlaybackService playback, OrchestrionCatalogService catalog)
        : base("Orchidee - Orchestrion-Katalog###OrchideeMain")
    {
        this.playback = playback;
        this.catalog = catalog;
        IsOpen = false;
        Size = new Vector2(740, 540);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(460, 310),
            MaximumSize = new Vector2(1200, 960),
        };
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Orchidee - Notenrollen-Vorschau");
        ImGui.SameLine();
        if (ImGui.SmallButton("Katalog neu laden")) catalog.Reload();
        ImGui.TextDisabled(catalog.Status);

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##orchidee-search", "Titel, Nummer, Kategorie oder Sample-ID suchen ...", ref search, 256);
        var categories = catalog.Entries.Select(x => x.Category).Distinct().OrderBy(x => x).ToArray();
        ImGui.SetNextItemWidth(260);
        if (ImGui.BeginCombo("Kategorie", category))
        {
            if (ImGui.Selectable("Alle", category == "Alle")) category = "Alle";
            foreach (var value in categories)
                if (ImGui.Selectable(value, category == value)) category = value;
            ImGui.EndCombo();
        }

        var visible = catalog.Entries.Where(x =>
            (category == "Alle" || x.Category == category) &&
            (string.IsNullOrWhiteSpace(search) ||
             ($"{x.NumberLabel} {x.Name} {x.Category} {x.SampleId}")
             .Contains(search, StringComparison.OrdinalIgnoreCase))).ToArray();
        ImGui.TextDisabled($"{visible.Length} / {catalog.Entries.Count} Eintraege");
        ImGui.Separator();

        // ImGui list clipper keeps rendering cost stable for hundreds of tracks.
        if (ImGui.BeginChild("##orchidee-tracks", new Vector2(0, -136), true))
        {
            if (ImGui.BeginTable("##orchidee-table", 4, ImGuiTableFlags.RowBg |
                ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("Nr.", ImGuiTableColumnFlags.WidthFixed, 53);
                ImGui.TableSetupColumn("Kategorie", ImGuiTableColumnFlags.WidthStretch, 1.1f);
                ImGui.TableSetupColumn("Titel", ImGuiTableColumnFlags.WidthStretch, 2.7f);
                ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 60);
                ImGui.TableHeadersRow();
                for (var i = 0; i < visible.Length; i++)
                {
                    var entry = visible[i];
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var selected = selectedId == entry.SampleId;
                    if (ImGui.Selectable($"{entry.NumberLabel}##{entry.SampleId}", selected,
                            ImGuiSelectableFlags.SpanAllColumns)) selectedId = entry.SampleId;
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(entry.Category);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(entry.Name);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(entry.SampleId.ToString());
                }
                ImGui.EndTable();
            }
        }
        ImGui.EndChild();

        OrchestrionEntry? track = selectedId.HasValue
            ? catalog.Entries.FirstOrDefault(x => x.SampleId == selectedId.Value) : null;
        if (track != null)
            ImGui.TextWrapped($"Auswahl: {track.Category} / {track.NumberLabel} - {track.Name} [Sample-ID {track.SampleId}]");
        else ImGui.TextDisabled("Notenrolle auswaehlen.");
        if (track == null) ImGui.BeginDisabled();
        if (ImGui.Button("Play", new Vector2(120, 32)) && track != null) playback.Play(track.SampleId);
        if (track == null) ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("Stop", new Vector2(120, 32))) playback.Stop();
        ImGui.TextDisabled(playback.Status);
        if (ImGui.SmallButton(diagnosticsVisible ? "Diagnose verbergen" : "Diagnose anzeigen"))
            diagnosticsVisible = !diagnosticsVisible;
        if (diagnosticsVisible)
        {
            ImGui.SameLine();
            if (ImGui.SmallButton("Diagnose kopieren")) ImGui.SetClipboardText(string.Join("\n", catalog.Diagnostics));
            foreach (var line in catalog.Diagnostics) ImGui.TextWrapped(line);
        }
    }
}
