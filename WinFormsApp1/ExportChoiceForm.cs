// ExportChoiceForm.cs

using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class ExportChoiceForm : Form
    {
        public ExportChoiceForm()
        {
            InitializeComponent();
        }

        // Os eventos abaixo são opcionais, pois o DialogResult já fecha o form.
        // Pode deixá-los vazios.
        private void btnExportCurrent_Click(object sender, EventArgs e)
        {
            // this.Close();
        }

        private void btnExportAll_Click(object sender, EventArgs e)
        {
            // this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // this.Close();
        }
    }
}