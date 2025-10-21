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
    public static class ClassResults
    {
        #region Private Fields
        private static string connectionString = ConfigurationManager.ConnectionStrings["LiftForceDb"].ConnectionString;
        #endregion

        #region Public Properties
        public static string ConnectionString
        {
            get => connectionString;
            set => connectionString = value;
        }
        #endregion

        #region Database Operations
        /// <summary>
        /// Salva o resultado COMPLETO do teste no banco de dados
        /// </summary>
        // Dentro da sua classe de banco de dados (ex: ClassResults.cs)

        public static void SaveTestResult(
            string wingType,
            string airfoil, // Novo
            double angleOfAttack, // Novo
            double windSpeed,
            double airDensity,
            double wingArea,
            double clCoefficient,
            double cdCoefficient, // Novo
            double liftForce,
            double dragForce, // Novo
            double efficiency, // Novo
            string cameraPerspective,
            double wingspan,
            double rope,
            double ropeAtRoot,
            double ropeAtEnd
        )
        {
            // A string de conexão deve ser acessível aqui
            string connectionString = ConfigurationManager.ConnectionStrings["LiftForceDb"].ConnectionString;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // COMANDO SQL ATUALIZADO com todas as novas colunas
                string query = @"
            INSERT INTO TestResults (
                WingType, Airfoil, AngleOfAttack, CameraPerspective,
                WindSpeed, AirDensity, WingArea,
                Wingspan, Rope, RopeAtRoot, RopeAtEnd,
                LiftCoefficient, DragCoefficient,
                LiftForce, DragForce, Efficiency
            ) VALUES (
                @WingType, @Airfoil, @AngleOfAttack, @CameraPerspective,
                @WindSpeed, @AirDensity, @WingArea,
                @Wingspan, @Rope, @RopeAtRoot, @RopeAtEnd,
                @LiftCoefficient, @DragCoefficient,
                @LiftForce, @DragForce, @Efficiency
            );";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // Adiciona todos os parâmetros (antigos e novos)
                    cmd.Parameters.AddWithValue("@WingType", wingType);
                    cmd.Parameters.AddWithValue("@Airfoil", airfoil); // Novo
                    cmd.Parameters.AddWithValue("@AngleOfAttack", angleOfAttack); // Novo
                    cmd.Parameters.AddWithValue("@CameraPerspective", cameraPerspective);
                    cmd.Parameters.AddWithValue("@WindSpeed", windSpeed);
                    cmd.Parameters.AddWithValue("@AirDensity", airDensity);
                    cmd.Parameters.AddWithValue("@WingArea", wingArea);
                    cmd.Parameters.AddWithValue("@Wingspan", wingspan);
                    cmd.Parameters.AddWithValue("@Rope", rope);
                    cmd.Parameters.AddWithValue("@RopeAtRoot", ropeAtRoot);
                    cmd.Parameters.AddWithValue("@RopeAtEnd", ropeAtEnd);
                    cmd.Parameters.AddWithValue("@LiftCoefficient", clCoefficient);

                    // NOVOS PARÂMETROS ADICIONADOS AQUI
                    cmd.Parameters.AddWithValue("@DragCoefficient", cdCoefficient);
                    cmd.Parameters.AddWithValue("@LiftForce", liftForce);
                    cmd.Parameters.AddWithValue("@DragForce", dragForce);
                    cmd.Parameters.AddWithValue("@Efficiency", efficiency);

                    try
                    {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // É uma boa prática registrar o erro em algum lugar
                        Console.WriteLine("Erro ao salvar no banco de dados: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Recupera TODOS os resultados dos testes do banco de dados
        /// </summary>
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
                        Airfoil,
                        AngleOfAttack,
                        WindSpeed,
                        AirDensity,
                        WingArea,
                        Wingspan,
                        Rope,
                        RopeAtRoot,
                        RopeAtEnd,
                        LiftCoefficient,
                        DragCoefficient,
                        LiftForce,
                        DragForce,
                        Efficiency,
                        CameraPerspective
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
                ShowErrorMessage($"Error counting tests: {ex.Message}");
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
                        Id, TestDate, WingType, Airfoil, AngleOfAttack,
                        WindSpeed, AirDensity, WingArea,
                        Wingspan, Rope, RopeAtRoot, RopeAtEnd,
                        LiftCoefficient, DragCoefficient,
                        LiftForce, DragForce, Efficiency, CameraPerspective
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
                ShowErrorMessage($"Error retrieving filtered data: {ex.Message}");
            }
            return dataTable;
        }

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

        public static void ConfigureColumns(DataGridView dataGridView)
        {
            if (dataGridView.Columns.Count == 0) return;

            if (dataGridView.Columns.Contains("Id"))
                dataGridView.Columns["Id"].Visible = false;

            var columnHeaders = new Dictionary<string, string>
            {
                ["WingType"] = "Wing Type",
                ["Airfoil"] = "Airfoil",
                ["AngleOfAttack"] = "AoA (°)",
                ["CameraPerspective"] = "Camera",
                ["WindSpeed"] = "Speed (m/s)",
                ["AirDensity"] = "Density (kg/m³)",
                ["WingArea"] = "Area (m²)",
                ["Wingspan"] = "Wingspan (m)",
                ["Rope"] = "Rope (m)",
                ["RopeAtRoot"] = "Rope at the Root (m)",
                ["RopeAtEnd"] = "Rope at the End (m)",
                ["LiftCoefficient"] = "CL",
                ["DragCoefficient"] = "CD",
                ["LiftForce"] = "Lift (N)",
                ["DragForce"] = "Drag (N)",
                ["Efficiency"] = "L/D",
                ["TestDate"] = "Date"
            };

            foreach (var header in columnHeaders)
            {
                if (dataGridView.Columns.Contains(header.Key))
                    dataGridView.Columns[header.Key].HeaderText = header.Value;
            }

            ConfigureNumericColumn(dataGridView, "WindSpeed", "F1");
            ConfigureNumericColumn(dataGridView, "AirDensity", "F3");
            ConfigureNumericColumn(dataGridView, "WingArea", "F2");
            ConfigureNumericColumn(dataGridView, "Wingspan", "F2");
            ConfigureNumericColumn(dataGridView, "Rope", "F2");
            ConfigureNumericColumn(dataGridView, "RopeAtRoot", "F2");
            ConfigureNumericColumn(dataGridView, "RopeAtEnd", "F2");
            ConfigureNumericColumn(dataGridView, "LiftCoefficient", "F4");
            ConfigureNumericColumn(dataGridView, "DragCoefficient", "F4");
            ConfigureNumericColumn(dataGridView, "LiftForce", "F2");
            ConfigureNumericColumn(dataGridView, "DragForce", "F2");
            ConfigureNumericColumn(dataGridView, "Efficiency", "F2");
            ConfigureNumericColumn(dataGridView, "AngleOfAttack", "F1");

            if (dataGridView.Columns.Contains("TestDate"))
            {
                dataGridView.Columns["TestDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
        }

        public static void RefreshDataGridView(DataGridView dataGridView)
        {
            LoadDataToGridView(dataGridView);
        }

        public static void RefreshDataGridView(Guna.UI2.WinForms.Guna2DataGridView dataGridView)
        {
            LoadDataToGridView(dataGridView);
        }

        public static void ConfigureGuna2DataGridView(Guna.UI2.WinForms.Guna2DataGridView dataGridView)
        {
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = false;
            dataGridView.AutoGenerateColumns = true;
        }

        public static void ConfigureGuna2Columns(Guna.UI2.WinForms.Guna2DataGridView dataGridView)
        {
            if (dataGridView.Columns.Count == 0) return;

            if (dataGridView.Columns.Contains("Id"))
                dataGridView.Columns["Id"].Visible = false;

            var columnHeaders = new Dictionary<string, string>
            {
                ["WingType"] = "Wing Type",
                ["Airfoil"] = "Airfoil",
                ["AngleOfAttack"] = "AoA (°)",
                ["CameraPerspective"] = "Camera",
                ["WindSpeed"] = "Speed (m/s)",
                ["AirDensity"] = "Density (kg/m³)",
                ["WingArea"] = "Area (m²)",
                ["Wingspan"] = "Wingspan (m)",
                ["Rope"] = "Rope (m)",
                ["RopeAtRoot"] = "Rope at the Root (m)",
                ["RopeAtEnd"] = "Rope at the End (m)",
                ["LiftCoefficient"] = "CL",
                ["DragCoefficient"] = "CD",
                ["LiftForce"] = "Lift (N)",
                ["DragForce"] = "Drag (N)",
                ["Efficiency"] = "L/D",
                ["TestDate"] = "Date"
            };

            foreach (var header in columnHeaders)
            {
                if (dataGridView.Columns.Contains(header.Key))
                    dataGridView.Columns[header.Key].HeaderText = header.Value;
            }

            ConfigureGuna2NumericColumn(dataGridView, "WindSpeed", "F1");
            ConfigureGuna2NumericColumn(dataGridView, "AirDensity", "F3");
            ConfigureGuna2NumericColumn(dataGridView, "WingArea", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "Wingspan", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "Rope", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "RopeAtRoot", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "RopeAtEnd", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "LiftCoefficient", "F4");
            ConfigureGuna2NumericColumn(dataGridView, "DragCoefficient", "F4");
            ConfigureGuna2NumericColumn(dataGridView, "LiftForce", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "DragForce", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "Efficiency", "F2");
            ConfigureGuna2NumericColumn(dataGridView, "AngleOfAttack", "F1");

            if (dataGridView.Columns.Contains("TestDate"))
            {
                dataGridView.Columns["TestDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
        }

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
        private static void ConfigureNumericColumn(DataGridView dataGridView, string columnName, string format)
        {
            if (dataGridView.Columns.Contains(columnName))
            {
                dataGridView.Columns[columnName].DefaultCellStyle.Format = format;
                dataGridView.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

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
                    WriteXmlElement(writer, "Airfoil", row["Airfoil"]);
                    WriteXmlElement(writer, "AngleOfAttack", row["AngleOfAttack"]);
                    WriteXmlElement(writer, "CameraPerspective", row["CameraPerspective"]);
                    WriteXmlElement(writer, "WindSpeed", row["WindSpeed"]);
                    WriteXmlElement(writer, "AirDensity", row["AirDensity"]);
                    WriteXmlElement(writer, "WingArea", row["WingArea"]);
                    WriteXmlElement(writer, "Wingspan", row["Wingspan"]);
                    WriteXmlElement(writer, "Rope", row["Rope"]);
                    WriteXmlElement(writer, "RopeAtRoot", row["RopeAtRoot"]);
                    WriteXmlElement(writer, "RopeAtEnd", row["RopeAtEnd"]);
                    WriteXmlElement(writer, "LiftCoefficient", row["LiftCoefficient"]);
                    WriteXmlElement(writer, "DragCoefficient", row["DragCoefficient"]);
                    WriteXmlElement(writer, "LiftForce", row["LiftForce"]);
                    WriteXmlElement(writer, "DragForce", row["DragForce"]);
                    WriteXmlElement(writer, "Efficiency", row["Efficiency"]);
                    WriteXmlElement(writer, "TestDate", row["TestDate"]);

                    writer.WriteEndElement(); // TestResult
                }

                writer.WriteEndElement(); // TestResults
                writer.WriteEndDocument();
            }
        }

        private static void WriteXmlElement(XmlWriter writer, string elementName, object value)
        {
            writer.WriteElementString(elementName, value?.ToString() ?? "");
        }

        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Utility Methods
        public static bool HasDataToExport()
        {
            DataTable results = GetAllTestResults();
            return results.Rows.Count > 0;
        }

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