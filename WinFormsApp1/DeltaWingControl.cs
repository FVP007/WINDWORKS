using System;
using System.Globalization;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class DeltaWingControl : UserControl, IWingControl
    {
        public double Rope
        {
            get
            {
                if (double.TryParse(ComboRope.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    return value;
                return 0.0;
            }
        }

        public double Wingspan
        {
            get
            {
                if (double.TryParse(ComboWingspan.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    return value;
                return 0.0;
            }
        }

        public double WingArea => (Wingspan * Rope) / 2.0;

        public DeltaWingControl()
        {
            InitializeComponent();
        }
    }
}