using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace mtg.Administration.Server
{
  partial class DeletedObjectsWidgetHandlers
  {

    public virtual void GetDeletedObjectsReportValue(Sungero.Domain.GetWidgetBarChartValueEventArgs e)
    {
      var tableRows = new List<mtg.Administration.Structures.DeletionsDocumentReport.TableRow>();
      var relationshipsInfo = new List<Structures.DeletionsDocumentReport.EntityInfo>();
      var kind = mtg.Administration.Reports.Resources.DeletionsDocumentReport.AllSelect;
      
      // Получаем диапазон дат по выбранному периоду
      var periodInfo = Administration.Functions.Module.GetPeriod(_parameters.Period.Value);
      
      //Запрос в БД, для получения всех удалённых объектов.
      mtg.Administration.Functions.Module.GetAllRelationsFromDB(relationshipsInfo, periodInfo.Start.ToString("yyyy-MM-dd 00:00:00"), periodInfo.End.ToString("yyyy-MM-dd 00:00:00"), kind);

      var objectCount =  relationshipsInfo.Count();

      foreach (var info in relationshipsInfo)
      {
        var tableRow = mtg.Administration.Structures.DeletionsDocumentReport.TableRow.Create();

        tableRow.DocumentId= info.EntityId;
        tableRow.DocumentName = info.EntityName;
        tableRow.Employee = info.EmployeeName;
        tableRow.HostName = info.HostName;
        tableRow.Date = info.Date;
        tableRow.EntityType = info.TypeName;
        tableRow.SourceType = info.Entitytypename;

        tableRows.Add(tableRow);
      }
      
      var countDoc = tableRows.Count(x => x.SourceType == mtg.Administration.Reports.Resources.DeletionsDocumentReport.DocumentTitle);
      var countDataBook = tableRows.Count(x => x.SourceType == mtg.Administration.Reports.Resources.DeletionsDocumentReport.DatabookTitle);
      var countTask = tableRows.Count(x => x.SourceType == mtg.Administration.Reports.Resources.DeletionsDocumentReport.TaskTitle);
      
      if (countDoc > 0)
      {
        //Создание серии docSeries.
        var docSeries = e.Chart.AddNewSeries(mtg.Administration.Constants.Module.DocumentsGuid.ToString(), mtg.Administration.Reports.Resources.DeletionsDocumentReport.DocumentTitle);
        //Добавление значения серии.
        docSeries.AddValue(mtg.Administration.Constants.Module.DocumentsGuid.ToString(), mtg.Administration.Reports.Resources.DeletionsDocumentReport.DocumentTitle, countDoc, Colors.Charts.Color4);
      }

      if (countDataBook > 0)
      {
        //Создание серии docSeries.
        var dataBookSeries = e.Chart.AddNewSeries(mtg.Administration.Constants.Module.DatabookGuid.ToString(), mtg.Administration.Reports.Resources.DeletionsDocumentReport.DatabookTitle);
        //Добавление значения серии.
        dataBookSeries.AddValue(mtg.Administration.Constants.Module.DatabookGuid.ToString(), mtg.Administration.Reports.Resources.DeletionsDocumentReport.DatabookTitle, countDataBook, Colors.Charts.Color1);
      }
      
      if (countTask > 0)
      {
        //Создание серии docSeries.
        var taskSeries = e.Chart.AddNewSeries(mtg.Administration.Constants.Module.TaskGuid.ToString(), mtg.Administration.Reports.Resources.DeletionsDocumentReport.TaskTitle);
        //Добавление значения серии.
        taskSeries.AddValue(mtg.Administration.Constants.Module.TaskGuid.ToString(), mtg.Administration.Reports.Resources.DeletionsDocumentReport.TaskTitle, countTask, Colors.Charts.Color3);
      }
    }

  }

}