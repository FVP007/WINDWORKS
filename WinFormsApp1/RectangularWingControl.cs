using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class RectangularWingControl : UserControl, IWingControl
    {
        // Propriedades públicas para acessar os valores
        public double Rope
        {
            get
            {
                try
                {
                    if (ComboAirDensity?.SelectedItem != null)
                    {
                        string selectedText = ComboAirDensity.SelectedItem.ToString();
                        if (double.TryParse(selectedText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                        {
                            return value;
                        }
                        else
                        {
                            Console.WriteLine($"Erro ao converter Rope: '{selectedText}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ComboAirDensity.SelectedItem is null");
                    }
                    return 0.0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro na propriedade Rope: {ex.Message}");
                    return 0.0;
                }
            }
        }

        public double Wingspan
        {
            get
            {
                try
                {
                    if (ComboWindSpeed?.SelectedItem != null)
                    {
                        string selectedText = ComboWindSpeed.SelectedItem.ToString();
                        if (double.TryParse(selectedText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                        {
                            return value;
                        }
                        else
                        {
                            Console.WriteLine($"Erro ao converter Wingspan: '{selectedText}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ComboWindSpeed.SelectedItem is null");
                    }
                    return 0.0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro na propriedade Wingspan: {ex.Message}");
                    return 0.0;
                }
            }
        }

        // Propriedade para calcular a área da asa retangular
        public double WingArea
        {
            get
            {
                try
                {
                    double rope = Rope;
                    double wingspan = Wingspan;
                    double area = rope * wingspan;
                    
                    Console.WriteLine($"Rectangular: Rope={rope:F2}, Wingspan={wingspan:F2}, Area={area:F4}");
                    
                    return area;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao calcular WingArea: {ex.Message}");
                    return 0.0;
                }
            }
        }

        public RectangularWingControl()
        {
            InitializeComponent();
           
        }

    }
}
