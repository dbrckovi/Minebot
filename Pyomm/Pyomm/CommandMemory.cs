using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Pyomm
{
  public class CommandMemory : GuiControl
  {
    public Command[] Memory = new Command[16];

    int _maxCommands = 16;
    bool _editMode = true;
    public int? HighlightedCommandIndex = null;
    
    public int MaxCommands
    {
      get { return _maxCommands; }
      set
      {
        _maxCommands = value;
        if (_maxCommands > 16) _maxCommands = 16;
        if (_maxCommands < 0) _maxCommands = 0;
        
        if (_maxCommands < 16)
        {
          for (int x = _maxCommands; x < 16; x++)
          {
            SetCommand(x, CommandType.Empty);
          }
        }
        
      
      }
    }

    public bool EditMode
    {
      get { return _editMode; }
      set { _editMode = value; }
    }

    public string Text;

    public delegate void IntDelegate(int x);
    public event IntDelegate CommandHighlighted;
    private void OnCommandHighlighted(int x)
    {
      CommandHighlighted?.Invoke(x);
    }

    public CommandMemory(Point location)
    {
      Location = location;
      Size = new Point(64, 338);
      for (int x = 0; x < 16; x++)
      {
        Memory[x] = new Command();
      }
    }

    public override void TriggerClick(Point location)
    {
      if (!Enabled) return;
      if (location.Y > this.Location.Y + 20 && location.Y < this.Location.Y + this.Size.Y)
      {
        int index = (location.Y - (this.Location.Y + 20)) / 20;
        if (index < MaxCommands)
        {
          HighlightedCommandIndex = index;
          OnCommandHighlighted(index);
        }
      }
    }

    public void SetCommand(int index, CommandType commandType)
    {
      if (index < _maxCommands)
      {
        Memory[index].CommandType = commandType;
        if (commandType == CommandType.Empty) Memory[index].Condition = HighlightType.None;
      }
    }

    public void SetCondition(int index, HighlightType condition)
    {
      if (index < _maxCommands) Memory[index].Condition = condition;
    }

    public override void Draw(SpriteBatch batch)
    {
      batch.DrawString(Asset.buttonFont, Text, new Vector2(Location.X, Location.Y), Color.White);
      for (int x = 0; x < MaxCommands; x++)
      {
        batch.Draw(Asset.commandBackground, new Vector2(Location.X, Location.Y + 20 + x * 20), Color.White);
        if (HighlightedCommandIndex.HasValue && HighlightedCommandIndex.Value == x)
        {
          batch.Draw(Asset.commandHighlight, new Vector2(Location.X, Location.Y + 20 + x * 20), Color.White);
        }

        #region Draw condition
        if (Memory[x].Condition != HighlightType.None)
        {
          Color conditionColor = Color.Black;
          string conditionText = "";

          switch (Memory[x].Condition)
          {
            case HighlightType.Red:
              {
                conditionColor = Color.LightCoral;
                conditionText = "R";
                break;
              }
            case HighlightType.Green:
              {
                conditionColor = Color.LightGreen;
                conditionText = "G";
                break;
              }
            case HighlightType.Blue:
              {
                conditionColor = Color.LightBlue;
                conditionText = "B";
                break;
              }
          }

          batch.DrawString(Asset.buttonFont, conditionText, new Vector2(Location.X + 8, Location.Y + 23 + x * 20), conditionColor);
        }
        #endregion Draw condition

        #region Draw command
        if (Memory[x].CommandType != CommandType.Empty)
        {
          Color commandColor = Color.White;

          switch (Memory[x].CommandType)
          {
            case CommandType.PaintRed: commandColor = Color.LightCoral; break;
            case CommandType.PaintGreen: commandColor = Color.LightGreen; break;
            case CommandType.PaintBlue: commandColor = Color.LightBlue; break;
          }

          batch.DrawString(Asset.buttonFont, Utility.GetCommand(Memory[x].CommandType), new Vector2(Location.X + 28, Location.Y + 23 + x * 20), commandColor);
        }
        #endregion Draw command
      }
    }
  }
}
