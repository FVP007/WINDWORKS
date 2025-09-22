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
        
        // Variáveis para controle dos ComboBoxes de seleção de tipos de asa
        private Dictionary<string, bool> wingTypeSelections = new Dictionary<string, bool>
        {
            { "Rectangular", true },
            { "Trapezoidal", false },
            { "Elliptical", false },
            { "Delta", false }
        };
        private string currentSimulatedWingType = "Rectangular";
        // Responsive layout variables
        private Size originalFormSize;
        private bool isResponsiveInitialized = false;
        ResultsForm ResultsForm = new ResultsForm();
        // Adicione estas propriedades na classe Form1:
        private RectangularWingControl rectangularControl;
        private EllipticalWingControl ellipticalControl;
        private TrapezoidalWingControl trapezoidalControl;
        private DeltaWingControl deltaControl;

        private Process vrmlProcess = null;

        public Form1()
        {
            InitializeComponent();
            ResultsForm resultsForm = new ResultsForm();
            // Existing setup
            ComboWindSpeed.DropDownStyle = ComboBoxStyle.DropDown;
            ComboAirDensity.DropDownStyle = ComboBoxStyle.DropDown;
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
            ComboWingType.SelectedIndex = 0;
            ComboCameraPerspective.SelectedIndex = 0;
            ComboWindSpeed.SelectedIndex = 0;
            ComboAirDensity.SelectedIndex = 0;
            ComboWindSpeed.SelectedIndexChanged += CheckFieldsFilled;
            ComboAirDensity.SelectedIndexChanged += CheckFieldsFilled;
            
            // Carregar o primeiro UserControl (Rectangular)
            LoadWingControl("Rectangular");
            
            // Sincronizar checkboxes com as seleções iniciais
            SyncCheckBoxesWithSelections();
            
            originalFormSize = this.Size;
            isResponsiveInitialized = true;


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

        private string GetFormulaHtmlForWingType(string wingType)
        {
            string mathJaxHeader = @"
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>
        <script type='text/javascript' id='MathJax-script' async
            src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'>
        </script>
        <style>
            html, body {
                margin: 0;
                padding: 0;
                height: 100%;
                overflow: hidden;
            }
            body { 
                font-family: 'Segoe UI', Arial, sans-serif;
                color: #333;
                display: flex;
                justify-content: center;
                align-items: center;
                padding: 0;
                background: #f5f5f5;
            }
            .formula {
                background: white;
                border-radius: 8px;
                padding: 20px;
                text-align: center;
                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                max-width: 90%;
                width: auto;
            }
            .description {
                margin-top: 15px;
                font-size: 0.9em;
                line-height: 1.6;
            }
            .mjx-chtml {
                margin: 0 !important;
            }
        </style>
    </head>";

            string formulaBody = wingType switch
            {
                "Rectangular" => @"
        <div class='formula'>
            $$ S = b \times c $$
            <div class='description'>
                <b>S</b>: Área da asa<br>
                <b>b</b>: Envergadura<br>
                <b>c</b>: Corda
            </div>
        </div>",

                "Trapezoidal" => @"
        <div class='formula'>
            $$ S = \frac{c_{root} + c_{tip}}{2} \times b $$
            <div class='description'>
                <b>c<sub>root</sub></b>: Corda na raiz<br>
                <b>c<sub>tip</sub></b>: Corda na ponta<br>
                <b>b</b>: Envergadura
            </div>
        </div>",

                "Elliptical" => @"
        <div class='formula'>
            $$ S = \frac{\pi}{4} \times b \times c_{root} $$
            <div class='description'>
                <b>π</b>: Pi<br>
                <b>b</b>: Envergadura<br>
                <b>c<sub>root</sub></b>: Corda na raiz
            </div>
        </div>",

                "Delta" => @"
        <div class='formula'>
            $$ S = \frac{b \times c_{root}}{2} $$
            <div class='description'>
                <b>b</b>: Envergadura<br>
                <b>c<sub>root</sub></b>: Corda na raiz
            </div>
        </div>",

                _ => @"<div class='formula'>Selecione um tipo de asa para ver a fórmula.</div>"
            };

            return $@"<!DOCTYPE html>
<html lang='pt-BR'>
    {mathJaxHeader}
    <body>{formulaBody}</body>
</html>";
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
                // Desabilitar o botão durante o processamento
                ButtonRunTest.Enabled = false;
                ButtonRunTest.Text = "Processando...";

                // Get values from interface fields
                if (!double.TryParse(ComboWindSpeed.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double windSpeed))
                {
                    MessageBox.Show("Por favor, insira um valor válido para a Velocidade do Vento", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!double.TryParse(ComboAirDensity.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double airDensity))
                {
                    MessageBox.Show("Por favor, insira um valor válido para a Densidade do Ar.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                double wingArea = GetCurrentWingArea();
                Console.WriteLine($"Área calculada: {wingArea:F4}");

                if (wingArea <= 0)
                {
                    var wingControl1 = GetCurrentWingControl();
                    string errorMsg = "Parâmetros da asa inválidos.\n\n";
                    if (wingControl1 != null)
                    {
                        errorMsg += $"Envergadura: {wingControl1.Wingspan:F2} m\n";
                        errorMsg += $"Corda: {wingControl1.Rope:F2} m\n";
                        errorMsg += $"Área calculada: {wingArea:F4} m²\n\n";
                        errorMsg += "Verifique se todos os campos estão preenchidos corretamente.";
                    }

                    MessageBox.Show(errorMsg, "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get wing type and camera perspective
                string wingType = ComboWingType.SelectedItem?.ToString() ?? "Rectangular";
                string cameraPerspective = ComboCameraPerspective.SelectedItem?.ToString() ?? "";

                // Atualizar o tipo de asa sendo simulado
                currentSimulatedWingType = wingType;

                // Calculate lift force
                double liftForce = CalculateLiftForce(airDensity, windSpeed, wingArea, coefficient);

                // Show result
                guna2HtmlLabelLiftForceValue.Text = $"{liftForce:F2}N";

                // Mostrar informações detalhadas da asa
                ShowWingDetails();

                            // Save to database with wing parameters
            var wingControl = GetCurrentWingControl();
            if (wingControl != null)
            {
                ClassResults.SaveTestResult(
                    wingType, 
                    windSpeed, 
                    airDensity, 
                    wingArea, 
                    coefficient, 
                    liftForce, 
                    cameraPerspective,
                    wingControl.Wingspan,
                    wingControl.Rope,
                    wingControl is TrapezoidalWingControl trapControl ? trapControl.RopeAtRoot : 0,
                    wingControl is TrapezoidalWingControl trapControl2 ? trapControl2.RopeAtEnd : 0
                );
            }
            else
            {
                // Fallback para compatibilidade
                ClassResults.SaveTestResult(wingType, windSpeed, airDensity, wingArea, coefficient, liftForce, cameraPerspective);
            }

                LoadVRMLModel(sender, e);
                // Show Highcharts graph with multiple wing types
                ShowChart(airDensity, windSpeed, coefficient);

                PageGraficoEnabled = true;
                ResultsForm resultsForm = new ResultsForm();
                resultsForm.UpdateResults();

                // Restaurar o botão
                ButtonRunTest.Text = "Run Test";
                ButtonRunTest.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro durante o teste: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Erro no ButtonRunTest_Click: {ex.Message}");

                // Restaurar o botão em caso de erro
                ButtonRunTest.Text = "Run Test";
                ButtonRunTest.Enabled = true;
            }
        }


        private void RecalculateChart()
        {
            if (PageGraficoEnabled)
            {
                try
                {
                    double density = double.Parse(ComboAirDensity.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
                    double maxSpeed = double.Parse(ComboWindSpeed.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
                    ShowChart(density, maxSpeed, coefficient);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao recalcular gráfico: {ex.Message}");
                }
            }
        }

        private async void ShowChart(double density, double maxSpeed, double coefficient)
        {
            // Obter a envergadura do tipo de asa principal sendo simulada
            double wingspan = GetCurrentWingArea() > 0 ? GetCurrentWingControl()?.Wingspan ?? 10.0 : 10.0;
            
            // Criar lista de séries para o gráfico
            List<object> series = new List<object>();
            
            // Cores específicas para cada tipo de asa
            var wingColors = new Dictionary<string, string>
            {
                { "Rectangular", "#FF0000" },  // Vermelho
                { "Trapezoidal", "#0000FF" },  // Azul
                { "Elliptical", "#FFFF00" },   // Amarelo
                { "Delta", "#00FF00" }         // Verde
            };

            // Gerar dados para cada tipo de asa selecionado
            foreach (var kvp in wingTypeSelections.Where(x => x.Value))
            {
                string wingType = kvp.Key;
                double area = CalculateWingAreaByType(wingType, wingspan);
                
                List<object[]> dataPoints = new List<object[]>();
                for (double v = 0; v <= maxSpeed; v += 1)
                {
                    double force = CalculateLiftForce(density, v, area, coefficient);
                    dataPoints.Add(new object[] { v, Math.Round(force, 2) });
                }

                series.Add(new
                {
                    name = wingType,
                    data = dataPoints,
                    color = wingColors[wingType],
                    lineWidth = 3,
                    marker = new { enabled = true, radius = 3 }
                });
            }

            string seriesJson = JsonSerializer.Serialize(series);


            // Criar legenda dinâmica
            string legendItems = string.Join(" | ", wingTypeSelections
                .Where(x => x.Value)
                .Select(x => $"<span style='color: {wingColors[x.Key]}'>●</span> {x.Key}"));

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
            height: calc(100vh - 160px);
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
            margin-bottom: 10px;
        }}
        .chart-legend {{
            font-size: 12px;
            color: #555;
            text-align: center;
            margin-top: 10px;
        }}
    </style>
</head>
<body>
    <div class='chart-header'>
        <div class='chart-title'>Wind Speed vs Lift Force</div>
        <div class='chart-subtitle'>Density: {density:F2} kg/m³ | Wingspan: {wingspan:F2} m | Coefficient: {coefficient:F2}</div>
        <div class='chart-legend'>{legendItems}</div>
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
            legend: {{ 
                enabled: true,
                layout: 'horizontal',
                align: 'center',
                verticalAlign: 'bottom',
                itemStyle: {{ fontSize: '11px' }}
            }},
            plotOptions: {{
                line: {{
                    marker: {{
                        enabled: true,
                        radius: 3,
                        lineWidth: 1
                    }},
                    lineWidth: 3
                }}
            }},
            series: {seriesJson},
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
            try
            {
                bool windSpeedValid = !string.IsNullOrWhiteSpace(ComboWindSpeed.Text);
                bool airDensityValid = !string.IsNullOrWhiteSpace(ComboAirDensity.Text);
                bool wingAreaValid = GetCurrentWingArea() > 0;
                
                ButtonRunTest.Enabled = windSpeedValid && airDensityValid && wingAreaValid;
                
                // Debug apenas quando necessário (comentado para performance)
                // Console.WriteLine($"WindSpeed: {windSpeedValid}, AirDensity: {airDensityValid}, WingArea: {wingAreaValid} (Área: {GetCurrentWingArea():F2})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na validação: {ex.Message}");
                ButtonRunTest.Enabled = false;
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
            string basePath = @"C:\WINDWORKS\Prints";

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

        }

        private void ComboWingType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = ComboWingType.SelectedItem?.ToString() ?? "";
            string formulaHtml = GetFormulaHtmlForWingType(selected);


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

        private void Form1_Load_1(object sender, EventArgs e)
        {

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
                        MessageBox.Show("Erro ao abrir: " + ex.Message);
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
        string wingType,
        string cameraPerspective,
        string windSpeed,
        string airDensity,
        string wingArea,
        string coefficient,
        string liftForce,
        string testDate
        )
        {
            ComboWingType.Text = wingType;
            ComboCameraPerspective.Text = cameraPerspective;
            ComboWindSpeed.Text = windSpeed;
            ComboAirDensity.Text = airDensity;
            // Não precisamos mais definir a área diretamente, ela será calculada pelos UserControls
            guna2HtmlLabelLiftForceValue.Text = liftForce;
        }

        private void xmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Dialog para escolher o tipo de exportação
                DialogResult choice = MessageBox.Show(
                    "Escolha o tipo de exportação:\n\n" +
                    "SIM - Exportar apenas o teste atual\n" +
                    "NÃO - Exportar todos os resultados salvos\n" +
                    "CANCELAR - Cancelar operação",
                    "Tipo de Exportação XML",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question
                );

                if (choice == DialogResult.Cancel)
                    return;

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Arquivos XML (*.xml)|*.xml";
                    saveFileDialog.Title = "Salvar arquivo XML";
                    saveFileDialog.FileName = choice == DialogResult.Yes ?
                        $"TestResult_{DateTime.Now:yyyyMMdd_HHmmss}.xml" :
                        $"AllResults_{DateTime.Now:yyyyMMdd_HHmmss}.xml";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;

                        if (choice == DialogResult.Yes)
                        {
                            ExportCurrentTestToXml(filePath);
                        }
                        else
                        {
                            ExportAllResultsToXml(filePath);
                        }

                        MessageBox.Show($"Dados exportados com sucesso para:\n{filePath}",
                            "Exportação Concluída",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        // Opção para abrir o arquivo após exportar
                        if (MessageBox.Show("Deseja abrir o arquivo XML exportado?",
                            "Abrir Arquivo",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao exportar XML: {ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ExportCurrentTestToXml(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();

            // Declaração XML
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

            // Elemento raiz
            XmlElement root = xmlDoc.CreateElement("LiftForceTest");
            xmlDoc.AppendChild(root);

            // Informações do teste
            XmlElement testInfo = xmlDoc.CreateElement("TestInformation");
            root.AppendChild(testInfo);

            AddXmlElement(xmlDoc, testInfo, "ExportDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AddXmlElement(xmlDoc, testInfo, "ExportType", "CurrentTest");
            AddXmlElement(xmlDoc, testInfo, "Application", "Lift Force Calculator");
            AddXmlElement(xmlDoc, testInfo, "Version", "1.0");

            // Dados do teste atual
            XmlElement testData = xmlDoc.CreateElement("TestData");
            root.AppendChild(testData);

            AddXmlElement(xmlDoc, testData, "WingType", ComboWingType.Text);
            AddXmlElement(xmlDoc, testData, "CameraPerspective", ComboCameraPerspective.Text);
            AddXmlElement(xmlDoc, testData, "WindSpeed", ComboWindSpeed.Text);
            AddXmlElement(xmlDoc, testData, "WindSpeedUnit", "m/s");
            AddXmlElement(xmlDoc, testData, "AirDensity", ComboAirDensity.Text);
            AddXmlElement(xmlDoc, testData, "AirDensityUnit", "kg/m³");
            AddXmlElement(xmlDoc, testData, "WingArea", GetCurrentWingArea().ToString("F2", CultureInfo.InvariantCulture));
            AddXmlElement(xmlDoc, testData, "WingAreaUnit", "m²");
            AddXmlElement(xmlDoc, testData, "LiftCoefficient", coefficient.ToString("F2", CultureInfo.InvariantCulture));
            AddXmlElement(xmlDoc, testData, "LiftForce", guna2HtmlLabelLiftForceValue.Text.Replace("N", ""));
            AddXmlElement(xmlDoc, testData, "LiftForceUnit", "N");
            AddXmlElement(xmlDoc, testData, "TestDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Parâmetros da asa
            var wingControl = GetCurrentWingControl();
            if (wingControl != null)
            {
                AddXmlElement(xmlDoc, testData, "Wingspan", wingControl.Wingspan.ToString("F2", CultureInfo.InvariantCulture));
                AddXmlElement(xmlDoc, testData, "WingspanUnit", "m");
                AddXmlElement(xmlDoc, testData, "Rope", wingControl.Rope.ToString("F2", CultureInfo.InvariantCulture));
                AddXmlElement(xmlDoc, testData, "RopeUnit", "m");
                
                if (wingControl is TrapezoidalWingControl trapControl)
                {
                    AddXmlElement(xmlDoc, testData, "RopeAtRoot", trapControl.RopeAtRoot.ToString("F2", CultureInfo.InvariantCulture));
                    AddXmlElement(xmlDoc, testData, "RopeAtRootUnit", "m");
                    AddXmlElement(xmlDoc, testData, "RopeAtEnd", trapControl.RopeAtEnd.ToString("F2", CultureInfo.InvariantCulture));
                    AddXmlElement(xmlDoc, testData, "RopeAtEndUnit", "m");
                }
            }

            // Fórmula utilizada
            XmlElement formula = xmlDoc.CreateElement("Formula");
            testData.AppendChild(formula);
            AddXmlElement(xmlDoc, formula, "Expression", "F = 0.5 × ρ × v² × S × Cl");
            AddXmlElement(xmlDoc, formula, "Description", "Lift Force = 0.5 × Air Density × Wind Speed² × Wing Area × Lift Coefficient");

            // Cálculo detalhado
            XmlElement calculation = xmlDoc.CreateElement("DetailedCalculation");
            testData.AppendChild(calculation);

            if (double.TryParse(ComboAirDensity.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double density) &&
                double.TryParse(ComboWindSpeed.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double speed))
            {
                double area = GetCurrentWingArea();
                double calculatedForce = CalculateLiftForce(density, speed, area, coefficient);

                AddXmlElement(xmlDoc, calculation, "Step1", $"F = 0.5 × {density} × {speed}² × {area} × {coefficient}");
                AddXmlElement(xmlDoc, calculation, "Step2", $"F = 0.5 × {density} × {Math.Pow(speed, 2)} × {area} × {coefficient}");
                AddXmlElement(xmlDoc, calculation, "Result", $"F = {calculatedForce:F2} N");
            }

            xmlDoc.Save(filePath);
        }

        private void ExportAllResultsToXml(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();

            // Declaração XML
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

            // Elemento raiz
            XmlElement root = xmlDoc.CreateElement("LiftForceTestResults");
            xmlDoc.AppendChild(root);

            // Informações da exportação
            XmlElement exportInfo = xmlDoc.CreateElement("ExportInformation");
            root.AppendChild(exportInfo);

            AddXmlElement(xmlDoc, exportInfo, "ExportDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AddXmlElement(xmlDoc, exportInfo, "ExportType", "AllResults");
            AddXmlElement(xmlDoc, exportInfo, "Application", "Lift Force Calculator");
            AddXmlElement(xmlDoc, exportInfo, "Version", "1.0");

            // Container para todos os testes
            XmlElement testsContainer = xmlDoc.CreateElement("Tests");
            root.AppendChild(testsContainer);

            try
            {
                // Primeiro, vamos descobrir quais colunas existem na tabela
                List<string> availableColumns = new List<string>();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Verificar estrutura da tabela
                    string checkColumnsQuery = "DESCRIBE TestResults";
                    using (MySqlCommand checkCommand = new MySqlCommand(checkColumnsQuery, connection))
                    {
                        using (MySqlDataReader checkReader = checkCommand.ExecuteReader())
                        {
                            while (checkReader.Read())
                            {
                                availableColumns.Add(checkReader["Field"].ToString());
                            }
                        }
                    }

                    // Construir query baseada nas colunas disponíveis
                    string baseQuery = "SELECT ";
                    List<string> selectColumns = new List<string>();

                    // Colunas obrigatórias que devem existir
                    string[] requiredColumns = { "Id", "WingType", "WindSpeed", "AirDensity", "WingArea", "LiftForce", "TestDate" };
                    foreach (string col in requiredColumns)
                    {
                        if (availableColumns.Contains(col))
                            selectColumns.Add(col);
                    }

                    // Colunas opcionais
                    string[] optionalColumns = { "CameraPerspective", "LiftCoefficient", "Coefficient", "Wingspan", "Rope", "RopeAtRoot", "RopeAtEnd" };
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

                                // Dados básicos do teste
                                XmlElement basicData = xmlDoc.CreateElement("BasicData");
                                test.AppendChild(basicData);

                                AddXmlElement(xmlDoc, basicData, "WingType", GetSafeValue(reader, "WingType"));
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

                                // Parâmetros de entrada
                                XmlElement inputParams = xmlDoc.CreateElement("InputParameters");
                                test.AppendChild(inputParams);

                                AddXmlElement(xmlDoc, inputParams, "WindSpeed", GetSafeValue(reader, "WindSpeed"));
                                AddXmlElement(xmlDoc, inputParams, "WindSpeedUnit", "m/s");
                                AddXmlElement(xmlDoc, inputParams, "AirDensity", GetSafeValue(reader, "AirDensity"));
                                AddXmlElement(xmlDoc, inputParams, "AirDensityUnit", "kg/m³");
                                AddXmlElement(xmlDoc, inputParams, "WingArea", GetSafeValue(reader, "WingArea"));
                                AddXmlElement(xmlDoc, inputParams, "WingAreaUnit", "m²");

                                // Parâmetros da asa (se disponíveis no banco)
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

                                // Tentar pegar o coeficiente de diferentes colunas possíveis
                                string coefficientValue = coefficient.ToString("F2", CultureInfo.InvariantCulture); // valor padrão
                                if (availableColumns.Contains("LiftCoefficient"))
                                    coefficientValue = GetSafeValue(reader, "LiftCoefficient");
                                else if (availableColumns.Contains("Coefficient"))
                                    coefficientValue = GetSafeValue(reader, "Coefficient");

                                AddXmlElement(xmlDoc, inputParams, "LiftCoefficient", coefficientValue);

                                // Resultado
                                XmlElement result = xmlDoc.CreateElement("Result");
                                test.AppendChild(result);

                                AddXmlElement(xmlDoc, result, "LiftForce", GetSafeValue(reader, "LiftForce"));
                                AddXmlElement(xmlDoc, result, "LiftForceUnit", "N");

                                // Fórmula (mesma para todos)
                                XmlElement formula = xmlDoc.CreateElement("Formula");
                                result.AppendChild(formula);
                                AddXmlElement(xmlDoc, formula, "Expression", "F = 0.5 × ρ × v² × S × Cl");
                                AddXmlElement(xmlDoc, formula, "Description", "Lift Force = 0.5 × Air Density × Wind Speed² × Wing Area × Lift Coefficient");
                            }


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

                XmlElement errorInfo = xmlDoc.CreateElement("Error");
                testsContainer.AppendChild(errorInfo);
                AddXmlElement(xmlDoc, errorInfo, "Message", "Erro ao acessar banco de dados");
                AddXmlElement(xmlDoc, errorInfo, "Details", ex.Message);
                AddXmlElement(xmlDoc, errorInfo, "Note", "Exportação realizada sem dados do banco");
            }

            xmlDoc.Save(filePath);
        }

        private string GetSafeValue(MySqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? "" : reader.GetValue(ordinal).ToString();
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

            UserControl control = null;
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
            }

            if (control != null)
            {
                control.Dock = DockStyle.Fill;
                panelWingArea.Controls.Add(control);
                
                // Popular as comboboxes com valores padrão
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

                if (control is RectangularWingControl rectControl)
                {
                    rectControl.ComboWindSpeed.Items.Clear();
                    rectControl.ComboAirDensity.Items.Clear();
                    
                    rectControl.ComboWindSpeed.Items.AddRange(wingspanValues.Cast<object>().ToArray());
                    rectControl.ComboAirDensity.Items.AddRange(ropeValues.Cast<object>().ToArray());
                    
                    if (rectControl.ComboWindSpeed.Items.Count > 0) rectControl.ComboWindSpeed.SelectedIndex = 0;
                    if (rectControl.ComboAirDensity.Items.Count > 0) rectControl.ComboAirDensity.SelectedIndex = 0;
                    
                    Console.WriteLine($"Rectangular: Wingspan={rectControl.ComboWindSpeed.SelectedItem}, Rope={rectControl.ComboAirDensity.SelectedItem}");
                }
                else if (control is TrapezoidalWingControl trapControl)
                {
                    trapControl.ComboWingspan.Items.Clear();
                    trapControl.ComboRopeAtRootComboRopeAtRoot.Items.Clear();
                    trapControl.ComboRopeAtEnd.Items.Clear();
                    
                    trapControl.ComboWingspan.Items.AddRange(wingspanValues.Cast<object>().ToArray());
                    trapControl.ComboRopeAtRootComboRopeAtRoot.Items.AddRange(ropeValues.Cast<object>().ToArray());
                    trapControl.ComboRopeAtEnd.Items.AddRange(ropeValues.Cast<object>().ToArray());
                    
                    if (trapControl.ComboWingspan.Items.Count > 0) trapControl.ComboWingspan.SelectedIndex = 0;
                    if (trapControl.ComboRopeAtRootComboRopeAtRoot.Items.Count > 0) trapControl.ComboRopeAtRootComboRopeAtRoot.SelectedIndex = 0;
                    if (trapControl.ComboRopeAtEnd.Items.Count > 0) trapControl.ComboRopeAtEnd.SelectedIndex = 0;
                    
                    Console.WriteLine($"Trapezoidal: Wingspan={trapControl.ComboWingspan.SelectedItem}, RopeRoot={trapControl.ComboRopeAtRootComboRopeAtRoot.SelectedItem}, RopeEnd={trapControl.ComboRopeAtEnd.SelectedItem}");
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

        private void AddWingControlEvents(UserControl control)
        {
            try
            {
                // Remover eventos existentes primeiro para evitar duplicação
                if (control is RectangularWingControl rectControl)
                {
                    rectControl.ComboWindSpeed.SelectedIndexChanged -= CheckFieldsFilled;
                    rectControl.ComboAirDensity.SelectedIndexChanged -= CheckFieldsFilled;
                    
                    rectControl.ComboWindSpeed.SelectedIndexChanged += CheckFieldsFilled;
                    rectControl.ComboAirDensity.SelectedIndexChanged += CheckFieldsFilled;
                }
                else if (control is TrapezoidalWingControl trapControl)
                {
                    trapControl.ComboWingspan.SelectedIndexChanged -= CheckFieldsFilled;
                    trapControl.ComboRopeAtRootComboRopeAtRoot.SelectedIndexChanged -= CheckFieldsFilled;
                    trapControl.ComboRopeAtEnd.SelectedIndexChanged -= CheckFieldsFilled;
                    
                    trapControl.ComboWingspan.SelectedIndexChanged += CheckFieldsFilled;
                    trapControl.ComboRopeAtRootComboRopeAtRoot.SelectedIndexChanged += CheckFieldsFilled;
                    trapControl.ComboRopeAtEnd.SelectedIndexChanged += CheckFieldsFilled;
                }
                else if (control is EllipticalWingControl ellipControl)
                {
                    ellipControl.ComboWingspan.SelectedIndexChanged -= CheckFieldsFilled;
                    ellipControl.ComboRope.SelectedIndexChanged -= CheckFieldsFilled;
                    
                    ellipControl.ComboWingspan.SelectedIndexChanged += CheckFieldsFilled;
                    ellipControl.ComboRope.SelectedIndexChanged += CheckFieldsFilled;
                }
                else if (control is DeltaWingControl deltaControl)
                {
                    deltaControl.ComboWingspan.SelectedIndexChanged -= CheckFieldsFilled;
                    deltaControl.ComboRope.SelectedIndexChanged -= CheckFieldsFilled;
                    
                    deltaControl.ComboWingspan.SelectedIndexChanged += CheckFieldsFilled;
                    deltaControl.ComboRope.SelectedIndexChanged += CheckFieldsFilled;
                }
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
                            Console.WriteLine($"Área calculada: {area}, Wingspan: {wingControl.Wingspan}, Rope: {wingControl.Rope}");
                        }
                        
                        return area;
                    }
                }
                return 0.0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao calcular área da asa: {ex.Message}");
                return 0.0;
            }
        }

        private IWingControl GetCurrentWingControl()
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
                
                details += $"Área da Asa: {wingControl.WingArea:F2} m²";
                
                // Você pode usar isso para mostrar em um MessageBox ou em um label
                Console.WriteLine(details);
            }
        }

        private void ComboWingType_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string selectedType = ComboWingType.SelectedItem?.ToString() ?? "";
            LoadWingControl(selectedType);

            
            // Atualizar a imagem da asa
            if (!string.IsNullOrEmpty(selectedType))
            {
                selectedWingType = selectedType;
                LabelWingType.Text = selectedType;
            }
            
            // Testar o UserControl carregado
            TestCurrentWingControl();
            LoadWingImage("Cima");
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
                    MessageBox.Show($"Não é possível desmarcar o tipo '{wingType}' pois ele está sendo simulado atualmente.", 
                                  "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Atualizar o dicionário de seleções
                wingTypeSelections[wingType] = isSelected;

                // Verificar se pelo menos um tipo está selecionado
                if (!wingTypeSelections.Values.Any(selected => selected))
                {
                    comboBox.SelectedIndex = 0;
                    wingTypeSelections[wingType] = true;
                    MessageBox.Show("Pelo menos um tipo de asa deve estar selecionado.", 
                                  "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Console.WriteLine($"Erro ao processar seleção de tipo de asa: {ex.Message}");
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
                    MessageBox.Show($"Não é possível desmarcar o tipo '{wingType}' pois ele está sendo simulado atualmente.", 
                                  "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Atualizar o dicionário de seleções
                wingTypeSelections[wingType] = checkBox.Checked;

                // Verificar se pelo menos um tipo está selecionado
                if (!wingTypeSelections.Values.Any(selected => selected))
                {
                    checkBox.Checked = true;
                    wingTypeSelections[wingType] = true;
                    MessageBox.Show("Pelo menos um tipo de asa deve estar selecionado.", 
                                  "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Console.WriteLine($"Erro ao processar mudança de checkbox: {ex.Message}");
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
    }
}
