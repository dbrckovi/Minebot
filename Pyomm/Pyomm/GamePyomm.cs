using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

namespace Pyomm
{
  public class GamePyomm : Game
  {
    GuiButton btnStartStop;
    GuiButton btnExit;

    private Level _currentLevel;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private bool _editMode = true;
    private Tile[,] _tiles = new Tile[16, 16];
    Color _playColor = new Color(117, 94, 57);
    Color _editColor = new Color(50, 45, 27);
    Tile selectedTile = null;
    PlayerInfo player = new PlayerInfo();
    InputHandler _input = new InputHandler();
    List<GuiControl> Controls = new List<GuiControl>();

    GuiControl _lastMouseDownControl = null;

    public GamePyomm()
    {
      _graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      IsMouseVisible = true;
    }

    protected override void Initialize()
    {
      Asset.LoadTextures(this);
      
      InitializeControls();
      
      _input.KeyPressed += _input_KeyPressed;
      _input.KeyReleased += _input_KeyReleased;
      _input.MousePressed += _input_MousePressed;
      _input.MouseReleased += _input_MouseReleased;

      _graphics.PreferredBackBufferWidth = 1280;
      _graphics.PreferredBackBufferHeight = 800;
      _graphics.ApplyChanges();
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      _currentLevel = new Level(File.ReadAllText("DefaultLevel.lev"));

      for (int x = 0; x <= _tiles.GetUpperBound(0); x++)
      {
        for (int y = 0; y <= _tiles.GetUpperBound(0); y++)
        {
          _tiles[x, y] = _currentLevel.Tiles[x, y].Copy();
        }
      }
      player.Direction = _currentLevel.PlayerStart.Direction;
      player.Location = _currentLevel.PlayerStart.Location;
    }

    private void InitializeControls()
    {
      btnStartStop = new GuiButton(new Point(1000, 6));
      btnStartStop.Text = "START MACHINE";
      btnStartStop.Enabled = true;
      btnStartStop.Click += BtnStartStop_Click;
      Controls.Add(btnStartStop);

      btnExit = new GuiButton(new Point(1136, 752));
      btnExit.Text = "EXIT (ESC)";
      btnExit.Click += BtnExit_Click;
      Controls.Add(btnExit);
    }

    private void BtnExit_Click(GuiControl control)
    {
      Exit();
    }

    private void BtnStartStop_Click(GuiControl control)
    {

    }

    private void _input_MouseReleased(MouseButton button, int x, int y)
    {
      if (button == MouseButton.Left)
      {
        #region Trigger control click
        if (_lastMouseDownControl != null)
        {
          GuiControl releasedControl = null;
          foreach (GuiControl ctl in Controls)
          {
            if (ctl.MouseHitTest(new Point(x, y)))
            {
              releasedControl = ctl;
              break;
            }
          }

          if (releasedControl == _lastMouseDownControl)
          {
            releasedControl.TriggerClick();
            _lastMouseDownControl = null;
          }
        }
        #endregion Trigger control click
      }
    }

    private void _input_MousePressed(MouseButton button, int x, int y)
    {
      if (button == MouseButton.Left)
      {
        #region Remember mouse down control
        _lastMouseDownControl = null;
        foreach (GuiControl ctl in Controls)
        {
          if (ctl.MouseHitTest(new Point(x,y)))
          {
            _lastMouseDownControl = ctl;
            break;
          }
        }
        #endregion Remember mouse down control
      }

      if (_editMode)
      {
        selectedTile = GetTileUnderMouse(new Vector2(x, y));

        if (selectedTile != null)
        {
          if (button == MouseButton.Right) selectedTile.Lava = !selectedTile.Lava;
          else if (button == MouseButton.Middle) selectedTile.HasOre = !selectedTile.HasOre;
        }
      }
    }

    private void _input_KeyPressed(Keys key)
    {
      if (_editMode)
      {
        if (key == Keys.A) player.Rotate(-1);
        else if (key == Keys.D) player.Rotate(1);
        else if (key == Keys.W)
        {
          Point newPlayerLocation = player.GetForwardLocation();
          if (newPlayerLocation.X >= 0 && newPlayerLocation.Y >= 0 && newPlayerLocation.X <= 15 && newPlayerLocation.Y <= 15) player.Location = newPlayerLocation;
        }

        if (selectedTile != null)
        {
          if (key == Keys.Space) player.Location = selectedTile.Index;
          else if (key == Keys.R) selectedTile.Flare = HighlightType.Red;
          else if (key == Keys.G) selectedTile.Flare = HighlightType.Green;
          else if (key == Keys.B) selectedTile.Flare = HighlightType.Blue;
          else if (key == Keys.N) selectedTile.Flare = HighlightType.None;
        }
      }
    }

    private void _input_KeyReleased(Keys key)
    {
      if (key == Keys.Escape) Exit();
    }

    protected override void LoadContent()
    {
      //sometimes doesn't get called (???)
    }

    protected override void Update(GameTime gameTime)
    {
      _input.Update();
      //MouseState mouse = Mouse.GetState();
      //KeyboardState keyboard = Keyboard.GetState();

      //if (mouse.LeftButton == ButtonState.Pressed)
      //{
      //  selectedTile = GetTileUnderMouse(new Vector2(mouse.X, mouse.Y));
      //}

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(_editMode ? _editColor : _playColor);

      _spriteBatch.Begin();
      Tile.DrawTiles(_spriteBatch, _tiles);
      player.Draw(_spriteBatch);
      if (selectedTile != null) DrawSelectedTile();

      foreach (GuiControl ctl in Controls)
      {
        ctl.Draw(_spriteBatch);
      }

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
