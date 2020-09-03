using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace Pyomm
{
  public class CPU
  {
    private Thread _thread = null;
    private bool _shouldAbort = true;

    //references to same objects and values on the main screen.
    private Tile[,] _tiles;
    private CommandMemory _memoryMain;
    private CommandMemory _memoryF1;
    private CommandMemory _memoryF2;
    private CommandMemory _memoryF3;
    private int _remainingOre = 0;
    private PlayerInfo _player;

    public bool Working
    {
      get { return _thread != null; }
    }

    public int Delay = 500;

    public delegate void VoidDelegate();
    public delegate void InstructionResultDelegate(InstructionResult result);
    
    public event VoidDelegate WorkingChanged;
    private void OnWorkingChanged()
    {
      WorkingChanged?.Invoke();
    }

    public event InstructionResultDelegate ExecutionFinished;
    private void OnExecutionFinished(InstructionResult result)
    {
      ExecutionFinished?.Invoke(result);
    }

    public void Start(Tile[,] tiles, CommandMemory main, CommandMemory f1, CommandMemory f2, CommandMemory f3, PlayerInfo player)
    {

      if (!Working)
      {
        _remainingOre = 0;
        foreach (Tile tile in tiles)
        {
          if (tile.HasOre) _remainingOre++;
        }
        if (_remainingOre == 0) throw new Exception("There are no ores to gather");

        _tiles = tiles;
        _memoryMain = main;
        _memoryF1 = f1;
        _memoryF2 = f2;
        _memoryF3 = f3;
        _player = player;

        _memoryMain.HighlightedCommandIndex = _memoryF1.HighlightedCommandIndex = _memoryF2.HighlightedCommandIndex = _memoryF3.HighlightedCommandIndex = null;

        ThreadStart start = new ThreadStart(Work);
        _thread = new Thread(start);
        _thread.IsBackground = true;
        OnWorkingChanged();
        _shouldAbort = false;
        _thread.Start();
      }
    }

    public void Stop()
    {
      _shouldAbort = true;
    }

    private void Work()
    {
      InstructionResult result = RunInstructions(_memoryMain);
      
      _memoryMain.HighlightedCommandIndex = _memoryF1.HighlightedCommandIndex = _memoryF2.HighlightedCommandIndex = _memoryF3.HighlightedCommandIndex = null;

      _thread = null;
      OnWorkingChanged();
      OnExecutionFinished(result);
    }

    /// <summary>
    /// Runs instructions in the given CommandMemory/function
    /// </summary>
    /// <param name="mem"></param>
    /// <returns></returns>
    private InstructionResult RunInstructions(CommandMemory mem)
    {
      InstructionResult ret = InstructionResult.Finished;

      for (int x = 0; x < mem.MaxCommands; x++)
      {
        mem.HighlightedCommandIndex = x;

        HighlightType condition = mem.Memory[x].Condition;
        CommandType command = mem.Memory[x].CommandType;

        if (command == CommandType.Empty) continue;

        if (condition == HighlightType.None || condition == _tiles[_player.Location.X, _player.Location.Y].Flare)
        {
          switch (command)
          {
            case CommandType.Go:
              {
                _player.MoveForward();
                ret = EvaluateMove();
                break;
              }
            case CommandType.RotateLeft:
              {
                _player.Rotate(-1);
                break;
              }
            case CommandType.RotateRight:
              {
                _player.Rotate(1);
                break;
              }
            case CommandType.PaintRed:
              {
                _tiles[_player.Location.X, _player.Location.Y].Flare = HighlightType.Red;
                break;
              }
            case CommandType.PaintGreen:
              {
                _tiles[_player.Location.X, _player.Location.Y].Flare = HighlightType.Green;
                break;
              }
            case CommandType.PaintBlue:
              {
                _tiles[_player.Location.X, _player.Location.Y].Flare = HighlightType.Blue;
                break;
              }
            case CommandType.F1:
              {
                mem.HighlightedCommandIndex = null;
                ret = RunInstructions(_memoryF1);
                break;
              }
            case CommandType.F2:
              {
                mem.HighlightedCommandIndex = null;
                ret = RunInstructions(_memoryF2);
                break;
              }
            case CommandType.F3:
              {
                mem.HighlightedCommandIndex = null;
                ret = RunInstructions(_memoryF3);
                break;
              }
          }

          if (ret != InstructionResult.Finished) return ret;

          if (command != CommandType.F1 && command != CommandType.F2 && command != CommandType.F3)
          {
            ret = Sleep();
            if (ret != InstructionResult.Finished) return ret;
          }
        }
      }

      return ret;
    }

    /// <summary>
    /// Waits for number of milliseconds defined by 'Delay' value.
    /// Used for delaying instructions. Responds to _shouldAbort flag
    /// </summary>
    /// <returns></returns>
    private InstructionResult Sleep()
    {
      for (int x = 0; x <= Delay; x += 10)
      {
        if (_shouldAbort) return InstructionResult.Cancelled;
        System.Threading.Thread.Sleep(10);
      }
      return InstructionResult.Finished;
    }

    private InstructionResult EvaluateMove()
    {
      if (_player.Location.X < 0 || _player.Location.Y < 0 || _player.Location.X > 15 || _player.Location.Y > 15) return InstructionResult.Death;
      
      Tile currentTile = _tiles[_player.Location.X, _player.Location.Y];
      if (currentTile.Lava) return InstructionResult.Death;
      if (currentTile.HasOre)
      {
        currentTile.HasOre = false;
        _remainingOre--;
        if (_remainingOre == 0) return InstructionResult.AllOreGathered;
      }

      return InstructionResult.Finished;
    }

  }
}
