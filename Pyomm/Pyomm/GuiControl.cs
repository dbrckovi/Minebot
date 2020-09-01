using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pyomm
{
  public abstract class GuiControl
  {
    private bool _enabled = true;
    public Point Location = new Point(0, 0);
    public Point Size = new Point(10, 10);
    public abstract void Draw(SpriteBatch batch);

    public bool Enabled
    {
      get { return _enabled; }
      set { _enabled = value; }
    }

    public delegate void ControlDelegate (GuiControl control);
    public event ControlDelegate Click;
    private void OnClick()
    {
      Click?.Invoke(this);
    }

    public bool MouseHitTest (Point mouseLocation)
    {
      return mouseLocation.X >= Location.X
        && mouseLocation.Y >= Location.Y
        && mouseLocation.X <= Location.X + Size.X
        && mouseLocation.Y <= Location.Y + Size.Y;
    }

    /// <summary>
    /// Thiggers click if control is enabled
    /// </summary>
    public void TriggerClick()
    {
      if (_enabled) OnClick();
    }
  }
}
