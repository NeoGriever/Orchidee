using System;
using System.IO;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;

namespace Orchidee.UI;

/// <summary>
/// PNG-backed appearance. Replace files in Assets/Categories without changing UI code.
/// Every category has inactive/idle/hover/active states.
/// </summary>
public sealed class OrchideeAssets
{
    private readonly ITextureProvider textures;
    private readonly string root;

    public OrchideeAssets(ITextureProvider textures)
    {
        this.textures = textures;
        root = Path.Combine(Path.GetDirectoryName(typeof(OrchideeAssets).Assembly.Location)!, "Assets");
    }

    public ISharedImmediateTexture? Category(string key, string state) => Load("Categories", $"{key}_{state}.png");
    public ISharedImmediateTexture? Background() => Load("Panel", "background.png");

    private ISharedImmediateTexture? Load(string folder, string file)
    {
        var path = Path.Combine(root, folder, file);
        return File.Exists(path) ? textures.GetFromFile(path) : null;
    }
}
