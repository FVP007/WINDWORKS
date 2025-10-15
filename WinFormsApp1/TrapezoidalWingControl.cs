using System;
using System.Globalization;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class TrapezoidalWingControl : UserControl, IWingControl
    {
        // Propriedade 'Rope' não é necessária aqui, pois temos RopeAtRoot e RopeAtEnd.
        // Removi para evitar confusão. Se precisar dela, ela deve calcular a corda média.
        public double Rope => (RopeAtRoot + RopeAtEnd) / 2.0;

        public double Wingspan
        {
            get
            {
                if (double.TryParse(ComboWingspan.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    return value;
                return 0.0;
            }
        }

        public double RopeAtRoot
        {
            get
            {
                // CORRIGIDO: Usando o nome correto do controle e lendo a propriedade .Text
                if (double.TryParse(ComboRopeAtRoot.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    return value;
                return 0.0;
            }
        }

        public double RopeAtEnd
        {
            get
            {
                if (double.TryParse(ComboRopeAtEnd.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    return value;
                return 0.0;
            }
        }

        public double WingArea => ((RopeAtRoot + RopeAtEnd) / 2.0) * Wingspan;

        public TrapezoidalWingControl()
        {
            InitializeComponent();
        }
    }
}