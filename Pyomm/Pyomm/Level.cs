using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pyomm
{
  public class Level
  {
    public Tile[,] Tiles = new Tile[16, 16];
    public PlayerInfo PlayerStart = new PlayerInfo();
    public int MainMemory = 10;
    int F1Memory = 5;
    int F2Memory = 5;
    int F3Memory = 5;
    List<Command> AllowedCommands = new List<Command>();

    public Level(string levelSpecification)
    {
      LoadLevel(levelSpecification);
    }

    private void LoadLevel(string levelSpecification)
    {
      int currentRow = 0;
      bool loadingSettings = false;
      string[] lines = levelSpecification.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
      foreach (string line in lines)
      {
        if (line.StartsWith(';')) continue;
        else if (line == "[TILES]") loadingSettings = false;
        else if (line == "[SETTINGS]") loadingSettings = true;
        else if (loadingSettings)
        {
          LoadSettingLine(line);
        }
        else
        {
          LoadTileLine(line, currentRow);
          currentRow++;
        }
      }
    }

    private void LoadTileLine(string line, int currentRow)
    {
      char[] chars = line.ToCharArray();
      for (int x = 0; x < chars.Length; x++)
      {
        Tile tile = new Tile(x, currentRow);
        switch(chars[x])
        {
          case 'X': tile.Lava = true; break;
          case 'O': tile.HasOre = true; break;
          case 'r': tile.Flare = HighlightType.Red; break;
          case 'g': tile.Flare = HighlightType.Green; break;
          case 'b': tile.Flare = HighlightType.Blue; break;
          case 'R': tile.Flare = HighlightType.Red; tile.HasOre = true; break;
          case 'G': tile.Flare = HighlightType.Green; tile.HasOre = true; break;
          case 'B': tile.Flare = HighlightType.Blue; tile.HasOre = true; break;
        }
        Tiles[x, currentRow] = tile;
      }
    }

    private void LoadSettingLine(string line)
    {
      string[] parts = line.Split('=');
      string key = parts[0];
      string value = parts[1];
      switch (key)
      {
        case "PlayerStart":
          {
            string[] coords = value.Split(',');
            PlayerStart.Location = new Point(int.Parse(coords[0]), int.Parse(coords[1]));
            break;
          }
        case "PlayerDirection":
          {
            PlayerStart.Direction = (Direction)int.Parse(value);
            break;
          }
        case "MainMemory": MainMemory = int.Parse(value); break;
        case "F1Memory": F1Memory = int.Parse(value); break;
        case "F2Memory": F2Memory = int.Parse(value); break;
        case "F3Memory": F3Memory = int.Parse(value); break;
        case "AllowedCommands":
          {
            string[] commands = value.Split(',');
            foreach (string command in commands)
            {
              AllowedCommands.Add(ParseCommand(command));
            }
            break;
          }
        case "SolutionMain":
          {
            //TODO: parse
            break;
          }
        case "SolutionF1":
          {
            //TODO: parse
            break;
          }
        case "SolutionF2":
          {
            //TODO: parse
            break;
          }
        case "SolutionF3":
          {
            //TODO: parse
            break;
          }
      }
    }

    private Command ParseCommand(string cmd)
    {
      switch(cmd)
      {
        case "RL": return Command.RotateLeft;
        case "RR": return Command.RotateRight;
        case "GO": return Command.Go;
        case "PR": return Command.PaintRed;
        case "PG": return Command.PaintGreen;
        case "PB": return Command.PaintBlue;
        default: throw new Exception("Unrecognized command");
      }
    }
  }
}
