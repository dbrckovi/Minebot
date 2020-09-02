using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pyomm
{
  public class InputHandler
  {
    private Point _lastMouseLocation = new Point(0, 0);
    private bool _initialScanComplete = false;
    private Dictionary<Keys, bool> _keyPressed = new Dictionary<Keys, bool>();  //used to determine key press/release events
    private Dictionary<MouseButton, bool> _mousePressed = new Dictionary<MouseButton, bool>();    //used to determine mouse press/release events

    public delegate void KeyDelegate(Keys key, bool shift);
    public delegate void MouseButtonDelegate(MouseButton button, int x, int y);
    public delegate void PointDelegate(Point p);

    public event KeyDelegate KeyPressed;
    private void OnKeyPressed(Keys key, bool shift)
    {
      KeyPressed?.Invoke(key, shift);
    }

    public event KeyDelegate KeyReleased;
    private void OnKeyReleased(Keys key, bool shift)
    {
      KeyReleased?.Invoke(key, shift);
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

    public event PointDelegate MouseMoved;
    private void OnMouseMoved(Point p)
    {
      MouseMoved?.Invoke(p);
    }

    public void Update()
    {
      MouseState mouse = Mouse.GetState();
      KeyboardState keyboard = Keyboard.GetState();

      if (mouse.X != _lastMouseLocation.X && mouse.Y != _lastMouseLocation.Y)
      {
        _lastMouseLocation.X = mouse.X;
        _lastMouseLocation.Y = mouse.Y;
        OnMouseMoved(_lastMouseLocation);
      }

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
        bool shiftPressed = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
          if (_keyPressed[key] != keyboard.IsKeyDown(key))
          {
            _keyPressed[key] = !_keyPressed[key];
            if (_keyPressed[key]) OnKeyPressed(key, shiftPressed);
            else OnKeyReleased(key, shiftPressed);
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
