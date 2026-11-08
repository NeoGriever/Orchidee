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
    public OrchestrionPlaybackService(IPluginLog log, IClientState clientState)
    {
        this.log = log;
        this.clientState = clientState;
    }

    public void Play(ushort sampleId)
    {
        if (!clientState.IsLoggedIn)
        {
            return;
        }
        try
        {
            if (startedByPlugin) OrchestrionManager.StopSample();
            OrchestrionManager.PlaySample(sampleId);
            CurrentSampleId = sampleId;
            startedByPlugin = true;
            log.Information("Orchidee: PlaySample({SampleId})", sampleId);
        }
        catch (Exception ex)
        {
            CurrentSampleId = null;
            startedByPlugin = false;
            log.Error(ex, "Orchidee: Could not start preview.");
        }
    }

    public void Stop()
    {
        if (!startedByPlugin) return;
        if (!clientState.IsLoggedIn)
        {
            CurrentSampleId = null;
            startedByPlugin = false;
            return;
        }
        try
        {
            OrchestrionManager.StopSample();
            startedByPlugin = false;
            CurrentSampleId = null;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Orchidee: Could not stop preview.");
        }
    }

    public void Dispose() => Stop();
}
