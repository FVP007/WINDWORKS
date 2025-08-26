using Microsoft.Web.WebView2.WinForms;
using System;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class FormulaForm : Form
    {
        private WebView2 webView;

        public FormulaForm()
        {
            InitializeForm();
            InitializeWebView();
        }

        private void InitializeForm()
        {
            this.Text = "Fórmulas Aerodinâmicas";
            this.Width = 850;
            this.Height = 650;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.BackColor = System.Drawing.SystemColors.Control;
        }

        private void InitializeWebView()
        {
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(webView);
            webView.Source = new Uri("about:blank");

            webView.NavigationCompleted += (s, e) =>
            {
                string html = @"<!DOCTYPE html>
<html lang='pt-BR'>
<head>
    <meta charset='UTF-8'>
    <script src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>
    <script>
        window.MathJax = {
            tex: { inlineMath: [['$', '$']] }
        };
    </script>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
            background: #f5f5f5;
        }
        .formula {
            background: white;
            border: 2px solid #ddd;
            border-radius: 8px;
            padding: 20px;
            margin: 15px 0;
            text-align: center;
            font-size: 28px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
    </style>
</head>
<body>
    <div class=""formula"">$ S = b \times c $</div>
    <div class=""formula"">$ S = \frac{(c_{raiz} + c_{ponta})}{2} \times b $</div>
    <div class=""formula"">$ S = \frac{\pi}{4} \times b \times c_{raiz} $</div>
    <div class=""formula"">$ S = \frac{b \times c_{raiz}}{2} $</div>
    <div class=""formula"">$ F_L = \frac{1}{2} \times \rho \times v^2 \times S \times C_L $</div></div>
</body>
</html>";
                webView.CoreWebView2.NavigateToString(html);
            };
        }

        private FormulaForm formulaForm;
            
        private void ShowFormulasForm()
        {
            if (formulaForm == null || formulaForm.IsDisposed)
            {
                formulaForm = new FormulaForm();
                formulaForm.Owner = this;
            }
            formulaForm.Show();
            formulaForm.BringToFront();
        }
    }
}