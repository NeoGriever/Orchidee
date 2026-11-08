using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Orchidee.Models;

namespace Orchidee.Services;

/// <summary>
/// Creates the catalogue from the installed client's EXD sheets, not the obsolete 2021 MP3 list.
/// An unresolved mapping is visibly marked, never silently presented as a verified display number.
/// </summary>
public sealed class OrchestrionCatalogService
{
    private readonly IDataManager data;
    private readonly IPluginLog log;
    public IReadOnlyList<OrchestrionEntry> Entries { get; private set; } = Array.Empty<OrchestrionEntry>();
    public string Status { get; private set; } = "Catalog not loaded yet.";

    public OrchestrionCatalogService(IDataManager data, IPluginLog log)
    {
        this.data = data;
        this.log = log;
        Reload();
    }

    public void Reload()
    {
        try
        {
            var songs = data.GetExcelSheet<Orchestrion>();
            var ui = data.GetExcelSheet<OrchestrionUiparam>();
            var categories = data.GetExcelSheet<OrchestrionCategory>();
            if (songs == null || ui == null || categories == null)
                throw new InvalidOperationException("A required Orchestrion data sheet is missing.");

            // Map categories from game data. Named fields are discovered defensively because
            // Lumina column names can change with updated game schemas.
            var categoryNames = new Dictionary<uint, string>();
            foreach (var cat in categories)
            {
                var rowId = RowId(cat);
                var name = ReadText(cat, "Name", "CategoryName");
                if (rowId.HasValue && !string.IsNullOrWhiteSpace(name))
                    categoryNames[rowId.Value] = name;
            }

            var uiRows = new Dictionary<uint, (uint? Category, int? Number)>();
            foreach (var row in ui)
            {
                var rowId = RowId(row);
                if (!rowId.HasValue) continue;
                var cat = ReadUInt(row, "OrchestrionCategory", "Category", "CategoryId");
                var number = ReadInt(row, "Order", "Number", "DisplayNumber", "SortOrder", "Index");
                uiRows[rowId.Value] = (cat, number);
            }

            var entries = new List<OrchestrionEntry>();
            foreach (var song in songs)
            {
                var id = RowId(song);
                if (id is null or 0 || id > ushort.MaxValue) continue;
                var name = ReadText(song, "Name");
                if (string.IsNullOrWhiteSpace(name)) continue;

                // The Orchestrion sheet is the canonical set of roll tracks. The UI sheet
                // distinguishes actually listed entries. Do not add generic BGM tracks.
                var uiKey = ReadUInt(song, "OrchestrionUiparam", "OrchestrionUIParam") ?? id;
                if (!uiRows.TryGetValue(uiKey.Value, out var meta)) continue;
                var categoryId = meta.Category ?? ReadUInt(song, "OrchestrionCategory", "Category");
                var category = categoryId.HasValue && categoryNames.TryGetValue(categoryId.Value, out var text)
                    ? text : "Uncategorized";
                var number = meta.Number is > 0 ? meta.Number : null;
                var isMapped = number.HasValue && category != "Uncategorized";
                entries.Add(new OrchestrionEntry((ushort)id.Value, name, category, number, isMapped));
            }

            Entries = entries.OrderBy(e => e.Category).ThenBy(e => e.DisplayNumber ?? int.MaxValue)
                .ThenBy(e => e.SampleId).ToArray();
            Status = $"{Entries.Count} orchestrion rolls loaded.";
            log.Information("Orchidee: {Status}", Status);
        }
        catch (Exception ex)
        {
            Entries = Array.Empty<OrchestrionEntry>();
            Status = "Could not load the Orchestrion catalog.";
            log.Error(ex, "Orchidee: Could not load the Orchestrion catalog.");
        }
    }

    private static uint? RowId(object row) => ReadUInt(row, "RowId");

    private static object? Property(object? value, string name)
    {
        if (value == null) return null;
        var type = value.GetType();
        var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop == null) return null;
        try { return prop.GetValue(value); }
        catch { return null; }
    }

    private static uint? ToUInt(object? value)
    {
        if (value == null) return null;
        try
        {
            // LazyRow<T> / RowRef<T> contain a RowId or Value whose RowId can be read.
            if (value is not string && value.GetType().IsGenericType)
            {
                var id = Property(value, "RowId") ?? Property(value, "Row");
                if (id != null) return Convert.ToUInt32(id);
            }
            return Convert.ToUInt32(value);
        }
        catch { return null; }
    }

    private static uint? ReadUInt(object row, params string[] properties)
    {
        foreach (var property in properties)
        {
            var result = ToUInt(Property(row, property));
            if (result.HasValue) return result;
        }
        return null;
    }

    private static int? ReadInt(object row, params string[] names)
    {
        foreach (var name in names)
        {
            try
            {
                var value = Property(row, name);
                if (value != null) return Convert.ToInt32(value);
            }
            catch { /* Try next alias. */ }
        }
        return null;
    }

    private static string ReadText(object row, params string[] names)
    {
        foreach (var name in names)
        {
            var value = Property(row, name);
            var text = value?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(text)) return text;
        }
        return string.Empty;
    }
}
