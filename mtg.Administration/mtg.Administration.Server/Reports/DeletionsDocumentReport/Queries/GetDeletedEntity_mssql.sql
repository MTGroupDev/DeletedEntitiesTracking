CREATE TABLE #tmp_result_table (
    DocumentId BIGINT,
    DocumentName NVARCHAR(MAX),
    Employee NVARCHAR(MAX),
    HostName NVARCHAR(MAX),
    Date DATETIME,
    EntityType NVARCHAR(255),
    SourceType NVARCHAR(255)
);


INSERT INTO #tmp_result_table
(DocumentId, DocumentName, Employee, HostName, Date, EntityType, SourceType)
SELECT 
    dh.entityid, dh.comment, r.name, dh.hostname, dh.historydate, dh.EntityType, N'Документы'
FROM sungero_content_dochistory AS dh
JOIN sungero_core_recipient AS r ON dh.[user] = r.id
WHERE dh.historydate >= '{0}'
  AND dh.historydate <  '{1}'
  AND LOWER(dh.action) = 'delete'
  AND ('{2}' = N'Документы' OR '{2}' = N'Все');


INSERT INTO #tmp_result_table
(DocumentId, DocumentName, Employee, HostName, Date, EntityType, SourceType)
SELECT 
    dc.entityid, dc.comment, r.name, dc.hostname, dc.historydate, dc.EntityType, N'Записи справочников'
FROM Sungero_Core_DatabookHistory AS dc
JOIN sungero_core_recipient AS r ON dc.[user] = r.id
WHERE dc.historydate >= '{0}' 
  AND dc.historydate <  '{1}'
  AND LOWER(dc.action) = 'delete'
  AND dc.EntityType NOT IN ('271898c8-18ca-4192-9892-e27b273ce5fc','f70d5828-e345-4111-9bdf-65a1a2189c43')
  AND ('{2}' = N'Записи справочников' OR '{2}' = N'Все');


INSERT INTO #tmp_result_table
(DocumentId, DocumentName, Employee, HostName, Date, EntityType, SourceType)
SELECT 
    wh.entityid, wh.comment, r.name, wh.hostname, wh.historydate, wh.EntityType, N'Задачи'
FROM sungero_wf_workflowhistory AS wh
JOIN sungero_core_recipient AS r ON wh.[user] = r.id
WHERE wh.historydate >= '{0}' 
  AND wh.historydate <  '{1}'
  AND LOWER(wh.action) = 'delete'
  AND ('{2}' = N'Задачи' OR '{2}' = N'Все');


SELECT *
FROM #tmp_result_table
ORDER BY Date ASC
OFFSET {3} ROWS FETCH NEXT {4} ROWS ONLY;