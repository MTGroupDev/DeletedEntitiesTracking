using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Metadata;
using Sungero.Domain.Shared;
using mtg.Administration.Structures;
using mtg.Administration.Structures.DeletionsDocumentReport;
using System.Collections.Generic;

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

      int offset = 0;
      List<Structures.DeletionsDocumentReport.EntityInfo> batch;

      do
      {
        batch = LoadRawDeletedEntities(startDate, endDate, kind, offset, Constants.Module.PageSize);

        foreach (var raw in batch)
        {
          if (string.IsNullOrWhiteSpace(raw.EntityGuid))
            continue;

          var entity = MapRawToEntityInfo(raw);
          EnrichEntityInfo(entity);
          deletionsObjectsInfo.Add(entity);
        }

        offset += batch.Count;

      } while (batch.Count == Constants.Module.PageSize);

      Logger.DebugFormat("GetAllRelationsFromDB. Done. Objects count {0} for period {1} - {2}. Kind: {3}.", deletionsObjectsInfo.Count(), startDate, endDate, kind);

      return deletionsObjectsInfo;
    }
    
    /// <summary>
    /// Выполнить SQL-запрос и вернуть сырые данные по удалённым сущностям.
    /// </summary>
    public List<Structures.DeletionsDocumentReport.EntityInfo> LoadRawDeletedEntities(string startDate, string endDate, string kind, int offset, int limit)
    {
      var list = new List<Structures.DeletionsDocumentReport.EntityInfo>();

      using (var connection = SQL.GetCurrentConnection())
        using (var command = connection.CreateCommand())
      {
        command.CommandText = string.Format(Queries.DeletionsDocumentReport.GetDeletedEntity, startDate, endDate, kind, offset, limit);

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            list.Add(new Structures.DeletionsDocumentReport.EntityInfo
                     {
                       EntityId = reader.GetInt64(0),
                       EntityName = reader.GetString(1),
                       EmployeeName = reader.GetString(2),
                       HostName = reader.GetString(3),
                       Date = reader.GetDateTime(4),
                       EntityGuid = reader.GetString(5),
                       Entitytypename = reader.GetString(6)
                     });
          }
        }
      }

      return list;
    }
    
    /// <summary>
    /// Преобразовать сырые данные БД в EntityInfo (без бизнес-логики).
    /// </summary>
    public Structures.DeletionsDocumentReport.EntityInfo MapRawToEntityInfo(Structures.DeletionsDocumentReport.EntityInfo raw)
    {
      var entity = Structures.DeletionsDocumentReport.EntityInfo.Create();

      entity.EntityId = raw.EntityId;
      entity.EntityName = raw.EntityName;
      entity.EmployeeName = raw.EmployeeName;
      entity.HostName = raw.HostName;
      entity.Date = raw.Date;
      entity.EntityGuid = raw.EntityGuid;
      entity.Entitytypename = raw.Entitytypename;

      return entity;
    }

    /// <summary>
    /// Дополнить информацию типом сущности.
    /// </summary>
    public void EnrichEntityInfo(Structures.DeletionsDocumentReport.EntityInfo entity)
    {
      entity.IsCollection = IsCollectionEntity(entity.EntityGuid);
      entity.TypeName = GetEntityDisplayName(entity.EntityGuid, entity.IsCollection);
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
      var counts = CountEntitiesByType(entities);

      foreach (var info in entities)
      {
        var row = CreateTableRow(info, reportSessionId, counts);
        tableRows.Add(row);
      }

      return tableRows;
    }
    
    /// <summary>
    /// Подсчет количества удалённых объектов каждого типа.
    /// </summary>
    /// <param name="entities">Список сущностей, полученных из БД.</param>
    /// <returns>Словарь: тип сущности, количество.</returns>
    public System.Collections.Generic.Dictionary<string, int> CountEntitiesByType(List<Structures.DeletionsDocumentReport.EntityInfo> entities)
    {
      return entities.GroupBy(e => e.Entitytypename).ToDictionary(g => g.Key, g => g.Count());
    }
    
    /// <summary>
    /// Определить текстовое значение SourceType для строки отчёта.
    /// </summary>
    /// <param name="info">Информация об удалённой сущности.</param>
    /// <param name="counts">Подсчитанное количество объектов по типам.</param>
    /// <returns>Строка формата "Тип (Количество)".</returns>
    public string GetSourceTypeForEntity(Structures.DeletionsDocumentReport.EntityInfo info,
                                         System.Collections.Generic.Dictionary<string, int> counts)
    {
      if (!counts.ContainsKey(info.Entitytypename))
        return string.Empty;

      return mtg.Administration.Reports.Resources.DeletionsDocumentReport.CountObjectsFormat(info.Entitytypename, counts[info.Entitytypename]);
    }
    
    /// <summary>
    /// Сформировать строку отчёта по одной удалённой сущности.
    /// </summary>
    /// <param name="info">Информация об удалённом объекте.</param>
    /// <param name="reportSessionId">Идентификатор сессии отчёта.</param>
    /// <param name="counts">Подсчитанное количество объектов по типам.</param>
    /// <returns>Строка таблицы отчёта.</returns>
    public mtg.Administration.Structures.DeletionsDocumentReport.TableRow CreateTableRow(Structures.DeletionsDocumentReport.EntityInfo info,
                                                                                         string reportSessionId,
                                                                                         System.Collections.Generic.Dictionary<string, int> counts)
    {
      var row = mtg.Administration.Structures.DeletionsDocumentReport.TableRow.Create();

      row.ReportSessionId = reportSessionId;
      row.DocumentId = info.EntityId;
      row.DocumentName = info.EntityName;
      row.Employee = info.EmployeeName;
      row.HostName = info.HostName;
      row.Date = info.Date;
      row.EntityType = info.TypeName;
      row.SourceType = GetSourceTypeForEntity(info, counts);

      return row;
    }


    /// <summary>
    /// Добавить в виджет данные по удалённым объектам для отображения на диаграмме.
    /// </summary>
    /// <param name="e">Аргументы события построения диаграммы виджета.</param>
    /// <param name="tableRows">Строки отчёта с информацией об удалённых объектах.</param>
    public virtual void AddDeletedObjectsSeries(Sungero.Domain.GetWidgetBarChartValueEventArgs e, List<mtg.Administration.Structures.DeletionsDocumentReport.TableRow> tableRows)
    {
      var resources = mtg.Administration.Reports.Resources.DeletionsDocumentReport;

      var counts = new List<Structures.Module.WidgetSeriesInfo>
      {
        new Structures.Module.WidgetSeriesInfo
        {
          Guid = mtg.Administration.Constants.Module.DocumentsGuid,
          Title = resources.DocumentTitle,
          Count = tableRows.Count(x => x.SourceType.Contains(resources.DocumentTitle)),
          Color = Constants.Module.ColorDocuments
        },
        new Structures.Module.WidgetSeriesInfo
        {
          Guid = mtg.Administration.Constants.Module.DatabookGuid,
          Title = resources.DatabookTitle,
          Count = tableRows.Count(x => x.SourceType.Contains(resources.DatabookTitle)),
          Color = Constants.Module.ColorDatabooks
        },
        new Structures.Module.WidgetSeriesInfo
        {
          Guid = mtg.Administration.Constants.Module.TaskGuid,
          Title = resources.TaskTitle,
          Count = tableRows.Count(x => x.SourceType.Contains(resources.TaskTitle)),
          Color = Constants.Module.ColorTasks
        }
      };

      foreach (var item in counts.Where(c => c.Count > 0))
      {
        var series = e.Chart.AddNewSeries(item.Guid.ToString(), item.Title);
        series.AddValue(item.Guid.ToString(), item.Title, item.Count, Sungero.Core.Colors.Parse(item.Color));
      }
    }
    
    /// <summary>
    /// Возвращает параметры периода.
    /// </summary>
    /// <param name="period">Название периода.</param>
    /// <returns>Структура с начальной датой, конечной датой и числом месяцев.</returns>
    [Remote]
    public Structures.Module.PeriodResult GetPeriod(string period)
    {
      var result = new Structures.Module.PeriodResult
      {
        Start = Calendar.Today.BeginningOfMonth(),
        End = Calendar.Today.EndOfMonth().NextDay(),
        Month = 0
      };

      switch (period)
      {
        case mtg.Administration.Constants.Module.MonthName:
          result.Start = Calendar.Today.BeginningOfMonth();
          break;
        case mtg.Administration.Constants.Module.ThreeMonthsName:
          result.Month = Constants.Module.QuarterMonths;
          break;
        case mtg.Administration.Constants.Module.HalfYearName:
          result.Month = Constants.Module.HalfYearMonths;
          break;
        case mtg.Administration.Constants.Module.YearName:
          result.Month = Constants.Module.YearMonths;
          break;
      }

      if (result.Month > 0)
        result.Start = mtg.Administration.PublicFunctions.Module.Remote.CalculateMonth(result.Month);

      return result;
    }

  }
}