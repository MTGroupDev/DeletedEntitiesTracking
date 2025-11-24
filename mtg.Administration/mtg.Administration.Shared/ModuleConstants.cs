using System;
using Sungero.Core;

namespace mtg.Administration.Constants
{
  public static class Module
  {
    // Наименование периода для виджета - "Год".
    public const string YearName = "Year";
    
    // Наименование периода для виджета - "Пол года".
    public const string HalfYearName = "HalfYear";
    
    // Наименование периода для виджета - "Три месяца".
    public const string ThreeMonthsName = "ThreeMonths";
    
    // Наименование периода для виджета - "Месяц".
    public const string MonthName = "Month";
    
    /// <summary>
    /// GUID Типа "Документы" для виджета.
    /// </summary>
    public static readonly Guid DocumentsGuid = new Guid("5A6D5EE2-516E-47B8-824E-5B82C0366F99");
    
    /// <summary>
    /// GUID Типа "Записи справочников" для виджета.
    /// </summary>
    public static readonly Guid DatabookGuid = new Guid("5DE18B37-3653-452C-A9CE-FC4BEF1B8189");
    
    /// <summary>
    /// GUID Типа "Задачи" для виджета.
    /// </summary>
    public static readonly Guid TaskGuid = new Guid("30a986af-916b-4088-bfb0-b9aa378e0a5a");
  }
}