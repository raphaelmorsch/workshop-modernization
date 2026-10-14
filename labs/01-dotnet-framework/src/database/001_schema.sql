/*
    Portal de Ordens de Serviço — esquema legado.
    SQL Server LocalDB. Integrated Security, sem usuário de banco.

    Recria as tabelas. Executar de novo apaga os dados da aplicação.
*/
IF DB_ID(N'PortalOrdens') IS NULL
BEGIN
    CREATE DATABASE PortalOrdens;
END
GO

USE PortalOrdens;
GO

IF OBJECT_ID(N'dbo.Attachments', N'U') IS NOT NULL
    DROP TABLE dbo.Attachments;
GO

IF OBJECT_ID(N'dbo.WorkOrders', N'U') IS NOT NULL
    DROP TABLE dbo.WorkOrders;
GO

CREATE TABLE dbo.WorkOrders
(
    Id          INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_WorkOrders PRIMARY KEY,
    Title       NVARCHAR(200)      NOT NULL,
    Description NVARCHAR(MAX)      NULL,
    Status      NVARCHAR(40)       NOT NULL,
    AssetCode   NVARCHAR(50)       NOT NULL,
    CreatedBy   NVARCHAR(200)      NOT NULL,
    CreatedAt   DATETIME2          NOT NULL,
    UpdatedAt   DATETIME2          NULL
);
GO

CREATE TABLE dbo.Attachments
(
    Id          INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Attachments PRIMARY KEY,
    WorkOrderId INT                NOT NULL,
    FileName    NVARCHAR(260)      NOT NULL,
    StoredPath  NVARCHAR(500)      NOT NULL,
    UploadedBy  NVARCHAR(200)      NOT NULL,
    UploadedAt  DATETIME2          NOT NULL,
    CONSTRAINT FK_Attachments_WorkOrders
        FOREIGN KEY (WorkOrderId) REFERENCES dbo.WorkOrders (Id)
);
GO

INSERT INTO dbo.WorkOrders (Title, Description, Status, AssetCode, CreatedBy, CreatedAt, UpdatedAt)
VALUES
(
    N'Vazamento na linha de ar',
    N'Troca da vedação do manifold.',
    N'EmExecucao',
    N'CP-014',
    N'LAB\operador',
    DATEADD(HOUR, -5, SYSDATETIME()),
    DATEADD(HOUR, -4, SYSDATETIME())
),
(
    N'Rolamento ruidoso no motor',
    N'Aguardando parada programada.',
    N'Aberta',
    N'MOT-221',
    N'LAB\operador',
    DATEADD(HOUR, -2, SYSDATETIME()),
    NULL
),
(
    N'Inspeção do painel elétrico',
    N'Checklist mensal concluído sem pendências.',
    N'Concluida',
    N'PNL-03',
    N'LAB\supervisor',
    DATEADD(DAY, -2, SYSDATETIME()),
    DATEADD(DAY, -2, SYSDATETIME())
);
GO
