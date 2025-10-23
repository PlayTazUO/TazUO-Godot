using System;
using System.Runtime.InteropServices;
using Godot;

namespace TazUO.Godot.Utility;

public class AssetHelper
{
    private static readonly TextureCache _textureCache = new();

    public static Texture2D? GetGumpTexture(ushort graphic, bool skipCache = false)
    {
        if (skipCache)
            return GetGumpTextureNoCache(graphic);

        var texture = _textureCache.GetTexture(graphic);

        if(texture != null)
            return texture;

        if (Client.Instance.fileManager != null)
        {
            var info = Client.Instance.fileManager.Gumps.GetGump(graphic);

            if (_textureCache.AddTexture(graphic, info.Pixels, info.Width, info.Height))
                return _textureCache.GetTexture(graphic);
        }

        return null;
    }

    private static Texture2D GetGumpTextureNoCache(ushort graphic)
    {
        var info = Client.Instance.fileManager.Gumps.GetGump(graphic);
        ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(info.Pixels);

        using var tempImage = Image.CreateFromData(info.Width, info.Height, false, Image.Format.Rgba8, byteSpan);
        return ImageTexture.CreateFromImage(tempImage);
    }
}
