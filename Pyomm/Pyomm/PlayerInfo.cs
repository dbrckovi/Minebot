using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Pyomm
{
  public class PlayerInfo
  {
    public Direction Direction = Direction.UpLeft;
    public Point Location = new Point(0, 0);

    /// <summary>
    /// Rotates the player one step to either direction
    /// </summary>
    /// <param name="rotationDirection">Positive value: Right. Negative value: Left</param>
    public void Rotate(int rotationDirection)
    {
      if (rotationDirection < -1) rotationDirection = -1;
      else if (rotationDirection > 1) rotationDirection = 1;

      int newValue = (int)this.Direction + rotationDirection;
      if (newValue > 5) newValue = 0;
      else if (newValue < 0) newValue = 5;

      this.Direction = (Direction)newValue;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
      Vector2 tileLocationInWorld = Tile.TileIndexToWorld(Location.X, Location.Y);
      Vector2 tileCenter = new Vector2(Tile.DefaultSize.X / 2, Tile.DefaultSize.Y / 2);
      float a = (int)Direction * MathHelper.TwoPi / 6 - MathHelper.TwoPi / 4;
      spriteBatch.Draw(Asset.tileMiner, tileLocationInWorld, null, Color.White, a, tileCenter, Vector2.One, SpriteEffects.None, 0f);
    }

    public void MoveForward()
    {
      Location = GetForwardLocation();
    }

    public Point GetForwardLocation()
    {
      /*
          ...tile layout...
  	         0,0   1,0   2,0   3,0
          0,1   1,1   2,1   3,1
	           0,2   1,2   2,2   3,2
          0,3   1,3   2,3   3,3
	           0,4   1,4   2,4   3,4 
       
          ...forward rules...

       			        Even			Odd
          Left		  X-1				X-1
          UpLeft	  Y-1			  X-1,Y-1
          UpRight	  X+1,Y-1				Y-1
          Right		  X+1				X+1
          DownRight	X+1,Y+1				Y+1
          DownLeft	Y+1			  X-1,Y+1
       */

      Point ret = Location;
      bool onEvenLine = Location.Y % 2 == 0;

      if (Direction == Direction.UpLeft || Direction == Direction.UpRight) ret.Y -= 1;
      else if (Direction == Direction.DownLeft || Direction == Direction.DownRight) ret.Y += 1;

      if (Direction == Direction.Left
         || (Direction == Direction.UpLeft && !onEvenLine)
         || (Direction == Direction.DownLeft && !onEvenLine))
      {
        ret.X -= 1;
      }
      else if (Direction == Direction.Right
        || (Direction == Direction.UpRight && onEvenLine)
        || (Direction == Direction.DownRight && onEvenLine))
      {
        ret.X += 1;
      }

      return ret;
    }
  }
}
