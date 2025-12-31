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
        //===================================\\
        //---|   Function Declarations   |---\\
        //===================================\\
        #region [Function Declarations]

        //#
        //## Logging/Output functionality
        //#
        #region [Logging/Output Functionality]

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

            if (!Console.IsInputRedirected)
            {
                Debug.WriteLine(str);
            }
#endif
        }
        public static void echoSl(object message = null)
        {
#if DEBUG
            string str;

            Console.Write(str = message?.ToString() ?? emptyStr);

            if (!Console.IsInputRedirected)
            {
                Debug.Write(str);
            }
#endif
        }
#pragma warning restore IDE1006






        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="details"> The string[] to update the label's text with. </param>
        public static void SetStatusLabelDetails(string[] details)
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
        public static void ResetStatusLabelDetails()
        {
            StatusDetails = null;
        }






        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="details"> The string to update the label's text with. </param>
        public static void SetSelectionLabelDetails(string[] details)
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
        public static void ResetSelectionLabelDetails()
        {
            SelectionDetails = null;
        }
        #endregion
        #endregion [Global Functions]
    }
}
