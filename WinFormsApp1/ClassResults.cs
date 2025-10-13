using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace WinFormsApp1
{
    /// <summary>
    /// Class responsible for managing all data and operations related to test results
    /// </summary>
    public static class ClassResults
    {
        #region Private Fields
        private static string connectionString = ConfigurationManager.ConnectionStrings["LiftForceDb"].ConnectionString;
        #endregion

        #region Public Properties
        /// <summary>
        /// Database connection string
        /// </summary>
        public static string ConnectionString
        {
            get => connectionString;
            set => connectionString = value;
        }
        #endregion

        #region Database Operations
        /// <summary>
        /// Salva o resultado do teste no banco de dados
        /// </summary>
        /// <param name="wingType">Tipo da asa</param>
        /// <param name="windSpeed">Velocidade do vento</param>
        /// <param name="airDensity">Densidade do ar</param>
        /// <param name="wingArea">Calculated wing area</param>
        /// <param name="coefficient">Lift coefficient</param>
        /// <param name="liftForce">Calculated lift force</param>
        /// <param name="cameraPerspective">Camera perspective</param>
        /// <param name="wingspan">Envergadura da asa</param>
        /// <param name="rope">Corda da asa</param>
        /// <param name="ropeAtRoot">Corda na raiz (para asas trapezoidais)</param>
        /// <param name="ropeAtEnd">Corda na ponta (para asas trapezoidais)</param>
        public static void SaveTestResult(string wingType, double windSpeed, double airDensity,
            double wingArea, double coefficient, double liftForce, string cameraPerspective,
            double wingspan = 0, double rope = 0, double ropeAtRoot = 0, double ropeAtEnd = 0)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = @"
                    INSERT INTO TestResults 
                    (WingType, WindSpeed, AirDensity, WingArea, Coefficient, LiftForce, CameraPerspective, 
                     Wingspan, Rope, RopeAtRoot, RopeAtEnd)
                    VALUES 
                    (@WingType, @WindSpeed, @AirDensity, @WingArea, @Coefficient, @LiftForce, @CameraPerspective,
                     @Wingspan, @Rope, @RopeAtRoot, @RopeAtEnd)";

                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@WingType", wingType);
                        command.Parameters.AddWithValue("@WindSpeed", windSpeed);
                        command.Parameters.AddWithValue("@AirDensity", airDensity);
                        command.Parameters.AddWithValue("@WingArea", wingArea);
                        command.Parameters.AddWithValue("@Coefficient", coefficient);
                        command.Parameters.AddWithValue("@LiftForce", liftForce);
                        command.Parameters.AddWithValue("@CameraPerspective", cameraPerspective ?? "");
                        command.Parameters.AddWithValue("@Wingspan", wingspan);
                        command.Parameters.AddWithValue("@Rope", rope);
                        command.Parameters.AddWithValue("@RopeAtRoot", ropeAtRoot);
                        command.Parameters.AddWithValue("@RopeAtEnd", ropeAtEnd);

                        command.ExecuteNonQuery();
                    }
                }

                
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving to database:{ex.Message}");
            }
        }

        /// <summary>
        /// Recupera todos os resultados dos testes do banco de dados
        /// </summary>
        /// <returns>DataTable com todos os resultados</returns>
        public static DataTable GetAllTestResults()
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string selectQuery = @"
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
                        RopeAtEnd
                    FROM TestResults 
                    ORDER BY TestDate DESC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(selectQuery, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao recuperar dados do banco: {ex.Message}");
            }
            return dataTable;
        }

        public static int GetTotalTestCount()
        {
            int count = 0;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string countQuery = "SELECT COUNT(*) FROM TestResults";
                    using (MySqlCommand command = new MySqlCommand(countQuery, connection))
                    {
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error counting tests:{ex.Message}");
            }
            return count;
        }

        public static DateTime? GetLastUpdate()
        {
            DateTime? lastUpdate = null;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string lastUpdateQuery = "SELECT MAX(TestDate) FROM TestResults";
                    using (MySqlCommand command = new MySqlCommand(lastUpdateQuery, connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            lastUpdate = Convert.ToDateTime(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error getting last update: {ex.Message}");
            }
            return lastUpdate;
        }

        /// <summary>
        /// Recupera resultados filtrados por tipo de asa
        /// </summary>
        /// <param name="wingType">Tipo de asa para filtrar</param>
        /// <returns>DataTable com resultados filtrados</returns>
        public static DataTable GetFilteredTestResults(string wingType)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string selectQuery = @"
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
                        RopeAtEnd
                    FROM TestResults 
                    WHERE WingType = @WingType
                    ORDER BY TestDate DESC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(selectQuery, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@WingType", wingType);
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error retrieving filtered data {ex.Message}");
            }
            return dataTable;
        }

        /// <summary>
        /// Recupera estatísticas por tipo de asa usando stored procedure
        /// </summary>
        /// <param name="wingType">Tipo de asa (opcional)</param>
        /// <returns>DataTable com estatísticas</returns>
        public static DataTable GetWingTypeStatistics(string? wingType = null)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand("sp_GetWingTypeStatistics", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@p_WingType", wingType);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error retrieving statistics: {ex.Message}");
            }
            return dataTable;
        }
        #endregion

        #region DataGridView Management
        /// <summary>
        /// Carrega todos os dados no DataGridView
        /// </summary>
        /// <param name="dataGridView">DataGridView a ser preenchido</param>
        public static void LoadDataToGridView(DataGridView dataGridView)
        {
            try
            {
                DataTable dataTable = GetAllTestResults();
                if (dataTable.Rows.Count == 0)
                {
                    ShowInfoMessage("Nenhum resultado encontrado no banco de dados.");
                    dataGridView.DataSource = null;
                    return;
                }

                ConfigureDataGridView(dataGridView);
                dataGridView.DataSource = dataTable;
                ConfigureColumns(dataGridView);
                dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                Console.WriteLine($"Carregados {dataTable.Rows.Count} registros no DataGridView.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao carregar dados: {ex.Message}");
            }
        }

        /// <summary>
        /// Carrega todos os dados no Guna2DataGridView
        /// </summary>
        /// <param name="dataGridView">Guna2DataGridView a ser preenchido</param>
        public static void LoadDataToGridView(Guna.UI2.WinForms.Guna2DataGridView dataGridView)
        {
            try
            {
                DataTable dataTable = GetAllTestResults();
                if (dataTable.Rows.Count == 0)
                {
                    ShowInfoMessage("Nenhum resultado encontrado no banco de dados.");
                    dataGridView.DataSource = null;
                    return;
                }

                ConfigureGuna2DataGridView(dataGridView);
                dataGridView.DataSource = dataTable;
                ConfigureGuna2Columns(dataGridView);
                dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                Console.WriteLine($"Carregados {dataTable.Rows.Count} registros no Guna2DataGridView.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao carregar dados: {ex.Message}");
            }
        }

        /// <summary>
        /// Carrega dados filtrados no DataGridView
        /// </summary>
        /// <param name="dataGridView">DataGridView a ser preenchido</param>
        /// <param name="wingType">Tipo de asa para filtrar (opcional)</param>
        public static void LoadFilteredData(DataGridView dataGridView, string? wingType = null)
        {
            try
            {
                DataTable dataTable = string.IsNullOrEmpty(wingType)
                    ? GetAllTestResults()
                    : GetFilteredTestResults(wingType);

                ConfigureDataGridView(dataGridView);
                dataGridView.DataSource = dataTable;
                ConfigureColumns(dataGridView);
                dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Erro ao filtrar dados: {ex.Message}");
            }
        }

        /// <summary>
        /// Configura as propriedades visuais do DataGridView
        /// </summary>
        /// <param name="dataGridView">DataGridView a ser configurado</param>
        public static void ConfigureDataGridView(DataGridView dataGridView)
        {
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = false;
            dataGridView.AutoGenerateColumns = true;
            dataGridView.BackgroundColor = Color.White;
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dataGridView.GridColor = Color.LightGray;
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridView.ColumnHeadersHeight = 30;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.LightBlue;
            dataGridView.DefaultCellStyle.SelectionBackColor = Color.DarkBlue;
            dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        /// <summary>
        /// Configura as colunas do DataGridView com formatação e cabeçalhos
        /// </summary>
        /// <param name="dataGridView">DataGridView a ser configurado</param>
        public static void ConfigureColumns(DataGridView dataGridView)
        {
            if (dataGridView.Columns.Count == 0) return;

            // Oculta a coluna ID
            if (dataGridView.Columns.Contains("Id"))
                dataGridView.Columns["Id"].Visible = false;

            // Define cabeçalhos em português
            var columnHeaders = new Dictionary<string, string>
            {
                ["WingType"] = "Wing Type",
                ["CameraPerspective"] = "Camera Perspective",
                ["WindSpeed"] = "Wind Speed (m/s)",
                ["AirDensity"] = "Air Density (kg/m³)",
                ["WingArea"] = "Wing Area (m²)",
                ["Coefficient"] = "Coeficiente",
                ["LiftForce"] = "Lift Force (N)",
                ["TestDate"] = "Test Date",
                ["Wingspan"] = "Wingspan (m)",
                ["Rope"] = "Chord (m)",
                ["RopeAtRoot"] = "Root Chord (m)",
                ["RopeAtEnd"] = "Tip Chord (m)"
            };

            foreach (var header in columnHeaders)
            {
                if (dataGridView.Columns.Contains(header.Key))
                    dataGridView.Columns[header.Key].HeaderText = header.Value;
            }

            // Configura formatação das colunas numéricas
            ConfigureNumericColumn(dataGridView, "WindSpeed", "F1");
            ConfigureNumericColumn(dataGridView, "AirDensity", "F3");
            ConfigureNumericColumn(dataGridView, "WingArea", "F2");
            ConfigureNumericColumn(dataGridView, "LiftForce", "F2");
            ConfigureNumericColumn(dataGridView, "Coefficient", "F2");
            ConfigureNumericColumn(dataGridView, "Wingspan", "F2");
            ConfigureNumericColumn(dataGridView, "Rope", "F2");
            ConfigureNumericColumn(dataGridView, "RopeAtRoot", "F2");
            ConfigureNumericColumn(dataGridView, "RopeAtEnd", "F2");

            // Configura coluna de data
            if (dataGridView.Columns.Contains("TestDate"))
            {
                dataGridView.Columns["TestDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                
            }
        }

        /// <summary>
        /// Atualiza o DataGridView recarregando os dados
        /// </summary>
        /// <param name="dataGridView">DataGridView a ser atualizado</param>
        public static void RefreshDataGridView(DataGridView dataGridView)
        {
            LoadDataToGridView(dataGridView);
        }

        /// <summary>
        /// Atualiza o Guna2DataGridView recarregando os dados
        /// </summary>
        /// <param name="dataGridView">Guna2DataGridView a ser atualizado</param>
        public static void RefreshDataGridView(Guna.UI2.WinForms.Guna2DataGridView dataGridView)
        {
            LoadDataToGridView(dataGridView);
        }

        /// <summary>
        /// Configura as propriedades visuais do Guna2DataGridView
        /// </summary>
        /// <param name="dataGridView">Guna2DataGridView a ser configurado</param>
        public static void ConfigureGuna2DataGridView(Guna.UI2.WinForms.Guna2DataGridView dataGridView)
        {
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = false;
            dataGridView.AutoGenerateColumns = true;
        }

        /// <summary>
        /// Configura as colunas do Guna2DataGridView com formatação e cabeçalhos
        /// </summary>
        /// <param name="dataGridView">Guna2DataGridView a ser configurado</param>
        public static void ConfigureGuna2Columns(Guna.UI2.WinForms.Guna2DataGridView dataGridView)
        {
            if (dataGridView.Columns.Count == 0) return;

            // Oculta a coluna ID
            if (dataGridView.Columns.Contains("Id"))
                dataGridView.Columns["Id"].Visible = false;

            // Define cabeçalhos em português
            var columnHeaders = new Dictionary<string, string>
            {
                ["WingType"] = "Wing Type",
                ["CameraPerspective"] = "Camera Perspective",
                ["WindSpeed"] = "Wind Speed (m/s)",
                ["AirDensity"] = "Air Density (kg/m³)",
                ["WingArea"] = "Wing Area (m²)",
                ["Coefficient"] = "Coeficiente",
                ["LiftForce"] = "Lift Force (N)",
                ["TestDate"] = "Test Date",
                ["Wingspan"] = "Wingspan (m)",
                ["Rope"] = "Chord (m)",
                ["RopeAtRoot"] = "Root Chord (m)",
                ["RopeAtEnd"] = "Tip Chord (m)"
            };

            foreach (var header in columnHeaders)
            {
                if (dataGridView.Columns.Contains(header.Key))
                    dataGridView.Columns[header.Key].HeaderText = header.Value;
            }

            // Configura formatação das colunas numéricas
            ConfigureGuna2NumericColumn(dataGridView, "WindSpeed", "F1");
            ConfigureGuna2NumericColumn(dataGridView, "AirDensity", "F3");
            ConfigureGuna2NumericColumn(dataGridView, "WingArea", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "LiftForce", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "Coefficient", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "Wingspan", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "Rope", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "RopeAtRoot", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "RopeAtEnd", "F2");

            // Configura coluna de data
            if (dataGridView.Columns.Contains("TestDate"))
            {
                dataGridView.Columns["TestDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            }
        }

        /// <summary>
        /// Configura formatação de uma coluna numérica específica no Guna2DataGridView
        /// </summary>
        private static void ConfigureGuna2NumericColumn(Guna.UI2.WinForms.Guna2DataGridView dataGridView, string columnName, string format)
        {
            if (dataGridView.Columns.Contains(columnName))
            {
                dataGridView.Columns[columnName].DefaultCellStyle.Format = format;
                dataGridView.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
        #endregion

        #region Data Export
        /// <summary>
        /// Exporta todos os resultados para um arquivo XML na área de trabalho
        /// </summary>
        public static void ExportResultsToXml()
        {
            try
            {
                DataTable results = GetAllTestResults();
                if (results.Rows.Count == 0)
                {
                    ShowInfoMessage("No results found for export.");
                    return;
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"Resultados_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xml");

                WriteXmlFile(results, filePath);

                ShowSuccessMessage($"XML file successfully generated at:\n{filePath}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error generating XML: {ex.Message}");
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Manipula a seleção de linha no DataGridView
        /// </summary>
        /// <param name="dataGridView">DataGridView</param>
        public static void HandleDataGridViewSelection(DataGridView dataGridView)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView.SelectedRows[0];
                string wingType = selectedRow.Cells["WingType"].Value?.ToString() ?? string.Empty;
                double liftForce = Convert.ToDouble(selectedRow.Cells["LiftForce"].Value ?? 0);

                Console.WriteLine($"Selected: {wingType} - Force: {liftForce}N");
            }
        }
        #endregion

        #region Private Helper Methods
        /// <summary>
        /// Configura formatação de uma coluna numérica específica
        /// </summary>
        private static void ConfigureNumericColumn(DataGridView dataGridView, string columnName, string format)
        {
            if (dataGridView.Columns.Contains(columnName))
            {
                dataGridView.Columns[columnName].DefaultCellStyle.Format = format;
                dataGridView.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        /// <summary>
        /// Escreve os dados em um arquivo XML
        /// </summary>
        private static void WriteXmlFile(DataTable results, string filePath)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("TestResults");

                foreach (DataRow row in results.Rows)
                {
                    writer.WriteStartElement("TestResult");

                    WriteXmlElement(writer, "WingType", row["WingType"]);
                    WriteXmlElement(writer, "CameraPerspective", row["CameraPerspective"]);
                    WriteXmlElement(writer, "WindSpeed", row["WindSpeed"]);
                    WriteXmlElement(writer, "AirDensity", row["AirDensity"]);
                    WriteXmlElement(writer, "WingArea", row["WingArea"]);
                    WriteXmlElement(writer, "Coefficient", row["Coefficient"]);
                    WriteXmlElement(writer, "LiftForce", row["LiftForce"]);
                    WriteXmlElement(writer, "TestDate", row["TestDate"]);
                    WriteXmlElement(writer, "Wingspan", row["Wingspan"]);
                    WriteXmlElement(writer, "Rope", row["Rope"]);
                    WriteXmlElement(writer, "RopeAtRoot", row["RopeAtRoot"]);
                    WriteXmlElement(writer, "RopeAtEnd", row["RopeAtEnd"]);

                    writer.WriteEndElement(); // TestResult
                }

                writer.WriteEndElement(); // TestResults
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Escreve um elemento XML tratando valores nulos
        /// </summary>
        private static void WriteXmlElement(XmlWriter writer, string elementName, object value)
        {
            writer.WriteElementString(elementName, value?.ToString() ?? "");
        }

        /// <summary>
        /// Mostra mensagem de erro
        /// </summary>
        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Mostra mensagem de sucesso
        /// </summary>
        private static void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Mostra mensagem informativa
        /// </summary>
        private static void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Verifica se há dados para exportar
        /// </summary>
        /// <returns>True se há dados, False caso contrário</returns>
        public static bool HasDataToExport()
        {
            DataTable results = GetAllTestResults();
            return results.Rows.Count > 0;
        }

        /// <summary>
        /// Obtém estatísticas gerais dos testes
        /// </summary>
        /// <returns>Dicionário com estatísticas</returns>
        public static Dictionary<string, object> GetGeneralStatistics()
        {
            var stats = new Dictionary<string, object>();

            try
            {
                DataTable data = GetAllTestResults();

                if (data.Rows.Count > 0)
                {
                    stats["TotalTests"] = data.Rows.Count;
                    stats["LastTestDate"] = data.Rows[0]["TestDate"];

                    var wingTypes = new HashSet<string>();
                    double totalLiftForce = 0;

                    foreach (DataRow row in data.Rows)
                    {
                        wingTypes.Add(row["WingType"].ToString() ?? string.Empty);
                        totalLiftForce += Convert.ToDouble(row["LiftForce"]);
                    }

                    stats["UniqueWingTypes"] = wingTypes.Count;
                    stats["AverageLiftForce"] = totalLiftForce / data.Rows.Count;
                }
                else
                {
                    stats["TotalTests"] = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating statistics: {ex.Message}");
                stats["Error"] = ex.Message;
            }

            return stats;
        }
        #endregion
    }
}