using System;
using System.Windows.Forms;

namespace NaughtyDogDCReader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args != null && args.Length > 0)
            {
                var path = args[0];

                if (System.IO.File.Exists(path))
                {
                    Application.Run(new Main(path));
                }
                else {
                    string complain = "Invalid file path provided to tool; starting without a preselected DC Script.";
                    
                    Console.WriteLine(complain);
                    System.Diagnostics.Debug.WriteLine(complain);

                    MessageBox.Show(complain, "How did you even manage that?");
                }
            }
            

            Application.Run(new Main());
        }
    }
}
