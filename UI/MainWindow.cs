using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Orchidee.Models;
using Orchidee.Services;

namespace Orchidee.UI;

/// <summary>PNG-skinned Orchestrion catalogue.</summary>
public sealed class MainWindow : Window
{
    private readonly OrchestrionPlaybackService playback;
    private readonly OrchestrionCatalogService catalog;
    private readonly OrchideeAssets assets;
    private string search = string.Empty;
    private int selectedCategory;
    private ushort? selectedId;

    private sealed record CategoryTab(string Name, string Key, string[] Aliases);
    private static readonly CategoryTab[] Tabs =
    {
        new("Locales I", "places1", ["Orte I", "Orte 1", "Locales I", "Locales 1"]),
        new("Locales II", "places2", ["Orte II", "Orte 2", "Locales II", "Locales 2"]),
        new("Dungeons 1", "dungeons1", ["Dungeons I", "Dungeons 1"]),
        new("Dungeons 2", "dungeons2", ["Dungeons II", "Dungeons 2"]),
        new("Primals", "primals", ["Primae", "Primals", "Trials"]),
        new("Raids 1", "raids1", ["Raids I", "Raids 1"]),
        new("Raids 2", "raids2", ["Raids II", "Raids 2"]),
        new("Ambient", "ambient", ["Umgebung", "Ambient", "Ambient Sounds"]),
        new("Quests", "quests", ["Aufträge", "Auftraege", "Quests"]),
        new("Other", "other", ["Andere", "Others", "Other", "Nicht zugeordnet", "Uncategorized"]),
        new("Seasonal Events", "seasonal", ["Saisonales Ereignis", "Seasonal", "Seasonal Events"]),
    };

    private static readonly Vector4 Gold = new(.85f, .72f, .48f, 1f);
    private static readonly Vector4 Muted = new(.61f, .69f, .72f, 1f);
    private static readonly Vector4 Text = new(.91f, .92f, .89f, 1f);

    public MainWindow(OrchestrionPlaybackService playback, OrchestrionCatalogService catalog, OrchideeAssets assets)
        : base("Orchidee###OrchideeMain", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.playback = playback;
        this.catalog = catalog;
        this.assets = assets;
        IsOpen = false;
        BgAlpha = 0;
        Size = new Vector2(810, 660);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(620, 520),
            MaximumSize = new Vector2(1400, 1100)
        };
    }

    private static bool Matches(OrchestrionEntry entry, CategoryTab tab)
    {
        var value = entry.Category.Trim();
        var recognized = Tabs.Any(t => t.Aliases.Any(a => string.Equals(a, value, StringComparison.OrdinalIgnoreCase)));
        if (tab.Key == "other" && !recognized) return true;
        return tab.Aliases.Any(a => string.Equals(a, value, StringComparison.OrdinalIgnoreCase));
    }

    private static uint Pack(float r, float g, float b, float a = 1f) =>
        ImGui.ColorConvertFloat4ToU32(new Vector4(r, g, b, a));

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(24, 22));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
    }

    public override void PostDraw() => ImGui.PopStyleVar(2);

    public override void Draw()
    {
        var start = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();
        var draw = ImGui.GetWindowDrawList();
        var bg = assets.Background();
        if (bg != null && bg.TryGetWrap(out var wrap, out _))
            draw.AddImage(wrap.Handle, start, start + size);
        else
            draw.AddRectFilled(start, start + size, Pack(.12f, .10f, .19f), 12);
        // Keep the stroke wholly inside the window's clip rectangle.
        draw.AddRect(start + new Vector2(2, 2), start + size - new Vector2(2, 2), Pack(.65f, .52f, .31f, .85f), 10, ImDrawFlags.None, 2);

        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 8);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 12);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(12, 9));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10, 10));
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(12, 8));
        ImGui.PushStyleColor(ImGuiCol.Text, Text);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(.10f,.17f,.21f,.92f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(.17f,.24f,.27f,1));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(.17f,.26f,.29f,1));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(.28f,.39f,.40f,1));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(.35f,.41f,.37f,1));
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(.045f,.085f,.11f,.70f));
        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(.28f,.36f,.37f,.85f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(.38f,.44f,.40f,.82f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(.48f,.43f,.32f,.85f));
        try { DrawContent(); }
        finally { ImGui.PopStyleColor(9); ImGui.PopStyleVar(5); }
    }

    private void DrawContent()
    {
        // Draggable custom header. Avoid dragging when interacting with controls.
        var top = ImGui.GetCursorScreenPos();
        var contentWidth = ImGui.GetContentRegionAvail().X;
        ImGui.InvisibleButton("##orchidee-drag", new Vector2(Math.Max(10, contentWidth - 42), 48));
        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            ImGui.SetWindowPos(ImGui.GetWindowPos() + ImGui.GetIO().MouseDelta);
        ImGui.SetCursorScreenPos(top + new Vector2(12, 5));
        ImGui.TextColored(Gold, "O R C H I D E E");
        ImGui.SetCursorScreenPos(top + new Vector2(12, 24));
        ImGui.TextColored(Muted, "ORCHESTRION  /  MUSIC ARCHIVE");
        ImGui.SetCursorScreenPos(top + new Vector2(contentWidth - 30, 3));
        if (ImGui.Button("X##close", new Vector2(30, 30))) IsOpen = false;
        ImGui.SetCursorScreenPos(top + new Vector2(0, 62));

        var width = ImGui.GetContentRegionAvail().X;
        const float gap = 8;
        var buttonWidth = (width - (Tabs.Length - 1) * gap) / Tabs.Length;
        for (var i = 0; i < Tabs.Length; i++)
        {
            if (i > 0) ImGui.SameLine(0, gap);
            DrawCategoryButton(i, buttonWidth);
        }
        ImGui.Dummy(new Vector2(0, 8));
        ImGui.TextColored(Gold, Tabs[selectedCategory].Name);
        ImGui.SameLine();
        ImGui.TextColored(Muted, " /  Orchestrion Rolls");

        ImGui.SetNextItemWidth(Math.Max(180, ImGui.GetContentRegionAvail().X - 132));
        ImGui.InputTextWithHint("##orchidee-search", "Search rolls...", ref search, 256);
        ImGui.SameLine();
        if (ImGui.Button("Refresh", new Vector2(122, 0))) catalog.Reload();

        var tab = Tabs[selectedCategory];
        var visible = catalog.Entries.Where(x => Matches(x, tab) &&
            (string.IsNullOrWhiteSpace(search) ||
             ($"{x.NumberLabel} {x.Name}").Contains(search, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.DisplayNumber ?? int.MaxValue).ThenBy(x => x.Name).ToArray();
        ImGui.TextColored(Muted, $"{visible.Length} tracks  ·  {catalog.Entries.Count} total");

        var height = Math.Max(110, ImGui.GetContentRegionAvail().Y - 104);
        if (ImGui.BeginChild("##tracklist", new Vector2(0, height), true))
        {
            if (visible.Length == 0) ImGui.TextColored(Muted,
                catalog.Entries.Count == 0 ? catalog.Status : "No rolls found in this category.");
            else if (ImGui.BeginTable("##orchidee-songs", 2, ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.PadOuterX))
            {
                for (var i = 0; i < visible.Length; i++)
                {
                    var e = visible[i];
                    ImGui.TableNextColumn();
                    var isSelected = selectedId == e.SampleId;
                    if (ImGui.Selectable($"{e.NumberLabel}   {e.Name}##{e.SampleId}", isSelected))
                        selectedId = e.SampleId;
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextUnformatted(e.Name);
                        ImGui.TextDisabled($"{tab.Name} / {e.NumberLabel}");
                        ImGui.EndTooltip();
                    }
                }
                ImGui.EndTable();
            }
        }
        ImGui.EndChild();

        var selected = selectedId.HasValue ? catalog.Entries.FirstOrDefault(e => e.SampleId == selectedId.Value) : null;
        ImGui.Dummy(new Vector2(0, 4));
        if (selected == null) ImGui.TextColored(Muted, "Select an orchestrion roll.");
        else ImGui.TextColored(Gold, $"{selected.NumberLabel}  ·  {selected.Name}");
        if (selected == null) ImGui.BeginDisabled();
        if (ImGui.Button("PLAY", new Vector2(120, 34)) && selected != null) playback.Play(selected.SampleId);
        if (selected == null) ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("STOP", new Vector2(120, 34))) playback.Stop();
    }

    private void DrawCategoryButton(int index, float width)
    {
        var tab = Tabs[index];
        var active = index == selectedCategory;
        var p = ImGui.GetCursorScreenPos();
        var height = Math.Min(width, 100);
        ImGui.InvisibleButton($"##cat-{tab.Key}", new Vector2(width, height));
        var hovered = ImGui.IsItemHovered();
        if (ImGui.IsItemClicked()) selectedCategory = index;
        var state = active ? "active" : hovered ? "hover" : "idle";
        var image = assets.Category(tab.Key, state) ?? assets.Category(tab.Key, "inactive");
        var iconSize = Math.Min(width, height);
        var iconPos = p + new Vector2((width - iconSize) * .5f, (height - iconSize) * .5f);
        if (image != null && image.TryGetWrap(out var texture, out _))
            ImGui.GetWindowDrawList().AddImage(texture.Handle, iconPos, iconPos + new Vector2(iconSize));
        else
        {
            ImGui.GetWindowDrawList().AddCircleFilled(iconPos + new Vector2(iconSize/2), iconSize*.47f,
                Pack(active ? .57f : .24f, active ? .44f : .34f, .31f));
            ImGui.GetWindowDrawList().AddCircle(iconPos + new Vector2(iconSize/2), iconSize*.47f, Pack(.82f,.72f,.51f));
        }
        if (hovered)
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(tab.Name);
            ImGui.EndTooltip();
        }
    }
}
