using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pyomm
{
  public static class Asset
  {
    public static Texture2D tileNormal;
    public static Texture2D tileLava;
    public static Texture2D tileRedOverlay;
    public static Texture2D tileGreenOverlay;
    public static Texture2D tileBlueOverlay;
    public static Texture2D tileOre;
    public static Texture2D tileMiner;
    public static Texture2D tileSelection;
    public static Texture2D buttonBackground;
    public static Texture2D commandBackground;
    public static Texture2D commandHighlight;
    public static SpriteFont buttonFont;
    public static SpriteFont messageFont;
    public static SpriteFont helpFont;

    public static void LoadTextures(GamePyomm game)
    {
      tileNormal = game.Content.Load<Texture2D>("Tile_Normal");
      tileLava = game.Content.Load<Texture2D>("Tile_Lava");
      tileRedOverlay = game.Content.Load<Texture2D>("Tile_Red_Overlay");
      tileGreenOverlay = game.Content.Load<Texture2D>("Tile_Green_Overlay");
      tileBlueOverlay = game.Content.Load<Texture2D>("Tile_Blue_Overlay");
      tileOre = game.Content.Load<Texture2D>("Tile_Ore");
      tileMiner = game.Content.Load<Texture2D>("Miner");
      tileSelection = game.Content.Load<Texture2D>("Tile_Selected");
      buttonBackground = game.Content.Load<Texture2D>("Button");
      commandBackground = game.Content.Load<Texture2D>("Command");
      commandHighlight = game.Content.Load<Texture2D>("CommandHighlight");
      buttonFont = game.Content.Load<SpriteFont>("GeneralFont");
      messageFont = game.Content.Load<SpriteFont>("Message");
      helpFont = game.Content.Load<SpriteFont>("Help");
    }
  }
}
