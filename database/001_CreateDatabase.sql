IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SensitiveWordsDb')
BEGIN
    CREATE DATABASE SensitiveWordsDb;
END
GO
