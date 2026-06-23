using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using DUOsaber;
namespace DUOSABER_form
{
    public partial class FormDUOSABER : Form
    {
        static DUOsaber.MainWindow DUOSABER;

        public FormDUOSABER()
        {
            InitializeComponent();
            viewTimer.Enabled = true;
        }

        private void viewTimer_Tick(object sender, EventArgs e)
        {
            DUOSABER = new MainWindow();
            ElementHost.EnableModelessKeyboardInterop(DUOSABER);
            DUOSABER.Show();
            Hide();
            viewTimer.Enabled = false;
        }
    }
}
