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
            MessageBox.Show("Results updated successfully.");
            lblTotalCount.Text = ClassResults.GetTotalTestCount().ToString();
            lblLastUpdate.Text = ClassResults.GetLastUpdate().ToString();
            ClassResults.LoadDataToGridView(dataGridViewResults);
        }

        private void gunaButtonRetakeTest_Click(object sender, EventArgs e)
        {
            if (dataGridViewResults.CurrentRow != null)
            {
                var row = dataGridViewResults.CurrentRow;
                string wingType = row.Cells["WingType"].Value?.ToString();
                string cameraPerspective = row.Cells["CameraPerspective"].Value?.ToString();
                string windSpeed = row.Cells["WindSpeed"].Value?.ToString();
                string airDensity = row.Cells["AirDensity"].Value?.ToString();
                string wingArea = row.Cells["WingArea"].Value?.ToString();
                string coefficient = row.Cells["Coefficient"].Value?.ToString();
                string liftForce = row.Cells["LiftForce"].Value?.ToString();
                string testDate = row.Cells["TestDate"].Value?.ToString();

                if (this.Owner is Form1 mainForm)
                {
                    mainForm.ReceiveDataFromResults(
                        wingType, cameraPerspective, windSpeed,
                        airDensity, wingArea, coefficient, liftForce, testDate
                    );
                }
            }
            else
            {
                MessageBox.Show("Selecione uma linha primeiro.");
            }
        }

    }
}
