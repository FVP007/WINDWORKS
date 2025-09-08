namespace WinFormsApp1
{
    partial class TrapezoidalWingControl
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
            ComboRopeAtEnd = new ComboBox();
            ComboRopeAtRootComboRopeAtRoot = new ComboBox();
            ComboWingspan = new ComboBox();
            LabelDensidadeAr = new Guna.UI2.WinForms.Guna2HtmlLabel();
            LabelVelocidadeVento = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2HtmlLabel1 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            guna2Panel7.SuspendLayout();
            SuspendLayout();
            // 
            // guna2Panel7
            // 
            guna2Panel7.BorderColor = Color.Black;
            guna2Panel7.BorderRadius = 1;
            guna2Panel7.BorderThickness = 1;
            guna2Panel7.Controls.Add(guna2HtmlLabel1);
            guna2Panel7.Controls.Add(ComboRopeAtEnd);
            guna2Panel7.Controls.Add(ComboRopeAtRootComboRopeAtRoot);
            guna2Panel7.Controls.Add(ComboWingspan);
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
            // ComboRopeAtEnd
            // 
            ComboRopeAtEnd.FormattingEnabled = true;
            ComboRopeAtEnd.Location = new Point(12, 141);
            ComboRopeAtEnd.Name = "ComboRopeAtEnd";
            ComboRopeAtEnd.Size = new Size(153, 23);
            ComboRopeAtEnd.TabIndex = 22;
            // 
            // ComboRopeAtRootComboRopeAtRoot
            // 
            ComboRopeAtRootComboRopeAtRoot.FormattingEnabled = true;
            ComboRopeAtRootComboRopeAtRoot.Location = new Point(12, 88);
            ComboRopeAtRootComboRopeAtRoot.Name = "ComboRopeAtRootComboRopeAtRoot";
            ComboRopeAtRootComboRopeAtRoot.Size = new Size(153, 23);
            ComboRopeAtRootComboRopeAtRoot.TabIndex = 21;
            // 
            // ComboWingspan
            // 
            ComboWingspan.FormattingEnabled = true;
            ComboWingspan.Location = new Point(12, 31);
            ComboWingspan.Name = "ComboWingspan";
            ComboWingspan.Size = new Size(153, 23);
            ComboWingspan.TabIndex = 20;
            // 
            // LabelDensidadeAr
            // 
            LabelDensidadeAr.BackColor = Color.Transparent;
            LabelDensidadeAr.Font = new Font("Segoe UI", 11.25F);
            LabelDensidadeAr.Location = new Point(12, 60);
            LabelDensidadeAr.Name = "LabelDensidadeAr";
            LabelDensidadeAr.Size = new Size(116, 22);
            LabelDensidadeAr.TabIndex = 18;
            LabelDensidadeAr.Text = "Rope at the Root (m)";
            // 
            // LabelVelocidadeVento
            // 
            LabelVelocidadeVento.BackColor = Color.Transparent;
            LabelVelocidadeVento.Font = new Font("Segoe UI", 11.25F);
            LabelVelocidadeVento.Location = new Point(12, 3);
            LabelVelocidadeVento.Name = "LabelVelocidadeVento";
            LabelVelocidadeVento.Size = new Size(69, 22);
            LabelVelocidadeVento.TabIndex = 17;
            LabelVelocidadeVento.Text = "Wingspan (m)";
            // 
            // guna2HtmlLabel1
            // 
            guna2HtmlLabel1.BackColor = Color.Transparent;
            guna2HtmlLabel1.Font = new Font("Segoe UI", 11.25F);
            guna2HtmlLabel1.Location = new Point(12, 117);
            guna2HtmlLabel1.Name = "guna2HtmlLabel1";
            guna2HtmlLabel1.Size = new Size(109, 22);
            guna2HtmlLabel1.TabIndex = 23;
            guna2HtmlLabel1.Text = "Rope at the End (m)";
            // 
            // TrapezoidalWingControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(guna2Panel7);
            Name = "TrapezoidalWingControl";
            Size = new Size(199, 164);
            guna2Panel7.ResumeLayout(false);
            guna2Panel7.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel guna2Panel7;
        public ComboBox ComboRopeAtRootComboRopeAtRoot;
        public ComboBox ComboWingspan;
        private Guna.UI2.WinForms.Guna2HtmlLabel LabelDensidadeAr;
        private Guna.UI2.WinForms.Guna2HtmlLabel LabelVelocidadeVento;
        public ComboBox ComboRopeAtEnd;
        private Guna.UI2.WinForms.Guna2HtmlLabel guna2HtmlLabel1;
    }
}
