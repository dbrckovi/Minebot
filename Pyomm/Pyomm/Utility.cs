using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pyomm
{
  public static class Utility
  {
    private static Dictionary<CommandType, string> _commandTypeCodes = new Dictionary<CommandType, string>()
    {
      { CommandType.RotateLeft, "RL" },
      { CommandType.RotateRight, "RR" },
      { CommandType.Go, "GO" },
      { CommandType.PaintRed, "PR" },
      { CommandType.PaintGreen, "PG" },
      { CommandType.PaintBlue, "PB" },
      { CommandType.F1, "F1" },
      { CommandType.F2, "F2" },
      { CommandType.F3, "F3" }
    };

    public static CommandType GetCommand(string commandCode)
    {
      foreach (KeyValuePair<CommandType, string> pair in _commandTypeCodes)
      {
        if (commandCode == pair.Value) return pair.Key;
      }

      throw new Exception($"Command code {commandCode} does not exist");
    }

    public static string GetCommand(CommandType commandType)
    {
      return _commandTypeCodes[commandType];
    }

    public static void DrawHelp(SpriteBatch batch)
    {
      string text = @"
Welcome to PYOMM - Programm your own mining machine

PROGRAMMING THE MINER:
The goal of the game is to program the miner to collect all ore pieces from each level.
To achieve this, insert the instructions into memory slots on the right. Simply select the slot, and press one of the instruction buttons below.
The instruction code will appear in the right part of the memory slot.

MEMORY SLOTS AND FUNCTIONS:
Memory slots are organized in four columns (functions).
When machine is started, it will start executing instructions from the main function from top to bottom until the end of main function is reached.
Other functions can be called by using appropriate 'Call function' instructions.

CONDITIONS:
Each instruction can also have a condition (left part of the memory slot)
There are 3 conditions: RED, GREEN, BLUE. 
Instructions with conditions will be executed only if miner is currently on the tile of the same color.
For example, an instruction with BLUE condition will be executed only if miner is currently standing on the blue tile.

KEYBOARD:
Instructions and conditions can be entered using keyboard:
W, Up     -> Go forward
A, Left   -> Rotate left
D, Right  -> Rotate right
Shift + R, Shift + G, Shift + B -> Paint current tile RED, GREEN or BLUE
F1,F2,F3  -> Call functions 1, 2 or 3
R, G, B   -> Add RED, GREEN or BLUE condition
Delete    -> Clear instruction and condition
Backspace -> Clear condition

EDITOR MODE:
Left click -> select the tile
Right click -> set or remove lava
Middle mouse click -> set or remove ore
Space       -> set machine starting position, or rotate the machine if it's already on that tile
R, G, B     -> paint the tile RED, GREEN or BLUE
N           -> Remove color from tile
";

      string[] lines = text.Split(Environment.NewLine);

      for (int x = 0; x <= lines.GetUpperBound(0); x++)
      {
        batch.DrawString(Asset.buttonFont, lines[x], new Vector2(20, x * 14), Color.White);
      }
    }
  }
}
