USE SensitiveWordsDb;
GO

BEGIN TRANSACTION;
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE name = 'SensitiveWords' AND xtype = 'U')
    BEGIN
        CREATE TABLE SensitiveWords (
            Id        INT           IDENTITY(1,1) PRIMARY KEY,
            Word      NVARCHAR(100) NOT NULL,
            CreatedAt DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
            UpdatedAt DATETIME2     NULL,

            CONSTRAINT UQ_SensitiveWords_Word UNIQUE (Word)
        );

        CREATE INDEX IX_SensitiveWords_Word ON SensitiveWords (Word);
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
