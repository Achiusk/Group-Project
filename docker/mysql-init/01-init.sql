-- MySQL initialization script for Mijn Energie
-- This runs automatically when the MySQL container starts for the first time

USE EindhovenEnergy;

-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
 Id INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    PostalCode VARCHAR(7) NOT NULL,
    HouseNumber VARCHAR(10) NOT NULL,
    HouseNumberAddition VARCHAR(10),
    Street VARCHAR(200) NOT NULL,
    City VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    HasSmartMeter BOOLEAN DEFAULT FALSE,
    NetbeheerderId VARCHAR(50),
    NetbeheerderName VARCHAR(100),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME,
    IsActive BOOLEAN DEFAULT TRUE,
    EmailVerified BOOLEAN DEFAULT FALSE,
    INDEX idx_email (Email),
    INDEX idx_postalcode (PostalCode)
);

-- Create Consumers table
CREATE TABLE IF NOT EXISTS Consumers (
    ConsumerId VARCHAR(20) PRIMARY KEY,
    DisplayName VARCHAR(100) NOT NULL,
    Address VARCHAR(200) NOT NULL,
    Wijk VARCHAR(50) NOT NULL,
    Postcode VARCHAR(10) NOT NULL,
 HasSolarPanels BOOLEAN DEFAULT FALSE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    INDEX idx_wijk (Wijk),
    INDEX idx_postcode (Postcode)
);

-- Create P1SensorData table
CREATE TABLE IF NOT EXISTS P1SensorData (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    MeterId VARCHAR(50) NOT NULL,
    ConsumerId VARCHAR(20),
    Address VARCHAR(200),
  Postcode VARCHAR(10),
    Wijk VARCHAR(50),
  Timestamp DATETIME NOT NULL,
    ElectricityDeliveredTariff1 DECIMAL(18,3) DEFAULT 0,
    ElectricityDeliveredTariff2 DECIMAL(18,3) DEFAULT 0,
    ElectricityReturnedTariff1 DECIMAL(18,3) DEFAULT 0,
    ElectricityReturnedTariff2 DECIMAL(18,3) DEFAULT 0,
    CurrentTariff INT DEFAULT 1,
    CurrentPowerConsumption DECIMAL(18,3) DEFAULT 0,
    CurrentPowerReturn DECIMAL(18,3) DEFAULT 0,
    GasConsumption DECIMAL(18,3) DEFAULT 0,
    GasTimestamp DATETIME,
  FOREIGN KEY (ConsumerId) REFERENCES Consumers(ConsumerId) ON DELETE CASCADE,
    INDEX idx_consumer (ConsumerId),
    INDEX idx_timestamp (Timestamp),
    INDEX idx_wijk (Wijk)
);

-- Insert sample Eindhoven neighborhoods (wijken)
INSERT INTO Consumers (ConsumerId, DisplayName, Address, Wijk, Postcode, HasSolarPanels) VALUES
('CONS-0001', 'Huishouden Strijp #1', 'Strijpsestraat 10, Eindhoven', 'Strijp', '5616 AA', TRUE),
('CONS-0002', 'Huishouden Strijp #2', 'Glaslaan 25, Eindhoven', 'Strijp', '5616 AB', FALSE),
('CONS-0003', 'Huishouden Woensel-Noord #1', 'Woenselse Markt 5, Eindhoven', 'Woensel-Noord', '5625 AA', TRUE),
('CONS-0004', 'Huishouden Woensel-Zuid #1', 'Kruisstraat 100, Eindhoven', 'Woensel-Zuid', '5625 BA', FALSE),
('CONS-0005', 'Huishouden Gestel #1', 'Gestelsestraat 45, Eindhoven', 'Gestel', '5613 AA', TRUE),
('CONS-0006', 'Huishouden Stratum #1', 'Stratumseind 20, Eindhoven', 'Stratum', '5611 AB', FALSE),
('CONS-0007', 'Huishouden Tongelre #1', 'Tongelresestraat 88, Eindhoven', 'Tongelre', '5641 AA', TRUE),
('CONS-0008', 'Huishouden Centrum #1', 'Vestdijk 50, Eindhoven', 'Centrum', '5611 CA', TRUE),
('CONS-0009', 'Huishouden Meerhoven #1', 'Meerhovendreef 15, Eindhoven', 'Meerhoven', '5658 AA', TRUE),
('CONS-0010', 'Huishouden Meerhoven #2', 'Zandrijk 30, Eindhoven', 'Meerhoven', '5658 BA', FALSE)
ON DUPLICATE KEY UPDATE DisplayName = VALUES(DisplayName);

-- Grant privileges
GRANT ALL PRIVILEGES ON EindhovenEnergy.* TO 'energie_user'@'%';
FLUSH PRIVILEGES;

SELECT 'Database initialized successfully!' AS Status;
