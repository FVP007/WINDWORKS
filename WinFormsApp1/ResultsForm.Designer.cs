namespace WinFormsApp1
{
    partial class ResultsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            panelHeader = new Panel();
            panelHeaderButtons = new Panel();
            gunaButtonRetakeTest = new Guna.UI2.WinForms.Guna2Button();
            lblTitle = new Label();
            panelMain = new Panel();
            panelTable = new Panel();
            dataGridViewResults = new Guna.UI2.WinForms.Guna2DataGridView();
            panelTableHeader = new Panel();
            lblTableTitle = new Label();
            statusStrip = new StatusStrip();
            lblStatusTotal = new ToolStripStatusLabel();
            lblTotalCount = new ToolStripStatusLabel();
            lblStatusSeparator = new ToolStripStatusLabel();
            lblLastUpdate = new ToolStripStatusLabel();
            panelHeader.SuspendLayout();
            panelHeaderButtons.SuspendLayout();
            panelMain.SuspendLayout();
            panelTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewResults).BeginInit();
            panelTableHeader.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = SystemColors.Control;
            panelHeader.Controls.Add(panelHeaderButtons);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(20, 15, 20, 15);
            panelHeader.Size = new Size(1200, 70);
            panelHeader.TabIndex = 2;
            // 
            // panelHeaderButtons
            // 
            panelHeaderButtons.Controls.Add(gunaButtonRetakeTest);
            panelHeaderButtons.Dock = DockStyle.Right;
            panelHeaderButtons.Location = new Point(928, 15);
            panelHeaderButtons.Name = "panelHeaderButtons";
            panelHeaderButtons.Size = new Size(252, 40);
            panelHeaderButtons.TabIndex = 0;
            // 
            // gunaButtonRetakeTest
            // 
            gunaButtonRetakeTest.BorderRadius = 6;
            gunaButtonRetakeTest.CustomizableEdges = customizableEdges1;
            gunaButtonRetakeTest.DisabledState.BorderColor = Color.DarkGray;
            gunaButtonRetakeTest.DisabledState.CustomBorderColor = Color.DarkGray;
            gunaButtonRetakeTest.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            gunaButtonRetakeTest.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            gunaButtonRetakeTest.Dock = DockStyle.Right;
            gunaButtonRetakeTest.FillColor = Color.SlateGray;
            gunaButtonRetakeTest.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gunaButtonRetakeTest.ForeColor = Color.White;
            gunaButtonRetakeTest.Location = new Point(125, 0);
            gunaButtonRetakeTest.Margin = new Padding(3, 3, 15, 3);
            gunaButtonRetakeTest.Name = "gunaButtonRetakeTest";
            gunaButtonRetakeTest.ShadowDecoration.CustomizableEdges = customizableEdges2;
            gunaButtonRetakeTest.Size = new Size(127, 40);
            gunaButtonRetakeTest.TabIndex = 1;
            gunaButtonRetakeTest.Text = "Retake Test";
            gunaButtonRetakeTest.Click += gunaButtonRetakeTest_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Dock = DockStyle.Left;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = SystemColors.ActiveCaptionText;
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(0, 8, 0, 0);
            lblTitle.Size = new Size(278, 38);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Aerodynamic Test System";
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.Silver;
            panelMain.Controls.Add(panelTable);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 70);
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(20);
            panelMain.Size = new Size(1200, 508);
            panelMain.TabIndex = 0;
            // 
            // panelTable
            // 
            panelTable.BackColor = Color.FromArgb(31, 41, 55);
            panelTable.Controls.Add(dataGridViewResults);
            panelTable.Controls.Add(panelTableHeader);
            panelTable.Dock = DockStyle.Fill;
            panelTable.Location = new Point(20, 20);
            panelTable.Name = "panelTable";
            panelTable.Size = new Size(1160, 468);
            panelTable.TabIndex = 0;
            // 
            // dataGridViewResults
            // 
            dataGridViewResults.AllowUserToAddRows = false;
            dataGridViewResults.AllowUserToDeleteRows = false;
            dataGridViewResults.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(37, 47, 63);
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(40, 40, 40);
            dataGridViewResults.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(55, 65, 81);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(55, 65, 81);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dataGridViewResults.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewResults.ColumnHeadersHeight = 35;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(45, 45, 48);
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle3.ForeColor = Color.White;
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(60, 60, 60);
            dataGridViewCellStyle3.SelectionForeColor = Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dataGridViewResults.DefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewResults.Dock = DockStyle.Fill;
            dataGridViewResults.GridColor = Color.FromArgb(60, 60, 60);
            dataGridViewResults.Location = new Point(0, 50);
            dataGridViewResults.MultiSelect = false;
            dataGridViewResults.Name = "dataGridViewResults";
            dataGridViewResults.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(40, 40, 40);
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = Color.White;
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(70, 130, 170);
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dataGridViewResults.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewResults.RowHeadersVisible = false;
            dataGridViewResults.RowTemplate.Height = 30;
            dataGridViewResults.Size = new Size(1160, 418);
            dataGridViewResults.TabIndex = 0;
            dataGridViewResults.ThemeStyle.AlternatingRowsStyle.BackColor = Color.FromArgb(37, 47, 63);
            dataGridViewResults.ThemeStyle.AlternatingRowsStyle.Font = null;
            dataGridViewResults.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            dataGridViewResults.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            dataGridViewResults.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            dataGridViewResults.ThemeStyle.BackColor = Color.White;
            dataGridViewResults.ThemeStyle.GridColor = Color.FromArgb(60, 60, 60);
            dataGridViewResults.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(55, 65, 81);
            dataGridViewResults.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewResults.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewResults.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dataGridViewResults.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewResults.ThemeStyle.HeaderStyle.Height = 35;
            dataGridViewResults.ThemeStyle.ReadOnly = true;
            dataGridViewResults.ThemeStyle.RowsStyle.BackColor = Color.FromArgb(31, 41, 55);
            dataGridViewResults.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewResults.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dataGridViewResults.ThemeStyle.RowsStyle.ForeColor = Color.White;
            dataGridViewResults.ThemeStyle.RowsStyle.Height = 30;
            dataGridViewResults.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(59, 130, 246);
            dataGridViewResults.ThemeStyle.RowsStyle.SelectionForeColor = Color.White;
            // 
            // panelTableHeader
            // 
            panelTableHeader.BackColor = SystemColors.Control;
            panelTableHeader.Controls.Add(lblTableTitle);
            panelTableHeader.Dock = DockStyle.Top;
            panelTableHeader.Location = new Point(0, 0);
            panelTableHeader.Name = "panelTableHeader";
            panelTableHeader.Padding = new Padding(20, 12, 20, 12);
            panelTableHeader.Size = new Size(1160, 50);
            panelTableHeader.TabIndex = 1;
            // 
            // lblTableTitle
            // 
            lblTableTitle.AutoSize = true;
            lblTableTitle.Dock = DockStyle.Left;
            lblTableTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTableTitle.ForeColor = Color.Black;
            lblTableTitle.Location = new Point(20, 12);
            lblTableTitle.Name = "lblTableTitle";
            lblTableTitle.Padding = new Padding(0, 2, 0, 0);
            lblTableTitle.Size = new Size(93, 22);
            lblTableTitle.TabIndex = 1;
            lblTableTitle.Text = "Test Results";
            // 
            // statusStrip
            // 
            statusStrip.BackColor = SystemColors.Control;
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusTotal, lblTotalCount, lblStatusSeparator, lblLastUpdate });
            statusStrip.Location = new Point(0, 578);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new Padding(20, 0, 20, 0);
            statusStrip.Size = new Size(1200, 22);
            statusStrip.TabIndex = 1;
            // 
            // lblStatusTotal
            // 
            lblStatusTotal.ForeColor = Color.Black;
            lblStatusTotal.Name = "lblStatusTotal";
            lblStatusTotal.Size = new Size(69, 17);
            lblStatusTotal.Text = "Total testes:";
            // 
            // lblTotalCount
            // 
            lblTotalCount.ForeColor = Color.Black;
            lblTotalCount.Name = "lblTotalCount";
            lblTotalCount.Size = new Size(13, 17);
            lblTotalCount.Text = "0";
            // 
            // lblStatusSeparator
            // 
            lblStatusSeparator.ForeColor = Color.Black;
            lblStatusSeparator.Name = "lblStatusSeparator";
            lblStatusSeparator.Size = new Size(113, 17);
            lblStatusSeparator.Text = "| Última atualização:";
            // 
            // lblLastUpdate
            // 
            lblLastUpdate.ForeColor = Color.Black;
            lblLastUpdate.Name = "lblLastUpdate";
            lblLastUpdate.Size = new Size(17, 17);
            lblLastUpdate.Text = "--";
            // 
            // ResultsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 24, 39);
            ClientSize = new Size(1200, 600);
            Controls.Add(panelMain);
            Controls.Add(statusStrip);
            Controls.Add(panelHeader);
            Name = "ResultsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sistema de Testes Aerodinâmicos";
            WindowState = FormWindowState.Maximized;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelHeaderButtons.ResumeLayout(false);
            panelMain.ResumeLayout(false);
            panelTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewResults).EndInit();
            panelTableHeader.ResumeLayout(false);
            panelTableHeader.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private Panel panelHeaderButtons;
        private Label lblTitle;
        private Panel panelMain;
        private Panel panelTable;
        private Guna.UI2.WinForms.Guna2DataGridView dataGridViewResults;
        private Panel panelTableHeader;
        private Label lblTableTitle;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusTotal;
        private ToolStripStatusLabel lblTotalCount;
        private ToolStripStatusLabel lblStatusSeparator;
        private ToolStripStatusLabel lblLastUpdate;
        private Guna.UI2.WinForms.Guna2Button gunaButtonRetakeTest;
    }
}