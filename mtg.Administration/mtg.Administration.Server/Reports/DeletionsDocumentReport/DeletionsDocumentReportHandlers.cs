using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.Metadata;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;

namespace mtg.Administration
{
  partial class DeletionsDocumentReportServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.DeletionsDocumentReport.SourceTableName, DeletionsDocumentReport.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      Logger.Debug("DeletionsDocumentReport. Start.");
      
      var report = DeletionsDocumentReport;
      var entities = mtg.Administration.Functions.Module.GetDeletedObjects(report.StartDate.Value.ToString("yyyy-MM-dd 00:00:00"),
                                       report.EndDate.Value.NextDay().ToString("yyyy-MM-dd 00:00:00"),
                                       report.Kind);

      var tableRows = mtg.Administration.Functions.Module.BuildDeletedObjectsTableRows(entities, report.ReportSessionId);
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.DeletionsDocumentReport.SourceTableName, tableRows);
      
      Logger.Debug("DeletionsDocumentReport. Done.");
    }
   
  }
}