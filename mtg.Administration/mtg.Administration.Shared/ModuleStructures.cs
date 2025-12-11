using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace mtg.Administration.Structures.Module
{
  partial class PeriodResult
  {
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int Month { get; set; }
  }
  
  /// <summary>
  /// Информация для одной серии на диаграмме виджета.
  /// </summary>
  partial class WidgetSeriesInfo
  {
    public Guid Guid { get; set; }
    public string Title { get; set; }
    public int Count { get; set; }
    public string Color { get; set; }
  }
  
}