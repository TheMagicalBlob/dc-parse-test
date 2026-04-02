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
        /// 
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



            // Newline
            propertiesWindowNewLineMammet = (message) =>
            {
                PropertyWindow.AppendLine(message, false);
                PropertyWindow.Update();
            };


            // Newline W/ Indent
            propertiesWindowMammet = (message) =>
            {
                PropertyWindow.AppendLine(message.Replace("\n", "\n" + Indentation));
                PropertyWindow.Update();
            };

        }






        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        //#
        //## Properties Panels Functionality Variables
        //#

        /// <summary>
        /// So far only used in handling wrapping
        /// </summary>
        private PropertyButton[] FirstAndLastPropertyButtons;


        /// <summary>
        /// The selected/highlighted button out of the loaded header item buttons
        /// </summary>
        private PropertyButton PropertySelection
        {
            get => _propertySelection;

            set {
                if (value != null)
                {
                    LoadSelectionPropertiesIntoPropertyEditor(value.DCProperty);
                }

                _propertySelection = value;
            }
        }
        private PropertyButton _propertySelection;





        /// <summary>
        /// The (vertical) scroll bar used to navigate the buttons populating the PropertyList when they bleed passed the bottom of the group box
        /// </summary>
        public VScrollBar PropertyListScrollBar;
        public int PaddingForPropertyListScrollBar;

        /// <summary>
        /// The (vertical) scroll bar used to navigate the rows populating the PropertyEditor when they bleed passed the bottom of the group box
        /// </summary>
        private VScrollBar PropertyEditorScrollBar;
        private int PaddingForPropertyEditorScrollBar;



        private readonly List<object[]> History;




        private int IndentationDepth
        {
            get => Indentation.Length < 4 ? 0 : Indentation.Length / 4;
            
            set => Indentation = new string(' ', value > 0 ? value * 4 : 0);
        }

        private string Indentation = emptyStr;




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

        /// <summary> //! </summary>
        public delegate void PropertiesWindowOutputWand(string msg);

        /// <summary> //! </summary>
        //public delegate void SubsequentPropertiesPanelPopulation(object structProperty, string structName);

        public delegate void PropertyPanelEventHandler(object MemberValue, string MemberName);


        private readonly PropertiesWindowOutputWand propertiesWindowMammet;
        private readonly PropertiesWindowOutputWand propertiesWindowNewLineMammet;


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

            IndentationDepth = 0;
        }
        #endregion
    }
}
