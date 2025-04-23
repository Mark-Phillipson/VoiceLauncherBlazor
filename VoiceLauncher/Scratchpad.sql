 SELECT * FROM Prompts  WHERE ID=22
 --Creates some SQL that will define if there are any duplicate rows in this table with the same description and date
    SELECT COUNT(*) AS DuplicateCount, Description, Date
    FROM Transactions
    GROUP BY Description, Date
    HAVING COUNT(*) > 1

     SELECT * FROM Transactions WHERE Description='AMZNMktplace' AND Date='2023-04-11'
        SELECT * FROM Transactions WHERE Description='SP SENACRE CYCLES' AND Date='2024-12-23'
    SELECT * FROM Transactions WHERE Description='LIDL GB MAIDSTONE' AND Date='2024-12-30'
    SELECT * FROM Transactions WHERE Description='THE CAFE BY THE CR' AND Date='2025-01-20'