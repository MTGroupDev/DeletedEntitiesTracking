using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace mtg.Administration.Structures.DeletionsDocumentReport
{
  /// <summary>
  /// Структура отчета.
  /// </summary>
  partial class TableRow
  {
    public string ReportSessionId { get; set; }
    
    public long DocumentId { get; set; }
    
    public string DocumentName { get; set; }
    
    public string Employee { get; set; }
    
    public string HostName { get; set; }
    
    public DateTime Date { get; set; }
    
    public string EntityType { get; set; }
    
    public string SourceType { get; set; }
  }
  
  /// <summary>
  /// Информация сущности
  /// </summary>
  partial class EntityInfo
  {
    public long EntityId { get; set; }
    
    public string EntityName { get; set; }
    
    public string EmployeeName { get; set; }
    
    public string HostName { get; set; }
    
    public DateTime Date { get; set; }
    
    public string EntityGuid { get; set; }
    
    public string TypeName { get; set; }
    
    public string Entitytypename { get; set; }
    
    public bool IsCollection { get; set; }
  }
}