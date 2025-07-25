﻿using System;
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
            InitializeAdditionalEventHandlers_DebugPanel();

            CloseBtn.Click += (a, b) => {
                Bingus.Visible = false;
                Venat?.Update();
            };
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
                    PrintPropertyDetailNL($"# [{cunt.Name}: {cunt.TabIndex}]");
                }

            }

            eh(this.Controls);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AbortButtonMammet(null, true);
                UpdateStatusLabel(new[] { null, "WARNING: No sidbase.bin found; please provide one before loading a DC file." });
        }

        private void debugShowInvalidSIDsCheckBox_CheckedChanged(object sender, EventArgs e)  => ShowInvalidSIDs = ((CheckBox)sender).Checked;
    }
}
