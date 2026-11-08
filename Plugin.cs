using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Orchidee.Services;
using Orchidee.UI;

namespace Orchidee;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/orchidee";

    [PluginService] private static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] private static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] private static IPluginLog Log { get; set; } = null!;
    [PluginService] private static IClientState ClientState { get; set; } = null!;

    private readonly WindowSystem windowSystem = new("Orchidee");
    private readonly MainWindow mainWindow;
    private readonly OrchestrionPlaybackService playback;

    public Plugin()
    {
        playback = new OrchestrionPlaybackService(Log, ClientState);
        mainWindow = new MainWindow(playback);
        windowSystem.AddWindow(mainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Oeffnet den Orchidee-Orchestrion-Test."
        });

        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += OpenWindow;
        PluginInterface.UiBuilder.OpenConfigUi += OpenWindow;

        // Absichtlich kein OpenWindow() im Konstruktor.
        Log.Information("Orchidee geladen. Fenster bleibt geschlossen bis /orchidee oder Dalamud Open.");
    }

    private void OnCommand(string command, string args) => OpenWindow();
    private void OpenWindow() => mainWindow.IsOpen = true;

    public void Dispose()
    {
        PluginInterface.UiBuilder.OpenConfigUi -= OpenWindow;
        PluginInterface.UiBuilder.OpenMainUi -= OpenWindow;
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        CommandManager.RemoveHandler(CommandName);
        playback.Dispose();
        windowSystem.RemoveAllWindows();
    }
}
