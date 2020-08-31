using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Net.Http.Headers;

namespace Pyomm
{
  public class GamePyomm : Game
  {
    private Level _currentLevel;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private bool _editMode = false;
    private Tile[,] _tiles = new Tile[16, 16];
    Color _playColor = new Color(117, 94, 57);
    Color _editColor = new Color(50, 45, 27);
    Tile selectedTile = null;

    public GamePyomm()
    {
      _graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      IsMouseVisible = true;
    }

    protected override void Initialize()
    {
      _graphics.PreferredBackBufferWidth = 1280;
      _graphics.PreferredBackBufferHeight = 800;
      _graphics.ApplyChanges();

      _currentLevel = new Level(File.ReadAllText("DefaultLevel.lev"));

      for (int x = 0; x <= _tiles.GetUpperBound(0); x++)
      {
        for (int y = 0; y <= _tiles.GetUpperBound(0); y++)
        {
          _tiles[x, y] = _currentLevel.Tiles[x, y].Copy();
        }
      }

      base.Initialize();
    }

    protected override void LoadContent()
    {
      _spriteBatch = new SpriteBatch(GraphicsDevice);
      Asset.LoadTextures(this);
    }

    protected override void Update(GameTime gameTime)
    {
      MouseState mouse = Mouse.GetState();
      KeyboardState keyboard = Keyboard.GetState();

      if (keyboard.IsKeyDown(Keys.Escape)) Exit();

      if (mouse.LeftButton == ButtonState.Pressed)
      {
        selectedTile = GetTileUnderMouse(new Vector2(mouse.X, mouse.Y));
      }

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(_editMode ? _editColor : _playColor);

      _spriteBatch.Begin();
      Tile.DrawTiles(_spriteBatch, _tiles);
      if (selectedTile != null) DrawSelectedTile();
      _spriteBatch.End();

      base.Draw(gameTime);
    }

    private void DrawSelectedTile()
    {
      Vector2 tileCenter = new Vector2(Tile.DefaultSize.X / 2, Tile.DefaultSize.Y / 2);
      _spriteBatch.Draw(Asset.tileSelection, selectedTile.World_Center, null, Color.White, 0f, tileCenter, Vector2.One, SpriteEffects.None, 0f);
    }

    private Tile GetTileUnderMouse(Vector2 mouse)
    {
      //TODO: rewrite without approximations

      Tile ret = null;
      float distanceToRet = 10000f;

      //approximate X and Y index of the tile
      int approxX = Convert.ToInt32(mouse.X / 57f); 
      int approxY = Convert.ToInt32(mouse.Y / 49f);

      //scan a couple of nearest tiles to determine the closest
      int scanXFrom = approxX - 2;
      int scanXTo = approxX + 2;
      int scanYFrom = approxY - 2;
      int scanYTo = approxY + 2;
      if (scanXFrom < 0) scanXFrom = 0;
      if (scanXFrom > 15) return null;
      if (scanXTo > 15) scanXTo = 15;
      if (scanXTo < 0) return null;
      if (scanYFrom < 0) scanYFrom = 0;
      if (scanYFrom > 15) return null;
      if (scanYTo > 15) scanYTo = 15;
      if (scanYTo < 0) return null;

      float minimumDistance = 32; 

      for (int x = scanXFrom; x <= scanXTo; x++)
      {
        for (int y = scanYFrom; y <= scanYTo; y++)
        {
          Vector2 tileCenter = _tiles[x, y].World_Center;
          float distance = Vector2.Distance(tileCenter, mouse);
          if (distance <= minimumDistance)
          {
            if (ret == null || distance < distanceToRet)
            {
              ret = _tiles[x, y];
              distanceToRet = distance;
            }
          }
        }
      }

      return ret;
    }
  }
}
