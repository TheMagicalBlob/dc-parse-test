using System.CodeDom;
using System.IO.Ports;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace NaughtyDogDCReader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [System.STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var scriptPath = null as string;
            try {
                if (args != null && args.Length > 0)
                {
                    if (args[0]?.Length > 0 && System.IO.File.Exists(args[0]))
                    {
                        scriptPath = args[0];
                    }
                    else {
                        var message = "Invalid file path provided to tool; starting without a preselected DC Script.";

                        System.Console.WriteLine(message);
                        System.Diagnostics.Debug.WriteLineIf(!System.Console.IsOutputRedirected, message);

                        MessageBox.Show(message, "Dingus.");
                    }

                }
            }
            catch (System.Exception err)
            {
                var message = err.Message;
#if DEBUG
                message += '\n' + err.StackTrace;
#endif
                System.Console.WriteLine(message);
                System.Diagnostics.Debug.WriteLineIf(!System.Console.IsOutputRedirected, message);

#if DEBUG
                MessageBox.Show(message, "Dingus.");
#endif
            }


            Application.Run(new Main(scriptPath));
        }
    }
}
