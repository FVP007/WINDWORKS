create database liftforcedb;
use liftforcedb;
CREATE TABLE testresults (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TestDate DATETIME NOT NULL,
    WingType VARCHAR(50) NOT NULL,
    Airfoil VARCHAR(100) NOT NULL,
    AngleOfAttack DECIMAL(5,2) NOT NULL,
    WindSpeed FLOAT NOT NULL,
    AirDensity FLOAT NOT NULL,
    WingArea FLOAT NOT NULL,
    LiftCoefficient DECIMAL(10,6),
    DragCoefficient DECIMAL(10,6),
    LiftForce FLOAT,
    DragForce DECIMAL(10,4),
    Efficiency DECIMAL(10,4),
    CameraPerspective VARCHAR(50),
    Wingspan DECIMAL(10,4),
    
    -- opcional: índice para buscas por tipo de asa
    INDEX idx_wingtype (WingType)
);
CREATE TABLE winggeometry (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TestResultId INT NOT NULL,
    Rope DECIMAL(10,4) NULL,         -- usado em asas retangulares
    RopeAtRoot DECIMAL(10,4) NULL,   -- usado em asas delta ou afiladas
    RopeAtEnd DECIMAL(10,4) NULL,    -- usado em asas delta ou afiladas
    FOREIGN KEY (TestResultId) REFERENCES testresults(Id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

select * from testresults, winggeometry;
INSERT INTO testresults (
    TestDate, WingType, Airfoil, AngleOfAttack, CameraPerspective,
    WindSpeed, AirDensity, WingArea, Wingspan,
    LiftCoefficient, DragCoefficient, LiftForce, DragForce, Efficiency
) VALUES (
    NOW(), 'Retangular', 'NACA 2412', 5.0, 'Frontal',
    15.5, 1.225, 2.0, 4.0,
    0.85, 0.02, 161.46, 3.80, 42.5
);

INSERT INTO winggeometry (
    TestResultId, Rope, RopeAtRoot, RopeAtEnd
) VALUES (
    LAST_INSERT_ID(), 0.5, NULL, NULL
);

-- Inserts para asa Trapezoidal (Afilada)
INSERT INTO testresults (
    TestDate, WingType, Airfoil, AngleOfAttack, CameraPerspective,
    WindSpeed, AirDensity, WingArea, Wingspan,
    LiftCoefficient, DragCoefficient, LiftForce, DragForce, Efficiency
) VALUES (
    NOW(), 'Trapezoidal', 'Eppler 423', 8.5, 'Lateral',
    20.0, 1.225, 2.5, 5.0,
    1.12, 0.035, 343.0, 10.72, 32.0
);

INSERT INTO winggeometry (
    TestResultId, Rope, RopeAtRoot, RopeAtEnd
) VALUES (
    LAST_INSERT_ID(), NULL, 0.6, 0.4
);

-- Inserts para asa Delta (Triangular)
INSERT INTO testresults (
    TestDate, WingType, Airfoil, AngleOfAttack, CameraPerspective,
    WindSpeed, AirDensity, WingArea, Wingspan,
    LiftCoefficient, DragCoefficient, LiftForce, DragForce, Efficiency
) VALUES (
    NOW(), 'Delta', 'NACA 0009', 12.0, 'Frontal',
    25.0, 1.225, 3.0, 6.0,
    1.05, 0.05, 482.81, 22.97, 21.0
);

INSERT INTO winggeometry (
    TestResultId, Rope, RopeAtRoot, RopeAtEnd
) VALUES (
    LAST_INSERT_ID(), NULL, 1.0, 0.05
);