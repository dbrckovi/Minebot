using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Pyomm
{
  public class GamePyomm : Game
  {
    GuiButton btnStartStop;
    GuiButton btnHelp;
    CommandMemory memoryMain;
    CommandMemory memoryF1;
    CommandMemory memoryF2;
    CommandMemory memoryF3;
    GuiButton btnConditionClear;
    GuiButton btnConditionRed;
    GuiButton btnConditionGreen;
    GuiButton btnConditionBlue;
    GuiButton btnCommandClear;
    GuiButton btnCommandGo;
    GuiButton btnCommandRotateLeft;
    GuiButton btnCommandRotateRight;
    GuiButton btnCommandPaintRed;
    GuiButton btnCommandPaintGreen;
    GuiButton btnCommandPaintBlue;
    GuiButton btnCommandF1;
    GuiButton btnCommandF2;
    GuiButton btnCommandF3;
    GuiButton btnSaveLevel;
    GuiButton btnClearLevel;

    CPU _cpu = new CPU();
    Level _currentLevel;
    GraphicsDeviceManager _graphics;
    SpriteBatch _spriteBatch;
    bool _editMode = false;
    bool _helpMode = false;
    Tile[,] _tiles = new Tile[16, 16];
    Color _playColor = new Color(117, 94, 57);
    Color _editColor = new Color(50, 45, 27);
    Tile selectedTile = null;
    PlayerInfo player = new PlayerInfo();
    InputHandler _input = new InputHandler();
    List<GuiControl> Controls = new List<GuiControl>();

    List<string> detectedLevels = new List<string>();
    int _currentLevelIndex = -1;

    private string _message;
    Color _messageColor = Color.White;
    bool _nextLevelAfterMessage = false;
    private DateTime _messageShownTime = DateTime.Now;

    GuiControl _lastMouseDownControl = null;

    private bool EditMode
    {
      get { return _editMode; }
      set
      {
        _editMode = value;
        memoryMain.EditMode = memoryF1.EditMode = memoryF2.EditMode = memoryF3.EditMode = value;
      }
    }

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
      _input.MouseMoved += _input_MouseMoved;
      _cpu.WorkingChanged += _cpu_WorkingChanged;
      _cpu.ExecutionFinished += _cpu_ExecutionFinished;

      _graphics.PreferredBackBufferWidth = 1280;
      _graphics.PreferredBackBufferHeight = 800;
      _graphics.ApplyChanges();
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      DetectLevels();
      if (detectedLevels.Count > 0)
      {
        LoadNextLevel();
      }
      else
      {
        LoadLevelFromFile("DefaultLevel.lev");
        
      }

      LoadCurrentLevelState();

      EnableOrDisableControls();
    }

    private void LoadLevelFromFile(string fileName)
    {
      _currentLevel = new Level(File.ReadAllText(fileName));
      ShowMessage(Path.GetFileNameWithoutExtension(fileName), Color.White);
    }

    private void LoadNextLevel()
    {
      if (detectedLevels.Count > (_currentLevelIndex + 1))
      {
        _currentLevelIndex++;
        LoadLevelFromFile(detectedLevels[_currentLevelIndex]);
      }
      else
      {
        ShowMessage("No more levels", Color.SkyBlue);
      }
    }

    private void DetectLevels()
    {
      string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      int i = 1;
      while (true)
      {
        string levelName = $"Level{i}.lev";
        string levelPath = Path.Combine(path, levelName);

        if (File.Exists(levelPath))
        {
          detectedLevels.Add(levelPath);
          i++;
        }
        else break;
      }
    }

    private void _cpu_ExecutionFinished(InstructionResult result)
    {
      switch (result)
      {
        case InstructionResult.AllOreGathered: ShowMessage("Congratulations!!! Level complete.", Color.Green); _nextLevelAfterMessage = !_editMode; break;
        case InstructionResult.Cancelled: ShowMessage("Execution cancelled!", Color.Yellow); break;
        case InstructionResult.Death: ShowMessage("Miner was destroyed!!!", Color.Red); break;
        case InstructionResult.Finished: ShowMessage("All ore must be mined!!!", Color.Red); break;
      }

      LoadCurrentLevelState();
    }

    private void _cpu_WorkingChanged()
    {
      btnStartStop.Text = _cpu.Working ? "Stop Machine" : "Start Machine";
      EnableOrDisableControls();
    }

    /// <summary>
    /// Removes highlighted command from all CommandMemory controls except the specified
    /// </summary>
    /// <param name="exceptThis"></param>
    private void RemoveCommandHighlights(CommandMemory exceptThis)
    {
      selectedTile = null;
      if (exceptThis != memoryMain) memoryMain.HighlightedCommandIndex = null;
      if (exceptThis != memoryF1) memoryF1.HighlightedCommandIndex = null;
      if (exceptThis != memoryF2) memoryF2.HighlightedCommandIndex = null;
      if (exceptThis != memoryF3) memoryF3.HighlightedCommandIndex = null;
    }

    private CommandMemory GetMemoryWithHighlight()
    {
      if (memoryMain.HighlightedCommandIndex != null) return memoryMain;
      else if (memoryF1.HighlightedCommandIndex != null) return memoryF1;
      else if (memoryF2.HighlightedCommandIndex != null) return memoryF2;
      else if (memoryF3.HighlightedCommandIndex != null) return memoryF3;
      else return null;
    }

    private void EnableOrDisableControls()
    {
      bool commandButtonsEnabled = !_cpu.Working && GetMemoryWithHighlight() != null;

      btnConditionClear.Enabled = btnConditionRed.Enabled = btnConditionGreen.Enabled = btnConditionBlue.Enabled
        = btnCommandClear.Enabled = btnCommandGo.Enabled = btnCommandRotateLeft.Enabled = btnCommandRotateRight.Enabled
        = btnCommandPaintRed.Enabled = btnCommandPaintGreen.Enabled = btnCommandPaintBlue.Enabled
        = btnCommandF1.Enabled = btnCommandF2.Enabled = btnCommandF3.Enabled
        = commandButtonsEnabled;

      btnHelp.Enabled = !_cpu.Working;
      btnSaveLevel.Enabled = btnClearLevel.Enabled = !_cpu.Working;
      btnSaveLevel.Visible = btnClearLevel.Visible = _editMode;

      memoryMain.Enabled = memoryF1.Enabled = memoryF2.Enabled = memoryF3.Enabled = !_cpu.Working;
    }

    private void _input_MouseMoved(Point p)
    {
      //if (_editMode) this.Window.Title = p.ToString();
    }

    /// <summary>
    /// Sets command condition to currently highlighted command
    /// </summary>
    /// <param name="condition"></param>
    private void SetCommandCondition(HighlightType condition)
    {
      CommandMemory mem = GetMemoryWithHighlight();
      if (mem != null) mem.SetCondition(mem.HighlightedCommandIndex.Value, condition);
    }

    /// <summary>
    /// Sets command type to currently highlighted command
    /// </summary>
    /// <param name="commandType"></param>
    private void SetCommandType(CommandType commandType)
    {
      CommandMemory mem = GetMemoryWithHighlight();
      if (mem != null) mem.SetCommand(mem.HighlightedCommandIndex.Value, commandType);
    }

    private void ShowMessage(string text, Color color)
    {
      _message = text;
      _messageColor = color;
      _messageShownTime = DateTime.Now;
    }

    private void LoadCurrentLevelState(bool clearInstructions = false)
    {
      selectedTile = null;
      for (int x = 0; x <= _tiles.GetUpperBound(0); x++)
      {
        for (int y = 0; y <= _tiles.GetUpperBound(0); y++)
        {
          _tiles[x, y] = _currentLevel.Tiles[x, y].Copy();
        }
      }
      player.Direction = _currentLevel.PlayerStart.Direction;
      player.Location = _currentLevel.PlayerStart.Location;
      memoryMain.MaxCommands = _currentLevel.MainMemory;
      memoryF1.MaxCommands = _currentLevel.F1Memory;
      memoryF2.MaxCommands = _currentLevel.F2Memory;
      memoryF3.MaxCommands = _currentLevel.F3Memory;

      if (clearInstructions)
      {
        for (int x = 0; x < 16; x++)
        {
          memoryMain.SetCommand(x, CommandType.Empty);
          memoryF1.SetCommand(x, CommandType.Empty);
          memoryF2.SetCommand(x, CommandType.Empty);
          memoryF3.SetCommand(x, CommandType.Empty);
        }
      }
    }

    private void SaveToCurrentLevel()
    {
      selectedTile = null;
      for (int x = 0; x <= _tiles.GetUpperBound(0); x++)
      {
        for (int y = 0; y <= _tiles.GetUpperBound(0); y++)
        {
          _currentLevel.Tiles[x, y] = _tiles[x, y].Copy();
        }
      }
      _currentLevel.PlayerStart.Direction = player.Direction;
      _currentLevel.PlayerStart.Location = player.Location;
    }

    private void InitializeControls()
    {
      btnStartStop = new GuiButton(new Point(950, 760));
      btnStartStop.Text = "START MACHINE";
      btnStartStop.Click += BtnStartStop_Click;
      Controls.Add(btnStartStop);

      btnHelp = new GuiButton(new Point(1150, 760));
      btnHelp.Text = "Help";
      btnHelp.Click += BtnHelp_Click;
      Controls.Add(btnHelp);

      btnConditionClear = new GuiButton(new Point(950, 400));
      btnConditionClear.Text = "None";
      btnConditionClear.Click += BtnConditionClear_Click;
      Controls.Add(btnConditionClear);

      btnConditionRed = new GuiButton(new Point(950, 434));
      btnConditionRed.Text = "On Red (R)";
      btnConditionRed.Click += BtnConditionRed_Click;
      Controls.Add(btnConditionRed);

      btnConditionGreen = new GuiButton(new Point(950, 468));
      btnConditionGreen.Text = "On Green (G)";
      btnConditionGreen.Click += BtnConditionGreen_Click;
      Controls.Add(btnConditionGreen);

      btnConditionBlue = new GuiButton(new Point(950, 502));
      btnConditionBlue.Text = "On Blue (B)";
      btnConditionBlue.Click += BtnConditionBlue_Click;
      Controls.Add(btnConditionBlue);

      btnCommandClear = new GuiButton(new Point(1100, 400));
      btnCommandClear.Text = "Clear";
      btnCommandClear.Click += BtnCommandClear_Click;
      Controls.Add(btnCommandClear);

      btnCommandGo = new GuiButton(new Point(1100, 434));
      btnCommandGo.Text = "Forward (GO)";
      btnCommandGo.Click += BtnCommandGo_Click;
      Controls.Add(btnCommandGo);

      btnCommandRotateLeft = new GuiButton(new Point(1100, 468));
      btnCommandRotateLeft.Text = "Rotate left (RL)";
      btnCommandRotateLeft.Click += BtnCommandRotateLeft_Click;
      Controls.Add(btnCommandRotateLeft);

      btnCommandRotateRight = new GuiButton(new Point(1100, 502));
      btnCommandRotateRight.Text = "Rotate right (RR)";
      btnCommandRotateRight.Click += BtnCommandRotateRight_Click;
      Controls.Add(btnCommandRotateRight);

      btnCommandPaintRed = new GuiButton(new Point(1100, 536));
      btnCommandPaintRed.Text = "Paint red (PR)";
      btnCommandPaintRed.Click += BtnCommandPaintRed_Click;
      Controls.Add(btnCommandPaintRed);

      btnCommandPaintGreen = new GuiButton(new Point(1100, 570));
      btnCommandPaintGreen.Text = "Paint red (PR)";
      btnCommandPaintGreen.Click += BtnCommandPaintGreen_Click;
      Controls.Add(btnCommandPaintGreen);

      btnCommandPaintBlue = new GuiButton(new Point(1100, 604));
      btnCommandPaintBlue.Text = "Paint blue (PB)";
      btnCommandPaintBlue.Click += BtnCommandPaintBlue_Click;
      Controls.Add(btnCommandPaintBlue);

      btnCommandF1 = new GuiButton(new Point(1100, 638));
      btnCommandF1.Text = "Call function 1 (F1)";
      btnCommandF1.Click += BtnCommandF1_Click;
      Controls.Add(btnCommandF1);

      btnCommandF2 = new GuiButton(new Point(1100, 672));
      btnCommandF2.Text = "Call function 2 (F2)";
      btnCommandF2.Click += BtnCommandF2_Click;
      Controls.Add(btnCommandF2);

      btnCommandF3 = new GuiButton(new Point(1100, 706));
      btnCommandF3.Text = "Call function 3 (F3)";
      btnCommandF3.Click += BtnCommandF3_Click;
      Controls.Add(btnCommandF3);

      btnClearLevel = new GuiButton(new Point(950, 672));
      btnClearLevel.Text = "Clear level";
      btnClearLevel.Click += BtnClearLevel_Click;
      Controls.Add(btnClearLevel);

      btnSaveLevel = new GuiButton(new Point(950, 706));
      btnSaveLevel.Text = "Save level";
      btnSaveLevel.Click += BtnSaveLevel_Click;
      Controls.Add(btnSaveLevel);

      memoryMain = new CommandMemory(new Point(950, 20));
      memoryMain.CommandHighlighted += MemoryMain_CommandHighlighted;
      memoryMain.Text = "Main";

      memoryF1 = new CommandMemory(new Point(1034, 20));
      memoryF1.CommandHighlighted += MemoryF1_CommandHighlighted;
      memoryF1.Text = "F1";

      memoryF2 = new CommandMemory(new Point(1118, 20));
      memoryF2.CommandHighlighted += MemoryF2_CommandHighlighted;
      memoryF2.Text = "F2";

      memoryF3 = new CommandMemory(new Point(1202, 20));
      memoryF3.CommandHighlighted += MemoryF3_CommandHighlighted;
      memoryF3.Text = "F3";

      Controls.Add(memoryMain);
      Controls.Add(memoryF1);
      Controls.Add(memoryF2);
      Controls.Add(memoryF3);
    }

    private void BtnClearLevel_Click(GuiControl control)
    {
      _currentLevel = new Level(File.ReadAllText("DefaultLevel.lev"));
      LoadCurrentLevelState(true);
    }

    private void BtnSaveLevel_Click(GuiControl control)
    {
      try
      {
        if (_editMode && !_cpu.Working)
        {
          SaveToCurrentLevel();
          string fileName = string.Format("Level_{0}.lev", DateTime.Now.ToString("dd.MM.yyyy HH_mm_ss"));
          File.WriteAllText(fileName, _currentLevel.SaveToString());
        }
      }
      catch
      {
        ShowMessage("ERROR", Color.Red);
      }
    }

    private void BtnHelp_Click(GuiControl control)
    {
      _helpMode = true;
    }

    private void MemoryF3_CommandHighlighted(int x)
    {
      RemoveCommandHighlights(memoryF3);
      EnableOrDisableControls();
    }

    private void MemoryF2_CommandHighlighted(int x)
    {
      RemoveCommandHighlights(memoryF2);
      EnableOrDisableControls();
    }

    private void MemoryF1_CommandHighlighted(int x)
    {
      RemoveCommandHighlights(memoryF1);
      EnableOrDisableControls();
    }

    private void MemoryMain_CommandHighlighted(int x)
    {
      RemoveCommandHighlights(memoryMain);
      EnableOrDisableControls();
    }

    private void BtnCommandF3_Click(GuiControl control)
    {
      SetCommandType(CommandType.F3);
    }

    private void BtnCommandF2_Click(GuiControl control)
    {
      SetCommandType(CommandType.F2);
    }

    private void BtnCommandF1_Click(GuiControl control)
    {
      SetCommandType(CommandType.F1);
    }

    private void BtnCommandPaintBlue_Click(GuiControl control)
    {
      SetCommandType(CommandType.PaintBlue);
    }

    private void BtnCommandPaintGreen_Click(GuiControl control)
    {
      SetCommandType(CommandType.PaintGreen);
    }

    private void BtnCommandPaintRed_Click(GuiControl control)
    {
      SetCommandType(CommandType.PaintRed);
    }

    private void BtnCommandRotateRight_Click(GuiControl control)
    {
      SetCommandType(CommandType.RotateRight);
    }

    private void BtnCommandRotateLeft_Click(GuiControl control)
    {
      SetCommandType(CommandType.RotateLeft);
    }

    private void BtnCommandGo_Click(GuiControl control)
    {
      SetCommandType(CommandType.Go);
    }

    private void BtnCommandClear_Click(GuiControl control)
    {
      SetCommandType(CommandType.Empty);
    }

    private void BtnConditionBlue_Click(GuiControl control)
    {
      SetCommandCondition(HighlightType.Blue);
    }

    private void BtnConditionGreen_Click(GuiControl control)
    {
      SetCommandCondition(HighlightType.Green);
    }

    private void BtnConditionRed_Click(GuiControl control)
    {
      SetCommandCondition(HighlightType.Red);
    }

    private void BtnConditionClear_Click(GuiControl control)
    {
      SetCommandCondition(HighlightType.None);
    }

    private void BtnStartStop_Click(GuiControl control)
    {
      try
      {
        if (_cpu.Working) _cpu.Stop();
        else
        {
          SaveToCurrentLevel();
          _cpu.Start(_tiles, memoryMain, memoryF1, memoryF2, memoryF3, player);
        }
      }
      catch (Exception ex)
      {
        ShowMessage(ex.Message, Color.Red);
      }
    }

    private void _input_MouseReleased(MouseButton button, int x, int y)
    {
      if (_helpMode) return;

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
            releasedControl.TriggerClick(new Point(x, y));
            _lastMouseDownControl = null;
          }
        }
        #endregion Trigger control click
      }
    }

    private void _input_MousePressed(MouseButton button, int x, int y)
    {
      if (_helpMode) return;

      if (button == MouseButton.Left)
      {
        #region Remember mouse down control
        _lastMouseDownControl = null;
        foreach (GuiControl ctl in Controls)
        {
          if (ctl.MouseHitTest(new Point(x, y)))
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
          ClearHighlightedCommand();
          EnableOrDisableControls();
          if (button == MouseButton.Right) selectedTile.Lava = !selectedTile.Lava;
          else if (button == MouseButton.Middle) selectedTile.HasOre = !selectedTile.HasOre;
        }
      }
    }

    private void ClearHighlightedCommand()
    {
      CommandMemory mem = GetMemoryWithHighlight();
      if (mem != null) mem.HighlightedCommandIndex = null;
    }

    private void _input_KeyPressed(Keys key, bool shift)
    {
      if (_helpMode) return;

      if (key == Keys.Add && _cpu.Delay > 100) _cpu.Delay /= 2;
      if (key == Keys.Subtract && _cpu.Delay < 1600) _cpu.Delay *= 2;

      if (!_cpu.Working)
      {
        if (_editMode)
        {
          //if (key == Keys.A) player.Rotate(-1);
          //else if (key == Keys.D) player.Rotate(1);
          //else if (key == Keys.W)
          //{
          //  Point newPlayerLocation = player.GetForwardLocation();
          //  if (newPlayerLocation.X >= 0 && newPlayerLocation.Y >= 0 && newPlayerLocation.X <= 15 && newPlayerLocation.Y <= 15) player.Location = newPlayerLocation;
          //}

          if (selectedTile != null)
          {
            if (key == Keys.Space)
            {
              if (player.Location == selectedTile.Index) player.Rotate(1);
              else player.Location = selectedTile.Index;
            }
            else if (key == Keys.R) selectedTile.Flare = HighlightType.Red;
            else if (key == Keys.G) selectedTile.Flare = HighlightType.Green;
            else if (key == Keys.B) selectedTile.Flare = HighlightType.Blue;
            else if (key == Keys.N) selectedTile.Flare = HighlightType.None;
          }
        }

        if (key == Keys.Up || key == Keys.W) SetCommandType(CommandType.Go);
        if (key == Keys.Left || key == Keys.A) SetCommandType(CommandType.RotateLeft);
        if (key == Keys.Right || key == Keys.D) SetCommandType(CommandType.RotateRight);
        if (key == Keys.R && shift) SetCommandType(CommandType.PaintRed);
        if (key == Keys.G && shift) SetCommandType(CommandType.PaintGreen);
        if (key == Keys.B && shift) SetCommandType(CommandType.PaintBlue);
        if (key == Keys.F1) SetCommandType(CommandType.F1);
        if (key == Keys.F2) SetCommandType(CommandType.F2);
        if (key == Keys.F3) SetCommandType(CommandType.F3);
        if (key == Keys.R && !shift) SetCommandCondition(HighlightType.Red);
        if (key == Keys.G && !shift) SetCommandCondition(HighlightType.Green);
        if (key == Keys.B && !shift) SetCommandCondition(HighlightType.Blue);
        if (key == Keys.Delete) SetCommandType(CommandType.Empty);
        if (key == Keys.Back) SetCommandCondition(HighlightType.None);
      }
    }

    private void _input_KeyReleased(Keys key, bool shift)
    {
      if (key == Keys.Escape)
      {
        if (_helpMode) _helpMode = false;
      }

      if (key == Keys.F12 && !_cpu.Working)
      {
        _editMode = !_editMode;
        selectedTile = null;
        EnableOrDisableControls();
      }
    }

    protected override void LoadContent()
    {
      //sometimes doesn't get called (???)
    }

    protected override void Update(GameTime gameTime)
    {
      _input.Update();

      if (_message != null)
      {
        TimeSpan span = DateTime.Now - _messageShownTime;
        if (span.TotalSeconds > 2)
        {
          _message = null;
          if (_nextLevelAfterMessage)
          {
            _nextLevelAfterMessage = false;
            LoadNextLevel();
            LoadCurrentLevelState(true);
          }
        }

      }


      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(_editMode ? _editColor : _playColor);

      _spriteBatch.Begin();

      if (_helpMode) Utility.DrawHelp(_spriteBatch);
      else
      {
        Tile.DrawTiles(_spriteBatch, _tiles);
        player.Draw(_spriteBatch);
        if (selectedTile != null) DrawSelectedTile();

        foreach (GuiControl ctl in Controls)
        {
          ctl.Draw(_spriteBatch);
        }

        _spriteBatch.DrawString(Asset.buttonFont, "Conditions", new Vector2(950, 380), Color.White);
        _spriteBatch.DrawString(Asset.buttonFont, "Instructions", new Vector2(1100, 380), Color.White);
        _spriteBatch.DrawString(Asset.buttonFont, $"Speed: {5 - Math.Log2(_cpu.Delay / 100)}", new Vector2(950, 600), Color.White);

        if (!string.IsNullOrEmpty(_message))
        {
          _spriteBatch.DrawString(Asset.messageFont, _message, new Vector2(153, 103), Color.Black);
          _spriteBatch.DrawString(Asset.messageFont, _message, new Vector2(150, 100), _messageColor);
        }

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
