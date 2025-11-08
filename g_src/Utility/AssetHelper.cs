using System;
using System.IO;
using System.Runtime.InteropServices;
using ClassicUO.Assets;
using ClassicUO.Utility;
using Godot;

namespace TazUO.Godot.Utility;

public class AssetHelper
{
    private static readonly TextureCache _textureCache = new();
    private static UOFileManager UoFileManager;

    public static UOFileManager _uoFileManager 
    {
        get
        {
            if (UoFileManager != null) return UoFileManager; 
            
            if (Engine.IsEditorHint())
            {
                UoFileManager = LoadFileManager(Client.EDITOR_ONLY_DATA_PATH);
                GD.Print("LOADED FROM EDITOR PATH");
                return UoFileManager;
            }
            
            GD.Print("LOADED FROM CLIENT");
            UoFileManager = Client.Instance.GetFileManager();
            return UoFileManager;
        }   
    }
    
    private static UOFileManager LoadFileManager(string path, string ver = "7.0.110.48")
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        if (!Path.Exists(path)) return null;

        if (ClientVersionHelper.TryParseFromFile(path.PathJoin("client.exe"), out var v))
            ver = v;
        
        if(ClientVersionHelper.IsClientVersionValid(ver, out var version))
        {
            var fileManager = new UOFileManager(version, path);
            fileManager.Load(false, "en");
            return fileManager;
        }
        
        return null;
    }

    public static Texture2D? GetGumpTexture(ushort graphic, bool skipCache = false)
    {
        if (skipCache)
            return GetGumpTextureNoCache(graphic);

        var texture = _textureCache.GetTexture(graphic);

        if(texture != null)
            return texture;
        
        var info = _uoFileManager.Gumps.GetGump(graphic);

        if (_textureCache.AddTexture(graphic, info.Pixels, info.Width, info.Height))
            return _textureCache.GetTexture(graphic);

        return null;
    }

    private static Texture2D GetGumpTextureNoCache(ushort graphic)
    {
        var info = _uoFileManager.Gumps.GetGump(graphic);
        ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(info.Pixels);

        using var tempImage = Image.CreateFromData(info.Width, info.Height, false, Image.Format.Rgba8, byteSpan);
        return ImageTexture.CreateFromImage(tempImage);
    }
}
