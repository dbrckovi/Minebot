using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Pyomm
{
  public class Tile
  {
    public static Vector2 DefaultSize = new Vector2(56, 64);
    public bool Lava = false;
    public bool HasOre = false;
    public HighlightType Flare = HighlightType.None;
    public Point Index;
    public Vector2 World_Center;

    public Tile(int x, int y)
    {
      Index.X = x;
      Index.Y = y;

      World_Center = TileIndexToWorld(x, y);
    }

    /// <summary>
    /// Copies the current tile to a new tile
    /// </summary>
    /// <returns></returns>
    public Tile Copy()
    {
      Tile ret = new Tile(Index.X, Index.Y);
      ret.Lava = Lava;
      ret.HasOre = HasOre;
      ret.Flare = Flare;
      ret.World_Center = World_Center;
      return ret;
    }

    public static void DrawTiles(SpriteBatch spriteBatch, Tile[,] _tiles)
    {
      for (int x = 0; x <= _tiles.GetUpperBound(0); x++)
      {
        for (int y = 0; y <= _tiles.GetUpperBound(1); y++)
        {
          DrawTile(x, y, _tiles[x, y], spriteBatch);
        }
      }
    }

    public static void DrawTile(int x, int y, Tile tile, SpriteBatch spriteBatch)
    {
      Vector2 tileLocationInWorld = TileIndexToWorld(x, y);
      Vector2 tileCenter = new Vector2(DefaultSize.X / 2, DefaultSize.Y / 2);
      Texture2D backTexture = tile.Lava ? Asset.tileLava : Asset.tileNormal;
      spriteBatch.Draw(backTexture, tileLocationInWorld, null, Color.White, 0f, tileCenter, Vector2.One, SpriteEffects.None, 0f);

      switch (tile.Flare)
      {
        case HighlightType.Red: spriteBatch.Draw(Asset.tileRedOverlay, tileLocationInWorld, null, Color.White, 0f, tileCenter, Vector2.One, SpriteEffects.None, 0f); break;
        case HighlightType.Green: spriteBatch.Draw(Asset.tileGreenOverlay, tileLocationInWorld, null, Color.White, 0f, tileCenter, Vector2.One, SpriteEffects.None, 0f); break;
        case HighlightType.Blue: spriteBatch.Draw(Asset.tileBlueOverlay, tileLocationInWorld, null, Color.White, 0f, tileCenter, Vector2.One, SpriteEffects.None, 0f); break;
      }

      if (tile.HasOre) spriteBatch.Draw(Asset.tileOre, tileLocationInWorld, null, Color.White, 0f, tileCenter, Vector2.One, SpriteEffects.None, 0f);
    }

    public static Vector2 TileIndexToWorld(int x, int y)
    {
      Vector2 ret = new Vector2();
      
      ret.Y = y * DefaultSize.Y * 3 / 4 + DefaultSize.Y / 2;
      ret.X = x * DefaultSize.X + DefaultSize.X / 2;
      if (y % 2 == 0) ret.X += DefaultSize.X / 2;

      return ret;
    }
  }
}
