using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace mtg.Administration.Client
{
  partial class DeletedObjectsWidgetHandlers
  {

    public virtual void ExecuteDeletedObjectsReportAction(Sungero.Domain.Client.ExecuteWidgetBarChartActionEventArgs e)
    {
      // Получение тип объекта.
      Guid sid;
      if (Guid.TryParse(e.SeriesId, out sid))
      {
        var report = mtg.Administration.Reports.GetDeletionsDocumentReport();

        // Получаем диапазон дат по выбранному периоду
        var periodInfo = Administration.Functions.Module.Remote.GetPeriod(_parameters.Period.Value);
        
        report.StartDate = periodInfo.Start;
        report.EndDate =  periodInfo.End;
        
        if (sid == mtg.Administration.Constants.Module.DocumentsGuid)
          report.Kind = mtg.Administration.Reports.Resources.DeletionsDocumentReport.DocumentTitle;
        else if (sid == mtg.Administration.Constants.Module.DatabookGuid)
          report.Kind = mtg.Administration.Reports.Resources.DeletionsDocumentReport.DatabookTitle;
        else if (sid == mtg.Administration.Constants.Module.TaskGuid)
          report.Kind = mtg.Administration.Reports.Resources.DeletionsDocumentReport.TaskTitle;
        

        report.ExportFormat = ReportExportFormat.Excel;
        report.Open();
      }
    }

  }
}