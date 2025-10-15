-- Script SQL para atualizar o banco de dados LiftForceDb
-- Adiciona novos campos para armazenar parâmetros específicos de cada tipo de asa

USE LiftForceDb;

-- Verificar se a tabela TestResults existe
SELECT 'Verificando estrutura atual da tabela TestResults...' AS Status;
DESCRIBE TestResults;

-- Adicionar novos campos se não existirem
-- Campo para envergadura (wingspan)
ALTER TABLE TestResults 
ADD COLUMN IF NOT EXISTS Wingspan DECIMAL(10,4) DEFAULT 0.0000 COMMENT 'Envergadura da asa em metros';

-- Campo para corda (rope)
ALTER TABLE TestResults 
ADD COLUMN IF NOT EXISTS Rope DECIMAL(10,4) DEFAULT 0.0000 COMMENT 'Corda da asa em metros';

-- Campo para corda na raiz (para asas trapezoidais)
ALTER TABLE TestResults 
ADD COLUMN IF NOT EXISTS RopeAtRoot DECIMAL(10,4) DEFAULT 0.0000 COMMENT 'Corda na raiz da asa em metros';

-- Campo para corda na ponta (para asas trapezoidais)
ALTER TABLE TestResults 
ADD COLUMN IF NOT EXISTS RopeAtEnd DECIMAL(10,4) DEFAULT 0.0000 COMMENT 'Corda na ponta da asa em metros';

-- Verificar a nova estrutura da tabela
SELECT 'Nova estrutura da tabela TestResults:' AS Status;
DESCRIBE TestResults;

-- Criar índices para melhorar performance das consultas
CREATE INDEX IF NOT EXISTS idx_wing_type ON TestResults(WingType);
CREATE INDEX IF NOT EXISTS idx_test_date ON TestResults(TestDate);
CREATE INDEX IF NOT EXISTS idx_wingspan ON TestResults(Wingspan);
CREATE INDEX IF NOT EXISTS idx_rope ON TestResults(Rope);

-- Verificar índices criados
SELECT 'Índices criados:' AS Status;
SHOW INDEX FROM TestResults;

-- Atualizar registros existentes (opcional - para compatibilidade)
-- Se você quiser preencher os novos campos com valores padrão para registros existentes:
-- UPDATE TestResults SET Wingspan = 0, Rope = 0, RopeAtRoot = 0, RopeAtEnd = 0 WHERE Wingspan IS NULL;

-- Verificar se a atualização foi bem-sucedida
SELECT 'Verificação final - contagem de registros:' AS Status;
SELECT COUNT(*) AS TotalRegistros FROM TestResults;

-- Mostrar alguns registros de exemplo
SELECT 'Registros de exemplo:' AS Status;
SELECT 
    Id,
    WingType,
    Wingspan,
    Rope,
    RopeAtRoot,
    RopeAtEnd,
    WingArea,
    TestDate
FROM TestResults 
ORDER BY TestDate DESC 
LIMIT 5;

-- Criar uma view para facilitar consultas comuns
CREATE OR REPLACE VIEW vw_WingTestResults AS
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
    Wingspan,
    Rope,
    RopeAtRoot,
    RopeAtEnd,
    -- Campo calculado para mostrar o tipo de asa com detalhes
    CASE 
        WHEN WingType = 'Rectangular' THEN CONCAT('Retangular - Envergadura: ', Wingspan, 'm, Corda: ', Rope, 'm')
        WHEN WingType = 'Elliptical' THEN CONCAT('Elíptica - Envergadura: ', Wingspan, 'm, Corda: ', Rope, 'm')
        WHEN WingType = 'Trapezoidal' THEN CONCAT('Trapezoidal - Envergadura: ', Wingspan, 'm, Raiz: ', RopeAtRoot, 'm, Ponta: ', RopeAtEnd, 'm')
        WHEN WingType = 'Delta' THEN CONCAT('Delta - Envergadura: ', Wingspan, 'm, Corda: ', Rope, 'm')
        ELSE CONCAT(WingType, ' - Envergadura: ', Wingspan, 'm, Corda: ', Rope, 'm')
    END AS WingDescription
FROM TestResults
ORDER BY TestDate DESC;

-- Verificar se a view foi criada
SELECT 'View criada:' AS Status;
SHOW CREATE VIEW vw_WingTestResults;

-- Criar stored procedure para inserir novos testes
DELIMITER //
CREATE OR REPLACE PROCEDURE sp_InsertWingTest(
    IN p_WingType VARCHAR(50),
    IN p_WindSpeed DECIMAL(10,2),
    IN p_AirDensity DECIMAL(10,4),
    IN p_WingArea DECIMAL(10,4),
    IN p_Coefficient DECIMAL(10,4),
    IN p_LiftForce DECIMAL(10,4),
    IN p_CameraPerspective VARCHAR(100),
    IN p_Wingspan DECIMAL(10,4),
    IN p_Rope DECIMAL(10,4),
    IN p_RopeAtRoot DECIMAL(10,4),
    IN p_RopeAtEnd DECIMAL(10,4)
)
BEGIN
    INSERT INTO TestResults (
        WingType, WindSpeed, AirDensity, WingArea, Coefficient, 
        LiftForce, CameraPerspective, Wingspan, Rope, RopeAtRoot, RopeAtEnd
    ) VALUES (
        p_WingType, p_WindSpeed, p_AirDensity, p_WingArea, p_Coefficient,
        p_LiftForce, p_CameraPerspective, p_Wingspan, p_Rope, p_RopeAtRoot, p_RopeAtEnd
    );
    
    SELECT LAST_INSERT_ID() AS NewTestId;
END //
DELIMITER ;

-- Verificar se a stored procedure foi criada
SELECT 'Stored procedure criada:' AS Status;
SHOW CREATE PROCEDURE sp_InsertWingTest;

-- Criar stored procedure para obter estatísticas por tipo de asa
DELIMITER //
CREATE OR REPLACE PROCEDURE sp_GetWingTypeStatistics(IN p_WingType VARCHAR(50))
BEGIN
    IF p_WingType IS NULL THEN
        -- Estatísticas gerais
        SELECT 
            WingType,
            COUNT(*) AS TotalTests,
            AVG(WingArea) AS AvgWingArea,
            AVG(LiftForce) AS AvgLiftForce,
            AVG(Wingspan) AS AvgWingspan,
            AVG(Rope) AS AvgRope,
            MIN(TestDate) AS FirstTest,
            MAX(TestDate) AS LastTest
        FROM TestResults 
        GROUP BY WingType
        ORDER BY TotalTests DESC;
    ELSE
        -- Estatísticas para um tipo específico
        SELECT 
            WingType,
            COUNT(*) AS TotalTests,
            AVG(WingArea) AS AvgWingArea,
            AVG(LiftForce) AS AvgLiftForce,
            AVG(Wingspan) AS AvgWingspan,
            AVG(Rope) AS AvgRope,
            AVG(RopeAtRoot) AS AvgRopeAtRoot,
            AVG(RopeAtEnd) AS AvgRopeAtEnd,
            MIN(TestDate) AS FirstTest,
            MAX(TestDate) AS LastTest
        FROM TestResults 
        WHERE WingType = p_WingType
        GROUP BY WingType;
    END IF;
END //
DELIMITER ;

-- Verificar se a stored procedure foi criada
SELECT 'Stored procedure de estatísticas criada:' AS Status;
SHOW CREATE PROCEDURE sp_GetWingTypeStatistics;

-- Testar a stored procedure
SELECT 'Testando stored procedure de estatísticas:' AS Status;
CALL sp_GetWingTypeStatistics(NULL);

-- Mensagem de conclusão
SELECT 'Atualização do banco de dados concluída com sucesso!' AS Status;
SELECT 'Novos campos adicionados: Wingspan, Rope, RopeAtRoot, RopeAtEnd' AS Info;
SELECT 'View vw_WingTestResults criada para facilitar consultas' AS Info;
SELECT 'Stored procedures criadas para inserção e estatísticas' AS Info;
