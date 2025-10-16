// ExportChoiceForm.Designer.cs
namespace WinFormsApp1
{
    partial class ExportChoiceForm
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
            this.labelQuestion = new System.Windows.Forms.Label();
            this.btnExportCurrent = new System.Windows.Forms.Button();
            this.btnExportAll = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // labelQuestion
            //
            this.labelQuestion.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelQuestion.Location = new System.Drawing.Point(12, 18);
            this.labelQuestion.Name = "labelQuestion";
            this.labelQuestion.Size = new System.Drawing.Size(376, 30);
            this.labelQuestion.TabIndex = 0;
            this.labelQuestion.Text = "Which data would you like to export to XML?";
            this.labelQuestion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnExportCurrent
            //
            this.btnExportCurrent.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnExportCurrent.Location = new System.Drawing.Point(15, 68);
            this.btnExportCurrent.Name = "btnExportCurrent";
            this.btnExportCurrent.Size = new System.Drawing.Size(110, 35);
            this.btnExportCurrent.TabIndex = 1;
            this.btnExportCurrent.Text = "Export Current";
            this.btnExportCurrent.UseVisualStyleBackColor = true;
            //
            // btnExportAll
            //
            this.btnExportAll.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btnExportAll.Location = new System.Drawing.Point(145, 68);
            this.btnExportAll.Name = "btnExportAll";
            this.btnExportAll.Size = new System.Drawing.Size(110, 35);
            this.btnExportAll.TabIndex = 2;
            this.btnExportAll.Text = "Export All";
            this.btnExportAll.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(275, 68);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 35);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // ExportChoiceForm
            //
            this.AcceptButton = this.btnExportCurrent;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 120); // Tamanho ajustado
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExportAll);
            this.Controls.Add(this.btnExportCurrent);
            this.Controls.Add(this.labelQuestion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportChoiceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose Export Type";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelQuestion;
        private System.Windows.Forms.Button btnExportCurrent;
        private System.Windows.Forms.Button btnExportAll;
        private System.Windows.Forms.Button btnCancel;
    }
}