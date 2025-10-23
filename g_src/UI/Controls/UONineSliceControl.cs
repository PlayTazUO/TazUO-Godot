using Godot;
using TazUO.Godot.Utility;

namespace TazUOGodot.g_src.UI.Controls;

public partial class UONineSliceControl : Control
{
	private readonly ushort _graphic;
	private readonly int _desiredWidth;
	private readonly int _desiredHeight;

	// The 9 texture pieces
	private TextureRect[] _pieces = new TextureRect[9];
	private Vector2[] _pieceSizes = new Vector2[9];

	// Offset calculations
	private int _offsetTop;
	private int _offsetBottom;
	private int _offsetLeft;
	private int _offsetRight;

	/// <summary>
	/// Creates a resizable nine-slice panel using UO gump graphics.
	/// </summary>
	/// <param name="graphic">Base graphic ID. Loads 9 textures from graphic to graphic+8 (skipping +4 for center).</param>
	/// <param name="width">Desired width of the control in pixels.</param>
	/// <param name="height">Desired height of the control in pixels.</param>
	public UONineSliceControl(ushort graphic, int width, int height)
	{
		_graphic = graphic;
		_desiredWidth = width;
		_desiredHeight = height;

		CustomMinimumSize = new Vector2(width, height);
		Size = new Vector2(width, height);
	}

	private void CreatePieces()
	{
		// Load textures and create all 9 TextureRect pieces
		for (int i = 0; i < 9; i++)
		{
			int graphicIndex = GetGraphicIndex(i);

			var texture = AssetHelper.GetGumpTexture((ushort)(_graphic + graphicIndex), true);

			_pieces[i] = new TextureRect();
			_pieces[i].Texture = texture;
			_pieces[i].ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;

			// Store texture size
			if (texture != null)
			{
				_pieceSizes[i] = texture.GetSize();
			}
			else
			{
				_pieceSizes[i] = Vector2.Zero;
			}

			// Set tiling for edges and center
			if (i == 1 || i == 3 || i == 4 || i == 6 || i == 8)
			{
				_pieces[i].StretchMode = TextureRect.StretchModeEnum.Tile;
			}
			else
			{
				_pieces[i].StretchMode = TextureRect.StretchModeEnum.Scale;
			}

			AddChild(_pieces[i]);
		}
	}

	private int GetGraphicIndex(int i)
	{
		// Map piece index to graphic index
		// Pieces 0-3 use graphics 0-3
		// Pieces 4-7 use graphics 5-8
		// Piece 8 (center) uses graphic 4
		if (i >= 4 && i < 8)
		{
			return i + 1;
		}
		if (i == 8)
		{
			return 4; // Center piece
		}
		return i;
	}

	private void CalculateOffsets()
	{
		// Calculate offsets based on piece sizes (matching original logic)
		_offsetTop = (int)Mathf.Max(_pieceSizes[0].Y, _pieceSizes[2].Y) - (int)_pieceSizes[1].Y;
		_offsetBottom = (int)Mathf.Max(_pieceSizes[5].Y, _pieceSizes[7].Y) - (int)_pieceSizes[6].Y;
		_offsetLeft = (int)Mathf.Abs(Mathf.Max(_pieceSizes[0].X, _pieceSizes[5].X) - _pieceSizes[2].X);
		_offsetRight = (int)Mathf.Max(_pieceSizes[2].X, _pieceSizes[7].X) - (int)_pieceSizes[4].X;
	}

	private void PositionPieces()
	{
		// Calculate actual space available for edges
		int availableWidth = _desiredWidth - (int)_pieceSizes[0].X - (int)_pieceSizes[2].X;
		int availableHeight = _desiredHeight - (int)_pieceSizes[0].Y - (int)_pieceSizes[5].Y;

		// Top-left corner (0)
		if (_pieces[0].Texture != null)
		{
			_pieces[0].Position = Vector2.Zero;
			_pieces[0].Size = _pieceSizes[0];
		}

		// Top edge (1) - tiled
		if (_pieces[1].Texture != null && availableWidth > 0)
		{
			_pieces[1].Position = new Vector2(_pieceSizes[0].X, 0);
			_pieces[1].Size = new Vector2(availableWidth, _pieceSizes[1].Y);
		}

		// Top-right corner (2)
		if (_pieces[2].Texture != null)
		{
			_pieces[2].Position = new Vector2(
				_desiredWidth - _pieceSizes[2].X,
				_offsetTop
			);
			_pieces[2].Size = _pieceSizes[2];
		}

		// Left edge (3) - tiled
		if (_pieces[3].Texture != null && availableHeight > 0)
		{
			_pieces[3].Position = new Vector2(0, _pieceSizes[0].Y);
			_pieces[3].Size = new Vector2(_pieceSizes[3].X, availableHeight);
		}

		// Right edge (4) - tiled
		if (_pieces[4].Texture != null && availableHeight > 0)
		{
			_pieces[4].Position = new Vector2(
				_desiredWidth - _pieceSizes[4].X,
				_pieceSizes[2].Y + _offsetTop
			);
			_pieces[4].Size = new Vector2(_pieceSizes[4].X, availableHeight);
		}

		// Bottom-left corner (5)
		if (_pieces[5].Texture != null)
		{
			_pieces[5].Position = new Vector2(
				0,
				_desiredHeight - _pieceSizes[5].Y
			);
			_pieces[5].Size = _pieceSizes[5];
		}

		// Bottom edge (6) - tiled
		if (_pieces[6].Texture != null && availableWidth > 0)
		{
			_pieces[6].Position = new Vector2(
				_pieceSizes[5].X,
				_desiredHeight - _pieceSizes[6].Y - _offsetBottom
			);
			_pieces[6].Size = new Vector2(availableWidth, _pieceSizes[6].Y);
		}

		// Bottom-right corner (7)
		if (_pieces[7].Texture != null)
		{
			_pieces[7].Position = new Vector2(
				_desiredWidth - _pieceSizes[7].X,
				_desiredHeight - _pieceSizes[7].Y
			);
			_pieces[7].Size = _pieceSizes[7];
		}

		// Center (8) - tiled
		if (_pieces[8].Texture != null && availableWidth > 0 && availableHeight > 0)
		{
			_pieces[8].Position = new Vector2(_pieceSizes[0].X, _pieceSizes[0].Y);
			_pieces[8].Size = new Vector2(
				availableWidth + _offsetLeft + _offsetRight,
				availableHeight
			);
		}
	}

	public override void _Ready()
	{
		base._Ready();

		CreatePieces();
		CalculateOffsets();

		// Debug output
		GD.Print($"=== UONineSliceControl Debug ===");
		GD.Print($"Desired: {_desiredWidth}x{_desiredHeight}");
		GD.Print($"Offsets - Top: {_offsetTop}, Bottom: {_offsetBottom}, Left: {_offsetLeft}, Right: {_offsetRight}");
		for (int i = 0; i < 9; i++)
		{
			GD.Print($"Piece {i}: TextureSize={_pieceSizes[i]}");
		}

		PositionPieces();

		// Debug output AFTER positioning
		GD.Print($"=== After Positioning ===");
		for (int i = 0; i < 9; i++)
		{
			GD.Print($"Piece {i}: Position={_pieces[i].Position}, Size={_pieces[i].Size}");
		}
	}
}
