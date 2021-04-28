///<summary>
///

///Main code stolen from Samuel Lai http://edgylogic.com/projects/display-brightness-vista-gadget/ :)
///
///</summary>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MonitorSun
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(args));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Details:\n"+ ex.Message, "Brightness controll not supported", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
