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
    public partial class TrapezoidalWingControl : UserControl, IWingControl
    {
        // Propriedades públicas para acessar os valores
        public double Rope
        {
            get
            {
                if (ComboRopeAtRootComboRopeAtRoot.SelectedItem != null && double.TryParse(ComboRopeAtRootComboRopeAtRoot.SelectedItem.ToString(), out double value))
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

        public double RopeAtRoot
        {
            get
            {
                if (ComboRopeAtRootComboRopeAtRoot.SelectedItem != null && double.TryParse(ComboRopeAtRootComboRopeAtRoot.SelectedItem.ToString(), out double value))
                    return value;
                return 0.0;
            }
        }

        public double RopeAtEnd
        {
            get
            {
                if (ComboRopeAtEnd.SelectedItem != null && double.TryParse(ComboRopeAtEnd.SelectedItem.ToString(), out double value))
                    return value;
                return 0.0;
            }
        }

        // Propriedade para calcular a área da asa trapezoidal
        public double WingArea => (RopeAtRoot + RopeAtEnd) / 2 * Wingspan;

        public TrapezoidalWingControl()
        {
            InitializeComponent();
        }
    }
}
