using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class ResultsForm : Form
    {
        public ResultsForm()
        {
            InitializeComponent();
            lblTotalCount.Text = ClassResults.GetTotalTestCount().ToString();
            lblLastUpdate.Text = ClassResults.GetLastUpdate().ToString();

            ClassResults.LoadDataToGridView(dataGridViewResults);
        }
        public void UpdateResults()
        {
            lblTotalCount.Text = ClassResults.GetTotalTestCount().ToString();
            lblLastUpdate.Text = ClassResults.GetLastUpdate().ToString();
            ClassResults.LoadDataToGridView(dataGridViewResults);
        }

        private void gunaButtonRetakeTest_Click(object sender, EventArgs e)
        {
            if (dataGridViewResults.CurrentRow != null)
            {
                var row = dataGridViewResults.CurrentRow;
                string wingType = row.Cells["WingType"].Value?.ToString() ?? string.Empty;
                string cameraPerspective = row.Cells["CameraPerspective"].Value?.ToString() ?? string.Empty;
                string windSpeed = row.Cells["WindSpeed"].Value?.ToString() ?? string.Empty;
                string airDensity = row.Cells["AirDensity"].Value?.ToString() ?? string.Empty;
                string wingArea = row.Cells["WingArea"].Value?.ToString() ?? string.Empty;
                string coefficient = row.Cells["Coefficient"].Value?.ToString() ?? string.Empty;
                string liftForce = row.Cells["LiftForce"].Value?.ToString() ?? string.Empty;
                string testDate = row.Cells["TestDate"].Value?.ToString() ?? string.Empty;

                if (this.Owner is Form1 mainForm)
                {
                    mainForm.ReceiveDataFromResults(
                        wingType ?? string.Empty, cameraPerspective ?? string.Empty, windSpeed ?? string.Empty,
                        airDensity ?? string.Empty, wingArea ?? string.Empty, coefficient ?? string.Empty, liftForce ?? string.Empty, testDate ?? string.Empty
                    );
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Select a line first.");
            }
        }

    }
}
