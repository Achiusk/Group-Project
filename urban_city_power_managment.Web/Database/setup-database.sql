-- =====================================================
-- Eindhoven Energy Management - Azure SQL Database Setup
-- For P1 Sensor Data from Dutch Smart Meters (DSMR)
-- =====================================================

-- Create database (run this separately in Azure Portal or via Azure CLI)
-- CREATE DATABASE EindhovenEnergy;

-- =====================================================
-- TABLES
-- =====================================================

-- Consumers (Huishoudens) Table
CREATE TABLE Consumers (
 ConsumerId NVARCHAR(50) NOT NULL PRIMARY KEY,
    DisplayName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(200) NULL,
  Wijk NVARCHAR(50) NULL,
    Postcode NVARCHAR(10) NULL,
    HasSolarPanels BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    
    INDEX IX_Consumers_Wijk (Wijk),
    INDEX IX_Consumers_Postcode (Postcode)
);

-- P1 Sensor Data Table (DSMR readings from smart meters)
CREATE TABLE P1SensorData (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    MeterId NVARCHAR(50) NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    ConsumerId NVARCHAR(50) NOT NULL,
    Address NVARCHAR(200) NULL,
    Postcode NVARCHAR(10) NULL,
    Wijk NVARCHAR(50) NULL,
    
    -- Electricity delivered (from grid)
    ElectricityDeliveredTariff1 DECIMAL(18,3) NOT NULL DEFAULT 0, -- Low tariff kWh
    ElectricityDeliveredTariff2 DECIMAL(18,3) NOT NULL DEFAULT 0, -- High tariff kWh
    
    -- Electricity returned (solar/wind to grid)
    ElectricityReturnedTariff1 DECIMAL(18,3) NOT NULL DEFAULT 0,  -- Low tariff kWh
    ElectricityReturnedTariff2 DECIMAL(18,3) NOT NULL DEFAULT 0,  -- High tariff kWh
    
    -- Current readings
    CurrentTariff INT NOT NULL DEFAULT 1,        -- 1 or 2
  CurrentPowerConsumption DECIMAL(18,3) NOT NULL DEFAULT 0,-- kW
    CurrentPowerReturn DECIMAL(18,3) NOT NULL DEFAULT 0,           -- kW
    
    -- Gas
    GasConsumption DECIMAL(18,3) NOT NULL DEFAULT 0,         -- m³
    GasTimestamp DATETIME2 NULL,
    
    CONSTRAINT FK_P1SensorData_Consumer FOREIGN KEY (ConsumerId) 
      REFERENCES Consumers(ConsumerId) ON DELETE CASCADE,
    
    INDEX IX_P1SensorData_ConsumerId (ConsumerId),
    INDEX IX_P1SensorData_Timestamp (Timestamp),
    INDEX IX_P1SensorData_Wijk (Wijk),
    INDEX IX_P1SensorData_ConsumerId_Timestamp (ConsumerId, Timestamp)
);

-- Gas Usage Table (per zone)
CREATE TABLE GasUsage (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    FlowRate DECIMAL(18,3) NOT NULL DEFAULT 0,          -- m³/h
    Pressure DECIMAL(18,3) NOT NULL DEFAULT 0,        -- bar
    Temperature DECIMAL(18,2) NOT NULL DEFAULT 0,       -- Celsius
    TotalConsumption DECIMAL(18,3) NOT NULL DEFAULT 0,  -- m³
    Location NVARCHAR(100) NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    IsNormal BIT NOT NULL DEFAULT 1,
    
    INDEX IX_GasUsage_Location (Location),
    INDEX IX_GasUsage_Timestamp (Timestamp)
);

-- Gas Leak Alerts Table
CREATE TABLE GasLeakAlerts (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Location NVARCHAR(100) NOT NULL,
    PressureDrop DECIMAL(18,3) NOT NULL DEFAULT 0,
    FlowRateAnomaly DECIMAL(18,3) NOT NULL DEFAULT 0,
    Severity INT NOT NULL DEFAULT 0,  -- 0=Info, 1=Low, 2=Medium, 3=High, 4=Critical
    DetectedAt DATETIME2 NOT NULL,
    IsResolved BIT NOT NULL DEFAULT 0,
    ResolvedAt DATETIME2 NULL,
    Description NVARCHAR(500) NULL,
    AffectedCustomers INT NOT NULL DEFAULT 0,
    
    INDEX IX_GasLeakAlerts_Location (Location),
    INDEX IX_GasLeakAlerts_DetectedAt (DetectedAt),
    INDEX IX_GasLeakAlerts_IsResolved (IsResolved),
    INDEX IX_GasLeakAlerts_Severity (Severity)
);

-- =====================================================
-- VIEWS for Dashboard Queries
-- =====================================================

-- View: Latest P1 readings per consumer
CREATE VIEW vw_LatestP1Readings AS
SELECT p.*
FROM P1SensorData p
INNER JOIN (
    SELECT ConsumerId, MAX(Timestamp) as MaxTimestamp
    FROM P1SensorData
    GROUP BY ConsumerId
) latest ON p.ConsumerId = latest.ConsumerId AND p.Timestamp = latest.MaxTimestamp;
GO

-- View: Wijk Statistics
CREATE VIEW vw_WijkStatistics AS
SELECT 
    c.Wijk,
    COUNT(DISTINCT c.ConsumerId) as TotalHouseholds,
    SUM(CASE WHEN c.HasSolarPanels = 1 THEN 1 ELSE 0 END) as HouseholdsWithSolar,
    ISNULL(SUM(lr.CurrentPowerConsumption), 0) as TotalCurrentConsumptionKw,
    ISNULL(SUM(lr.CurrentPowerReturn), 0) as TotalCurrentSolarReturnKw
FROM Consumers c
LEFT JOIN vw_LatestP1Readings lr ON c.ConsumerId = lr.ConsumerId
WHERE c.IsActive = 1
GROUP BY c.Wijk;
GO

-- View: City-wide Statistics
CREATE VIEW vw_CityStatistics AS
SELECT 
    COUNT(DISTINCT c.ConsumerId) as TotalHouseholds,
    SUM(CASE WHEN c.HasSolarPanels = 1 THEN 1 ELSE 0 END) as HouseholdsWithSolar,
    ISNULL(SUM(lr.CurrentPowerConsumption) / 1000, 0) as TotalCurrentConsumptionMw,
    ISNULL(SUM(lr.CurrentPowerReturn) / 1000, 0) as TotalCurrentSolarReturnMw
FROM Consumers c
LEFT JOIN vw_LatestP1Readings lr ON c.ConsumerId = lr.ConsumerId
WHERE c.IsActive = 1;
GO

-- =====================================================
-- STORED PROCEDURES
-- =====================================================

-- SP: Insert P1 Sensor Reading
CREATE PROCEDURE sp_InsertP1Reading
  @MeterId NVARCHAR(50),
  @ConsumerId NVARCHAR(50),
    @ElectricityDeliveredTariff1 DECIMAL(18,3),
    @ElectricityDeliveredTariff2 DECIMAL(18,3),
    @ElectricityReturnedTariff1 DECIMAL(18,3),
    @ElectricityReturnedTariff2 DECIMAL(18,3),
    @CurrentTariff INT,
    @CurrentPowerConsumption DECIMAL(18,3),
    @CurrentPowerReturn DECIMAL(18,3),
    @GasConsumption DECIMAL(18,3),
    @GasTimestamp DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Id NVARCHAR(450) = NEWID();
    DECLARE @Timestamp DATETIME2 = GETUTCDATE();
    
    -- Get consumer info
    DECLARE @Address NVARCHAR(200), @Postcode NVARCHAR(10), @Wijk NVARCHAR(50);
    SELECT @Address = Address, @Postcode = Postcode, @Wijk = Wijk
    FROM Consumers WHERE ConsumerId = @ConsumerId;
    
    INSERT INTO P1SensorData (
        Id, MeterId, Timestamp, ConsumerId, Address, Postcode, Wijk,
        ElectricityDeliveredTariff1, ElectricityDeliveredTariff2,
        ElectricityReturnedTariff1, ElectricityReturnedTariff2,
        CurrentTariff, CurrentPowerConsumption, CurrentPowerReturn,
      GasConsumption, GasTimestamp
    )
VALUES (
     @Id, @MeterId, @Timestamp, @ConsumerId, @Address, @Postcode, @Wijk,
      @ElectricityDeliveredTariff1, @ElectricityDeliveredTariff2,
        @ElectricityReturnedTariff1, @ElectricityReturnedTariff2,
        @CurrentTariff, @CurrentPowerConsumption, @CurrentPowerReturn,
    @GasConsumption, @GasTimestamp
    );
    
    SELECT @Id as InsertedId;
END;
GO

-- SP: Get Consumer Energy Summary
CREATE PROCEDURE sp_GetConsumerSummary
    @ConsumerId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Today DATE = CAST(GETUTCDATE() AS DATE);
  DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1);
    
    SELECT 
        c.ConsumerId,
   c.DisplayName,
        c.Address,
        c.Wijk,
        c.Postcode,
    c.HasSolarPanels,
        lr.CurrentPowerConsumption as CurrentPowerKw,
        lr.CurrentPowerReturn as CurrentSolarReturnKw,
    ISNULL((SELECT SUM(CurrentPowerConsumption) FROM P1SensorData WHERE ConsumerId = @ConsumerId AND CAST(Timestamp AS DATE) = @Today), 0) as TodayConsumptionKwh,
        ISNULL((SELECT SUM(CurrentPowerReturn) FROM P1SensorData WHERE ConsumerId = @ConsumerId AND CAST(Timestamp AS DATE) = @Today), 0) as TodaySolarReturnKwh,
        ISNULL((SELECT MAX(GasConsumption) FROM P1SensorData WHERE ConsumerId = @ConsumerId AND Timestamp >= @MonthStart), 0) as MonthGasConsumptionM3,
        lr.Timestamp as LastUpdate
    FROM Consumers c
    LEFT JOIN vw_LatestP1Readings lr ON c.ConsumerId = lr.ConsumerId
    WHERE c.ConsumerId = @ConsumerId;
END;
GO

-- =====================================================
-- SAMPLE DATA for Eindhoven
-- =====================================================

-- Insert Eindhoven Wijken (Neighborhoods) with sample consumers
INSERT INTO Consumers (ConsumerId, DisplayName, Address, Wijk, Postcode, HasSolarPanels)
VALUES
    ('CONS-0001', 'Huishouden Strijp #1', 'Strijpsestraat 1, Eindhoven', 'Strijp', '5616 AA', 1),
    ('CONS-0002', 'Huishouden Strijp #2', 'Strijpsestraat 25, Eindhoven', 'Strijp', '5616 AB', 0),
    ('CONS-0003', 'Huishouden Woensel-Noord #1', 'Woenselsestraat 10, Eindhoven', 'Woensel-Noord', '5625 CA', 1),
    ('CONS-0004', 'Huishouden Woensel-Noord #2', 'Woenselsestraat 45, Eindhoven', 'Woensel-Noord', '5625 CB', 1),
    ('CONS-0005', 'Huishouden Centrum #1', 'Hoogstraat 5, Eindhoven', 'Centrum', '5611 HK', 0),
    ('CONS-0006', 'Huishouden Centrum #2', 'Markt 12, Eindhoven', 'Centrum', '5611 EB', 0),
    ('CONS-0007', 'Huishouden Gestel #1', 'Gestelsestraat 8, Eindhoven', 'Gestel', '5615 LC', 1),
    ('CONS-0008', 'Huishouden Stratum #1', 'Stratumseind 15, Eindhoven', 'Stratum', '5611 EN', 0),
    ('CONS-0009', 'Huishouden Tongelre #1', 'Tongelresestraat 22, Eindhoven', 'Tongelre', '5613 DH', 1),
    ('CONS-0010', 'Huishouden Meerhoven #1', 'Meerhoven 100, Eindhoven', 'Meerhoven', '5658 GH', 1);

-- Insert sample P1 sensor readings
INSERT INTO P1SensorData (Id, MeterId, Timestamp, ConsumerId, Address, Postcode, Wijk,
    ElectricityDeliveredTariff1, ElectricityDeliveredTariff2, ElectricityReturnedTariff1, ElectricityReturnedTariff2,
    CurrentTariff, CurrentPowerConsumption, CurrentPowerReturn, GasConsumption, GasTimestamp)
VALUES
    (NEWID(), 'E0000001', GETUTCDATE(), 'CONS-0001', 'Strijpsestraat 1, Eindhoven', '5616 AA', 'Strijp',
     2500.123, 1800.456, 850.789, 620.321, 1, 1.250, 0.850, 450.567, GETUTCDATE()),
    (NEWID(), 'E0000002', GETUTCDATE(), 'CONS-0002', 'Strijpsestraat 25, Eindhoven', '5616 AB', 'Strijp',
     3200.456, 2100.789, 0, 0, 1, 2.100, 0, 680.234, GETUTCDATE()),
    (NEWID(), 'E0000003', GETUTCDATE(), 'CONS-0003', 'Woenselsestraat 10, Eindhoven', '5625 CA', 'Woensel-Noord',
     1800.789, 1200.123, 1200.456, 900.789, 2, 0.800, 1.500, 320.678, GETUTCDATE()),
    (NEWID(), 'E0000004', GETUTCDATE(), 'CONS-0004', 'Woenselsestraat 45, Eindhoven', '5625 CB', 'Woensel-Noord',
     2100.321, 1500.654, 950.123, 700.456, 1, 1.100, 1.200, 410.890, GETUTCDATE()),
    (NEWID(), 'E0000005', GETUTCDATE(), 'CONS-0005', 'Hoogstraat 5, Eindhoven', '5611 HK', 'Centrum',
     4500.654, 3200.987, 0, 0, 2, 3.500, 0, 850.123, GETUTCDATE());

-- Insert sample gas alerts
INSERT INTO GasLeakAlerts (Id, Location, PressureDrop, FlowRateAnomaly, Severity, DetectedAt, IsResolved, Description, AffectedCustomers)
VALUES
    (NEWID(), 'Strijp', 0.85, 25.5, 2, DATEADD(HOUR, -2, GETUTCDATE()), 0, 'Matige drukdaling in sector 3A, inspectie aanbevolen', 150),
    (NEWID(), 'Centrum', 0.35, 12.0, 1, DATEADD(DAY, -1, GETUTCDATE()), 1, 'Kleine drukafwijking gedetecteerd, opgelost', 50);

PRINT 'Eindhoven Energy Database setup completed successfully!';
GO
