using System;
using System.Collections.Generic;
using System.Text;

namespace Pyomm
{
  public class Command
  {
    public CommandType CommandType = CommandType.Empty;
    public HighlightType Condition = HighlightType.None;
  }
}
