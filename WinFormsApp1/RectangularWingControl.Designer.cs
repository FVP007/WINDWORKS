namespace WinFormsApp1
{
    partial class RectangularWingControl
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            guna2Panel7 = new Guna.UI2.WinForms.Guna2Panel();
            ComboAirDensity = new ComboBox();
            ComboWindSpeed = new ComboBox();
            LabelDensidadeAr = new Guna.UI2.WinForms.Guna2HtmlLabel();
            LabelVelocidadeVento = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2Panel7.SuspendLayout();
            SuspendLayout();
            // 
            // guna2Panel7
            // 
            guna2Panel7.BorderColor = Color.Black;
            guna2Panel7.BorderRadius = 1;
            guna2Panel7.BorderThickness = 1;
            guna2Panel7.Controls.Add(ComboAirDensity);
            guna2Panel7.Controls.Add(ComboWindSpeed);
            guna2Panel7.Controls.Add(LabelDensidadeAr);
            guna2Panel7.Controls.Add(LabelVelocidadeVento);
            guna2Panel7.CustomizableEdges = customizableEdges1;
            guna2Panel7.Dock = DockStyle.Fill;
            guna2Panel7.Location = new Point(0, 0);
            guna2Panel7.Name = "guna2Panel7";
            guna2Panel7.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Panel7.Size = new Size(199, 164);
            guna2Panel7.TabIndex = 17;
            // 
            // ComboAirDensity
            // 
            ComboAirDensity.FormattingEnabled = true;
            ComboAirDensity.Location = new Point(12, 108);
            ComboAirDensity.Name = "ComboAirDensity";
            ComboAirDensity.Size = new Size(153, 23);
            ComboAirDensity.TabIndex = 21;
            // 
            // ComboWindSpeed
            // 
            ComboWindSpeed.FormattingEnabled = true;
            ComboWindSpeed.Location = new Point(12, 51);
            ComboWindSpeed.Name = "ComboWindSpeed";
            ComboWindSpeed.Size = new Size(153, 23);
            ComboWindSpeed.TabIndex = 20;
            // 
            // LabelDensidadeAr
            // 
            LabelDensidadeAr.BackColor = Color.Transparent;
            LabelDensidadeAr.Font = new Font("Segoe UI", 11.25F);
            LabelDensidadeAr.Location = new Point(61, 80);
            LabelDensidadeAr.Name = "LabelDensidadeAr";
            LabelDensidadeAr.Size = new Size(38, 22);
            LabelDensidadeAr.TabIndex = 18;
            LabelDensidadeAr.Text = "Rope";
            // 
            // LabelVelocidadeVento
            // 
            LabelVelocidadeVento.BackColor = Color.Transparent;
            LabelVelocidadeVento.Font = new Font("Segoe UI", 11.25F);
            LabelVelocidadeVento.Location = new Point(41, 23);
            LabelVelocidadeVento.Name = "LabelVelocidadeVento";
            LabelVelocidadeVento.Size = new Size(69, 22);
            LabelVelocidadeVento.TabIndex = 17;
            LabelVelocidadeVento.Text = "Wingspan";
            // 
            // RectangularWingControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(guna2Panel7);
            Name = "RectangularWingControl";
            Size = new Size(199, 164);
            guna2Panel7.ResumeLayout(false);
            guna2Panel7.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel guna2Panel7;
        public ComboBox ComboAirDensity;
        public ComboBox ComboWindSpeed;
        private Guna.UI2.WinForms.Guna2HtmlLabel LabelDensidadeAr;
        private Guna.UI2.WinForms.Guna2HtmlLabel LabelVelocidadeVento;
    }
}
