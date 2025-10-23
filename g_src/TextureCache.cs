using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;

namespace TazUO.Godot;

/// <summary>
/// Manages a cache of textures packed into texture atlases with automatic expansion.
/// Maps graphic IDs to texture regions within atlases using Godot's AtlasTexture.
/// </summary>
public class TextureCache
{
    private readonly int _maxAtlasSize;
    private readonly List<AtlasPage> _atlasPages = new();
    private readonly Dictionary<ushort, AtlasTexture> _textureMap = new();

    public TextureCache(int maxAtlasSize = 2048)
    {
        _maxAtlasSize = maxAtlasSize;
    }

    /// <summary>
    /// Adds a texture to the cache from pixel data.
    /// </summary>
    /// <param name="graphicId">The graphic ID to associate with this texture</param>
    /// <param name="pixels">Pixel data as uint array (ARGB format)</param>
    /// <param name="width">Texture width</param>
    /// <param name="height">Texture height</param>
    /// <returns>True if the texture was added successfully</returns>
    public bool AddTexture(ushort graphicId, Span<uint> pixels, int width, int height)
    {
        if (_textureMap.ContainsKey(graphicId))
        {
            return false; // Already exists
        }

        // Find or create an atlas page that can fit this texture
        AtlasPage page = FindOrCreateAtlasPage(width, height);
        if (page == null)
        {
            GD.PrintErr($"Texture {graphicId} is too large for atlas (max size: {_maxAtlasSize})");
            return false;
        }

        // Convert uint[] to byte[]
        ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(pixels);

        // Allocate space in the atlas
        if (page.TryAllocate(width, height, out Rect2I region))
        {
            // Update the atlas texture with the new image data
            page.UpdateRegion(region, byteSpan, width, height);

            // Create an AtlasTexture pointing to this region
            var atlasTexture = new AtlasTexture
            {
                Atlas = page.BaseTexture,
                Region = new Rect2(region.Position.X, region.Position.Y, region.Size.X, region.Size.Y)
            };

            // Store the mapping
            _textureMap[graphicId] = atlasTexture;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the AtlasTexture for a given graphic ID.
    /// </summary>
    /// <param name="graphicId">The graphic ID to look up</param>
    /// <returns>The atlas texture, or null if not found</returns>
    public AtlasTexture GetTexture(ushort graphicId)
    {
        return _textureMap.TryGetValue(graphicId, out var atlasTexture) ? atlasTexture : null;
    }

    /// <summary>
    /// Gets the texture region for a given graphic ID.
    /// </summary>
    /// <param name="graphicId">The graphic ID to look up</param>
    /// <returns>The region rect, or Rect2.Zero if not found</returns>
    public Rect2 GetRegion(ushort graphicId)
    {
        return _textureMap.TryGetValue(graphicId, out var atlasTexture) ? atlasTexture.Region : new Rect2(0, 0, 0, 0);
    }

    /// <summary>
    /// Gets the base texture containing all the atlas textures for a graphic ID.
    /// </summary>
    /// <param name="graphicId">The graphic ID to look up</param>
    /// <returns>The base ImageTexture, or null if not found</returns>
    public ImageTexture GetBaseTexture(ushort graphicId)
    {
        return _textureMap.TryGetValue(graphicId, out var atlasTexture) ? atlasTexture.Atlas as ImageTexture : null;
    }

    /// <summary>
    /// Checks if a graphic ID exists in the cache.
    /// </summary>
    public bool HasTexture(ushort graphicId)
    {
        return _textureMap.ContainsKey(graphicId);
    }

    /// <summary>
    /// Clears all cached textures.
    /// </summary>
    public void Clear()
    {
        // Dispose all AtlasTextures
        foreach (var atlasTexture in _textureMap.Values)
        {
            atlasTexture.Dispose();
        }
        _textureMap.Clear();

        // Dispose all atlas pages
        foreach (var page in _atlasPages)
        {
            page.Dispose();
        }
        _atlasPages.Clear();
    }

    private AtlasPage FindOrCreateAtlasPage(int width, int height)
    {
        if (width > _maxAtlasSize || height > _maxAtlasSize)
        {
            return null; // Texture too large
        }

        // Try to find an existing page with space
        foreach (var page in _atlasPages)
        {
            if (page.CanFit(width, height))
            {
                return page;
            }
        }

        // Create a new atlas page
        var newPage = new AtlasPage(_maxAtlasSize);
        _atlasPages.Add(newPage);
        return newPage;
    }

    /// <summary>
    /// Represents a single texture atlas page using a simple shelf packing algorithm.
    /// </summary>
    private class AtlasPage
    {
        private readonly int _size;
        private int _currentY;
        private int _currentX;
        private int _currentRowHeight;
        private Image _image;

        public ImageTexture BaseTexture { get; private set; }

        public AtlasPage(int size)
        {
            _size = size;
            _currentY = 0;
            _currentX = 0;
            _currentRowHeight = 0;

            // Create empty atlas texture
            _image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            _image.Fill(new Color(0, 0, 0, 0)); // Transparent
            BaseTexture = ImageTexture.CreateFromImage(_image);
        }

        public bool CanFit(int width, int height)
        {
            // Try current row
            if (_currentX + width <= _size && _currentY + height <= _size)
            {
                return true;
            }

            // Try next row
            if (width <= _size && _currentY + _currentRowHeight + height <= _size)
            {
                return true;
            }

            return false;
        }

        public bool TryAllocate(int width, int height, out Rect2I region)
        {
            // Try to fit in current row
            if (_currentX + width <= _size && _currentY + height <= _size)
            {
                region = new Rect2I(_currentX, _currentY, width, height);
                _currentX += width;
                _currentRowHeight = Math.Max(_currentRowHeight, height);
                return true;
            }

            // Move to next row
            if (width <= _size && _currentY + _currentRowHeight + height <= _size)
            {
                _currentY += _currentRowHeight;
                _currentX = 0;
                _currentRowHeight = 0;

                region = new Rect2I(_currentX, _currentY, width, height);
                _currentX += width;
                _currentRowHeight = height;
                return true;
            }

            region = new Rect2I(0, 0, 0, 0);
            return false;
        }

        public void UpdateRegion(Rect2I region, ReadOnlySpan<byte> pixels, int width, int height)
        {
            // Convert span to byte array for Godot API
            byte[] pixelArray = pixels.ToArray();

            // Create a temporary image from the pixel data
            var tempImage = Image.CreateFromData(width, height, false, Image.Format.Rgba8, pixelArray);

            // Blit the temporary image onto the atlas at the specified region
            _image.BlitRect(tempImage, new Rect2I(0, 0, width, height), region.Position);

            // Update the texture
            BaseTexture.Update(_image);
        }

        public void Dispose()
        {
            BaseTexture?.Dispose();
            BaseTexture = null;
            _image = null;
        }
    }
}
