using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace mtg.Administration
{
  partial class DeletionsDocumentReportClientHandlers
  {
    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      DeletionsDocumentReport.ReportSessionId = Guid.NewGuid().ToString();
      if (DeletionsDocumentReport.StartDate == null && DeletionsDocumentReport.EndDate == null && DeletionsDocumentReport.Kind == null)
      {
        var modal = Dialogs.CreateInputDialog(mtg.Administration.Reports.Resources.DeletionsDocumentReport.EnterReportPeriod);
        var startDate = modal.AddDate(mtg.Administration.Reports.Resources.DeletionsDocumentReport.StartDate, true, Calendar.Today.BeginningOfMonth());
        var endDate = modal.AddDate(mtg.Administration.Reports.Resources.DeletionsDocumentReport.EndDate, true, Calendar.Today.EndOfMonth());
        var  type = modal.AddSelect(mtg.Administration.Reports.Resources.DeletionsDocumentReport.TypeHeader, true, 0)
          .From(mtg.Administration.Reports.Resources.DeletionsDocumentReport.AllSelect, mtg.Administration.Reports.Resources.DeletionsDocumentReport.DocumentTitle, mtg.Administration.Reports.Resources.DeletionsDocumentReport.DatabookTitle, mtg.Administration.Reports.Resources.DeletionsDocumentReport.TaskTitle);
        var  format = modal.AddSelect(mtg.Administration.Reports.Resources.DeletionsDocumentReport.ReportFormatName, true, 1).From(ReportExportFormat.Word.ToString(), ReportExportFormat.Excel.ToString(), ReportExportFormat.Pdf.ToString());
        
        if (modal.Show() == DialogButtons.Ok)
        {
          DeletionsDocumentReport.StartDate = startDate.Value;
          DeletionsDocumentReport.EndDate = endDate.Value;
          DeletionsDocumentReport.Kind = type.Value;
          
          if (format.Value.Equals(ReportExportFormat.Word.ToString()))
            DeletionsDocumentReport.ExportFormat = ReportExportFormat.Word;
          
          if (format.Value.Equals(ReportExportFormat.Excel.ToString()))
            DeletionsDocumentReport.ExportFormat = ReportExportFormat.Excel;
          
          if (format.Value.Equals(ReportExportFormat.Pdf.ToString()))
            DeletionsDocumentReport.ExportFormat = ReportExportFormat.Pdf;
        }
        else
          e.Cancel = true;
      }
    }
  }
}