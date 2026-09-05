SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE CommercialManagement;
GO

IF OBJECT_ID(N'dbo.LignesCommande', N'U') IS NULL
AND OBJECT_ID(N'dbo.Commandes', N'U') IS NULL
AND OBJECT_ID(N'dbo.Produits', N'U') IS NULL
AND OBJECT_ID(N'dbo.Clients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clients (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clients PRIMARY KEY,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        Identifiant NVARCHAR(30) NOT NULL,
        Nom NVARCHAR(120) NOT NULL,
        Prenom NVARCHAR(80) NULL,
        Email NVARCHAR(180) NOT NULL,
        Telephone NVARCHAR(30) NULL,
        Adresse NVARCHAR(250) NULL,
        Ville NVARCHAR(100) NULL,
        CodePostal NVARCHAR(20) NULL
    );

    CREATE UNIQUE INDEX IX_Clients_Email ON dbo.Clients (Email);
    CREATE UNIQUE INDEX IX_Clients_Identifiant ON dbo.Clients (Identifiant);

    CREATE TABLE dbo.Produits (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Produits PRIMARY KEY,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        Reference NVARCHAR(40) NOT NULL,
        Libelle NVARCHAR(160) NOT NULL,
        Description NVARCHAR(500) NULL,
        PrixUnitaire DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL
    );

    CREATE UNIQUE INDEX IX_Produits_Reference ON dbo.Produits (Reference);

    CREATE TABLE dbo.Commandes (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Commandes PRIMARY KEY,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        Numero NVARCHAR(30) NOT NULL,
        ClientId INT NOT NULL,
        DateCommande DATETIME2 NOT NULL,
        Statut INT NOT NULL,
        Total DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_Commandes_Clients_ClientId
            FOREIGN KEY (ClientId) REFERENCES dbo.Clients (Id)
    );

    CREATE UNIQUE INDEX IX_Commandes_Numero ON dbo.Commandes (Numero);
    CREATE INDEX IX_Commandes_ClientId ON dbo.Commandes (ClientId);

    CREATE TABLE dbo.LignesCommande (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LignesCommande PRIMARY KEY,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        CommandeId INT NOT NULL,
        ProduitId INT NOT NULL,
        Quantite INT NOT NULL,
        PrixUnitaire DECIMAL(18,2) NOT NULL,
        TotalLigne AS (Quantite * PrixUnitaire) PERSISTED,
        CONSTRAINT FK_LignesCommande_Commandes_CommandeId
            FOREIGN KEY (CommandeId) REFERENCES dbo.Commandes (Id) ON DELETE CASCADE,
        CONSTRAINT FK_LignesCommande_Produits_ProduitId
            FOREIGN KEY (ProduitId) REFERENCES dbo.Produits (Id)
    );

    CREATE INDEX IX_LignesCommande_CommandeId ON dbo.LignesCommande (CommandeId);
    CREATE INDEX IX_LignesCommande_ProduitId ON dbo.LignesCommande (ProduitId);

    CREATE TABLE dbo.__EFMigrationsHistory (
        MigrationId NVARCHAR(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
        ProductVersion NVARCHAR(32) NOT NULL
    );

    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260904213600_InitialCreate', N'10.0.0');
END
GO
