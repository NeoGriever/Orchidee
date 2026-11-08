# Orchidee – Minimaler Dalamud-Orchestrion-Test

Ein minimaler **C#/.NET-10-Dalamud-Plugin-Prototyp**. **Nicht als bereits erfolgreich getesteter Player verstehen.**

## Funktionsumfang

- Das Fenster ist **beim Laden geschlossen**.
- Oeffnen ueber `/orchidee` oder den **Open**-Button in Dalamuds Pluginverwaltung (OpenMainUi / OpenConfigUi).
- Exakt zwei Buttons: **Play** und **Stop**.
- Play versucht `OrchestrionManager.PlaySample(1)` (nativ, lokal).
- Stop ruft `OrchestrionManager.StopSample()` auf.
- Beim Entladen stoppt das Plugin nur, falls es selbst einen Play-Aufruf gestartet hat.
- Keine Besitzdaten-Aenderung, kein `PlayTrack()`-Aufruf, keine Netzwerkpakete durch Plugin-Code.

## Wichtige Testgrenze

Die sichtbare Notenrollen-Nummer **001** ist nicht automatisch nachgewiesen als identisch mit der internen `ushort`-ID **1** von `PlaySample`. Diese Zuordnung ist eine **bewusste Test-Hypothese**, kein gesicherter Fakt. Ebenso ist ungetestet, ob `PlaySample()` nicht freigeschaltete Titel abspielt und ob der erforderliche Spielkontext (z. B. Position/Orchestrion-UI) gegeben sein muss.

Der angezeigte Status `PlaySample(1) aufgerufen` bedeutet **nicht**, dass eine korrekte Audiodatei abgespielt wurde. Bestaetige das akustisch.

## Umgebung

Dem hochgeladenen Beispielprojekt `IAmReady` entsprechend:

- Dalamud.NET.Sdk **15.0.0**
- .NET **10** (durch das SDK vorgegeben)
- `Dalamud.Bindings.ImGui`
- Die beigelegte `NuGet.Config` verweist auf das lokale SDK-Paket-Verzeichnis `/home/mind/local-builds/Dalamud.NET.Sdk/nupkg` sowie nuget.org.

## Bauen unter CachyOS (fish)

```fish
cd /pfad/zu/Orchidee
dotnet --info
dotnet restore --configfile NuGet.Config
dotnet build -c Debug --no-restore
find bin/Debug -maxdepth 3 -type f -name 'Orchidee.dll'
```

Falls das lokale SDK an einem anderen Ort liegt:

```fish
ls -lah /home/mind/local-builds/Dalamud.NET.Sdk/nupkg
find /home/mind/local-builds/Dalamud.NET.Sdk/nupkg -maxdepth 1 -iname '*15.0.0*nupkg'
```

In Dalamud unter **Dev Plugins** den Debug-DLL-Pfad eintragen und Plugin laden. Der Ort des Dev-Plugin-Menues kann je nach Dalamud-Version variieren.

## Geplanter Test

1. Einloggen und nicht in einer Cutscene sein.
2. Plugin laden: Fenster soll **geschlossen** bleiben.
3. In Dalamud Pluginverwaltung **Open** verwenden. Fenster soll erscheinen.
4. Fenster schliessen und `/orchidee` in den Chat eingeben. Fenster soll erscheinen.
5. **Play** druecken. Dokumentieren, ob Ton hoerbar ist und ob er wirklich Titel 001 entspricht.
6. **Stop** druecken. Pruefen, ob der Ton endet.
7. Dasselbe einmal nahe einem Orchestrion ausprobieren, falls Schritt 5 keinen Ton ergibt.
8. Im Fehlerfall Dalamud-Log und ggf. native Aufruf-Fehlermeldung sichern.

## Sicherheit / Grenzen

Die native FFXIV-Funktion ist nicht durch die Autoren dieses Projekts kontrolliert. Native Interop-Aufrufe koennen bei inkompatiblen Client-Versionen scheitern oder abstuerzen. Deshalb zuerst mit aktuellem Dalamud und ohne andere Audio-Plugins testen. Plugins dieser Art sind nicht offiziell von Square Enix unterstuetzt.

### Technische Referenz

- https://ffxiv.wildwolf.dev/api/FFXIVClientStructs.FFXIV.Client.Game.OrchestrionManager.html
- https://github.com/aers/FFXIVClientStructs
# Orchidee
