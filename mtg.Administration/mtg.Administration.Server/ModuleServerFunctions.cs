using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Metadata;
using Sungero.Domain.Shared;

namespace mtg.Administration.Server
{
  public partial class ModuleFunctions
  {
    /// <summary>
    /// Вычислить месяц начала периода.
    /// </summary>
    [Public, Remote]
    public static DateTime CalculateMonth(int month)
    {
      var startDates = Calendar.Today.BeginningOfMonth();
      
      for (int i = 0; i < month; i++)
        startDates = startDates.PreviousMonth();
      
      return startDates;
    }
    
    /// <summary>
    /// Получить сведения об удалённых сущностях за указанный период.
    /// </summary>
    /// <param name="startDate">Дата начала периода.</param>
    /// <param name="endDate">Дата окончания периода.</param>
    /// <param name="kind">Тип объектов для выборки.</param>
    /// <returns>Список структур с информацией об удалённых сущностях.</returns>
    public virtual List<Structures.DeletionsDocumentReport.EntityInfo> GetDeletedObjects(string startDate, string endDate, string kind)
    {
      var entities = new List<Structures.DeletionsDocumentReport.EntityInfo>();
      GetAllRelationsFromDB(entities, startDate, endDate, kind);
      return entities;
    }
    
    /// <summary>
    /// Получить все удалённые объекты из базы данных.
    /// </summary>
    /// <param name="relationshipsInfo">Структура удалённых объектов.</param>
    /// <param name="StartDate">Дата начала периода.</param>
    /// <param name="EndDate">Дата окончания периода</param>
    /// <param name="Kind">Тип объекта</param>
    /// <returns>Удаленные объекты</returns>
    public virtual List<Structures.DeletionsDocumentReport.EntityInfo> GetAllRelationsFromDB(List<Structures.DeletionsDocumentReport.EntityInfo> deletionsObjectsInfo, string startDate, string endDate, string kind)
    {
      Logger.Debug("GetAllRelationsFromDB. Start.");
      
      using (var connection = SQL.GetCurrentConnection())
      {
        using (var command = connection.CreateCommand())
        {
          command.CommandText = string.Format(Queries.DeletionsDocumentReport.GetDeletedEntity, startDate, endDate, kind);
          
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              var entityInfo = Structures.DeletionsDocumentReport.EntityInfo.Create();
              
              entityInfo.EntityId = reader.GetInt64(0);
              entityInfo.EntityName = reader.GetString(1);
              entityInfo.EmployeeName = reader.GetString(2);
              entityInfo.HostName = reader.GetString(3);
              entityInfo.Date = reader.GetDateTime(4);
              entityInfo.EntityGuid = reader.GetString(5);
              entityInfo.Entitytypename = reader.GetString(6);
              entityInfo.IsCollection = IsCollectionEntity(entityInfo.EntityGuid);
              entityInfo.TypeName = GetEntityDisplayName(entityInfo.EntityGuid, entityInfo.IsCollection);
              
              // Если не нашли GUID записи, то не обрабатываем.
              if (!string.IsNullOrWhiteSpace(entityInfo.EntityGuid))
                deletionsObjectsInfo.Add(entityInfo);
            }
          }
        }
      }
      
      Logger.DebugFormat("GetAllRelationsFromDB. Done. Objects count {0} for the period: {1} - {2}. Kind: {3}.", deletionsObjectsInfo.Count(), startDate, endDate, kind);
      
      return deletionsObjectsInfo;
    }
    
    /// <summary>
    /// Определить, является ли сущность коллекцией (по GUID'у).
    /// </summary>
    /// <param name="entityGuid">GUID сущности.</param>
    /// <returns>True - если сущность является коллекцией, иначе False.</returns>
    public static bool IsCollectionEntity(string entityGuid)
    {
      if (string.IsNullOrWhiteSpace(entityGuid))
        return false;
      
      var entityType = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(Guid.Parse(entityGuid));
      
      // TODO: Протестировать typeof() при адаптации на новые версии.
      return typeof(Sungero.Domain.Shared.IChildEntity).IsAssignableFrom(entityType);
    }
    
    /// <summary>
    /// Получить наименование типа сущности.
    /// </summary>
    /// <param name="entityGuid">GUID сущности.</param>
    /// <param name="isCollectionEntity">Признак, является ли сущность коллекцией.</param>
    /// <returns>Наименование типа сущности.</returns>
    public static string GetEntityDisplayName(string entityGuid, bool isCollectionEntity)
    {
      if (string.IsNullOrWhiteSpace(entityGuid))
        return string.Empty;
      
      var entityType = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(Guid.Parse(entityGuid));
      var metadata = Sungero.Domain.Shared.TypeExtension.GetEntityMetadata(entityType);
      
      if (isCollectionEntity)
      {
        var mainMetadata = metadata.MainEntityMetadata;
        
        if (mainMetadata != null)
          return mainMetadata.GetDisplayName();
      }
      
      return metadata.GetDisplayName();
    }
    
    /// <summary>
    /// Сформировать строки таблицы отчёта по удалённым объектам.
    /// </summary>
    /// <param name="entities">Список удалённых сущностей, полученных из бд.</param>
    /// <param name="reportSessionId">Ид сессии.</param>
    /// <returns>Список строк таблицы с информацией об удалённых объектах.</returns>
    public virtual List<mtg.Administration.Structures.DeletionsDocumentReport.TableRow> BuildDeletedObjectsTableRows(List<Structures.DeletionsDocumentReport.EntityInfo> entities, string reportSessionId)
    {
      var tableRows = new List<mtg.Administration.Structures.DeletionsDocumentReport.TableRow>();
      
      var countDoc = entities.Count(x => x.Entitytypename == mtg.Administration.Reports.Resources.DeletionsDocumentReport.DocumentTitle);
      var countDataBook = entities.Count(x => x.Entitytypename == mtg.Administration.Reports.Resources.DeletionsDocumentReport.DatabookTitle);
      var countTask = entities.Count(x => x.Entitytypename == mtg.Administration.Reports.Resources.DeletionsDocumentReport.TaskTitle);
      
      foreach (var info in entities)
      {
        var row = mtg.Administration.Structures.DeletionsDocumentReport.TableRow.Create();

        row.ReportSessionId = reportSessionId;
        
        row.DocumentId = info.EntityId;
        row.DocumentName = info.EntityName;
        row.Employee = info.EmployeeName;
        row.HostName = info.HostName;
        row.Date = info.Date;
        row.EntityType = info.TypeName;
        
        if (info.Entitytypename == mtg.Administration.Reports.Resources.DeletionsDocumentReport.DocumentTitle)
          row.SourceType = mtg.Administration.Reports.Resources.DeletionsDocumentReport.CountObjectsFormat(info.Entitytypename, countDoc);
        else if (info.Entitytypename == mtg.Administration.Reports.Resources.DeletionsDocumentReport.DatabookTitle)
          row.SourceType = mtg.Administration.Reports.Resources.DeletionsDocumentReport.CountObjectsFormat(info.Entitytypename, countDataBook);
        else if (info.Entitytypename == mtg.Administration.Reports.Resources.DeletionsDocumentReport.TaskTitle)
          row.SourceType = mtg.Administration.Reports.Resources.DeletionsDocumentReport.CountObjectsFormat(info.Entitytypename, countTask);

        tableRows.Add(row);
      }
      
      return tableRows;
    }

    /// <summary>
    /// Добавить в виджет данные по удалённым объектам для отображения на диаграмме.
    /// </summary>
    /// <param name="e">Аргументы события построения диаграммы виджета.</param>
    /// <param name="tableRows">Строки отчёта с информацией об удалённых объектах.</param>
    public virtual void AddDeletedObjectsSeries(Sungero.Domain.GetWidgetBarChartValueEventArgs e, List<mtg.Administration.Structures.DeletionsDocumentReport.TableRow> tableRows)
    {
      var resources = mtg.Administration.Reports.Resources.DeletionsDocumentReport;

      var counts = new[]
      {
        new { Guid = mtg.Administration.Constants.Module.DocumentsGuid, Title = resources.DocumentTitle, Count = tableRows.Count(x => x.SourceType.Contains(resources.DocumentTitle)), Color = Colors.Charts.Color4 },
        new { Guid = mtg.Administration.Constants.Module.DatabookGuid, Title = resources.DatabookTitle, Count = tableRows.Count(x => x.SourceType.Contains(resources.DatabookTitle)), Color = Colors.Charts.Color1 },
        new { Guid = mtg.Administration.Constants.Module.TaskGuid, Title = resources.TaskTitle, Count = tableRows.Count(x => x.SourceType.Contains(resources.TaskTitle)), Color = Colors.Charts.Color3 }
      };
      
      foreach (var item in counts.Where(c => c.Count > 0))
      {
        var series = e.Chart.AddNewSeries(item.Guid.ToString(), item.Title);
        series.AddValue(item.Guid.ToString(), item.Title, item.Count, item.Color);
      }
    }

  }
}