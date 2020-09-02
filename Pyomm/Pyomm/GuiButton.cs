using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pyomm
{
  public class GuiButton : GuiControl
  {
    private Color _enabledColor = Color.Black;
    private Color _disabledColor = Color.Gray;
    private Color _mouseOverColor = Color.Blue; //TODO: mouse over

    private string _text = "Button";

    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }

    public GuiButton(Point location)
    {
      Location = location;
      Size = new Point(Asset.buttonBackground.Width, Asset.buttonBackground.Height);
    }

    public override void Draw(SpriteBatch batch)
    {
      if (!Visible) return;
      batch.Draw(Asset.buttonBackground, new Vector2(Location.X, Location.Y), Color.White);
      batch.DrawString(Asset.buttonFont, Text, new Vector2(Location.X + 10, Location.Y + 10), this.Enabled ? _enabledColor : _disabledColor);
    }
  }
}
