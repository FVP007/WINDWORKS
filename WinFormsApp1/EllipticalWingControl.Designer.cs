namespace WinFormsApp1
{
    partial class EllipticalWingControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            LabelVelocidadeVento = new Guna.UI2.WinForms.Guna2HtmlLabel();
            LabelDensidadeAr = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2Panel7 = new Guna.UI2.WinForms.Guna2Panel();
            ComboRope = new ComboBox();
            ComboWingspan = new ComboBox();
            guna2HtmlLabel1 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2HtmlLabel2 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2Panel7.SuspendLayout();
            SuspendLayout();
            // 
            // LabelVelocidadeVento
            // 
            LabelVelocidadeVento.BackColor = Color.Transparent;
            LabelVelocidadeVento.Font = new Font("Segoe UI", 11.25F);
            LabelVelocidadeVento.Location = new Point(61, 29);
            LabelVelocidadeVento.Name = "LabelVelocidadeVento";
            LabelVelocidadeVento.Size = new Size(69, 22);
            LabelVelocidadeVento.TabIndex = 17;
            LabelVelocidadeVento.Text = "Wingspan (m)";
            // 
            // LabelDensidadeAr
            // 
            LabelDensidadeAr.BackColor = Color.Transparent;
            LabelDensidadeAr.Font = new Font("Segoe UI", 11.25F);
            LabelDensidadeAr.Location = new Point(48, 86);
            LabelDensidadeAr.Name = "LabelDensidadeAr";
            LabelDensidadeAr.Size = new Size(112, 22);
            LabelDensidadeAr.TabIndex = 18;
            LabelDensidadeAr.Text = "Rope at the root";
            // 
            // guna2Panel7
            // 
            guna2Panel7.BorderColor = Color.Black;
            guna2Panel7.BorderRadius = 1;
            guna2Panel7.BorderThickness = 1;
            guna2Panel7.Controls.Add(ComboRope);
            guna2Panel7.Controls.Add(ComboWingspan);
            guna2Panel7.Controls.Add(guna2HtmlLabel1);
            guna2Panel7.Controls.Add(guna2HtmlLabel2);
            guna2Panel7.CustomizableEdges = customizableEdges3;
            guna2Panel7.Dock = DockStyle.Fill;
            guna2Panel7.Location = new Point(0, 0);
            guna2Panel7.Name = "guna2Panel7";
            guna2Panel7.ShadowDecoration.CustomizableEdges = customizableEdges4;
            guna2Panel7.Size = new Size(200, 212);
            guna2Panel7.TabIndex = 18;
            // 
            // ComboRope
            // 
            ComboRope.FormattingEnabled = true;
            ComboRope.Location = new Point(12, 108);
            ComboRope.Name = "ComboRope";
            ComboRope.Size = new Size(153, 23);
            ComboRope.TabIndex = 21;
            // 
            // ComboWingspan
            // 
            ComboWingspan.FormattingEnabled = true;
            ComboWingspan.Location = new Point(12, 51);
            ComboWingspan.Name = "ComboWingspan";
            ComboWingspan.Size = new Size(153, 23);
            ComboWingspan.TabIndex = 20;
            // 
            // guna2HtmlLabel1
            // 
            guna2HtmlLabel1.BackColor = Color.Transparent;
            guna2HtmlLabel1.Font = new Font("Segoe UI", 11.25F);
            guna2HtmlLabel1.Location = new Point(61, 80);
            guna2HtmlLabel1.Name = "guna2HtmlLabel1";
            guna2HtmlLabel1.Size = new Size(38, 22);
            guna2HtmlLabel1.TabIndex = 18;
            guna2HtmlLabel1.Text = "Rope (m)";
            // 
            // guna2HtmlLabel2
            // 
            guna2HtmlLabel2.BackColor = Color.Transparent;
            guna2HtmlLabel2.Font = new Font("Segoe UI", 11.25F);
            guna2HtmlLabel2.Location = new Point(41, 23);
            guna2HtmlLabel2.Name = "guna2HtmlLabel2";
            guna2HtmlLabel2.Size = new Size(69, 22);
            guna2HtmlLabel2.TabIndex = 17;
            guna2HtmlLabel2.Text = "Wingspan";
            // 
            // EllipticalWingControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(guna2Panel7);
            Name = "EllipticalWingControl";
            Size = new Size(200, 212);
            guna2Panel7.ResumeLayout(false);
            guna2Panel7.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2HtmlLabel LabelVelocidadeVento;
        private Guna.UI2.WinForms.Guna2HtmlLabel LabelDensidadeAr;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel7;
        public ComboBox ComboRope;
        public ComboBox ComboWingspan;
        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel1;
        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel2;
    }
}
