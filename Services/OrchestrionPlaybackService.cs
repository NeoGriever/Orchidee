using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Orchidee.Services;

/// <summary>
/// Minimaler Versuch mit der nativen Orchestrion-Sample-Funktion.
/// Achtung: Die interne Sample-ID 1 wird hier bewusst als Test-Hypothese
/// fuer die sichtbare Notenrollen-Nummer 001 verwendet. Die Abbildung
/// muss anhand der Spiel-EXD-Daten separat verifiziert werden.
/// </summary>
public sealed class OrchestrionPlaybackService : IDisposable
{
    private const ushort SampleId = 1;
    private readonly IPluginLog log;
    private readonly IClientState clientState;
    private bool startedByPlugin;

    public string Status { get; private set; } = "Bereit (noch nicht getestet).";

    public OrchestrionPlaybackService(IPluginLog log, IClientState clientState)
    {
        this.log = log;
        this.clientState = clientState;
    }

    public void Play()
    {
        if (!clientState.IsLoggedIn)
        {
            Status = "Nicht eingeloggt. Bitte im Spiel testen.";
            return;
        }

        try
        {
            // Spielclient-Funktion, keine serverseitige PlayTrack-Anforderung.
            OrchestrionManager.PlaySample(SampleId);
            startedByPlugin = true;
            Status = "PlaySample(1) aufgerufen. Ob Ton erklingt, bitte im Spiel pruefen.";
            log.Information("Orchidee: PlaySample(1) aufgerufen (sichtbare 001 noch nicht verifiziert).");
        }
        catch (Exception ex)
        {
            Status = "Play fehlgeschlagen. Siehe Dalamud-Log.";
            log.Error(ex, "Orchidee: PlaySample(1) fehlgeschlagen.");
        }
    }

    public void Stop()
    {
        if (!clientState.IsLoggedIn)
        {
            Status = "Nicht eingeloggt. Stop nicht ausgefuehrt.";
            return;
        }

        try
        {
            OrchestrionManager.StopSample();
            startedByPlugin = false;
            Status = "StopSample() aufgerufen.";
            log.Information("Orchidee: StopSample() aufgerufen.");
        }
        catch (Exception ex)
        {
            Status = "Stop fehlgeschlagen. Siehe Dalamud-Log.";
            log.Error(ex, "Orchidee: StopSample() fehlgeschlagen.");
        }
    }

    public void Dispose()
    {
        // Aufraeumen nur, wenn diese Plugininstanz ein Sample gestartet hat.
        if (startedByPlugin && clientState.IsLoggedIn)
            Stop();
    }
}
