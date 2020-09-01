using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pyomm
{
  public class InputHandler
  {
    private bool _initialScanComplete = false;
    private Dictionary<Keys, bool> _keyPressed = new Dictionary<Keys, bool>();  //used to determine key press/release events
    private Dictionary<MouseButton, bool> _mousePressed = new Dictionary<MouseButton, bool>();    //used to determine mouse press/release events

    public delegate void KeyDelegate(Keys key);
    public delegate void MouseButtonDelegate(MouseButton button, int x, int y);

    public event KeyDelegate KeyPressed;
    private void OnKeyPressed(Keys key)
    {
      KeyPressed?.Invoke(key);
    }

    public event KeyDelegate KeyReleased;
    private void OnKeyReleased(Keys key)
    {
      KeyReleased?.Invoke(key);
    }

    public event MouseButtonDelegate MousePressed;
    private void OnMousePressed(MouseButton button, int x, int y)
    {
      MousePressed?.Invoke(button, x, y);
    }

    public event MouseButtonDelegate MouseReleased;
    private void OnMouseReleased(MouseButton button, int x, int y)
    {
      MouseReleased?.Invoke(button, x, y);
    }

    public void Update()
    {
      MouseState mouse = Mouse.GetState();
      KeyboardState keyboard = Keyboard.GetState();

      if (!_initialScanComplete)
      {
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
          _keyPressed.Add(key, keyboard.IsKeyDown(key));
        }

        foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
        {
          switch (button)
          {
            case MouseButton.Left: _mousePressed.Add(MouseButton.Left, mouse.LeftButton == ButtonState.Pressed); break;
            case MouseButton.Right: _mousePressed.Add(MouseButton.Right, mouse.RightButton == ButtonState.Pressed); break;
            case MouseButton.Middle: _mousePressed.Add(MouseButton.Middle, mouse.MiddleButton == ButtonState.Pressed); break;
          }
        }

        _initialScanComplete = true;
      }
      else
      {
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
          if (_keyPressed[key] != keyboard.IsKeyDown(key))
          {
            _keyPressed[key] = !_keyPressed[key];
            if (_keyPressed[key]) OnKeyPressed(key);
            else OnKeyReleased(key);
          }
        }

        foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
        {
          bool newMouseState = false;
          
          switch (button)
          {
            case MouseButton.Left: newMouseState = mouse.LeftButton == ButtonState.Pressed; break;
            case MouseButton.Right: newMouseState = mouse.RightButton == ButtonState.Pressed; break;
            case MouseButton.Middle: newMouseState = mouse.MiddleButton == ButtonState.Pressed; break;
          }

          if (_mousePressed[button] != newMouseState)
          {
            _mousePressed[button] = newMouseState;
            if (newMouseState) OnMousePressed(button, mouse.X, mouse.Y);
            else OnMouseReleased(button, mouse.X, mouse.Y);
          }
        }
      }
    }
  }
}
