using System;
using System.Globalization;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class RectangularWingControl : UserControl, IWingControl
    {
        public double Rope
        {
            get
            {
                // CORRIGIDO: Lendo a propriedade .Text em vez de SelectedItem
                if (double.TryParse(ComboRope.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    return value;
                return 0.0;
            }
        }

        public double Wingspan
        {
            get
            {
                // CORRIGIDO: Lendo a propriedade .Text em vez de SelectedItem
                if (double.TryParse(ComboWingspan.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    return value;
                return 0.0;
            }
        }

        public double WingArea => Rope * Wingspan;

        public RectangularWingControl()
        {
            InitializeComponent();
        }
    }
}