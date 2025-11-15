using System;

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
        /// <br/>Scrolls To Keep The Newest Line In View.
        /// </summary>
        /// <param name="str"> The String to Output. </param>
        public void AppendLine(string str = "", bool scroll = true)
        {
            AppendText(str + '\n');
            Update();

            if (scroll)
            {
                ScrollToCaret();
            }
        }



        public void UpdateLine(string newMsg, int line, bool scroll = true)
        {
            while (line >= Lines.Length)
            {
                AppendText("\n");
            }

            var lines = Lines;
            lines[line] = newMsg ?? " ";

            Lines = lines;
            Update();

            if (scroll)
            {
                ScrollToCaret();
            }
        }
    }





    /// <summary>
    /// Custom TextBox Class to Better Handle Default TextBox Contents.
    /// </summary>
    public class TextBox : System.Windows.Forms.TextBox
    {
        /// <summary> Create a new winforms TextBox control. </summary>
        public TextBox()
        {

        }


        public override string Text
        {
            get => base.Text;

            set => base.Text = value?.Replace("\"", string.Empty);
        }
    }





    public class Label : System.Windows.Forms.Label
    {
        public bool IsSeparatorLine { get; set; } = false;


        public bool StretchToFitForm
        {
            get => _stretchToFitForm & IsSeparatorLine;
            set => _stretchToFitForm = value;
        }
        private bool _stretchToFitForm = false;
    }


    public class PropertyButton : System.Windows.Forms.Button
    {
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
    #endregion
}
