using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NaughtyDogDCReader
{
    //=====================================\\
    //---|   Custom Class Extensions   |---\\
    //=====================================\\
    #region [Custom Class Extensions]

    /// <summary>
    /// Custom RichTextBox class because bite me.
    /// </summary>
    public class RichTextBox : System.Windows.Forms.RichTextBox
    {
        /// <summary>
        /// Appends Text to The Current Text of A Text Box, Followed By The Standard Line Terminator.
        /// <br/>
        /// </summary>
        /// <param name="str"> The String to Output. </param>
        public void AppendLine(string str = "")
        {
            AppendText(str + '\n');
            Update();

            if (Scroll)
            {
                ScrollToCaret();
            }
        }




        /// <summary>
        /// Update a specific <paramref name="Line"/> in the LogWindow's output with the provided <paramref name="Message"/>
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Line"></param>
        /// <param name="Scroll"></param>
        public void UpdateLine(string Message, int Line)
        {
            while (Line >= Lines.Length)
            {
                AppendText("\n");
            }

            var lines = Lines;
            lines[Line] = Message ?? " ";

            Lines = lines;
        }


        new public void AppendText(string Message)
        {
            base.AppendText(Message ?? string.Empty);

            Update();

            if (Scroll)
            {
                ScrollToCaret();
            }
        }

        public bool Scroll;
    }



    public class GroupBox : System.Windows.Forms.GroupBox
    {
        public GroupBox() : base()
        {
            Paint += RemoveGroupBoxBorderAndText;
        }

        private void RemoveGroupBoxBorderAndText(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.Clear(((GroupBox) sender).BackColor);
        }

        public static readonly int GroupBoxContentsOffset = 6;
    }



    /// <summary>
    /// Custom TextBox Class to Better Handle Default TextBox Contents.
    /// </summary>
    public class TextBox : System.Windows.Forms.TextBox
    {
        /// <summary> Create a new winforms TextBox control. </summary>
        public TextBox()
        {
            BackColor = Color.FromArgb(42, 42, 42);
            Font = Main.TextFont;
            ForeColor = SystemColors.Window;
            TabIndex = 3;
            TabStop = false;
        }


        public override string Text
        {
            get => base.Text;

            set => base.Text = value?.Replace("\"", string.Empty);
        }
    }








    /// <summary>
    /// Custom Button class extension for use of additional PropertyWindow-Specific features.<br/>
    /// (DCProperty property for better readablity, rather than using the "Tag" property)
    /// </summary>
    public class PropertyButton : System.Windows.Forms.Button
    {
        public PropertyButton()
        {
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
        }


        /// <summary>
        /// The property associated with the current PropertyWindow button;
        /// </summary>
        public object DCProperty
        {
            get => _dcProperty;

            set {
                _dcProperty = value;
            }
        }

        private object _dcProperty;
    }






    public class Label : System.Windows.Forms.Label
    {
        public bool IsSeparatorLine { get; set; } = false;


        public bool StretchToFitForm
        {
            get => _stretchToFitForm && IsSeparatorLine;
            set => _stretchToFitForm = value;
        }
        private bool _stretchToFitForm = false;
    }







    /// <summary>
    /// Small Form class extention for horizontal & vertical SeparatorLines
    /// </summary>
    public class Form : System.Windows.Forms.Form
    {
        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        public Point[][] HSeparatorLines;

        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        public Point[][] VSeparatorLines;
    }
#endregion
}
