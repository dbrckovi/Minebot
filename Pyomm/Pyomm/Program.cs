using System;

namespace Pyomm
{
  public static class Program
  {
    [STAThread]
    static void Main()
    {
      using (GamePyomm game = new GamePyomm())
      {
        game.Run();
      }
    }
  }
}
