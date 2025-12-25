CREATE TEMP TABLE IF NOT EXISTS tmp_result_table (
    DocumentId BIGINT,
    DocumentName VARCHAR,
    Employee VARCHAR,
    HostName VARCHAR,
    Date TIMESTAMP,
    EntityType VARCHAR,
    SourceType VARCHAR
) ON COMMIT DROP;

TRUNCATE tmp_result_table;

-- 1. Документы
INSERT INTO tmp_result_table
(DocumentId, DocumentName, Employee, HostName, Date, EntityType, SourceType)
SELECT 
    dh.entityid, dh.comment, r.name, dh.hostname, dh.historydate, dh.EntityType, 'Документы'
FROM sungero_content_dochistory AS dh
JOIN sungero_core_recipient AS r ON dh.user = r.id
WHERE dh.historydate >= '{0}'::timestamp 
  AND dh.historydate < '{1}'::timestamp
  AND dh.comment IS NOT NULL 
  AND dh.comment <> '' 
  AND LOWER(dh.action) = 'delete'
  AND ('{2}' = 'Документы' OR '{2}' = 'Все');

-- 2. Записи справочников
INSERT INTO tmp_result_table
(DocumentId, DocumentName, Employee, HostName, Date, EntityType, SourceType)
SELECT 
    dc.entityid, dc.comment, r.name, dc.hostname, dc.historydate, dc.EntityType, 'Записи справочников'
FROM Sungero_Core_DatabookHistory AS dc
JOIN sungero_core_recipient AS r ON dc.user = r.id
WHERE dc.historydate >= '{0}'::timestamp 
  AND dc.historydate <  '{1}'::timestamp
  AND LOWER(dc.action) = 'delete'
  AND dc.EntityType NOT IN ('271898c8-18ca-4192-9892-e27b273ce5fc','f70d5828-e345-4111-9bdf-65a1a2189c43')
  AND ('{2}' = 'Записи справочников' OR '{2}' = 'Все');

-- 3. Бизнес-процессы
INSERT INTO tmp_result_table
(DocumentId, DocumentName, Employee, HostName, Date, EntityType, SourceType)
SELECT 
    wh.entityid, wh.comment, r.name, wh.hostname, wh.historydate, wh.EntityType, 'Задачи'
FROM sungero_wf_workflowhistory AS wh
JOIN sungero_core_recipient AS r ON wh.user = r.id
WHERE wh.historydate >= '{0}'::timestamp 
  AND wh.historydate <  '{1}'::timestamp
  AND LOWER(wh.action) = 'delete'
  AND ('{2}' = 'Задачи' OR '{2}' = 'Все');

-- выборка с пагинацией
SELECT *
FROM tmp_result_table
ORDER BY Date ASC
LIMIT {4} OFFSET {3};