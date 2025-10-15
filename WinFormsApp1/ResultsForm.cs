using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class ResultsForm : Form
    {
        public ResultsForm()
        {
            InitializeComponent();
            // Estes métodos precisam existir na sua classe ClassResults
             lblTotalCount.Text = ClassResults.GetTotalTestCount().ToString();
             lblLastUpdate.Text = ClassResults.GetLastUpdate().ToString();
             ClassResults.LoadDataToGridView(dataGridViewResults);
        }

        public void UpdateResults()
        {
            //stes métodos precisam existir na sua classe ClassResults
            lblTotalCount.Text = ClassResults.GetTotalTestCount().ToString();
            lblLastUpdate.Text = ClassResults.GetLastUpdate().ToString();
            ClassResults.LoadDataToGridView(dataGridViewResults);
        }

        private void gunaButtonRetakeTest_Click(object sender, EventArgs e)
        {
            if (dataGridViewResults.CurrentRow != null)
            {
                var row = dataGridViewResults.CurrentRow;

                // --- LENDO TODOS OS DADOS NECESSÁRIOS (ANTIGOS E NOVOS) ---
                string wingType = row.Cells["WingType"].Value?.ToString() ?? string.Empty;
                string airfoil = row.Cells["Airfoil"].Value?.ToString() ?? string.Empty;
                string angleOfAttack = row.Cells["AngleOfAttack"].Value?.ToString() ?? string.Empty;
                string cameraPerspective = row.Cells["CameraPerspective"].Value?.ToString() ?? string.Empty;
                string windSpeed = row.Cells["WindSpeed"].Value?.ToString() ?? string.Empty;
                string airDensity = row.Cells["AirDensity"].Value?.ToString() ?? string.Empty;

                // Parâmetros da geometria da asa
                string wingspan = row.Cells["Wingspan"].Value?.ToString() ?? string.Empty;
                string rope = row.Cells["Rope"].Value?.ToString() ?? string.Empty;
                string ropeAtRoot = row.Cells["RopeAtRoot"].Value?.ToString() ?? string.Empty;
                string ropeAtEnd = row.Cells["RopeAtEnd"].Value?.ToString() ?? string.Empty;

                // Tenta chamar o Form1 (o formulário principal)
                if (this.Owner is Form1 mainForm)
                {
                    // Chama o método atualizado em Form1 para passar os parâmetros
                    mainForm.ReceiveDataFromResults(
                        wingType, airfoil, angleOfAttack, cameraPerspective,
                        windSpeed, airDensity, wingspan, rope, ropeAtRoot, ropeAtEnd
                    );
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a row first to retake the test.", "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}