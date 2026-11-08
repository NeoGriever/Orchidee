using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Orchidee.Services;

namespace Orchidee.UI;

public sealed class MainWindow : Window
{
    private readonly OrchestrionPlaybackService playback;

    public MainWindow(OrchestrionPlaybackService playback)
        : base("Orchidee - Orchestrion-Test###OrchideeMain")
    {
        this.playback = playback;
        IsOpen = false; // Kein automatisches Oeffnen bei Pluginstart.
        Size = new Vector2(390, 160);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(340, 140),
            MaximumSize = new Vector2(620, 300),
        };
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Notenrolle 001 - Wailers And Waterwheels");
        ImGui.TextDisabled("Test-Hypothese: native Sample-ID = 1");
        ImGui.Separator();

        if (ImGui.Button("Play", new Vector2(140, 36)))
            playback.Play();

        ImGui.SameLine();
        if (ImGui.Button("Stop", new Vector2(140, 36)))
            playback.Stop();

        ImGui.Separator();
        ImGui.TextWrapped(playback.Status);
    }
}
