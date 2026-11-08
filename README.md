# Orchidee

Orchidee is a Dalamud plugin for browsing Orchestrion rolls and previewing them locally. It reads track names, categories, and display numbers from the installed game's EXD sheets.

## Use

Open the window with `/orchidee` or Dalamud's **Open** button. Choose a category, select a roll, and use **Play** or **Stop** to control its preview. The search field filters the selected category. **Refresh** reloads the catalog from game data.

The preview uses `OrchestrionManager.PlaySample` and `StopSample`. It does not change ownership data or invoke the network-facing `PlayTrack` method. The plugin stops a preview on unload only if it started that preview.

The window background is loaded from `Assets/Panel/background.png`. Category icons are loaded from `Assets/Categories`; each category can provide idle, hover, active, and inactive PNGs.

## Build

The project uses Dalamud.NET.Sdk 15 and .NET 10. `NuGet.Config` includes a local SDK package source.

```sh
dotnet restore --configfile NuGet.Config
dotnet build -c Debug --no-restore
```

Load `bin/Debug/Orchidee.dll` as a Dalamud development plugin. In game, verify that the selected roll previews correctly and that **Stop** ends it; compilation alone cannot confirm native playback behavior.
