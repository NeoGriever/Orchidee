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
    [PluginService] private static IDataManager DataManager { get; set; } = null!;
    [PluginService] private static ITextureProvider TextureProvider { get; set; } = null!;

    private readonly WindowSystem windowSystem = new("Orchidee");
    private readonly MainWindow mainWindow;
    private readonly OrchestrionPlaybackService playback;

    public Plugin()
    {
        playback = new OrchestrionPlaybackService(Log, ClientState);
        var catalog = new OrchestrionCatalogService(DataManager, Log);
        mainWindow = new MainWindow(playback, catalog, new OrchideeAssets(TextureProvider, PluginInterface.AssemblyLocation.DirectoryName ?? AppContext.BaseDirectory));
        windowSystem.AddWindow(mainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo((_, _) => OpenWindow())
        {
            HelpMessage = "Open the Orchidee Orchestrion catalog."
        });
        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += OpenWindow;
        PluginInterface.UiBuilder.OpenConfigUi += OpenWindow;
    }

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
