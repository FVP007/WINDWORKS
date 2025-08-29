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
    public partial class EllipticalWingControl : UserControl, IWingControl
    {
        // Propriedades públicas para acessar os valores
        public double Rope
        {
            get
            {
                if (ComboRope.SelectedItem != null && double.TryParse(ComboRope.SelectedItem.ToString(), out double value))
                    return value;
                return 0.0;
            }
        }

        public double Wingspan
        {
            get
            {
                if (ComboWingspan.SelectedItem != null && double.TryParse(ComboWingspan.SelectedItem.ToString(), out double value))
                    return value;
                return 0.0;
            }
        }

        // Propriedade para calcular a área da asa elíptica
        public double WingArea => (Math.PI / 4) * Wingspan * Rope;

        public EllipticalWingControl()
        {
            InitializeComponent();
        }
    }
}
