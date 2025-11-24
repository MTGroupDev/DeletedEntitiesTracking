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
        var startDate = Calendar.Today.BeginningOfMonth();
        var endDate = Calendar.Today.EndOfMonth();
        var month = 0;
        
        if (_parameters.Period.Value == mtg.Administration.Constants.Module.MonthName)
          startDate = Calendar.Today.BeginningOfMonth();
        else if (_parameters.Period.Value == mtg.Administration.Constants.Module.ThreeMonthsName)
          month = 2;
        else if (_parameters.Period.Value == mtg.Administration.Constants.Module.HalfYearName)
          month = 5;
        else if (_parameters.Period.Value == mtg.Administration.Constants.Module.YearName)
          month = 11;
        
        if (month > 0)
          startDate = mtg.Administration.PublicFunctions.Module.Remote.CalculateMonth(month);
        
        report.StartDate = startDate;
        report.EndDate =  endDate;
        
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