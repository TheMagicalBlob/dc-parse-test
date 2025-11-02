using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]
        //#
        //## Global Look/Feel-Related Variables
        //#

        public static Color AppColour = Color.FromArgb(20, 20, 20);
        public static Color AppColourLight = Color.FromArgb(42, 42, 42);
        public static Color AppColourSpecial = Color.FromArgb(125, 183, 245);
        public static Color AppAccentColour = Color.FromArgb(210, 240, 250); // Why did I choose this colour specifically? I forget.

        public static Pen FormDecorationPen = new Pen(AppAccentColour); // Colouring for Border Drawing

        public static Font MainFont = new Font("Gadugi", 8.25f, FontStyle.Bold); // For the vast majority of controls; anything the user doesn't edit, really.
        public static Font TextFont = new Font("Segoe UI Semibold", 7.5f); // For option controls with customized contents
        public static Font DefaultTextFont = new Font("Segoe UI Semibold", 9f, FontStyle.Italic); // For option controls in default states

        public static int SubformVerticalOffset = 50;

#if DEBUG
        /// <summary> Disable drawing of form border/separator lines </summary>
        public static bool noDraw;
#endif
        #endregion
    }
}
