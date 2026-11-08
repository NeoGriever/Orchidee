using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Orchidee.Services;

/// <summary>Client-only native Orchestrion preview. Never invokes PlayTrack (network-facing).</summary>
public sealed class OrchestrionPlaybackService : IDisposable
{
    private readonly IPluginLog log;
    private readonly IClientState clientState;
    private bool startedByPlugin;

    public ushort? CurrentSampleId { get; private set; }
    public string Status { get; private set; } = "Bereit.";

    public OrchestrionPlaybackService(IPluginLog log, IClientState clientState)
    {
        this.log = log;
        this.clientState = clientState;
    }

    public void Play(ushort sampleId)
    {
        if (!clientState.IsLoggedIn)
        {
            Status = "Nicht eingeloggt.";
            return;
        }
        try
        {
            if (startedByPlugin) OrchestrionManager.StopSample();
            OrchestrionManager.PlaySample(sampleId);
            CurrentSampleId = sampleId;
            startedByPlugin = true;
            Status = $"PlaySample({sampleId}) aufgerufen.";
            log.Information("Orchidee: PlaySample({SampleId})", sampleId);
        }
        catch (Exception ex)
        {
            CurrentSampleId = null;
            startedByPlugin = false;
            Status = "Wiedergabe fehlgeschlagen. Siehe Log.";
            log.Error(ex, "Orchidee: PlaySample fehlgeschlagen.");
        }
    }

    public void Stop()
    {
        if (!startedByPlugin) { Status = "Keine Vorschau von Orchidee aktiv."; return; }
        if (!clientState.IsLoggedIn)
        {
            CurrentSampleId = null;
            startedByPlugin = false;
            Status = "Nicht eingeloggt.";
            return;
        }
        try
        {
            OrchestrionManager.StopSample();
            startedByPlugin = false;
            CurrentSampleId = null;
            Status = "Vorschau gestoppt.";
        }
        catch (Exception ex)
        {
            Status = "Stop fehlgeschlagen. Siehe Log.";
            log.Error(ex, "Orchidee: StopSample fehlgeschlagen.");
        }
    }

    public void Dispose() => Stop();
}
