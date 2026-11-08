# Orchidee – Overlay-Update: Orchestrion-Katalog

Das ZIP im bestehenden Projektverzeichnis (`Orchidee`, dort wo `Orchidee.csproj` liegt) entpacken. Es enthaelt **keinen zusaetzlichen Oberordner**.

## Funktionen
- Nur `Orchestrion`-Rollen aus den EXD-Tabellen des lokal installierten FFXIV-Clients (kein vollstaendiger allgemeiner BGM-Index).
- Suchbare Liste mit Filter nach Kategorie, Auswahl und Play/Stop.
- Nutzt `OrchestrionManager.PlaySample(Orchestrion-RowId)`, nie serverseitiges `PlayTrack`.
- Kein Autostart des Fensters. `/orchidee` und Dalamud `Open` bleiben erhalten.
- Diagnostik zeigt die erkannten Spalten der drei EXD-Tabellen und den Zuordnungsstatus.

## Wichtig: Validierung im Spiel
Die Lumina-Spaltennamen und die konkrete Referenz zwischen `Orchestrion` und `OrchestrionUiparam` sind noch **nicht in deiner Clientversion gegengeprueft**. Eintraege ohne sicher ermittelte Kategorie/Nummer zeigen `---`. Wenn die sichtbare Nummerierung leer bleibt, kopiere den Diagnosebericht. Keine fehlenden Nummern werden geraten.

## Build
```
dotnet restore --configfile NuGet.Config
dotnet build -c Debug --no-restore
```

## Releaser
`Orchidee.csproj` (`<Version>`) und `Orchidee.json` (`AssemblyVersion`) werden **nicht** ueberschrieben. Die Releaser-Konfiguration bleibt unveraendert. Der Releaser erwartet laut bereitgestelltem Python-Skript bei `packaging=sdk` das SDK-Paket `.../Orchidee/latest.zip` nach `dotnet build -c Release`.

## Bekannter Test
- Sample ID 1 = `A Cold Wind`
- Sample ID 2 = `Wailers And Waterwheels` (Orte I, 001)

## Nicht enthalten
- Verifizierte Spielort-/Herkunftsdaten; sind nur eine spaetere optionale Erweiterung.
- Urheberrechtlich geschuetzte Audiodateien.
