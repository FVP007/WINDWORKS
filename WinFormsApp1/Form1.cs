using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using StbImageSharp;
using System.Globalization;
using System.Threading;
using System.Web;
using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.Web.WebView2.Wpf;
using System.Text.RegularExpressions;
using System.Xml;
using System.Data;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Drawing; // <-- Needed for Size, Point, etc.

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // Global variables
        double coefficient = 1.2;
        private string selectedWingType = "Rectangular";
        private bool PageGraficoEnabled = false;
        private bool PageResultadosEnabled = false;
        private string connectionString = "Server=localhost;Database=LiftForceDb;Uid=root;";

        // Responsive layout variables
        private Size originalFormSize;
        private bool isResponsiveInitialized = false;

       
        private Process vrmlProcess = null;

        public Form1()
        {
            InitializeComponent();

            // Existing setup
            ComboWindSpeed.DropDownStyle = ComboBoxStyle.DropDown;
            ComboAirDensity.DropDownStyle = ComboBoxStyle.DropDown;
            ComboWingArea.DropDownStyle = ComboBoxStyle.DropDown;
            this.Load += Form1_Load;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            ButtonRunTest.Enabled = false;
            this.KeyPreview = true;
            SetupResponsiveLayout();

        }

        private void SetupResponsiveLayout()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.WindowState = FormWindowState.Normal;
            ConfigurePanelAnchors();

            this.Resize += Form1_Resize;
        }

        private void ConfigurePanelAnchors()
        {
            guna2Panel3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            guna2Panel3.MinimumSize = new Size(205, 400);
            guna2Panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            guna2Panel6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            guna2Panel6.MinimumSize = new Size(250, 400);

            ConfigureInternalControls();
        }

        private void ConfigureInternalControls()
        {
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Location = new Point(0, 0);
            int buttonHeight = 60;
            tabControl1.Size = new Size(guna2Panel2.Width, guna2Panel2.Height - buttonHeight);
            ButtonRunTest.Anchor = AnchorStyles.Bottom;
            PictureBoxModelImage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webViewChart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LabelWingType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LabelWingType.Height = 42;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ComboWingType.Items.AddRange(new object[] { "Rectangular", "Elliptical", "Trapezoidal", "Delta" });
            ComboCameraPerspective.Items.AddRange(new object[] { "Front View", "Side View", "Isometric View" });
            ComboWindSpeed.Items.AddRange(new object[] { 10, 20, 30, 40, 50 });
            ComboAirDensity.Items.AddRange(new object[] { 1.225, 1.18, 1.15, 1.12, 1.10 });
            ComboWingArea.Items.AddRange(new object[] { 0.1, 0.25, 0.5, 1.0, 2.0, 5.0 });
            ComboWingType.SelectedIndex = 0;
            ComboCameraPerspective.SelectedIndex = 0;
            ComboWindSpeed.SelectedIndex = 0;
            ComboAirDensity.SelectedIndex = 0;
            ComboWingArea.SelectedIndex = 0;
            ComboWindSpeed.SelectedIndexChanged += CheckFieldsFilled;
            ComboAirDensity.SelectedIndexChanged += CheckFieldsFilled;
            ComboWingArea.SelectedIndexChanged += CheckFieldsFilled;
            ComboWingType.SelectedIndexChanged += ComboWingType_SelectedIndexChanged;
            originalFormSize = this.Size;
            isResponsiveInitialized = true;
            LoadDataToGridView();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (!isResponsiveInitialized) return;
            RecalculateButtonPositions();
            AdjustTabControlSize();
        }

        private void RecalculateButtonPositions()
        {
            if (guna2Panel2.Width <= 0) return;
            int centerX = guna2Panel2.Width / 2;
            int buttonWidth = ButtonRunTest.Width;
            ButtonRunTest.Location = new Point(centerX - buttonWidth / 2, guna2Panel2.Height - ButtonRunTest.Height - 10);
            
        }

        private void AdjustTabControlSize()
        {
            if (guna2Panel2.Width <= 0 || guna2Panel2.Height <= 0) return;
            int buttonSpace = 60;
            tabControl1.Size = new Size(guna2Panel2.Width, Math.Max(200, guna2Panel2.Height - buttonSpace));
        }

        private void OptimizeForFullScreen()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 280F);
                tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 55F);
                tableLayoutPanel1.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 45F);
            }
            else
            {
                tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 250F);
                tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 60F);
                tableLayoutPanel1.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 40F);

            }
        }

        // Override WndProc for maximize/restore
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MAXIMIZE = 0xF030;
            const int SC_RESTORE = 0xF120;

            if (m.Msg == WM_SYSCOMMAND)
            {
                if (m.WParam.ToInt32() == SC_MAXIMIZE || m.WParam.ToInt32() == SC_RESTORE)
                {
                    base.WndProc(ref m);
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        OptimizeForFullScreen();
                        RecalculateButtonPositions();
                        AdjustTabControlSize();
                    }));
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void SaveTestResult(string wingType, double windSpeed, double airDensity, double wingArea, double coefficient, double liftForce, string cameraPerspective)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = @"
                INSERT INTO TestResults 
                (WingType, WindSpeed, AirDensity, WingArea, Coefficient, LiftForce, CameraPerspective)
                VALUES 
                (@WingType, @WindSpeed, @AirDensity, @WingArea, @Coefficient, @LiftForce, @CameraPerspective)";

                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@WingType", wingType);
                        command.Parameters.AddWithValue("@WindSpeed", windSpeed);
                        command.Parameters.AddWithValue("@AirDensity", airDensity);
                        command.Parameters.AddWithValue("@WingArea", wingArea);
                        command.Parameters.AddWithValue("@Coefficient", coefficient);
                        command.Parameters.AddWithValue("@LiftForce", liftForce);
                        command.Parameters.AddWithValue("@CameraPerspective", cameraPerspective ?? "");

                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Resultado salvo no banco de dados com sucesso!", "Sucesso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar no banco de dados: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GetAllTestResults()
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
                MessageBox.Show($"Erro ao recuperar dados do banco: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

 
        private DataTable GetWingTypeStatistics(string wingType = null)
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
                MessageBox.Show($"Erro ao recuperar estatísticas: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        private void ComboWingType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //achei interessante talvez usemos
        }

        private void LoadVRMLModel(object sender, EventArgs e)
        {
            try
            {
                // Fecha processos VRML existentes antes de abrir um novo
                CloseExistingVRMLProcesses();

                string wingType = ComboWingType.SelectedItem?.ToString() ?? "Rectangular";
                int windSpeed = int.Parse(ComboWindSpeed.SelectedItem?.ToString() ?? "10");

                UpdateVRMLFile(wingType, windSpeed);

                string scenePath = "C:\\TCC_2025\\Vrml\\cena.wrl";
                string programPath = "C:\\Program Files\\ParallelGraphics\\RapidAuthorViewer\\RapidAuthorViewer.exe";

                
                if (!File.Exists(programPath))
                {
                    MessageBox.Show($"RapidAuthorViewer não encontrado em:\n{programPath}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(scenePath))
                {
                    MessageBox.Show($"Arquivo VRML não encontrado em:\n{scenePath}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                vrmlProcess = new Process();
                vrmlProcess.StartInfo.FileName = programPath;
                vrmlProcess.StartInfo.Arguments = $"\"{scenePath}\"";
                vrmlProcess.StartInfo.UseShellExecute = true;
                vrmlProcess.Start();

                Console.WriteLine($"VRML aberto com tipo de asa: {wingType}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir VRML: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateVRMLFile(string airplaneType, int windSpeed)
        {
            try
            {
                string vrmlPath = "C:\\TCC_2025\\Vrml\\cena.wrl";
                string vrmlContent = File.ReadAllText(vrmlPath);
                string pattern = @"Inline { url "":WingTypes/\w+.wrl"" }";
                string replacement = $@"Inline {{ url "":WingTypes/{airplaneType}.wrl"" }}";

                vrmlContent = Regex.Replace(vrmlContent, pattern, replacement);

                float cycleInterval = (float)(6.28 / (windSpeed / 0.25)); //alterar o 0.25 (raio do ventilador) se necessário
                pattern = @"cycleInterval\s+\d+(\.\d+)?";
                replacement = $@"cycleInterval {cycleInterval}";
                vrmlContent = Regex.Replace(vrmlContent, pattern, replacement);

                pattern = @"position\s+-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s+#Camera Position";
                replacement = CameraPosition();
                vrmlContent = Regex.Replace(vrmlContent, pattern, replacement);

                pattern = @"orientation\s+-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s+-?\d+(\.\d+)?\s+#Camera Orientation";
                replacement = CameraOrientation();
                vrmlContent = Regex.Replace(vrmlContent, pattern, replacement);

                File.WriteAllText(vrmlPath, vrmlContent);

                Console.WriteLine($"VRML atualizado para tipo de avião: {airplaneType}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar VRML: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CameraPosition()
        {
            int indexComboCameraPerspective = ComboCameraPerspective.SelectedIndex;
            switch (indexComboCameraPerspective)
            {
                case 0:
                    return "position 25 -138 35.6202430725098 #Camera Position";
                case 1:
                    return "position 35.78701577186584 3.219329833984 40.6202430725098 #Camera Position";
                case 2:
                    return "position 15.78701577186584 -128.219329833984 37.6202430725098 #Camera Position";
                default:
                    return "";
            }
        }
        private string CameraOrientation()
        {
            int indexComboCameraPerspective = ComboCameraPerspective.SelectedIndex;
            switch (indexComboCameraPerspective)
            {
                case 0:
                    return "orientation 1.1 -1.1 -1.1 2.1 #Camera Orientation";
                case 1:
                    return "orientation 0.1 -1.1 -1.2 3 #Camera Orientation";
                case 2:
                    return "orientation 0.728832358121872 -0.999999999999 -0.999999999999 2.23173069953918 #Camera Orientation";
                default:
                    return "";
            }
        }

        // General formula for lift force
        private double CalculateLiftForce(double density, double speed, double area, double coefficient)
        {
            return 0.5 * density * Math.Pow(speed, 2) * area * coefficient;
        }

        public void ButtonRunTest_Click(object sender, EventArgs e)
        {
            // Get values from interface fields
            if (!double.TryParse(ComboWindSpeed.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double windSpeed))
            {
                MessageBox.Show("Please enter a valid value for Wind Speed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!double.TryParse(ComboAirDensity.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double airDensity))
            {
                MessageBox.Show("Please enter a valid value for Air Density.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!double.TryParse(ComboWingArea.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double wingArea))
            {
                MessageBox.Show("Please enter a valid value for Wing Area", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get wing type and camera perspective
            string wingType = ComboWingType.SelectedItem?.ToString() ?? "Rectangular";
            string cameraPerspective = ComboCameraPerspective.SelectedItem?.ToString() ?? "";

            // Calculate lift force
            double liftForce = CalculateLiftForce(airDensity, windSpeed, wingArea, coefficient);

            // Show result
            guna2HtmlLabelLiftForceValue.Text = $"{liftForce:F2}N";

            // Save to database
            SaveTestResult(wingType, windSpeed, airDensity, wingArea, coefficient, liftForce, cameraPerspective);

            LoadVRMLModel(sender, e);
            // Show Highcharts graph
            ShowChart(airDensity, windSpeed, wingArea, coefficient);

            PageGraficoEnabled = true;
            PageResultadosEnabled = true;

        }


        private async void ShowChart(double density, double maxSpeed, double area, double coefficient)
        {
            
            List<object[]> dataPoints = new List<object[]>();
            for (double v = 0; v <= maxSpeed; v += 1)
            {
                double force = CalculateLiftForce(density, v, area, coefficient);
                dataPoints.Add(new object[] { v, Math.Round(force, 2) });
            }

            string dataPointsJson = JsonSerializer.Serialize(dataPoints);

            
            string html = $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Lift Chart</title>
    <script src='https://code.highcharts.com/highcharts.js'></script>
    <style>
        html, body {{
            height: 100vh;
            width: 100%;
            margin: 0;
            padding: 20px;
            box-sizing: border-box;
        }}
        #container {{
            height: calc(100vh - 120px);
            width: 100%;
            margin: 0;
            padding: 0;
        }}
        .chart-header {{
            text-align: center;
            margin-bottom: 10px;
        }}
        .chart-title {{
            font-size: 18px;
            font-weight: bold;
            color: #333;
            margin-bottom: 5px;
        }}
        .chart-subtitle {{
            font-size: 14px;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class='chart-header'>
        <div class='chart-title'>Wind Speed vs Lift Force</div>
        <div class='chart-subtitle'>Density: {density:F2} kg/m³ | Area: {area:F2} m² | Coefficient: {coefficient:F2}</div>
    </div>
    <div id='container'></div>
    <script>
        Highcharts.chart('container', {{
            chart: {{
                type: 'line',
                backgroundColor: 'transparent',
                margin: [20, 20, 60, 80]
            }},
            title: {{ text: '' }},
            subtitle: {{ text: '' }},
            xAxis: {{
                title: {{ text: 'Wind Speed (m/s)', style: {{ fontSize: '12px', fontWeight: 'bold' }} }},
                min: 0,
                max: {maxSpeed.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)},
                gridLineWidth: 1,
                gridLineColor: '#e6e6e6',
                labels: {{ style: {{ fontSize: '11px' }} }}
            }},
            yAxis: {{
                title: {{ text: 'Lift Force (N)', style: {{ fontSize: '12px', fontWeight: 'bold' }} }},
                min: 0,
                gridLineWidth: 1,
                gridLineColor: '#e6e6e6',
                labels: {{ style: {{ fontSize: '11px' }} }}
            }},
            legend: {{ enabled: false }},
            plotOptions: {{
                line: {{
                    marker: {{
                        enabled: true,
                        radius: 3,
                        lineWidth: 1,
                        lineColor: '#007bff',
                        fillColor: '#ffffff'
                    }},
                    lineWidth: 3,
                    color: '#007bff'
                }}
            }},
            series: [{{
                name: 'Lift',
                data: {dataPointsJson}
            }}],
            credits: {{ enabled: false }},
            responsive: {{
                rules: [{{
                    condition: {{ maxWidth: 500 }},
                    chartOptions: {{
                        legend: {{
                            layout: 'horizontal',
                            align: 'center',
                            verticalAlign: 'bottom'
                        }}
                    }}
                }}]
            }}
        }});
    </script>
</body>
</html>";

            await webViewChart.EnsureCoreWebView2Async();
            webViewChart.NavigateToString(html);
        }

        private void CheckFieldsFilled(object sender, EventArgs e)
        {
            ButtonRunTest.Enabled =
                !string.IsNullOrWhiteSpace(ComboWindSpeed.Text) &&
                !string.IsNullOrWhiteSpace(ComboAirDensity.Text) &&
                !string.IsNullOrWhiteSpace(ComboWingArea.Text);
        }
        private void ValidateNumericInput(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void ButtonYX_Click(object sender, EventArgs e)
        {
            LoadWingImage("Cima");
        }

        private void ButtonZY_Click(object sender, EventArgs e)
        {
            LoadWingImage("Frente");
        }

        private void ButtonZX_Click(object sender, EventArgs e)
        {
            LoadWingImage("Perfil");
        }

        private void LoadWingImage(string viewType)
        {
            try
            {
                if (ComboWingType.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, selecione um tipo de asa.", "Aviso",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                selectedWingType = ComboWingType.SelectedItem.ToString();
                LabelWingType.Text = selectedWingType;
                string imagePath = GetImagePath(selectedWingType, viewType);
                if (string.IsNullOrEmpty(imagePath))
                {
                    MessageBox.Show($"Tipo de asa '{selectedWingType}' não reconhecido.", "Erro",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(imagePath))
                {
                    MessageBox.Show($"Imagem não encontrada: {imagePath}", "Erro",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (PictureBoxModelImage.Image != null)
                {
                    PictureBoxModelImage.Image.Dispose();
                }
                PictureBoxModelImage.Image = Image.FromFile(imagePath);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Arquivo de imagem não encontrado.", "Erro",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado ao carregar a imagem: {ex.Message}", "Erro",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetImagePath(string wingType, string viewType)
        {
            string basePath = @"C:\TCC_2025\Prints";

            return wingType switch
            {
                "Rectangular" => Path.Combine(basePath, "Reta", $"{viewType}.png"),
                "Elliptical" => Path.Combine(basePath, "Eliptica", $"{viewType}.png"),
                "Trapezoidal" => Path.Combine(basePath, "Trapezoidal", $"{viewType}.png"),
                "Delta" => Path.Combine(basePath, "Delta", $"{viewType}.png"),
                _ => null
            };
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {

            if (e.TabPage == PageGrafico && !PageGraficoEnabled)
            {
                e.Cancel = true;
            }
            if (e.TabPage == PageResultados && !PageResultadosEnabled)
            {
                e.Cancel = true;
            }
            if (e.TabPage == PageResultados)
            {
                ButtonRunTest.Visible = false;
                
            }
            else
            {
                ButtonRunTest.Visible = true;
                
            }

        }

        
        

        private void ButtonGenerateXML_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable results = GetAllTestResults();
                if (results.Rows.Count == 0)
                {
                    MessageBox.Show("Nenhum resultado encontrado para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Criação do XML
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = System.Text.Encoding.UTF8,
                    OmitXmlDeclaration = false
                };

                // Caminho para salvar o arquivo
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Resultados.xml");

                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    writer.WriteStartDocument(true);
                    writer.WriteStartElement("TestResults"); // Tag raiz corrigida

                    foreach (DataRow row in results.Rows)
                    {
                        writer.WriteStartElement("TestResult"); // Elemento individual
                                                                // Removido o elemento WingType duplicado
                        writer.WriteElementString("WingType", row["WingType"]?.ToString() ?? "");
                        writer.WriteElementString("CameraPerspective", row["CameraPerspective"]?.ToString() ?? "");
                        writer.WriteElementString("WindSpeed", row["WindSpeed"]?.ToString() ?? "");
                        writer.WriteElementString("AirDensity", row["AirDensity"]?.ToString() ?? "");
                        writer.WriteElementString("WingArea", row["WingArea"]?.ToString() ?? "");
                        writer.WriteElementString("Coefficient", row["Coefficient"]?.ToString() ?? "");
                        writer.WriteElementString("LiftForce", row["LiftForce"]?.ToString() ?? "");
                        writer.WriteElementString("TestDate", row["TestDate"]?.ToString() ?? "");
                        writer.WriteEndElement(); // TestResult
                    }

                    writer.WriteEndElement(); // TestResults
                    writer.WriteEndDocument();
                }

                MessageBox.Show($"Arquivo XML gerado com sucesso em:\n{filePath}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao gerar XML: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Adicione este método na sua classe Form1
        private void CloseExistingVRMLProcesses()
        {
            try
            {
                // Fecha o processo específico que foi iniciado por esta aplicação
                if (vrmlProcess != null && !vrmlProcess.HasExited)
                {
                    vrmlProcess.Kill();
                    vrmlProcess.Dispose();
                    vrmlProcess = null;
                    Console.WriteLine("Processo VRML anterior fechado.");
                }

                
                Process[] processes = Process.GetProcessesByName("Electron");
                foreach (Process process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao fechar processo: {ex.Message}");
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fechar processos VRML: {ex.Message}");
            }
        }

        // Adicione este método ao evento FormClosing do formulário
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Fecha o processo VRML ao fechar a aplicação
            CloseExistingVRMLProcesses();
        }

        // Use o nome correto do controle do seu Designer
        private DataGridView dataGridViewResults => dataGridView1;

        // Carrega todos os dados no DataGridView
        private void LoadDataToGridView()
        {
            try
            {
                DataTable dataTable = GetAllTestResults();
                if (dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Nenhum resultado encontrado no banco de dados.", "Informação",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dataGridViewResults.DataSource = null;
                    return;
                }

                ConfigureDataGridView();
                dataGridViewResults.DataSource = dataTable;
                ConfigureColumns();
                dataGridViewResults.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                Console.WriteLine($"Carregados {dataTable.Rows.Count} registros no DataGridView.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
        private void ConfigureDataGridView()
        {
            dataGridViewResults.AllowUserToAddRows = false;
            dataGridViewResults.AllowUserToDeleteRows = false;
            dataGridViewResults.ReadOnly = true;
            dataGridViewResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewResults.MultiSelect = false;
            dataGridViewResults.AutoGenerateColumns = true;
            dataGridViewResults.BackgroundColor = Color.White;
            dataGridViewResults.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewResults.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dataGridViewResults.GridColor = Color.LightGray;
            dataGridViewResults.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dataGridViewResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewResults.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridViewResults.ColumnHeadersHeight = 30;
            dataGridViewResults.AlternatingRowsDefaultCellStyle.BackColor = Color.LightBlue;
            dataGridViewResults.DefaultCellStyle.SelectionBackColor = Color.DarkBlue;
            dataGridViewResults.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        
        private void ConfigureColumns()
        {
            if (dataGridViewResults.Columns.Count == 0) return;

            if (dataGridViewResults.Columns.Contains("Id"))
                dataGridViewResults.Columns["Id"].Visible = false;

            var columnHeaders = new Dictionary<string, string>
            {
                ["WingType"] = "Tipo de Asa",
                ["CameraPerspective"] = "Perspectiva da Câmera",
                ["WindSpeed"] = "Velocidade do Vento (m/s)",
                ["AirDensity"] = "Densidade do Ar (kg/m³)",
                ["WingArea"] = "Área da Asa (m²)",
                ["Coefficient"] = "Coeficiente",
                ["LiftForce"] = "Força de Sustentação (N)",
                ["TestDate"] = "Data do Teste"
            };

            foreach (var header in columnHeaders)
            {
                if (dataGridViewResults.Columns.Contains(header.Key))
                    dataGridViewResults.Columns[header.Key].HeaderText = header.Value;
            }

            if (dataGridViewResults.Columns.Contains("WindSpeed"))
            {
                dataGridViewResults.Columns["WindSpeed"].DefaultCellStyle.Format = "F1";
                dataGridViewResults.Columns["WindSpeed"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dataGridViewResults.Columns.Contains("AirDensity"))
            {
                dataGridViewResults.Columns["AirDensity"].DefaultCellStyle.Format = "F3";
                dataGridViewResults.Columns["AirDensity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dataGridViewResults.Columns.Contains("WingArea"))
            {
                dataGridViewResults.Columns["WingArea"].DefaultCellStyle.Format = "F2";
                dataGridViewResults.Columns["WingArea"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dataGridViewResults.Columns.Contains("LiftForce"))
            {
                dataGridViewResults.Columns["LiftForce"].DefaultCellStyle.Format = "F2";
                dataGridViewResults.Columns["LiftForce"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dataGridViewResults.Columns.Contains("Coefficient"))
            {
                dataGridViewResults.Columns["Coefficient"].DefaultCellStyle.Format = "F2";
                dataGridViewResults.Columns["Coefficient"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dataGridViewResults.Columns.Contains("TestDate"))
            {
                dataGridViewResults.Columns["TestDate"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                dataGridViewResults.Columns["TestDate"].Width = 150;
            }
        }

        // Atualiza o DataGridView (chame após inserir/atualizar registros)
        private void RefreshDataGridView()
        {
            LoadDataToGridView();
        }

        // Filtra dados por tipo de asa
        private void LoadFilteredData(string wingType = null)
        {
            try
            {
                DataTable dataTable = string.IsNullOrEmpty(wingType)
                    ? GetAllTestResults()
                    : GetFilteredTestResults(wingType);

                ConfigureDataGridView();
                dataGridViewResults.DataSource = dataTable;
                ConfigureColumns();
                dataGridViewResults.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao filtrar dados: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Busca dados filtrados
        private DataTable GetFilteredTestResults(string wingType)
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
                    CameraPerspective
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
                MessageBox.Show($"Erro ao recuperar dados filtrados: {ex.Message}", "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        // Evento para botão "Carregar Dados"
        private void ButtonLoadData_Click(object sender, EventArgs e)
        {
            LoadDataToGridView();
        }

       

        
        private void DataGridViewResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewResults.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewResults.SelectedRows[0];
                string wingType = selectedRow.Cells["WingType"].Value?.ToString();
                double liftForce = Convert.ToDouble(selectedRow.Cells["LiftForce"].Value ?? 0);
                Console.WriteLine($"Selecionado: {wingType} - Força: {liftForce}N");
                
            }
        }

        
    }
}
