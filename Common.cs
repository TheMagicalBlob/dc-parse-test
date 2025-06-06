﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace weapon_data
{
    public partial class Main
    {

        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        //#
        //## Script Parsing Globals
        //#
        public byte[] sidbase
        {
            get => _sidbase;

            set {
                _sidbase = value;

                if (value.Length > 8)
                {
                    sidLength = BitConverter.ToInt64(sidbase, 0) * 16;
                }
            }
        }
        private byte[] _sidbase;
        public long sidLength;

        public static DCFileHeader DCFile;

        public List<object[]> DecodedIDS = new List<object[]>(1000);



        //#
        //## Form Functionality Globals
        //#
        public bool abort = false;
        public bool redirect = 
#if DEBUG
            true
#else
            false
#endif
        ;

        public static string ActiveLabel;

        
        public static Label activeScriptLabel;
        public static Button abortBtn;


        private Thread binThread;
        public delegate void binThreadFormWand(object obj); //! god I need to read about delegates lmao
        public delegate string[] binThreadFormWandOutputRead(); //! god I need to read about delegates lmao
        public delegate void binThreadFormWandArray(string msg, int? line); //! god I need to read about delegates lmao

        public binThreadFormWand outputMammet = new binThreadFormWand((obj) => OutputWindow.AppendLine(obj.ToString()));
        public binThreadFormWand labelMammet = new binThreadFormWand(UpdateLabel);
        public binThreadFormWand abortButtonMammet  = new binThreadFormWand((obj) => abortBtn.Visible = (bool)obj);
        public binThreadFormWand reloadButtonMammet = new binThreadFormWand((_) =>
        {
            if (Venat == null) {
                return;
            }

            Venat.ReloadScriptBtn.Enabled ^= true;
            Venat.ReloadScriptBtn.Font = new Font(Venat?.ReloadScriptBtn.Font.FontFamily, Venat.ReloadScriptBtn.Font.Size, Venat.ReloadScriptBtn.Font.Style ^ FontStyle.Strikeout);
        });

        public binThreadFormWandOutputRead outputReadMammet = new binThreadFormWandOutputRead(() => OutputWindow.Lines);
            
        public binThreadFormWandArray outputMammetSameLine = new binThreadFormWandArray((msg, line) =>
        {
            OutputWindow.UpdateLine(msg, line ?? 1 - 1);
            OutputWindow.Update();
            
            //OutputWindow.Text = OutputWindow.Text.Remove(OutputWindow.Text.LastIndexOf("\n")) + '\n' + obj;
            //OutputWindow.Update();
        });

        
        /// <summary> Boolean global to set the type of dialogue to use for the GamedataFolder path box's browse button. </summary>
        public static bool LegacyFolderSelectionDialogue = true;

        /// <summary> Return the current state of the options page. </summary>
        public static bool OptionsPageIsOpen { get => Azem.Visible; }

        /// <summary> Boolean global for keeping track of the current mouse state. </summary>
        public static bool MouseIsDown = false;


        /// <summary>
        /// Store Expected Options Form Offset
        /// </summary>
        public static Point OptionsFormLocation;

        /// <summary> Variable for Smooth Form Dragging. </summary>
        public static Point MouseDif;
        
        /// <summary> MainPage Form Pointer/Refference. </summary>
        public static Main Venat;

        /// <summary> OptionsPage Form Pointer/Refference. </summary>
        public static OptionsPage Azem;

        /// <summary> OutputWindow Pointer/Ref Because I'm Lazy. </summary>
        public static RichTextBox OutputWindow;



        //#
        //## Look/Feel-Related Variables
        //#

        public static Color AppColour = Color.FromArgb(125, 183, 245);
        public static Color AppColourLight = Color.FromArgb(210, 240, 250);

        public static Pen pen = new Pen(AppColourLight); // Colouring for Border Drawing

        public static readonly Font MainFont        = new Font("Gadugi", 8.25f, FontStyle.Bold); // For the vast majority of controls; anything the user doesn't edit, really.
        public static readonly Font TextFont        = new Font("Segoe UI Semibold", 9f); // For option controls with customized contents
        public static readonly Font DefaultTextFont = new Font("Segoe UI Semibold", 9f, FontStyle.Italic); // For option controls in default states
        #endregion

        
        
        //=========================================\\
        //--|   Global Function Delcarations   |---\\
        //=========================================\\
        #region [Global Function Delcarations]

        //#
        //## Form Functionality Functions
        //#
        #region [Form Functionality Functions]

        /// <summary>
        /// Handle Form Dragging for Borderless Form.
        /// </summary>
        public static void MoveForm()
        {
            if(MouseIsDown && Venat != null)
            {
                Venat.Location = new Point(MousePosition.X - MouseDif.X, MousePosition.Y - MouseDif.Y);
                if (Azem != null)
                    Azem.Location = new Point(MousePosition.X - MouseDif.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 40);
                
                Venat.Update();
                Azem?.Update();
            }
        }

        
        /// <summary>
        /// Draw a Thin Border for the Control On-Paint
        /// </summary>
        public static void PaintBorder(object sender, PaintEventArgs e)
        {
            var ItemPtr = sender as Form;

            var Border = new Point[]
            {
                Point.Empty,
                new Point(ItemPtr.Width-1, 0),
                new Point(ItemPtr.Width-1, ItemPtr.Height-1),
                new Point(0, ItemPtr.Height-1),
                Point.Empty
            };

            e.Graphics.Clear(Color.FromArgb(0, 0, 0));
            e.Graphics.DrawLines(pen, Border);
        }
        #endregion


        //#
        //## Logging functionaliy
        //#
        #region [Logging Functionality]

        public static void echo(object message = null)
        {
            var str = message.ToString();
            Console.WriteLine(str);
            if (!Console.IsInputRedirected) {
                Debug.WriteLine(str);
            }

            if (Venat?.redirect ?? false || str.Contains("ERROR"))
            {
                Venat?.PrintNL(str);
            }
        }


        public void CTUpdateLabel(object str)
        {
            Venat?.Invoke(labelMammet, new object[] { str });
        }

        
        public static void UpdateLabel(object str)
        {
            activeScriptLabel.Text = "Selected Script: " + str;
        }

        public void PrintLL(string str, int line = 1)
        {
            
#if DEBUG
            // Debug Output
            if (!Console.IsOutputRedirected)
            {
                Console.WriteLine(str);
            }
            else
                Debug.WriteLine(str ?? "null");
#endif

            Venat?.Invoke(outputMammetSameLine, str, line < 1 ? 1 : line);
        }

        /// <summary>
        /// Output Misc. Messages to the Main Output Window (the big-ass richtext box).
        /// </summary>
        public void PrintNL(object str = null)
        {
            if (str == null)
                str = string.Empty;

#if DEBUG
            // Debug Output
            if (!Console.IsOutputRedirected)
            {
                Console.WriteLine(str);
            }
            else
                Debug.WriteLine(str ?? "null");
#endif

            Venat?.Invoke(outputMammet, new object[] { str.ToString() });
        }

        public string[] GetOutputWindowLines() => (string[]) Venat?.Invoke(outputReadMammet);
        #endregion


        
        /// <summary>
        /// Use the Buffer class to copy and return a smaller sub-array from a provided <paramref name="array"/>.
        /// </summary>
        /// <param name="array"> The larger array from which to take the sub-array returned. </param>
        /// <param name="index"> The start index in <paramref name="array"/> from which the copying starts. </param>
        /// <param name="len"> The length of the sub-array to be returned. Defaults to 8 bytes. </param>
        /// <returns> A byte array of 8 bytes (or an optional different length) copied from the specified <paramref name="index"/> in <paramref name="array"/>. </returns>
        public byte[] GetSubArray(byte[] array, int index, int len = 8)
        {
            var ret = new byte[8];
            Buffer.BlockCopy(array, index, ret, 0, len);

            return ret;
        }

        


        /// <summary>
        /// Parse the current sidbase.bin for the string representation of the provided 64-bit fnv1a hash.
        /// </summary>
        /// <param name="bytesToDecode"> The 8-byte array of bytes to decode. </param>
        /// <returns> Either the decoded version of the provided hash, or the string representation of said SID if it could not be decoded. </returns>
        public string DecodeSIDHash(byte[] bytesToDecode)
        {            
            if (bytesToDecode.Length == 8)
            {
                for (long mainArrayIndex = 0, subArrayIndex = 0; mainArrayIndex < sidLength; subArrayIndex = 0, mainArrayIndex+=8)
                {
                    if (sidbase[mainArrayIndex] != (byte)bytesToDecode[subArrayIndex])
                    {
                        continue;
                    }


                    // Scan for the rest of the bytes
                    while ((subArrayIndex < 8 && mainArrayIndex < sidbase.Length) && sidbase[mainArrayIndex + subArrayIndex] == (byte)bytesToDecode[subArrayIndex]) // while (subArrayIndex < 8 && sidbase[mainArrayIndex++] == (byte)bytesToDecode[subArrayIndex++]) how the fuck does this behave differently?? I need sleep.
                    {
                        subArrayIndex++;
                    }

                    // continue if there was only a partial match
                    if (subArrayIndex != 8)
                    {
                        continue;
                    }
                

                    // Read the string pointer
                    var stringPtr = BitConverter.ToInt64(sidbase, (int)(mainArrayIndex + subArrayIndex));
                    if (stringPtr >= sidbase.Length)
                    {
                        throw new IndexOutOfRangeException($"ERROR: Invalid Pointer Read for String Data!\n    str* 0x{stringPtr:X} >= len 0x{sidbase.Length:X}.");
                    }


                    // Parse and add the string to the array
                    var stringBuffer = string.Empty;

                    while (sidbase[stringPtr] != 0)
                    {
                        stringBuffer += Encoding.UTF8.GetString(sidbase, (int)stringPtr++, 1);
                    }

                
                    return stringBuffer;
                }

                return BitConverter.ToString(bytesToDecode).Replace("-", string.Empty);
            }
            else {
                echo($"Invalid SID provided; unexpected length of \"{bytesToDecode?.Length ?? 0}\". Must be 8 bytes.");
                return "INVALID_SID_64";
            }
        }

        #endregion
    }



    //=====================================\\
    //---|   Custom Class Extensions   |---\\
    //=====================================\\
    #region [Custom Class Extensions]

    /// <summary>
    /// Custom RichTextBox class because bite me.
    /// </summary>
    public class RichTextBox : System.Windows.Forms.RichTextBox {

        /// <summary> Appends Text to The Currrent Text of A Text Box, Followed By The Standard Line Terminator.<br/>Scrolls To Keep The Newest Line In View. </summary>
        /// <param name="str"> The String To Output. </param>
        public void AppendLine(string str = "")
        {
            AppendText($"{str}\n");
            ScrollToCaret();
        }

        public void UpdateLine(string newMsg, int line)
        {
            var lines = Lines; // Not sure why I can't directly write to the array and have it actually update
            lines[line - 1] = newMsg;
            Lines = lines;
            Update();
        }
    }

    /// <summary> Custom TextBox Class to Better Handle Default TextBox Contents. </summary>
    public class TextBox : System.Windows.Forms.TextBox
    {
        /// <summary> Create a new winforms TextBox control. </summary>
        public TextBox()
        {
            TextChanged += SetDefaultText; // Save the first Text assignment as the DefaultText
            Font = Main.DefaultTextFont;

            GotFocus += (sender, args) => ReadyControl();
            LostFocus += (sender, args) => ResetControl(false); // Reset control if nothing was entered, or the text is a portion of the default text
        }



        /// <summary>
        /// Default Control Text to Be Displayed When "Empty".
        /// </summary>
        private string DefaultText;
        public override string Text
        {
            get => base.Text;

            set {
                base.Text = value?.Replace("\"", string.Empty);
            }
        }



        // Help Better Keep Track of Whether the User's Changed the Text, Because I'm a Moron.
        public bool IsDefault() => Text == DefaultText;

        /// <summary>
        /// Yoink Default Text From First Text Assignment (Ideally right after being created).
        /// </summary>
        private void SetDefaultText(object _, EventArgs __)
        {
            DefaultText = Text;
            Font = Main.DefaultTextFont;

            TextChanged -= SetDefaultText;
        }


        private void ReadyControl()
        {
            if(IsDefault()) {
                Clear();

                Font = Main.TextFont;
            }
        }


        public void Reset() => ResetControl(true);
        private void ResetControl(bool forceReset)
        {
            if(Text.Length < 1 || DefaultText.Contains(Text) || forceReset)
            {
                Text = DefaultText;
                Font = Main.DefaultTextFont;
            }

        }


        /// <summary>
        /// Set Control Text and State Properly (meh).
        /// </summary>
        public void Set(string text)
        {
            if (text != string.Empty && !DefaultText.Contains(text))
            {   
                Text = text;
                Font = Main.TextFont;
            }
        }
    }
    #endregion
}
