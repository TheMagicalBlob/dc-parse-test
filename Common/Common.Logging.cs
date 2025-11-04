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
        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]
        #endregion








        //===================================\\
        //---|   Function Delcarations   |---\\
        //===================================\\
        #region [Function Delcarations]

        //#
        //## Miscellaneous Functions
        //#
        #region [Miscellaneous Functions]

        /// <summary>
        /// Get a sub-array of the specified <paramref name="length"/> from a larger <paramref name="array"/> of bytes, starting at the <paramref name="Address"/> specified.
        /// </summary>
        /// <param name="array"> The array from which to take the sub-array. </param>
        /// <param name="Address"> The start address of the sub-array within <paramref name="array"/>. </param>
        /// <param name="length"> The length of the sub-array. </param>
        /// <returns> What the hell do you think. </returns>
        public static byte[] GetSubArray(byte[] array, int Address, int length = 8)
        {
            if (length == 0)
            {
                return Array.Empty<byte>();
            }


            // Build return string.
            for (var ret = new byte[length];; ret[length - 1] = array[Address + (length-- - 1)])
            {
                if (length <= 0)
                {
                    return ret;
                }
            }
        }



        /// <summary>
        /// //!
        /// </summary>
        /// <param name="array"></param>
        /// <param name="subarray"></param>
        /// <param name="Address"></param>
        public static void WriteSubArray(byte[] array, byte[] subarray, int Address)
        {
            for (var length = subarray.Length - 1;; array[Address + length] = subarray[length--])
            {
                if (length < 1)
                {
                    return;
                }
            }
        }


        
        /// <summary>
        /// Reads a string from <paramref name="buffer"/> at the specified <paramref name="startAddress"/>, until the string terminator is read. <br/>
        /// Encoding: Converts the bytes to a char, so whatever string encoding format that results in.
        /// </summary>
        /// <param name="buffer"> The array of bytes from which to read the returned string. </param>
        /// <param name="startAddress"> The address in <paramref name="buffer"/> at which to start reading the returned string. </param>
        /// <param name="terminator"> The terminator for the string (defaults to the standard string terminator; 0x00). </param>
        /// <returns> Home with the Milk. </returns>
        public static string ReadString(byte[] buffer, int startAddress, byte terminator = 0)
        {
            var str = string.Empty;

            if (startAddress > buffer.Length)
            {
                return string.Empty;
            }

            do {
                str += (char) buffer[startAddress++];
            }
            while (startAddress < buffer.Length && buffer[startAddress] != terminator);

            return str;
        }
        #endregion






        //#
        //## Logging/Output functionaliy
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
        public static void UpdateStatusLabel(string[] details)
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
            Venat?.Invoke(Venat.statusLabelResetMammet);
        }



        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="details"> The string to update the label's text with. </param>
        public static void UpdateSelectionLabel(string[] details)
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
            Venat?.Invoke(Venat.selectionLabelResetMammet);
        }
        #endregion
        #endregion [Global Functions]
    }
}
