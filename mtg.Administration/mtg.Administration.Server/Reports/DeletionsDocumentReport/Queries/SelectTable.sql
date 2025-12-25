select *
from Sungero_Reports_DeletionsEntity
where ReportSessionId = @ReportSessionId
ORDER BY SourceType, Date ASC