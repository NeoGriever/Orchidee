# Orchidee – UI Design Patch

Source reference: NeoGriever/Orchidee, branch `main`, read 2026-11-08.

Copy the contents of this ZIP **into the repository root**, overwriting `Plugin.cs`, `Orchidee.csproj` and `UI/MainWindow.cs`. The existing catalog and playback services stay unchanged.

## Appearance

- Borderless, draggable header, custom panel background, rounded accented visual controls.
- Fixed category order: Orte 1, Orte 2, Dungeons 1, Dungeons 2, Primae, Raids 1, Raids 2, Umgebung, Aufträge, Andere, Saisonales Ereignis.
- Each category button has four PNG states under `Assets/Categories/{key}_{state}.png`.
- Tooltips reveal the full names. Replacement artwork should retain 1:1 aspect ratio and ideally 128x128 (or 256x256) resolution.
- `Assets/Panel/background.png` is the replaceable panel artwork, currently 1100x740.
- If assets cannot load, vector fallback circles remain visible.
- The catalogue displays visible roll number and title, never the internal sample ID. Search applies within the selected category.
- The plugin remains closed after loading; open with `/orchidee` or the Dalamud Open action.

## Known constraints

- The existing Lumina-derived catalogue was preserved untouched.
- Some localized category names may differ from the aliases in `UI/MainWindow.cs`. Unknown categories are grouped into "Andere" to prevent tracks from silently disappearing.
- The panel background is drawn across the ImGui window. The custom header can be dragged.
- No local Dalamud SDK is available in the authoring environment, therefore compilation and runtime visual validation must be done in the installed Dalamud environment.
- The project version is intentionally unchanged; the user's DalamudReleaser handles version bumps.

## Build

```fish
dotnet restore --configfile NuGet.Config
dotnet build -c Debug --no-restore
```

## Assets paths

Images are copied by the project file to the build output (`Assets/...`). The loader resolves paths relative to the executing plugin assembly. If testing via a custom loader that relocates the assembly without `Assets`, the vector fallback is used.
