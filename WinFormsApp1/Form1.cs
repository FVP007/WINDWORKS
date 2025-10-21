using Microsoft.Data.SqlClient;
using System.IO;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MySql.Data.MySqlClient;
using StbImageSharp;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing; // <-- Needed for Size, Point, etc.
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;


namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
#region Variáveis Globais 
 
         // Enum para identificar o tipo de gráfico 
         private enum ChartType 
         { 
             LiftCurve,      // Curva de Sustentação (CL) 
             DragPolar,      // Polar de Arrasto (CL x CD) 
             Efficiency      // Eficiência (L/D) 
         }

        private Dictionary<string, Dictionary<double, (double CL, double CD)>> airfoilData = new Dictionary<string, Dictionary<double, (double CL, double CD)>>(StringComparer.OrdinalIgnoreCase);

        // Coeficientes do último teste (para exportação e recálculo) 
        private double lastClCoefficient = 0.0; 
         private double lastCdCoefficient = 0.0; 
 
         // Controle de UI 
         private string selectedWingType = "Rectangular"; 
         private bool PageGraficoEnabled = false; 
         private Dictionary<string, bool> wingTypeSelections = new Dictionary<string, bool> 
         { 
             { "Rectangular", true }, { "Trapezoidal", false }, { "Elliptical", false }, { "Delta", false } 
         }; 
         private string currentSimulatedWingType = "Rectangular"; 
         
         // Outras variáveis 
         private string connectionString = ConfigurationManager.ConnectionStrings["LiftForceDb"].ConnectionString; 
         private Size originalFormSize; 
         private bool isResponsiveInitialized = false; 
         ResultsForm ResultsForm = new ResultsForm(); 
         private Process? vrmlProcess = null; 
 
         #endregion 
         public Form1() 
         { 
             InitializeComponent(); 
             this.Load += Form1_Load; 
             
             ComboWindSpeed.DropDownStyle = ComboBoxStyle.DropDown; 
             ComboAirDensity.DropDownStyle = ComboBoxStyle.DropDown;
             PictureBoxModelImage.Image = Image.FromFile(GetImagePath("Rectangular", "Cima"));
             Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US"); 
             Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US"); 
             ButtonRunTest.Enabled = false; 
             this.KeyPreview = true; 
             SetupResponsiveLayout(); 
         }

        // Form1.cs

        

        

        



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

         private void Form1_Load(object? sender, EventArgs e) 
         {
            ComboWingType.Items.Clear();
            ComboBoxAirFoil.Items.Clear();
            ComboCameraPerspective.Items.Clear();
            ComboWindSpeed.Items.Clear();
            ComboAirDensity.Items.Clear();
            // Carrega todos os dados necessários 
            LoadAirfoilData(); 
            
 
             // Popula ComboBoxes 
             ComboWingType.Items.AddRange(new object[] { "Rectangular", "Elliptical", "Trapezoidal", "Delta" }); 
             ComboCameraPerspective.Items.AddRange(new object[] { "Front View", "Side View", "Isometric View" }); 
             ComboWindSpeed.Items.AddRange(new object[] { 10, 20, 30, 40, 50 }); 
             ComboAirDensity.Items.AddRange(new object[] { 1.225, 1.18, 1.15, 1.12, 1.10 }); 
 
             foreach (var airfoilName in airfoilData.Keys) 
             { 
                 ComboBoxAirFoil.Items.Add(airfoilName); 
             } 
             if (ComboBoxAirFoil.Items.Count > 0) 
             { 
                 ComboBoxAirFoil.SelectedIndex = 0; 
             } 
 
             // Define seleções padrão 
             ComboWingType.SelectedIndex = 0; 
             ComboCameraPerspective.SelectedIndex = 0; 
             ComboWindSpeed.SelectedIndex = 0; 
             ComboAirDensity.SelectedIndex = 0; 
 
             // Associa eventos 
             ComboWindSpeed.SelectedIndexChanged += CheckFieldsFilled; 
             ComboAirDensity.SelectedIndexChanged += CheckFieldsFilled; 
             ComboBoxAirFoil.SelectedIndexChanged += ComboBoxAirFoil_SelectedIndexChanged; 
 
             // Configurações de UI 
             LoadWingControl("Rectangular"); 
             SyncCheckBoxesWithSelections(); 
             originalFormSize = this.Size; 
             isResponsiveInitialized = true; 
         } 

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (!isResponsiveInitialized) return;
            RecalculateButtonPositions();
            AdjustTabControlSize();
            
        }

        
 
         private double CalculateLiftForce(double density, double speed, double area, double coefficient) 
         { 
             return 0.5 * density * Math.Pow(speed, 2) * area * coefficient; 
         }

        private async void ShowAllCharts(double density, double maxSpeed, double wingArea, string selectedAirfoil, double currentCL, double currentCD)
        {
            string forceVsSpeedData = GenerateForceVsSpeedData(density, maxSpeed, currentCL, currentCD);
            string liftCurveData = GenerateProfileCharacteristicData(selectedAirfoil, ChartType.LiftCurve);
            string dragPolarData = GenerateProfileCharacteristicData(selectedAirfoil, ChartType.DragPolar);
            string efficiencyData = GenerateProfileCharacteristicData(selectedAirfoil, ChartType.Efficiency);

            // HTML responsivo otimizado
            string html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Aerodynamic Analysis</title>
    <script src='https://code.highcharts.com/themes/dark-unica.js'></script>
    <script src='https://code.highcharts.com/highcharts.js'></script>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        html, body {{ height: 100%; width: 100%; overflow: hidden; font-family: 'Segoe UI', sans-serif; }}
        .container {{ height: 100vh; width: 100vw; display: flex; flex-direction: column; }}
        .tabs {{ display: flex; padding: 8px; background-color: #f8f9fa; border-bottom: 2px solid #dee2e6; flex-shrink: 0; gap: 8px; }}
        .tab {{ background: #e9ecef; color: #495057; border: none; padding: 10px 16px; border-radius: 6px; cursor: pointer; font-size: 14px; font-weight: 500; transition: all 0.2s ease; }}
        .tab:hover {{ background: #dee2e6; transform: translateY(-1px); }}
        .tab.active {{ background: #007bff; color: white; box-shadow: 0 2px 4px rgba(0,123,255,0.3); }}
        .chart-wrapper {{ flex: 1; padding: 12px; overflow: hidden; min-height: 0; }}
        .chart-container {{ height: 100%; width: 100%; display: none; }}
        .chart-container.active {{ display: block; }}
        .highcharts-container, .highcharts-root {{ width: 100% !important; height: 100% !important; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='tabs'>
            <button class='tab active' onclick='showChart(1)'>Force vs Speed</button>
            <button class='tab' onclick='showChart(2)'>Lift Curve</button>
            <button class='tab' onclick='showChart(3)'>Drag Polar</button>
            <button class='tab' onclick='showChart(4)'>Efficiency (L/D)</button>
        </div>
        <div class='chart-wrapper'>
            <div id='chart1' class='chart-container active'></div>
            <div id='chart2' class='chart-container'></div>
            <div id='chart3' class='chart-container'></div>
            <div id='chart4' class='chart-container'></div>
        </div>
    </div>
    <script>
        let charts = {{}};
        
        function showChart(num) {{
            for (let i = 1; i <= 4; i++) {{
                document.getElementById('chart' + i).classList.remove('active');
                document.querySelectorAll('.tab')[i - 1].classList.remove('active');
            }}
            document.getElementById('chart' + num).classList.add('active');
            document.querySelectorAll('.tab')[num - 1].classList.add('active');
            if (charts['chart' + num]) {{ 
                setTimeout(() => charts['chart' + num].reflow(), 100);
            }}
        }}

        function cleanDragPolarData(rawData) {{
            if (!rawData || !rawData[0] || !rawData[0].data) return [];
            let data = rawData[0].data
                .filter(point => point && Array.isArray(point) && point.length === 2 &&
                               !isNaN(point[0]) && !isNaN(point[1]) &&
                               isFinite(point[0]) && isFinite(point[1]) && point[0] >= 0)
                .sort((a, b) => a[0] - b[0]);
            return data;
        }}

        const baseConfig = {{
            chart: {{ animation: false, style: {{ fontFamily: 'Segoe UI' }} }},
            credits: {{ enabled: false }},
            responsive: {{ rules: [{{ condition: {{ maxWidth: 500 }}, chartOptions: {{ legend: {{ enabled: false }} }} }}] }}
        }};

        charts.chart1 = Highcharts.chart('chart1', Highcharts.merge(baseConfig, {{
            title: {{ text: 'Wind Speed vs Force' }},
            xAxis: {{ title: {{ text: 'Wind Speed (m/s)' }} }},
            yAxis: {{ title: {{ text: 'Force (N)' }} }},
            tooltip: {{ shared: true }},
            chart: {{ type: 'spline', events: {{ load: function() {{ {forceVsSpeedData}.forEach(s => this.addSeries(s)); }} }} }}
        }}));

        charts.chart2 = Highcharts.chart('chart2', Highcharts.merge(baseConfig, {{
            title: {{ text: 'Profile Lift Curve' }},
            subtitle: {{ text: 'Profile: {selectedAirfoil}' }},
            xAxis: {{ title: {{ text: 'Angle of Attack (°)' }} }},
            yAxis: {{ title: {{ text: 'Lift Coefficient (CL)' }} }},
            series: [{{ name: '{selectedAirfoil}', data: [] }}],
            chart: {{ type: 'spline', events: {{ load: function() {{ this.series[0].setData({liftCurveData}[0].data); }} }} }}
        }}));

        charts.chart3 = Highcharts.chart('chart3', Highcharts.merge(baseConfig, {{
            title: {{ text: 'Profile Drag Polar' }},
            subtitle: {{ text: 'Profile: {selectedAirfoil}' }},
            xAxis: {{ title: {{ text: 'Drag Coefficient (CD)' }}, min: 0 }},
            yAxis: {{ title: {{ text: 'Lift Coefficient (CL)' }} }},
            tooltip: {{ formatter: function() {{ return '<b>CD:</b> ' + this.x.toFixed(4) + '<br/><b>CL:</b> ' + this.y.toFixed(4); }} }},
            plotOptions: {{ spline: {{ marker: {{ enabled: true, radius: 3 }} }} }},
            series: [{{ name: '{selectedAirfoil}', data: [] }}],
            chart: {{ type: 'spline', events: {{ load: function() {{ this.series[0].setData(cleanDragPolarData({dragPolarData})); }} }} }}
        }}));

        charts.chart4 = Highcharts.chart('chart4', Highcharts.merge(baseConfig, {{
            title: {{ text: 'Profile Efficiency Curve' }},
            subtitle: {{ text: 'Profile: {selectedAirfoil}' }},
            xAxis: {{ title: {{ text: 'Angle of Attack (°)' }} }},
            yAxis: {{ title: {{ text: 'Efficiency L/D' }} }},
            series: [{{ name: '{selectedAirfoil}', data: [] }}],
            chart: {{ type: 'spline', events: {{ load: function() {{ this.series[0].setData({efficiencyData}[0].data); }} }} }}
        }}));

        window.addEventListener('resize', () => Object.values(charts).forEach(c => c && c.reflow()));
    </script>
</body>
</html>";

            await webViewChart.EnsureCoreWebView2Async();
            webViewChart.NavigateToString(html);
        }
        private string GenerateForceVsSpeedData(double density, double maxSpeed, double cl, double cd)
        {
            var series = new List<object>();
            var wingControl = GetCurrentWingControl();

            if (wingControl == null) return "[]";

            // Dicionário com as cores fixas para cada tipo de asa
            var wingColors = new Dictionary<string, string>
    {
        { "Rectangular", "#FF0000" }, // Vermelho
        { "Trapezoidal", "#0000FF" }, // Azul
        { "Elliptical",  "#D4AC0D" }, // Amarelo/Dourado para melhor visibilidade
        { "Delta",       "#28B463" }  // Verde
    };

            // Pega os parâmetros do teste que foi executado
            double baseWingspan = wingControl.Wingspan;
            double originalTestArea = wingControl.WingArea;
            string originalTestWingType = ComboWingType.SelectedItem?.ToString() ?? "";

            // Itera sobre todos os tipos de asa que estão selecionados nos checkboxes
            foreach (var selection in wingTypeSelections.Where(s => s.Value == true))
            {
                string currentWingType = selection.Key;
                double wingAreaToUse;

                // Se o tipo de asa atual for o mesmo do teste original, usa a área exata daquele teste.
                if (currentWingType == originalTestWingType)
                {
                    wingAreaToUse = originalTestArea;
                }
                else // Para os outros tipos de asa (comparativos), calcula a área com base na envergadura.
                {
                    wingAreaToUse = CalculateWingAreaByType(currentWingType, baseWingspan);
                }

                var liftPoints = new List<object[]>();
                for (double v = 0; v <= maxSpeed; v += 0.5)
                {
                    liftPoints.Add(new object[] { v, Math.Round(CalculateLiftForce(density, v, wingAreaToUse, cl), 2) });
                }

                // Adiciona a série de Sustentação com a cor correta
                series.Add(new
                {
                    name = $"Lift ({currentWingType})",
                    data = liftPoints,
                    color = wingColors.ContainsKey(currentWingType) ? wingColors[currentWingType] : "#000000" // Cor preta como padrão se não encontrar
                });

                // --- ARRASTO COMENTADO ---
                // A lógica do arrasto está aqui. Se o orientador aprovar,
                // basta remover os comentários (/* e */) para reativá-la.
                /*
                var dragPoints = new List<object[]>();
                for (double v = 0; v <= maxSpeed; v += 0.5)
                {
                    dragPoints.Add(new object[] { v, Math.Round(CalculateLiftForce(density, v, wingAreaToUse, cd), 2) });
                }

                series.Add(new {
                    name = $"Arrasto ({currentWingType})",
                    data = dragPoints,
                    color = wingColors.ContainsKey(currentWingType) ? wingColors[currentWingType] : "#808080",
                    dashStyle = "dash"
                });
                */
            }

            return JsonSerializer.Serialize(series);
        }
        private string GenerateProfileCharacteristicData(string airfoilName, ChartType chartType)
        {
            if (!airfoilData.ContainsKey(airfoilName)) return "[]";
            var dataPoints = new List<object[]>();
            var profileData = airfoilData[airfoilName].OrderBy(p => p.Key);
            foreach (var point in profileData)
            {
                double angle = point.Key;
                double cl = point.Value.CL;
                double cd = point.Value.CD;
                switch (chartType)
                {
                    case ChartType.LiftCurve:
                        dataPoints.Add(new object[] { angle, cl });
                        break;
                    case ChartType.DragPolar:
                        dataPoints.Add(new object[] { cd, cl });
                        break;
                    case ChartType.Efficiency:
                        double efficiency = (cd > 0.0001) ? cl / cd : 0;
                        dataPoints.Add(new object[] { angle, efficiency });
                        break;
                }
            }
            var series = new List<object> { new { name = airfoilName, data = dataPoints } };
            return JsonSerializer.Serialize(series);
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

        
        

        private void LoadVRMLModel(object sender, EventArgs e)
        {
            try
            {
                // Fecha processos VRML existentes antes de abrir um novo
                CloseExistingVRMLProcesses();

                string wingType = ComboWingType.SelectedItem?.ToString() ?? "Rectangular";
                int windSpeed = int.Parse(ComboWindSpeed.SelectedItem?.ToString() ?? "10");

                UpdateVRMLFile(wingType, windSpeed);

                string scenePath = "C:\\WINDWORKS\\Vrml\\cena.wrl";
                string programPath = "C:\\Program Files\\ParallelGraphics\\RapidAuthorViewer\\RapidAuthorViewer.exe";


                if (!File.Exists(programPath))
                {
                    MessageBox.Show($"RapidAuthorViewer not found at:\n{programPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(scenePath))
                {
                    MessageBox.Show($"VRML file not found at:\n{scenePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Error opening VRML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateVRMLFile(string airplaneType, int windSpeed)
        {
            try
            {
                string vrmlPath = "C:\\WINDWORKS\\Vrml\\cena.wrl";
                string vrmlContent = File.ReadAllText(vrmlPath);
                string pattern = @"Inline { url "":WingTypes/\w+.wrl"" }";
                string replacement = $@"Inline {{ url "":WingTypes/{airplaneType}.wrl"" }}";

                vrmlContent = Regex.Replace(vrmlContent, pattern, replacement);

                float cycleInterval = (float)(6.28 / (windSpeed / 3)); // alterar o 3 (raio do ventilador) para ajustar a velocidade
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

                Console.WriteLine($"VRML updated for airplane type: {airplaneType}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating VRML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

       

        // Métodos de cálculo de área por envergadura fixa
        static double AreaAsaRetangular(double b)
        {
            double c = b / 5.0;
            return b * c;
        }

        static double AreaAsaTrapezoidal(double b)
        {
            double cRaiz = b / 4.0;
            double cPonta = b / 8.0;
            return ((cRaiz + cPonta) / 2.0) * b;
        }

        static double AreaAsaEliptica(double b)
        {
            double cRaiz = b / 6.0;
            return (Math.PI / 4.0) * b * cRaiz;
        }

        static double AreaAsaDelta(double b)
        {
            double cRaiz = b / 3.0;
            return (b * cRaiz) / 2.0;
        }

        private double CalculateWingAreaByType(string wingType, double wingspan)
        {
            return wingType switch
            {
                "Rectangular" => AreaAsaRetangular(wingspan),
                "Trapezoidal" => AreaAsaTrapezoidal(wingspan),
                "Elliptical" => AreaAsaEliptica(wingspan),
                "Delta" => AreaAsaDelta(wingspan),
                _ => AreaAsaRetangular(wingspan)
            };
        }

        public void ButtonRunTest_Click(object sender, EventArgs e)
        {
            try
            {
                SyncCheckBoxesWithSelections();
                // Desabilitar o botão durante o processamento
                ButtonRunTest.Enabled = false;
                ButtonRunTest.Text = "Processing...";

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

                double wingArea = GetCurrentWingArea();
                Console.WriteLine($"Calculated area: {wingArea:F4}");

                if (wingArea <= 0)
                {
                    var wingControl1 = GetCurrentWingControl();
                    string errorMsg = "Invalid wing parameters.\n\n";
                    if (wingControl1 != null)
                    {
                        errorMsg += $"Wingspan: {wingControl1.Wingspan:F2} m\n";
                        errorMsg += $"Rope: {wingControl1.Rope:F2} m\n";
                        errorMsg += $"Calculated Area: {wingArea:F4} m²\n\n";
                        errorMsg += "Please check that all fields are filled correctly.";
                    }

                    MessageBox.Show(errorMsg, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get wing type and camera perspective
                string selectedAirfoil = (ComboBoxAirFoil.SelectedItem?.ToString() ?? "").Trim();
                if (string.IsNullOrEmpty(selectedAirfoil))
                {
                    MessageBox.Show("Please select an airfoil type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                double angleOfAttack = TrackBarAngleAttack.Value; // Assuming TrackBarAngleOfAttack is the slider
                                                                  // Linha corrigida:
                (double clCoefficient, double cdCoefficient) = GetCoefficientsInterpolated(selectedAirfoil, angleOfAttack);
                lastClCoefficient = clCoefficient;
                lastCdCoefficient = cdCoefficient;


                if (clCoefficient == 0.0 && !airfoilData.ContainsKey(selectedAirfoil))
                {
                    // Error message already shown by GetClInterpolated
                    return;
                }

                string wingType = ComboWingType.SelectedItem?.ToString() ?? "Rectangular";
                var keys = wingTypeSelections.Keys.ToList(); // Pega uma cópia das chaves para iterar com segurança
                foreach (var key in keys)
                {
                    wingTypeSelections[key] = false;
                }

                // 2. Define o tipo de asa ATUAL como o principal da simulação.
                this.currentSimulatedWingType = wingType;

                // 3. Marca APENAS o tipo de asa que acabou de ser testado.
                this.wingTypeSelections[wingType] = true;

                // 4. Sincroniza a interface (os checkboxes) para refletir a limpeza e a nova seleção.
                SyncCheckBoxesWithSelections();
                string cameraPerspective = ComboCameraPerspective.SelectedItem?.ToString() ?? "";

                // Atualizar o tipo de asa sendo simulado
                currentSimulatedWingType = wingType;

                // Calculato
                double liftForce = CalculateLiftForce(airDensity, windSpeed, wingArea, clCoefficient);
                double dragForce = CalculateLiftForce(airDensity, windSpeed, wingArea, cdCoefficient); // Reutiliza a mesma fórmula
                double efficiency = 0;
                if (dragForce > 0.0001) // Evita divisão por zero
                {
                    efficiency = liftForce / dragForce;
                }
                // Show result
                guna2HtmlLabelForceValue.Text = $"{liftForce:F2} N";
                guna2HtmlLabelDragValue.Text = $"{dragForce:F2} N"; // Label para o arrasto
                guna2HtmlLabelLiftCoeffValue.Text = $"{clCoefficient:F2}"; // Label para o CL
                guna2HtmlLabelDragCoeffValue.Text = $"{cdCoefficient:F2}"; // Label para o CD
                guna2HtmlLabelEfficiencyValue.Text = $"{(efficiency > 0 ? efficiency.ToString("F2") : "N/A")}"; // Label para a eficiência

                // Mostrar informações detalhadas da asa
                ShowWingDetails();
                ShowAllCharts(airDensity, windSpeed, wingArea, selectedAirfoil, clCoefficient, cdCoefficient);
                UpdateProfileCharts();
                // Save to database with wing parameters
                var wingControl = GetCurrentWingControl();
                // Dentro do método public void ButtonRunTest_Click(...) no seu Form1.cs

                // Encontre este bloco:
                if (wingControl != null)
                {
                    // SUBSTITUA a linha ClassResults.SaveTestResult(...) por esta chamada completa:
                    ClassResults.SaveTestResult(
                        wingType,
                        selectedAirfoil,      // Passando o aerofólio
                        angleOfAttack,        // Passando o ângulo de ataque
                        windSpeed,
                        airDensity,
                        wingArea,
                        clCoefficient,
                        cdCoefficient,        // Passando o coeficiente de arrasto
                        liftForce,
                        dragForce,            // Passando a força de arrasto
                        efficiency,           // Passando a eficiência
                        cameraPerspective,
                        wingControl.Wingspan,
                        wingControl.Rope,
                        (wingControl is TrapezoidalWingControl trapControl) ? trapControl.RopeAtRoot : 0,
                        (wingControl is TrapezoidalWingControl trapControl2) ? trapControl2.RopeAtEnd : 0
                    );
                }
                LoadVRMLModel(sender, e);
                

                PageGraficoEnabled = true;
                ResultsForm resultsForm = new ResultsForm();
                resultsForm.UpdateResults();

                // Restaurar o botão
                ButtonRunTest.Text = "Run Test";
                ButtonRunTest.Enabled = true;
                
                // Verificar campos novamente após o teste
                CheckFieldsFilled(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during test: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Erro no ButtonRunTest_Click: {ex.Message}");

                // Restaurar o botão em caso de erro
                ButtonRunTest.Text = "Run Test";
                ButtonRunTest.Enabled = true;
                
                // Verificar campos novamente após erro
                CheckFieldsFilled(null, null);
            }
            UpdateProfileCharts();
        }
        private async void UpdateProfileCharts()
        {
            // Só atualiza se os gráficos já foram carregados pelo menos uma vez
            if (!PageGraficoEnabled || webViewChart.CoreWebView2 == null) return;

            if (ComboBoxAirFoil.SelectedItem == null) return;
            string selectedAirfoil = ComboBoxAirFoil.SelectedItem.ToString();

            // Gera os novos dados para cada gráfico do perfil
            string liftCurveData = GenerateProfileCharacteristicData(selectedAirfoil, ChartType.LiftCurve);
            string dragPolarData = GenerateProfileCharacteristicData(selectedAirfoil, ChartType.DragPolar);
            string efficiencyData = GenerateProfileCharacteristicData(selectedAirfoil, ChartType.Efficiency);

            // Usa JavaScript para atualizar as séries de dados dos gráficos sem recarregar a página
            // O 'try...catch' no JS evita erros caso o gráfico ainda não esteja 100% pronto
            await webViewChart.ExecuteScriptAsync($@"
        try {{
            charts.chart2.series[0].update({{ name: '{selectedAirfoil}' }});
            charts.chart2.series[0].setData({liftCurveData}[0].data);
            charts.chart2.setTitle(null, {{ text: 'Perfil: {selectedAirfoil}' }});

            charts.chart3.series[0].update({{ name: '{selectedAirfoil}' }});
            charts.chart3.series[0].setData({dragPolarData}[0].data);
            charts.chart3.setTitle(null, {{ text: 'Perfil: {selectedAirfoil}' }});

            charts.chart4.series[0].update({{ name: '{selectedAirfoil}' }});
            charts.chart4.series[0].setData({efficiencyData}[0].data);
            charts.chart4.setTitle(null, {{ text: 'Perfil: {selectedAirfoil}' }});
        }} catch (e) {{
            console.log('Error updating charts: ' + e);
        }}
    ");
        }

        private void RecalculateChart()
        {
            // Só recalcula se um teste já foi executado
            if (!PageGraficoEnabled) return;

            try
            {
                // Pega os parâmetros do último teste válido
                double density = double.Parse(ComboAirDensity.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
                double maxSpeed = double.Parse(ComboWindSpeed.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
                string selectedAirfoil = ComboBoxAirFoil.SelectedItem?.ToString() ?? "";

                // Se não houver airfoil selecionado, não faz nada
                if (string.IsNullOrEmpty(selectedAirfoil)) return;

                // Chama a rotina principal de gráficos com os dados do último teste,
                // mas a nova seleção de wing types será usada por GenerateForceVsSpeedData
                ShowAllCharts(density, maxSpeed, 0, selectedAirfoil, lastClCoefficient, lastCdCoefficient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recalculating chart: {ex.Message}");
            }
        }




        private void CheckFieldsFilled(object? sender, EventArgs e)
        {
            try
            {
                // Verificar se o botão não está em processamento
                if (ButtonRunTest.Text == "Processing...")
                {
                    return; // Não alterar estado durante processamento
                }

                bool windSpeedValid = !string.IsNullOrWhiteSpace(ComboWindSpeed.Text);
                bool airDensityValid = !string.IsNullOrWhiteSpace(ComboAirDensity.Text);
                bool wingAreaValid = GetCurrentWingArea() > 0;

                // Verificar também os ComboBoxes dos UserControls
                bool userControlValid = CheckUserControlFields();

                bool allFieldsValid = windSpeedValid && airDensityValid && wingAreaValid && userControlValid;
                
                // Só alterar o estado se realmente mudou
                if (ButtonRunTest.Enabled != allFieldsValid)
                {
                    ButtonRunTest.Enabled = allFieldsValid;
                    ButtonRunTest.Text = "Run Test"; // Garantir que o texto esteja correto
                }

                // Debug apenas quando necessário (comentado para performance)
                Console.WriteLine($"WindSpeed: {windSpeedValid}, AirDensity: {airDensityValid}, WingArea: {wingAreaValid}, UserControl: {userControlValid} (Area: {GetCurrentWingArea():F2})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                // Em caso de erro, tentar reabilitar o botão se não estiver processando
                if (ButtonRunTest.Text != "Processing...")
                {
                    ButtonRunTest.Enabled = false;
                    ButtonRunTest.Text = "Run Test";
                }
            }
        }

        private bool CheckUserControlFields()
        {
            try
            {
                if (panelWingArea.Controls.Count == 0)
                    return false;

                var control = panelWingArea.Controls[0];

                if (control is RectangularWingControl rectControl)
                {
                    // CORRIGIDO: Checando os controles corretos
                    bool wingspanValid = !string.IsNullOrWhiteSpace(rectControl.ComboWingspan.Text);
                    bool ropeValid = !string.IsNullOrWhiteSpace(rectControl.ComboRope.Text);
                    return wingspanValid && ropeValid;
                }
                else if (control is TrapezoidalWingControl trapControl)
                {
                    bool wingspanValid = !string.IsNullOrWhiteSpace(trapControl.ComboWingspan.Text);
                    bool ropeRootValid = !string.IsNullOrWhiteSpace(trapControl.ComboRopeAtRoot.Text);
                    bool ropeEndValid = !string.IsNullOrWhiteSpace(trapControl.ComboRopeAtEnd.Text);
                    return wingspanValid && ropeRootValid && ropeEndValid;
                }
                else if (control is EllipticalWingControl ellipControl)
                {
                    bool wingspanValid = !string.IsNullOrWhiteSpace(ellipControl.ComboWingspan.Text);
                    bool ropeValid = !string.IsNullOrWhiteSpace(ellipControl.ComboRope.Text);
                    return wingspanValid && ropeValid;
                }
                else if (control is DeltaWingControl deltaControl)
                {
                    bool wingspanValid = !string.IsNullOrWhiteSpace(deltaControl.ComboWingspan.Text);
                    bool ropeValid = !string.IsNullOrWhiteSpace(deltaControl.ComboRope.Text);
                    return wingspanValid && ropeValid;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar campos do UserControl: {ex.Message}");
                return false;
            }
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
                    MessageBox.Show("Please select a wing type.", "Notice",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                selectedWingType = ComboWingType.SelectedItem.ToString() ?? string.Empty;
                LabelWingType.Text = selectedWingType;
                string imagePath = GetImagePath(selectedWingType, viewType!);
                if (string.IsNullOrEmpty(imagePath))
                {
                    MessageBox.Show($"Wing type '{selectedWingType}' not recognized.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!File.Exists(imagePath))
                {
                    MessageBox.Show($"Image not found: {imagePath}", "Error",
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
                MessageBox.Show("Image file not found.", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error loading image: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetImagePath(string wingType, string viewType)
        {
            string basePath = @"C:\WINDWORKS\Prints";

            return wingType switch
            {
                "Rectangular" => Path.Combine(basePath, "Reta", $"{viewType}.png"),
                "Elliptical" => Path.Combine(basePath, "Eliptica", $"{viewType}.png"),
                "Trapezoidal" => Path.Combine(basePath, "Trapezoidal", $"{viewType}.png"),
                "Delta" => Path.Combine(basePath, "Delta", $"{viewType}.png"),
                _ => string.Empty
            };
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {

            if (e.TabPage == PageGrafico && !PageGraficoEnabled)
            {
                e.Cancel = true;
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


        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Arquivos XML (*.xml)|*.xml";
                openFileDialog.Title = "Selecione um arquivo XML";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string caminhoArquivo = openFileDialog.FileName;

                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = "rundll32.exe",
                            Arguments = "shell32.dll,OpenAs_RunDLL " + caminhoArquivo,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening: " + ex.Message);
                    }
                }
            }
        }

        private void resultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResultsForm resultsForm = this.ResultsForm;
            if (resultsForm == null || resultsForm.IsDisposed)
            {
                resultsForm = new ResultsForm();
                resultsForm.Owner = this;
            }
            resultsForm.Show();
            resultsForm.BringToFront();
        }
        public void ReceiveDataFromResults(
    string wingType, string airfoil, string angleOfAttack, string cameraPerspective,
    string windSpeed, string airDensity, string wingspan, string rope,
    string ropeAtRoot, string ropeAtEnd
    )
{
    try
    {
        // 1. Popula os ComboBoxes principais
        ComboWingType.Text = wingType; // Isso vai disparar o evento para carregar o UserControl correto
        ComboBoxAirFoil.Text = airfoil;
        ComboCameraPerspective.Text = cameraPerspective;
        ComboWindSpeed.Text = windSpeed;
        ComboAirDensity.Text = airDensity;

        // 2. Define o Ângulo de Ataque no TrackBar
        if (decimal.TryParse(angleOfAttack, out decimal angle))
        {
            TrackBarAngleAttack.Value = (int)angle;
        }

        // 3. Aguarda um instante para o UserControl ser carregado e então popula seus valores
        this.BeginInvoke((Action)(() => {
            var wingControl = GetCurrentWingControl();
            if (wingControl == null) return;

            // Popula os campos específicos do UserControl carregado
            if (wingControl is RectangularWingControl rectControl)
            {
                rectControl.ComboWingspan.Text = wingspan;
                rectControl.ComboRope.Text = rope;
            }
            else if (wingControl is TrapezoidalWingControl trapControl)
            {
                trapControl.ComboWingspan.Text = wingspan;
                trapControl.ComboRopeAtRoot.Text = ropeAtRoot;
                trapControl.ComboRopeAtEnd.Text = ropeAtEnd;
            }
            else if (wingControl is EllipticalWingControl ellipControl)
            {
                ellipControl.ComboWingspan.Text = wingspan;
                ellipControl.ComboRope.Text = rope;
            }
            else if (wingControl is DeltaWingControl deltaControl)
            {
                deltaControl.ComboWingspan.Text = wingspan;
                deltaControl.ComboRope.Text = rope;
            }
        }));
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading test data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        // No seu Form1.cs

        private void xmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Cria e mostra a nossa nova janela de diálogo customizada
                using (var choiceForm = new ExportChoiceForm())
                {
                    DialogResult choice = choiceForm.ShowDialog();

                    if (choice == DialogResult.Cancel)
                        return;

                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Arquivos XML (*.xml)|*.xml";
                        saveFileDialog.Title = "Salvar arquivo XML";
                        saveFileDialog.FileName = choice == DialogResult.Yes ? // "Yes" corresponde ao botão "Export Current"
                            $"TestResult_{DateTime.Now:yyyyMMdd_HHmmss}.xml" :
                            $"AllResults_{DateTime.Now:yyyyMMdd_HHmmss}.xml";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            string filePath = saveFileDialog.FileName;

                            if (choice == DialogResult.Yes)
                            {
                                ExportCurrentTestToXml(filePath);
                            }
                            else // DialogResult.No corresponde ao botão "Export All"
                            {
                                ExportAllResultsToXml(filePath);
                            }

                            MessageBox.Show($"Data exported successfully to:\n{filePath}",
                                "Export Completed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            if (MessageBox.Show("Do you want to open the exported XML file?",
                                "Open File",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                Process.Start(new ProcessStartInfo()
                                {
                                    FileName = filePath,
                                    UseShellExecute = true
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting XML: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private string GetSafeValue(MySqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? "" : reader.GetValue(ordinal).ToString()!;
            }
            catch
            {
                return "";
            }
        }

        private void AddXmlElement(XmlDocument doc, XmlElement parent, string elementName, string elementValue)
        {
            XmlElement element = doc.CreateElement(elementName);
            element.InnerText = elementValue ?? "";
            parent.AppendChild(element);
        }
        private void LoadWingControl(string type)
        {
            panelWingArea.Controls.Clear();

            UserControl? control = null;
            switch (type)
            {
                case "Rectangular":
                    control = new RectangularWingControl();
                    break;
                case "Elliptical":
                    control = new EllipticalWingControl();
                    break;
                case "Trapezoidal":
                    control = new TrapezoidalWingControl();
                    break;
                case "Delta":
                    control = new DeltaWingControl();
                    break;
                default:
                    control = null; // Or throw an exception, depending on desired behavior
                    break;
            }

            if (control != null)
            {
                control.Dock = DockStyle.Fill;
                panelWingArea.Controls.Add(control);
                PopulateWingControlValues(control);

                // Adicionar evento para verificar campos quando os valores mudarem
                if (control is IWingControl wingControl)
                {
                    // Adicionar eventos para as comboboxes dos UserControls
                    AddWingControlEvents(control);
                }
            }
        }

        private void PopulateWingControlValues(UserControl control)
        {
            try
            {
                // Valores padrão para as comboboxes
                double[] ropeValues = { 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0 };
                double[] wingspanValues = { 5.0, 10.0, 15.0, 20.0, 25.0, 30.0, 35.0, 40.0, 45.0, 50.0 };

                // EM: private void PopulateWingControlValues(UserControl control)
                if (control is RectangularWingControl rectControl)
                {
                    // CORRIGIDO: Usando os nomes corretos (provavelmente ComboWingspan e ComboRope)
                    rectControl.ComboWingspan.Items.Clear();
                    rectControl.ComboRope.Items.Clear();

                    rectControl.ComboWingspan.Items.AddRange(wingspanValues.Cast<object>().ToArray());
                    rectControl.ComboRope.Items.AddRange(ropeValues.Cast<object>().ToArray());

                    if (rectControl.ComboWingspan.Items.Count > 0) rectControl.ComboWingspan.SelectedIndex = 0;
                    if (rectControl.ComboRope.Items.Count > 0) rectControl.ComboRope.SelectedIndex = 0;
                }
                else if (control is TrapezoidalWingControl trapControl)
                {
                    trapControl.ComboWingspan.Items.Clear();
                    trapControl.ComboRopeAtRoot.Items.Clear();
                    trapControl.ComboRopeAtEnd.Items.Clear();

                    trapControl.ComboWingspan.Items.AddRange(wingspanValues.Cast<object>().ToArray());
                    trapControl.ComboRopeAtRoot.Items.AddRange(ropeValues.Cast<object>().ToArray());
                    trapControl.ComboRopeAtEnd.Items.AddRange(ropeValues.Cast<object>().ToArray());

                    if (trapControl.ComboWingspan.Items.Count > 0) trapControl.ComboWingspan.SelectedIndex = 0;
                    if (trapControl.ComboRopeAtRoot.Items.Count > 0) trapControl.ComboRopeAtRoot.SelectedIndex = 0;
                    if (trapControl.ComboRopeAtEnd.Items.Count > 0) trapControl.ComboRopeAtEnd.SelectedIndex = 0;

                    Console.WriteLine($"Trapezoidal: Wingspan={trapControl.ComboWingspan.SelectedItem}, RopeRoot={trapControl.ComboRopeAtRoot.SelectedItem}, RopeEnd={trapControl.ComboRopeAtEnd.SelectedItem}");
                }
                else if (control is EllipticalWingControl ellipControl)
                {
                    ellipControl.ComboWingspan.Items.Clear();
                    ellipControl.ComboRope.Items.Clear();

                    ellipControl.ComboWingspan.Items.AddRange(wingspanValues.Cast<object>().ToArray());
                    ellipControl.ComboRope.Items.AddRange(ropeValues.Cast<object>().ToArray());

                    if (ellipControl.ComboWingspan.Items.Count > 0) ellipControl.ComboWingspan.SelectedIndex = 0;
                    if (ellipControl.ComboRope.Items.Count > 0) ellipControl.ComboRope.SelectedIndex = 0;

                    Console.WriteLine($"Elliptical: Wingspan={ellipControl.ComboWingspan.SelectedItem}, Rope={ellipControl.ComboRope.SelectedItem}");
                }
                else if (control is DeltaWingControl deltaControl)
                {
                    deltaControl.ComboWingspan.Items.Clear();
                    deltaControl.ComboRope.Items.Clear();

                    deltaControl.ComboWingspan.Items.AddRange(wingspanValues.Cast<object>().ToArray());
                    deltaControl.ComboRope.Items.AddRange(ropeValues.Cast<object>().ToArray());

                    if (deltaControl.ComboWingspan.Items.Count > 0) deltaControl.ComboWingspan.SelectedIndex = 0;
                    if (deltaControl.ComboRope.Items.Count > 0) deltaControl.ComboRope.SelectedIndex = 0;

                    Console.WriteLine($"Delta: Wingspan={deltaControl.ComboWingspan.SelectedItem}, Rope={deltaControl.ComboRope.SelectedItem}");
                }

                // Aguardar um pouco para garantir que os valores sejam definidos
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao popular valores: {ex.Message}");
            }
        }

        // Método auxiliar para criar eventos que chamam CheckFieldsFilled
        private void OnUserControlFieldChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"UserControl field changed: {sender.GetType().Name}");
            CheckFieldsFilled(sender, e);
        }

        private void AddWingControlEvents(UserControl control)
        {
            try
            {
                // Remover eventos existentes primeiro para evitar duplicação
                if (control is RectangularWingControl rectControl)
                {
                    rectControl.ComboWingspan.SelectedIndexChanged -= OnUserControlFieldChanged;
                    rectControl.ComboRope.SelectedIndexChanged -= OnUserControlFieldChanged;
                    rectControl.ComboWingspan.TextChanged -= OnUserControlFieldChanged;
                    rectControl.ComboRope.TextChanged -= OnUserControlFieldChanged;

                    rectControl.ComboWingspan.SelectedIndexChanged += OnUserControlFieldChanged;
                    rectControl.ComboRope.SelectedIndexChanged += OnUserControlFieldChanged;
                    rectControl.ComboWingspan.TextChanged += OnUserControlFieldChanged;
                    rectControl.ComboRope.TextChanged += OnUserControlFieldChanged;
                }
                else if (control is TrapezoidalWingControl trapControl)
                {
                    trapControl.ComboWingspan.SelectedIndexChanged -= OnUserControlFieldChanged;
                    trapControl.ComboRopeAtRoot.SelectedIndexChanged -= OnUserControlFieldChanged;
                    trapControl.ComboRopeAtEnd.SelectedIndexChanged -= OnUserControlFieldChanged;
                    trapControl.ComboWingspan.TextChanged -= OnUserControlFieldChanged;
                    trapControl.ComboRopeAtRoot.TextChanged -= OnUserControlFieldChanged;
                    trapControl.ComboRopeAtEnd.TextChanged -= OnUserControlFieldChanged;

                    trapControl.ComboWingspan.SelectedIndexChanged += OnUserControlFieldChanged;
                    trapControl.ComboRopeAtRoot.SelectedIndexChanged += OnUserControlFieldChanged;
                    trapControl.ComboRopeAtEnd.SelectedIndexChanged += OnUserControlFieldChanged;
                    trapControl.ComboWingspan.TextChanged += OnUserControlFieldChanged;
                    trapControl.ComboRopeAtRoot.TextChanged += OnUserControlFieldChanged;
                    trapControl.ComboRopeAtEnd.TextChanged += OnUserControlFieldChanged;
                }
                else if (control is EllipticalWingControl ellipControl)
                {
                    ellipControl.ComboWingspan.SelectedIndexChanged -= OnUserControlFieldChanged;
                    ellipControl.ComboRope.SelectedIndexChanged -= OnUserControlFieldChanged;
                    ellipControl.ComboWingspan.TextChanged -= OnUserControlFieldChanged;
                    ellipControl.ComboRope.TextChanged -= OnUserControlFieldChanged;

                    ellipControl.ComboWingspan.SelectedIndexChanged += OnUserControlFieldChanged;
                    ellipControl.ComboRope.SelectedIndexChanged += OnUserControlFieldChanged;
                    ellipControl.ComboWingspan.TextChanged += OnUserControlFieldChanged;
                    ellipControl.ComboRope.TextChanged += OnUserControlFieldChanged;
                }
                else if (control is DeltaWingControl deltaControl)
                {
                    deltaControl.ComboWingspan.SelectedIndexChanged -= OnUserControlFieldChanged;
                    deltaControl.ComboRope.SelectedIndexChanged -= OnUserControlFieldChanged;
                    deltaControl.ComboWingspan.TextChanged -= OnUserControlFieldChanged;
                    deltaControl.ComboRope.TextChanged -= OnUserControlFieldChanged;

                    deltaControl.ComboWingspan.SelectedIndexChanged += OnUserControlFieldChanged;
                    deltaControl.ComboRope.SelectedIndexChanged += OnUserControlFieldChanged;
                    deltaControl.ComboWingspan.TextChanged += OnUserControlFieldChanged;
                    deltaControl.ComboRope.TextChanged += OnUserControlFieldChanged;
                }
                
                // Verificar campos imediatamente após adicionar eventos
                CheckFieldsFilled(null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao adicionar eventos: {ex.Message}");
            }
        }

        private double GetCurrentWingArea()
        {
            try
            {
                if (panelWingArea.Controls.Count > 0)
                {
                    var control = panelWingArea.Controls[0];
                    if (control is IWingControl wingControl)
                    {
                        double area = wingControl.WingArea;

                        // Debug para identificar problemas
                        if (area <= 0)
                        {
                            Console.WriteLine($"Calculated area: {area}, Wingspan: {wingControl.Wingspan}, Rope: {wingControl.Rope}");
                        }

                        return area;
                    }
                }
                return 0.0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating wing area: {ex.Message}");
                return 0.0;
            }
        }

        private IWingControl? GetCurrentWingControl()
        {
            if (panelWingArea.Controls.Count > 0)
            {
                var control = panelWingArea.Controls[0];
                if (control is IWingControl wingControl)
                {
                    return wingControl;
                }
            }
            return null;
        }

        private void ShowWingDetails()
        {
            var wingControl = GetCurrentWingControl();
            if (wingControl != null)
            {
                string wingType = ComboWingType.SelectedItem?.ToString() ?? "";
                string details = $"Tipo de Asa: {wingType}\n";
                details += $"Envergadura: {wingControl.Wingspan:F2} m\n";
                details += $"Corda: {wingControl.Rope:F2} m\n";

                if (wingControl is TrapezoidalWingControl trapControl)
                {
                    details += $"Corda na Raiz: {trapControl.RopeAtRoot:F2} m\n";
                    details += $"Corda na Ponta: {trapControl.RopeAtEnd:F2} m\n";
                }

                details += $"Wing Area: {wingControl.WingArea:F2} m²";

                // Você pode usar isso para mostrar em um MessageBox ou em um label
                Console.WriteLine(details);
            }
        }

        private void ComboWingType_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string selectedType = ComboWingType.SelectedItem?.ToString() ?? "";
            LoadWingControl(selectedType);
            if (!string.IsNullOrEmpty(selectedType))
            {
                PictureBoxModelImage.Image = Image.FromFile(GetImagePath(selectedType, "Cima"));
                selectedWingType = selectedType;
                LabelWingType.Text = selectedType;
            }
            TestCurrentWingControl();


        }

        private void ComboWingTypeSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var comboBox = sender as Guna.UI2.WinForms.Guna2ComboBox;
                if (comboBox == null) return;

                string wingType = comboBox.Name.Replace("Combo", "");
                bool isSelected = comboBox.SelectedIndex == 0;

                // Verificar se é o tipo que está sendo simulado atualmente
                if (wingType == currentSimulatedWingType && !isSelected)
                {
                    // Não permitir desmarcar o tipo que está sendo simulado
                    comboBox.SelectedIndex = 0;
                    MessageBox.Show($"Cannot uncheck wing type '{wingType}' as it is currently being simulated.",
                                  "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Atualizar o dicionário de seleções
                wingTypeSelections[wingType] = isSelected;

                // Verificar se pelo menos um tipo está selecionado
                if (!wingTypeSelections.Values.Any(selected => selected))
                {
                    comboBox.SelectedIndex = 0;
                    wingTypeSelections[wingType] = true;
                    MessageBox.Show("At least one wing type must be selected.",
                                  "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Recalcular o gráfico se já estiver habilitado
                if (PageGraficoEnabled)
                {
                    RecalculateChart();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing wing type selection: {ex.Message}");
            }
        }

        private void CheckBoxWingType_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                
                var checkBox = sender as CheckBox;
                if (checkBox == null) return;

                // Mapear checkbox para tipo de asa
                string wingType = checkBox.Name switch
                {
                    "checkBox1" => "Rectangular",
                    "checkBox2" => "Trapezoidal",
                    "checkBox3" => "Elliptical",
                    "checkBox4" => "Delta",
                    _ => ""
                };

                if (string.IsNullOrEmpty(wingType)) return;

                // Verificar se é o tipo que está sendo simulado atualmente
                if (wingType == currentSimulatedWingType && !checkBox.Checked)
                {
                    // Não permitir desmarcar o tipo que está sendo simulado
                    checkBox.Checked = true;
                    MessageBox.Show($"Cannot uncheck wing type '{wingType}' as it is currently being simulated.",
                                  "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Atualizar o dicionário de seleções
                wingTypeSelections[wingType] = checkBox.Checked;

                // Verificar se pelo menos um tipo está selecionado
                if (!wingTypeSelections.Values.Any(selected => selected))
                {
                    checkBox.Checked = true;
                    wingTypeSelections[wingType] = true;
                    MessageBox.Show("At least one wing type must be selected.",
                                  "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Recalcular o gráfico se já estiver habilitado
                if (PageGraficoEnabled)
                {
                    RecalculateChart();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing checkbox change: {ex.Message}");
            }
        }

        private void SyncCheckBoxesWithSelections()
        {
            try
            {
                // Sincronizar checkboxes com o estado atual das seleções
                checkBox1.Checked = wingTypeSelections["Rectangular"];
                checkBox2.Checked = wingTypeSelections["Trapezoidal"];
                checkBox3.Checked = wingTypeSelections["Elliptical"];
                checkBox4.Checked = wingTypeSelections["Delta"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao sincronizar checkboxes: {ex.Message}");
            }
        }

        private void TestCurrentWingControl()
        {
            try
            {
                var wingControl = GetCurrentWingControl();
                if (wingControl != null)
                {
                    Console.WriteLine($"=== Teste do UserControl: {wingControl.GetType().Name} ===");
                    Console.WriteLine($"Wingspan: {wingControl.Wingspan:F4}");
                    Console.WriteLine($"Rope: {wingControl.Rope:F4}");
                    Console.WriteLine($"WingArea: {wingControl.WingArea:F4}");
                    Console.WriteLine("=====================================");
                }
                else
                {
                    Console.WriteLine("Nenhum UserControl encontrado!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no teste: {ex.Message}");
            }
        }

       

        private void TrackBarAngleAttack_ValueChanged(object sender, EventArgs e)
        {
            LabelAngleAttack.Text = $"Attack Angle ({TrackBarAngleAttack.Value.ToString()}°)";
        }

        private void ComboBoxAirFoil_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (ComboBoxAirFoil.SelectedItem == null) return;

            try
            {
                string selectedAirfoil = ComboBoxAirFoil.SelectedItem.ToString();
                // Define o caminho base para as imagens dos perfis
                string imagePath = $@"C:\WINDWORKS\WinFormsApp1\AirfoilImages\{selectedAirfoil}.Gif";

                // Limpa a imagem anterior para evitar que fique "presa" se a nova não for encontrada
                if (pictureBoxAirfoilProfile.Image != null)
                {
                    pictureBoxAirfoilProfile.Image.Dispose();
                    pictureBoxAirfoilProfile.Image = null;
                }

                // Verifica se a imagem existe no caminho especificado
                if (File.Exists(imagePath))
                {
                    // Carrega a imagem no PictureBox
                    pictureBoxAirfoilProfile.Image = Image.FromFile(imagePath);
                    // Ajusta a imagem para caber no controle sem distorcer
                    pictureBoxAirfoilProfile.SizeMode = PictureBoxSizeMode.Zoom;
                    
                }
                else
                {
                    // Se a imagem não for encontrada, deixa o PictureBox vazio
                    // e informa o usuário no console (ou com um MessageBox, se preferir).
                    Console.WriteLine($"Imagem para o perfil '{selectedAirfoil}' não encontrada em: {imagePath}");
                    pictureBoxAirfoilProfile.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao carregar a imagem do perfil: {ex.Message}", "Erro de Imagem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pictureBoxAirfoilProfile.Image = null;
            }
            UpdateProfileCharts();
        }

        

        private void LoadAirfoilData()
        {
            string airfoilDataPath = @"c:\WINDWORKS\WinFormsApp1\AirfoilData";
            if (!Directory.Exists(airfoilDataPath))
            {
                MessageBox.Show($"Diretório de dados não encontrado: {airfoilDataPath}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            airfoilData.Clear();
            foreach (string filePath in Directory.GetFiles(airfoilDataPath, "*.csv"))
            {
                string airfoilName = Path.GetFileNameWithoutExtension(filePath).Trim();
                var localProfileData = new Dictionary<double, (double CL, double CD)>();
                try
                {
                    foreach (string line in File.ReadLines(filePath).Skip(1))
                    {
                        string[] parts = line.Split(',');
                        // Trecho novo e corrigido:
                        if (parts.Length == 3 &&
                            double.TryParse(parts[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double angle) &&
                            double.TryParse(parts[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double cl) &&
                            double.TryParse(parts[2].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double cd))
                        {
                            localProfileData[angle] = (cl, cd);
                        }
                    }
                    airfoilData[airfoilName] = localProfileData;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao ler o arquivo {airfoilName}.csv: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void ExportCurrentTestToXml(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

            XmlElement root = xmlDoc.CreateElement("LiftForceTest");
            xmlDoc.AppendChild(root);

            // Informações do teste
            XmlElement testInfo = xmlDoc.CreateElement("TestInformation");
            root.AppendChild(testInfo);
            AddXmlElement(xmlDoc, testInfo, "Application", "Wind Force Calculator");
            AddXmlElement(xmlDoc, testInfo, "Version", "1.0");
            AddXmlElement(xmlDoc, testInfo, "ExportDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AddXmlElement(xmlDoc, testInfo, "ExportType", "CurrentTest");

            // Dados do teste atual
            XmlElement testData = xmlDoc.CreateElement("TestData");
            root.AppendChild(testData);

            // MANTIDO: Configurações originais
            AddXmlElement(xmlDoc, testData, "WingType", ComboWingType.Text);
            AddXmlElement(xmlDoc, testData, "Airfoil", ComboBoxAirFoil.Text);
            AddXmlElement(xmlDoc, testData, "AngleOfAttack", TrackBarAngleAttack.Value.ToString());
            AddXmlElement(xmlDoc, testData, "CameraPerspective", ComboCameraPerspective.Text);
            AddXmlElement(xmlDoc, testData, "WindSpeed", ComboWindSpeed.Text);
            AddXmlElement(xmlDoc, testData, "AirDensity", ComboAirDensity.Text);
            AddXmlElement(xmlDoc, testData, "WingArea", GetCurrentWingArea().ToString("F2", CultureInfo.InvariantCulture));

            // MANTIDO: Coeficientes
            AddXmlElement(xmlDoc, testData, "LiftCoefficient_CL", lastClCoefficient.ToString("F4", CultureInfo.InvariantCulture));
            AddXmlElement(xmlDoc, testData, "DragCoefficient_CD", lastCdCoefficient.ToString("F4", CultureInfo.InvariantCulture));

            // MANTIDO: Forças
            AddXmlElement(xmlDoc, testData, "LiftForce", guna2HtmlLabelForceValue.Text);
            AddXmlElement(xmlDoc, testData, "DragForce", guna2HtmlLabelDragValue.Text);
            AddXmlElement(xmlDoc, testData, "Efficiency_LD", guna2HtmlLabelEfficiencyValue.Text);
            AddXmlElement(xmlDoc, testData, "TestDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // ADICIONADO: Parâmetros da asa (NOVO - estava faltando!)
            XmlElement wingParams = xmlDoc.CreateElement("WingParameters");
            testData.AppendChild(wingParams);

            var wingControl = GetCurrentWingControl();
            if (wingControl != null)
            {
                AddXmlElement(xmlDoc, wingParams, "Wingspan", wingControl.Wingspan.ToString("F2", CultureInfo.InvariantCulture));
                AddXmlElement(xmlDoc, wingParams, "Rope", wingControl.Rope.ToString("F2", CultureInfo.InvariantCulture));

                if (wingControl is TrapezoidalWingControl trapControl)
                {
                    AddXmlElement(xmlDoc, wingParams, "RopeAtRoot", trapControl.RopeAtRoot.ToString("F2", CultureInfo.InvariantCulture));
                    AddXmlElement(xmlDoc, wingParams, "RopeAtEnd", trapControl.RopeAtEnd.ToString("F2", CultureInfo.InvariantCulture));
                }
            }

            // MANTIDO: Cálculo detalhado
            XmlElement calculation = xmlDoc.CreateElement("DetailedCalculation");
            testData.AppendChild(calculation);

            if (double.TryParse(ComboAirDensity.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double density) &&
                double.TryParse(ComboWindSpeed.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double speed))
            {
                double area = GetCurrentWingArea();
                double calculatedForce = CalculateLiftForce(density, speed, area, lastClCoefficient);

                AddXmlElement(xmlDoc, calculation, "LiftForceCalculation", $"F_L = 0.5 * {density} * {speed}² * {area:F2} * {lastClCoefficient:F4} = {calculatedForce:F2} N");

                // ADICIONADO: Cálculo do arrasto (NOVO - estava faltando!)
                double calculatedDrag = CalculateLiftForce(density, speed, area, lastCdCoefficient);
                AddXmlElement(xmlDoc, calculation, "DragForceCalculation", $"F_D = 0.5 * {density} * {speed}² * {area:F2} * {lastCdCoefficient:F4} = {calculatedDrag:F2} N");
            }

            xmlDoc.Save(filePath);
        }

        private void ExportAllResultsToXml(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

            XmlElement root = xmlDoc.CreateElement("LiftForceTestResults");
            xmlDoc.AppendChild(root);

            // MANTIDO: Informações da exportação
            XmlElement exportInfo = xmlDoc.CreateElement("ExportInformation");
            root.AppendChild(exportInfo);
            AddXmlElement(xmlDoc, exportInfo, "ExportDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AddXmlElement(xmlDoc, exportInfo, "ExportType", "AllResults");
            AddXmlElement(xmlDoc, exportInfo, "Application", "Lift Force Calculator");
            AddXmlElement(xmlDoc, exportInfo, "Version", "1.0");

            // MANTIDO: Container para todos os testes
            XmlElement testsContainer = xmlDoc.CreateElement("Tests");
            root.AppendChild(testsContainer);

            try
            {
                List<string> availableColumns = new List<string>();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // MANTIDO: Verificar estrutura da tabela
                    string checkColumnsQuery = "DESCRIBE TestResults";
                    using (MySqlCommand checkCommand = new MySqlCommand(checkColumnsQuery, connection))
                    {
                        using (MySqlDataReader checkReader = checkCommand.ExecuteReader())
                        {
                            while (checkReader.Read())
                            {
                                availableColumns.Add(checkReader["Field"]?.ToString() ?? string.Empty);
                            }
                        }
                    }

                    // MANTIDO: Construir query
                    string baseQuery = "SELECT ";
                    List<string> selectColumns = new List<string>();

                    // MANTIDO: Colunas obrigatórias
                    string[] requiredColumns = { "Id", "WingType", "WindSpeed", "AirDensity", "WingArea", "LiftForce", "TestDate" };
                    foreach (string col in requiredColumns)
                    {
                        if (availableColumns.Contains(col))
                            selectColumns.Add(col);
                    }

                    // MELHORADO: Mais colunas opcionais
                    string[] optionalColumns = {
                "CameraPerspective", "LiftCoefficient", "Coefficient",
                "Wingspan", "Rope", "RopeAtRoot", "RopeAtEnd",
                "Airfoil", "AngleOfAttack", "DragCoefficient", "DragForce", "Efficiency" // ADICIONADO
            };
                    foreach (string col in optionalColumns)
                    {
                        if (availableColumns.Contains(col))
                            selectColumns.Add(col);
                    }

                    if (selectColumns.Count == 0)
                    {
                        throw new Exception("Nenhuma coluna reconhecida encontrada na tabela TestResults");
                    }

                    string query = baseQuery + string.Join(", ", selectColumns) + " FROM TestResults ORDER BY TestDate DESC";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            int testCount = 0;
                            while (reader.Read())
                            {
                                testCount++;
                                XmlElement test = xmlDoc.CreateElement("Test");
                                test.SetAttribute("id", GetSafeValue(reader, "Id"));
                                test.SetAttribute("number", testCount.ToString());
                                testsContainer.AppendChild(test);

                                // MANTIDO: Dados básicos do teste
                                XmlElement basicData = xmlDoc.CreateElement("BasicData");
                                test.AppendChild(basicData);

                                AddXmlElement(xmlDoc, basicData, "WingType", GetSafeValue(reader, "WingType"));

                                // ADICIONADO: Airfoil e AngleOfAttack (NOVO)
                                if (availableColumns.Contains("Airfoil"))
                                    AddXmlElement(xmlDoc, basicData, "Airfoil", GetSafeValue(reader, "Airfoil"));

                                if (availableColumns.Contains("AngleOfAttack"))
                                    AddXmlElement(xmlDoc, basicData, "AngleOfAttack", GetSafeValue(reader, "AngleOfAttack"));

                                if (availableColumns.Contains("CameraPerspective"))
                                    AddXmlElement(xmlDoc, basicData, "CameraPerspective", GetSafeValue(reader, "CameraPerspective"));

                                if (availableColumns.Contains("TestDate"))
                                {
                                    string dateValue = GetSafeValue(reader, "TestDate");
                                    if (DateTime.TryParse(dateValue, out DateTime testDate))
                                        AddXmlElement(xmlDoc, basicData, "TestDate", testDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                    else
                                        AddXmlElement(xmlDoc, basicData, "TestDate", dateValue);
                                }

                                // MANTIDO: Parâmetros de entrada
                                XmlElement inputParams = xmlDoc.CreateElement("InputParameters");
                                test.AppendChild(inputParams);

                                AddXmlElement(xmlDoc, inputParams, "WindSpeed", GetSafeValue(reader, "WindSpeed"));
                                AddXmlElement(xmlDoc, inputParams, "WindSpeedUnit", "m/s");
                                AddXmlElement(xmlDoc, inputParams, "AirDensity", GetSafeValue(reader, "AirDensity"));
                                AddXmlElement(xmlDoc, inputParams, "AirDensityUnit", "kg/m³");
                                AddXmlElement(xmlDoc, inputParams, "WingArea", GetSafeValue(reader, "WingArea"));
                                AddXmlElement(xmlDoc, inputParams, "WingAreaUnit", "m²");

                                // MANTIDO: Parâmetros da asa
                                if (availableColumns.Contains("Wingspan"))
                                {
                                    AddXmlElement(xmlDoc, inputParams, "Wingspan", GetSafeValue(reader, "Wingspan"));
                                    AddXmlElement(xmlDoc, inputParams, "WingspanUnit", "m");
                                }
                                if (availableColumns.Contains("Rope"))
                                {
                                    AddXmlElement(xmlDoc, inputParams, "Rope", GetSafeValue(reader, "Rope"));
                                    AddXmlElement(xmlDoc, inputParams, "RopeUnit", "m");
                                }
                                if (availableColumns.Contains("RopeAtRoot"))
                                {
                                    AddXmlElement(xmlDoc, inputParams, "RopeAtRoot", GetSafeValue(reader, "RopeAtRoot"));
                                    AddXmlElement(xmlDoc, inputParams, "RopeAtRootUnit", "m");
                                }
                                if (availableColumns.Contains("RopeAtEnd"))
                                {
                                    AddXmlElement(xmlDoc, inputParams, "RopeAtEnd", GetSafeValue(reader, "RopeAtEnd"));
                                    AddXmlElement(xmlDoc, inputParams, "RopeAtEndUnit", "m");
                                }

                                // CORRIGIDO: Coeficiente (a lógica estava duplicada!)
                                string coefficientValue = "N/A";
                                if (availableColumns.Contains("LiftCoefficient"))
                                    coefficientValue = GetSafeValue(reader, "LiftCoefficient");
                                else if (availableColumns.Contains("Coefficient"))
                                    coefficientValue = GetSafeValue(reader, "Coefficient");

                                AddXmlElement(xmlDoc, inputParams, "Coefficient", coefficientValue);

                                // ADICIONADO: DragCoefficient (NOVO - estava faltando!)
                                if (availableColumns.Contains("DragCoefficient"))
                                    AddXmlElement(xmlDoc, inputParams, "DragCoefficient", GetSafeValue(reader, "DragCoefficient"));

                                // MANTIDO: Resultado
                                XmlElement result = xmlDoc.CreateElement("Result");
                                test.AppendChild(result);

                                AddXmlElement(xmlDoc, result, "LiftForce", GetSafeValue(reader, "LiftForce"));
                                AddXmlElement(xmlDoc, result, "LiftForceUnit", "N");

                                // ADICIONADO: DragForce e Efficiency (NOVO - estava faltando!)
                                if (availableColumns.Contains("DragForce"))
                                {
                                    AddXmlElement(xmlDoc, result, "DragForce", GetSafeValue(reader, "DragForce"));
                                    AddXmlElement(xmlDoc, result, "DragForceUnit", "N");
                                }

                                if (availableColumns.Contains("Efficiency"))
                                    AddXmlElement(xmlDoc, result, "Efficiency", GetSafeValue(reader, "Efficiency"));

                                // MANTIDO: Fórmula
                                XmlElement formula = xmlDoc.CreateElement("Formula");
                                result.AppendChild(formula);
                                AddXmlElement(xmlDoc, formula, "Expression", "F = 0.5 × ρ × v² × S × Cl");
                                AddXmlElement(xmlDoc, formula, "Description", "Lift Force = 0.5 × Air Density × Wind Speed² × Wing Area × Lift Coefficient");
                            }

                            // MANTIDO: Estatísticas
                            XmlElement statistics = xmlDoc.CreateElement("Statistics");
                            root.AppendChild(statistics);
                            AddXmlElement(xmlDoc, statistics, "TotalTests", testCount.ToString());
                            AddXmlElement(xmlDoc, statistics, "DatabaseColumns", string.Join(", ", availableColumns));
                            AddXmlElement(xmlDoc, statistics, "ExportDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // MANTIDO: Tratamento de erro
                XmlElement errorInfo = xmlDoc.CreateElement("Error");
                testsContainer.AppendChild(errorInfo);
                AddXmlElement(xmlDoc, errorInfo, "Message", "Erro ao acessar banco de dados");
                AddXmlElement(xmlDoc, errorInfo, "Details", ex.Message);
                AddXmlElement(xmlDoc, errorInfo, "Note", "Export performed without database data");
            }

            xmlDoc.Save(filePath);
        }



        private (double CL, double CD) GetCoefficientsInterpolated(string airfoilName, double angleOfAttack)
        {
            if (!airfoilData.ContainsKey(airfoilName))
            {
                MessageBox.Show($"Dados para o perfil '{airfoilName}' não foram encontrados.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (0.0, 0.0);
            }
            var specificProfileData = airfoilData[airfoilName];
            var angles = specificProfileData.Keys.OrderBy(a => a).ToList();

            if (angleOfAttack < angles.First()) return specificProfileData[angles.First()];
            if (angleOfAttack > angles.Last()) return specificProfileData[angles.Last()];

            for (int i = 0; i < angles.Count - 1; i++)
            {
                double x1 = angles[i];
                double x2 = angles[i + 1];
                if (angleOfAttack >= x1 && angleOfAttack <= x2)
                {
                    var p1 = specificProfileData[x1];
                    var p2 = specificProfileData[x2];
                    double cl = p1.CL + (angleOfAttack - x1) * (p2.CL - p1.CL) / (x2 - x1);
                    double cd = p1.CD + (angleOfAttack - x1) * (p2.CD - p1.CD) / (x2 - x1);
                    return (cl, cd);
                }
            }
            return (0.0, 0.0);
        }
        
    }
}
