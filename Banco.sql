CREATE DATABASE LiftForceDB;
USE LiftForceDB;
CREATE TABLE TestResults (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TestDate DATETIME NOT NULL DEFAULT NOW(),
    WingType VARCHAR(50) NOT NULL,
    WindSpeed FLOAT NOT NULL,
    AirDensity FLOAT NOT NULL,
    WingArea FLOAT NOT NULL,
    Coefficient FLOAT NOT NULL,
    LiftForce FLOAT NOT NULL,
    CameraPerspective VARCHAR(50)
);

-- Dados de Exemplo
INSERT INTO TestResults (WingType, WindSpeed, AirDensity, WingArea, Coefficient, LiftForce, CameraPerspective)
VALUES 
    ('Rectangular', 20, 1.225, 1.0, 1.2, 294.0, 'Front View'),
    ('Elliptical', 30, 1.18, 0.5, 1.2, 318.6, 'Side View'),
    ('Trapezoidal', 25, 1.15, 2.0, 1.2, 862.5, 'Isometric View'),
    ('Delta', 35, 1.12, 1.5, 1.2, 1234.8, 'Front View');

--  Consulta para verificar os dados inseridos
SELECT * FROM TestResults ORDER BY TestDate DESC;

-- Consulta para estatísticas básicas
SELECT 
    WingType,
    COUNT(*) as TotalTests,
    AVG(LiftForce) as AverageLiftForce,
    MIN(LiftForce) as MinLiftForce,
    MAX(LiftForce) as MaxLiftForce,
    AVG(WindSpeed) as AverageWindSpeed
FROM TestResults 
GROUP BY WingType;

-- View para relatórios mais detalhados
CREATE VIEW vw_TestResultsReport AS
SELECT 
    Id,
    TestDate,
    WingType,
    WindSpeed,
    AirDensity,
    WingArea,
    Coefficient,
    LiftForce,
    CameraPerspective,
    -- Campos calculados
    ROUND(LiftForce / WingArea, 2) as LiftForcePerArea,
    CASE 
        WHEN LiftForce < 100 THEN 'Low'
        WHEN LiftForce < 500 THEN 'Medium'
        WHEN LiftForce < 1000 THEN 'High'
        ELSE 'Very High'
    END as LiftForceCategory
FROM TestResults;

-- Stored procedure para obter estatísticas por tipo de asa
DELIMITER //
CREATE PROCEDURE sp_GetWingTypeStatistics(
    IN p_WingType VARCHAR(50)
)
BEGIN
    SELECT 
        WingType,
        COUNT(*) as TotalTests,
        AVG(LiftForce) as AverageLiftForce,
        MIN(LiftForce) as MinLiftForce,
        MAX(LiftForce) as MaxLiftForce,
        AVG(WindSpeed) as AverageWindSpeed,
        AVG(AirDensity) as AverageAirDensity,
        AVG(WingArea) as AverageWingArea,
        MIN(TestDate) as FirstTest,
        MAX(TestDate) as LastTest
    FROM TestResults
    WHERE (p_WingType IS NULL OR WingType = p_WingType)
    GROUP BY WingType
    ORDER BY AverageLiftForce DESC;
END //
DELIMITER ;
-- Chamar a stored procedure para todos os tipos de asa:
CALL sp_GetWingTypeStatistics(NULL);
-- Chamar a stored procedure para um tipo específico:
 CALL sp_GetWingTypeStatistics('Rectangular');
-- Consultar a view
SELECT * FROM vw_TestResultsReport;