using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

#pragma warning disable IDE0011
namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //=======================================================\\
        //---|   Logging/Output Functionality Declarations   |---\\
        //=======================================================\\
        #region [Logging/Output Functionality Declarations]

        /// <summary>
        /// Echo a provided string (or string representation of an object) to the standard console output.
        /// <br/> Appends an empty new line if no message is provided.
        /// </summary>
#pragma warning disable IDE1006 // bug off, this one's lowercase
        public static void echo(object message = null)
        {
#if DEBUG
            string str;

            Console.WriteLine(str = message?.ToString() ?? emptyStr);
            Debug.WriteLineIf(!Console.IsOutputRedirected, str);

            if (!Console.IsOutputRedirected)
            {
                Debug.WriteLine(str);
            }
#endif
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <param name="message"></param>
        public static void _echo(object message = null)
        {
#if DEBUG
            string str;

            Console.Write(str = message?.ToString() ?? emptyStr);

            if (!Console.IsOutputRedirected)
            {
                Debug.Write(str);
            }
#endif
        }
#pragma warning restore IDE1006






        /// <summary>
        /// Print the provided <paramref name="Message"/> to the LogWindow, followed by a newline
        /// </summary>
        /// <param name="Message"> The message to Append to the LogWindow's text property. </param>
        public void Log(string Message)
        {
            LogWindow.AppendLine(Message);
        }






        /// <summary>
        /// Print the provided <paramref name="Message"/> to the LogWindow.
        /// </summary>
        /// <param name="Message"> The message to Append to the LogWindow's text property. </param>
        public void _Log(string Message)
        {
            LogWindow.AppendText(Message);
        }






        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="details"> The string[] to update the label's text with. </param>
        public static void UpdateStatusLabel(string details)
        {
            if ((details?.Length ?? 0) < 1)
            {
                echo($"ERROR: Empty or null string array provided for status label details.");
                return;
            }

            StatusDetails = details;
        }




        /// <summary>
        /// Reset the ScriptStatusLabel to it's default value.
        /// </summary>
        public static void ResetStatusLabel()
        {
            StatusDetails = null;
        }






        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="details"> The string to update the label's text with. </param>
        public static void UpdateSelectionLabel(string details)
        {
            if ((details?.Length ?? 0) < 1)
            {
                echo($"ERROR: Empty or null string array provided for selection label details.");
                return;
            }

            SelectionDetails = details;
        }






        /// <summary>
        /// Reset the ScriptSelectionLabel to it's default value.
        /// </summary>
        public static void ResetSelectionLabel()
        {
            SelectionDetails = null;
        }
        #endregion
    }
}
