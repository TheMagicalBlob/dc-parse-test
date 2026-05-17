using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;

namespace NaughtyDogDCReader
{
    public partial class PropertyPanels
    {
        /// <summary>
        /// Initialize a new instance of the PropertiesHandler class.<br/><br/>
        /// Used for management of the PropertiesPanel and PropertiesWindow (struct buttons & details display).
        /// </summary>
        public PropertyPanels()
        {
            //##-> Properties Handler Variable Declarations
            DefaultPropertyListButtonHeight = 23;
            DefaultPropertyEditorRowHeight = 23;

            Changes = new List<object[]>();
            History = new List<object[]>();
        }












        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        //#
        //## Properties Panels Functionality Variables
        //#
        private readonly List<object[]> History;


        /// <summary>
        /// Made it a variable in case it's needed for scaling. May try and implement that at some point, since I'm designing these on a fairly low-res screen.
        /// </summary>
        private readonly int DefaultPropertyListButtonHeight;


        /// <summary>
        /// Made it a variable in case it's needed for scaling. May try and implement that at some point, since I'm designing these on a fairly low-res screen.
        /// </summary>
        private readonly int DefaultPropertyEditorRowHeight;
        #endregion












        //===============================================\\
        //--|   Miscellaneous Function Declarations   |--\\
        //===============================================\\
        #region [Miscellaneous Function Declarations]

        /// <summary>
        /// //!
        /// </summary>
        /// <param name="GroupBox"></param>
        /// <param name="HostBoxScrollBarReference"></param>
        /// <param name="cumulativeButtonHeight"></param>
        private void CreateScrollBarForGroupBox(Control GroupBox, ref VScrollBar HostBoxScrollBarReference, int EntryCount)
        {
            var cumulativeButtonHeight = (DefaultPropertyListButtonHeight * EntryCount) - 1; // I don't know why it's off by a pixel and I'm sick of fucking with it.

            if (cumulativeButtonHeight < PropertySelectionPanel.Height)
            {
                return;
            }


            if (!Venat.Controls.Contains(HostBoxScrollBarReference))
            {
                if (HostBoxScrollBarReference == null)
                {
                    HostBoxScrollBarReference = new VScrollBar()
                    {
                        Name = "PropertiesPanelScrollBar",
                        Height = GroupBox.Height - 2,
                        Width = 20, // Default width's a bit fat
                        //LargeChange = DefaultPropertyButtonHeight * 4, // Not sure which context the LargeChange is even used in, honestly
                    };
                        

                    HostBoxScrollBarReference.Location = new Point((GroupBox.Parent.Location.X + GroupBox.Width) - (HostBoxScrollBarReference.Width + 1), GroupBox.Parent.Location.Y);

                    HostBoxScrollBarReference.Scroll += (_, args) => ScrollPropertyListButtons(GroupBox, args);
                }

                Venat.Controls.Add(HostBoxScrollBarReference);
            }


            HostBoxScrollBarReference.BringToFront();
                
            HostBoxScrollBarReference.Maximum = cumulativeButtonHeight - GroupBox.Height + (NaughtyDogDCReader.GroupBox.GroupBoxContentsOffset * 2);

            HostBoxScrollBarReference.SmallChange = DefaultPropertyListButtonHeight;
        }






        /// <summary>
        /// Reset all instance members in the current PropertiesHandler (clear all added controls, reset static ones to default states, clear variables)
        /// </summary>
        public void ResetPanels()
        {
            PropertySelectionPanel.Controls.Clear();
            PropertyEditorPanel.Controls.Clear();

            FirstAndLastPropertyButtons = null;
            PropertySelection = null;

            Venat.Controls.Remove(PropertyListScrollBar);
            PropertyListScrollBar = null;

            Venat.Controls.Remove(PropertyEditorScrollBar);
            PropertyEditorScrollBar = null;
        }






        /// <summary>
        /// Prepend a space to any capitalized letter that follows a lowercase one.
        /// </summary>
        /// <returns> The provided <paramref name="StructName">, now spaced out rather than camel/pascal-case. </returns>
        private string SpaceOutStructName(string StructName)
        {
            if (!System.Globalization.CultureInfo.CurrentCulture.ToString().ToLower().StartsWith("en"))
            {
                echo("Warning- Unexpected culture info. No idea whether this matters. I'm fairly sure it doesn't in this case, though.");
            }

            var str = string.Empty;

            for (var charIndex = 0; charIndex < StructName.Length; charIndex++)
            {
                if (StructName[charIndex] <= 122u && StructName[charIndex] >= 97u)
                {
                    if (charIndex + 1 != StructName.Length)
                    {
                        if (StructName[charIndex + 1] >= 65u && StructName[charIndex + 1] <= 90u)
                        {
                            str += $"{StructName[charIndex]} ";
                            continue;
                        }
                    }
                }

                str += StructName[charIndex];
            }

            return str;
        }
        #endregion
    }
}
