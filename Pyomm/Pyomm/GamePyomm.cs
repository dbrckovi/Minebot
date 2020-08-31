using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

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

      Vector2 approx = Tile.WorldToTileIndex(mouse.X, mouse.Y);

      //int approxIndexY = mouse.Y / (Convert.ToInt32(Tile.DefaultSize.Y * 3/4));
      //int approxIndexX = mouse.X / Convert.ToInt32(Tile.DefaultSize.X);
      
      
      this.Window.Title = $"{mouse.X}, {mouse.Y}     |     {approx.X}, {approx.Y}";

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(_editMode ? _editColor : _playColor);

      _spriteBatch.Begin();
      Tile.DrawTiles(_spriteBatch, _tiles);
      _spriteBatch.End();

      base.Draw(gameTime);
    }
  }
}
