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


namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // Global variables
        double coefficient = 1.2;
        private string selectedWingType = "Rectangular";
        private bool PageGraficoEnabled = false;
        private bool PageResultadosEnabled = false;
        // Connection string 
        private string connectionString = "Server=localhost;Database=LiftForceDb;Uid=root;";

        public Form1()
        {
            InitializeComponent();
            ComboWindSpeed.DropDownStyle = ComboBoxStyle.DropDown;
            ComboAirDensity.DropDownStyle = ComboBoxStyle.DropDown;
            ComboWingArea.DropDownStyle = ComboBoxStyle.DropDown;
            this.Load += Form1_Load;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            ButtonRunTest.Enabled = false;
            this.KeyPreview = true;


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

        // Método adicional para obter estatísticas usando a stored procedure
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

        private void Form1_Load(object sender, EventArgs e)
        {
            ComboWingType.Items.AddRange(new object[] { "Rectangular", "Elliptical", "Trapezoidal", "Delta" });

            ComboCameraPerspective.Items.AddRange(new object[] { "Front View", "Side View", "Isometric View" });

            ComboWindSpeed.Items.AddRange(new object[] { 10, 20, 30, 40, 50 });

            // Values for air density (kg/m³) — typical approximations at sea level
            ComboAirDensity.Items.AddRange(new object[] { 1.225, 1.18, 1.15, 1.12, 1.10 });

            // Values for wing area (m²)
            ComboWingArea.Items.AddRange(new object[] { 0.1, 0.25, 0.5, 1.0, 2.0, 5.0 });

            // Optional: set the first selected item in each combo
            ComboWingType.SelectedIndex = 0;
            ComboCameraPerspective.SelectedIndex = 0;
            ComboWindSpeed.SelectedIndex = 0;
            ComboAirDensity.SelectedIndex = 0;
            ComboWingArea.SelectedIndex = 0;

            ComboWindSpeed.SelectedIndexChanged += CheckFieldsFilled;
            ComboAirDensity.SelectedIndexChanged += CheckFieldsFilled;
            ComboWingArea.SelectedIndexChanged += CheckFieldsFilled;

            ComboWingType.SelectedIndexChanged += ComboWingType_SelectedIndexChanged;
        }

        private void ComboWingType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //achei interessante talvez usemos
        }

        private void LoadVRMLModel(object sender, EventArgs e)
        {
            try
            {
                string wingType = ComboWingType.SelectedItem?.ToString() ?? "Rectangular";
                int windSpeed = int.Parse(ComboWindSpeed.SelectedItem?.ToString() ?? "10");

                UpdateVRMLFile(wingType, windSpeed);

                string scenePath = "C:\\TCC_2025\\Vrml\\cena.wrl";
                string programPath = "C:\\Program Files\\ParallelGraphics\\RapidAuthorViewer\\RapidAuthorViewer.exe";

                var process = new Process();
                process.StartInfo.FileName = programPath;
                process.StartInfo.Arguments = $"\"{scenePath}\"";
                process.Start();

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
                    return "position 25 -138 35.6202430725098 #Camera Position";
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
                    return "orientation 1.1 -1.1 -1.1 2.1 #Camera Orientation";
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
            // Gerar pontos para o gráfico
            List<object[]> dataPoints = new List<object[]>();
            for (double v = 0; v <= maxSpeed; v += 1)
            {
                double force = CalculateLiftForce(density, v, area, coefficient);
                dataPoints.Add(new object[] { v, Math.Round(force, 2) });
            }

            string dataPointsJson = JsonSerializer.Serialize(dataPoints);

            // Use interpolação de string para inserir os valores no HTML
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
                ButtonGenerateXML.Visible = true; 
            }
            else
            {
                ButtonRunTest.Visible = true; 
                ButtonGenerateXML.Visible = false; 
            }

        }
    }
}