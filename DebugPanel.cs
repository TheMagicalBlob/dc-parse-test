using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using static weapon_data.Main;
using System.Drawing;

namespace weapon_data
{
    public partial class DebugPanel : Form
    {
        public DebugPanel()
        {
            InitializeComponent();
            InitializeAdditionalEventHandlers();
        }

        
        //#
        //## Debug Variable Declarations
        //#
        #region [debug variable declarations]

        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        public Point[][] HSeparatorLines;
        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        public Point[][] VSeparatorLines;
        #endregion



        //#
        //## Debug Function Declarations
        //#
        #region [debug function declarations]
        
        private void debugMiscBtn_Click(object sender, EventArgs e)
        {
            var ffs = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            foreach (var item in ffs)
            {
                echo($"[{item}]: {(byte) item} => {(byte) item.ToString().ToUpper() [0]}");
            }

            for(byte beh = 90; beh < 98; beh++)
            {
                echo($"{(char)beh}");
            }
            echo((char)123);
        }
        
        private void debugDisableLinesBtn_CheckedChanged(object sender, EventArgs e)
        {
            #if DEBUG
            noDraw ^= true;
            CreateGraphics().Clear(BackColor);
            Refresh();
            #endif
        }
        

        // verify tab index adjustment is working as expected
        private void debugTabCheckBtn_Click(object sender, EventArgs e)
        {
            void eh(System.Windows.Forms.Control.ControlCollection controls)
            {
                foreach (Control cunt in controls)
                {
                    if (cunt.HasChildren)
                    {
                        eh (cunt.Controls);
                    }
                    PrintNL($"# [{cunt.Name}: {cunt.TabIndex}]");
                }

            }

            eh(this.Controls);
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Bingus.Visible = false;
            Venat?.Update();
        }
        #endregion
    }
}
