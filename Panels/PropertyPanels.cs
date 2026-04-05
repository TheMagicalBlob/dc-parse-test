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
        //================================\\
        //--|   Class Initialization   |--\\
        //================================\\

        /// <summary>
        /// Initialize a new instance of the PropertiesHandler class.<br/><br/>
        /// Used for management of the PropertiesPanel and PropertiesWindow (struct buttons & details display).
        /// </summary>
        public PropertyPanels()
        {
            //#
            //## Properties Handler Variable Declarations
            //#
            DefaultPropertyListButtonHeight = 23;
            DefaultPropertyEditorRowHeight = 23;

            Changes = new List<object[]>();
            History = new List<object[]>();




            //#
            //## Create the various delegates for the Properties Handler, so we can do shit across multiple threads
            //#

            setupPropertyListPopulation = SetupPropertyListPopulation;

            spawnVariableEditorBox = SpawnVariableEditorBox;
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






        //#
        //## Threading-Related Variables (threads, delegates, and mammets)
        //#
        public delegate void PropertyPanelEventHandler(object MemberValue, string MemberName);

        public readonly PropertyPanelEventHandler setupPropertyListPopulation;

        public PropertyPanelEventHandler spawnVariableEditorBox;
        #endregion










        //===============================================\\
        //--|   Miscellaneous Function Declarations   |--\\
        //===============================================\\
        #region [Miscellaneous Function Declarations]

        /// <summary>
        /// //!
        /// </summary>
        /// <param name="groupBox"></param>
        /// <param name="hostBoxScrollBarReference"></param>
        /// <param name="cumulativeButtonHeight"></param>
        private void CreateScrollBarForGroupBox(Control groupBox, ref VScrollBar hostBoxScrollBarReference, int cumulativeButtonHeight)
        {
            if (!Venat.Controls.Contains(hostBoxScrollBarReference))
            {
                if (hostBoxScrollBarReference == null)
                {
                    hostBoxScrollBarReference = new VScrollBar()
                    {
                        Name = "PropertiesPanelScrollBar",
                        Height = groupBox.Height - 2,
                        Width = 20, // Default width's a bit fat
                        //LargeChange = DefaultPropertyButtonHeight * 4, // Not sure which context the LargeChange is even used in, honestly
                    };
                        

                    hostBoxScrollBarReference.Location = new Point((groupBox.Parent.Location.X + groupBox.Width) - (hostBoxScrollBarReference.Width + 1), groupBox.Parent.Location.Y);

                    hostBoxScrollBarReference.Scroll += (_, args) => ScrollPropertyListButtons(groupBox, args);
                }

                Venat.Controls.Add(hostBoxScrollBarReference);
            }


            hostBoxScrollBarReference.BringToFront();
                
            hostBoxScrollBarReference.Maximum = (cumulativeButtonHeight - groupBox.Height) + (GroupBox.GroupBoxContentsOffset * 2);

            hostBoxScrollBarReference.SmallChange = DefaultPropertyListButtonHeight;
        }






        /// <summary>
        /// Reset all instance members in the current PropertiesHandler (clear all added controls, reset static ones to default states, clear variables)
        /// </summary>
        public void ResetPanels()
        {
            PropertyWindow.Clear();
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
