using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace weapon_data
{
    public partial class Main
    {
        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        private Button[] HeaderItemButtons;
        private List<Button> SubItemButtons;

        /// <summary> Offset used for adjusting the tab index of buttons after the property buttons, as well as get the item index from the button's tab index. </summary>
        private int TabIndexBase;
        
        private Button HeaderSelection
        {
            get => _headerSelection;

            set {
                if (value != null)
                {
                    DisplayHeaderItemDetails((int)value.Tag);
                }

                _headerSelection = value;
            }
        }
        private Button _headerSelection;

        private Button SubItemSelection
        {
            get => _subItemSelection;

            set {
                if (value != null)
                {
                    DisplayHeaderItemDetails((int)value.Tag);
                }

                _subItemSelection = value;
            }
        }
        private Button _subItemSelection;


        public delegate void PropertiesPanelWand(string dcFileName, DCFileHeader dcEntries);

        public PropertiesPanelWand propertiesPanelMammet;
        #endregion



        
        //=================================\\
        //--|   Function Delcarations   |--\\
        //=================================\\
        #region [Function Delcarations]


        /// <summary>
        /// Populate the output window with details about the highlighted header item
        /// </summary>
        /// <param name="itemIndex"> The index of the item in the HeaderItems array or whatever the fuck I named it, fight me. </param>
        private void DisplayHeaderItemDetails(int itemIndex)
        {
            PropertiesWindow.Clear();
            var item = DCScript.Items[itemIndex];
            var itemType = item.Type;

            UpdateSelectionLabel(new[] { null, item.Name.DecodedID == "UNKNOWN_SID_64" ? item.Name.EncodedID : item.Name.DecodedID });

            // Update Properties Window
            PrintPropertyDetailNL(itemType.DecodedID);
            PrintPropertyDetailNL($"Address: {item.StructAddress:X}\n");

            var children = item.GetType().GetFields();
            foreach (var f in children)
            {
                PrintPropertyDetailNL($"# {f.Name} | {f}");
            }
            return;

            switch (itemType.RawID)
            {
                case KnownSIDs.map:
                    switch (item.Name.RawID) //!
                    {
                        case KnownSIDs.weapon_gameplay_defs:
                            break;

                            
                        default:
                            echo($"unhandled map \"{item.Name}\".");
                            break;
                    }
                    break;

                case KnownSIDs.array:
                    break;

                default:
                    echo($"unhandled struct type \"{itemType}\".");
                    break;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void LoadHeaderItemContents(int headerItemIndex)
        {
            
        }

        

        private void HighlightHeaderButton(Button sender)
        {
            if (HeaderSelection != null)
            {
                HeaderSelection.Font = new Font(HeaderSelection.Font.FontFamily, HeaderSelection.Font.Size, (FontStyle) 0);
            }
                    
            (HeaderSelection = sender)
            .Font = new Font(HeaderSelection.Font.FontFamily, HeaderSelection.Font.Size, FontStyle.Underline);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcFileName"></param>
        /// <param name="dcEntries"></param>
        private void PopulatePropertiesPanel(string dcFileName, DCFileHeader dcScript)
        {
            Button newButton()
            {
                var btn = new Button()
                {
                    Font = MainFont,

                    BackColor = Color.FromArgb(255, 20, 20, 20),
                    ForeColor = Color.White,

                    FlatStyle = 0
                };

                                
                btn.MouseDown += new MouseEventHandler((sender, e) =>
                {
                    MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                    MouseIsDown = true;
                });

                btn.MouseUp   += new MouseEventHandler((sender, e) =>
                {
                    MouseIsDown = false;
                    if (OptionsPageIsOpen) {
                        Azem.BringToFront();
                    }
                
                });
                
                btn.MouseMove += new MouseEventHandler((sender, e) => MoveForm());


                return btn;
            }


            Button currentButton;
            var dcEntries = dcScript.Items;
            var dcLen = dcEntries.Length;

            HeaderItemButtons = new Button[dcEntries.Length];
            TabIndexBase = optionsMenuDropdownBtn.TabIndex - 1;


            for (var i = 0; i < dcLen; ++i)
            {
                string label;
                var dcEntry = dcEntries[i];
                PropertiesPanel.Controls.Add(currentButton = newButton());
                currentButton.Location = new Point(1, 7 + currentButton.Height * i);


                // Apply header item name as button text
                label = dcEntry.Name.DecodedID;
                if (label == "UNKNOWN_SID_64"
            #if DEBUG
                || label == "INVALID_SID_64"
            #endif
                    )
                {
                    label = dcEntry.Name.EncodedID;
                }
                currentButton.Text = label;


                // Apply header item type id as button name
                label = dcEntry.Type.DecodedID;
                if (label == "UNKNOWN_SID_64"
            #if DEBUG
                || label == "INVALID_SID_64"
            #endif
                    )
                {
                    label = dcEntry.Type.EncodedID;
                }
                currentButton.Name = label;


                // Style the control
                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;


                // Save the index of the header item tied to the control via the button's Tag property
                currentButton.Tag = i;


                // Apply event handlers to the control
                currentButton.GotFocus += (button, _) => HighlightHeaderButton(button as Button);
                currentButton.Click += (button, _) => HighlightHeaderButton(button as Button);

                currentButton.KeyDown += (sender, arg) =>
                {
                    if (arg.KeyData == Keys.Down)
                    {
                        if ((int)currentButton.Tag == HeaderItemButtons.Length - 1)
                        {
                            HeaderItemButtons[0].Focus();
                        }
                        else {
                            HeaderItemButtons[(int)currentButton.Tag + 1].Focus();
                        }
                    }
                    else if (arg.KeyData == Keys.Up)
                    {
                        if ((int)currentButton.Tag == 0)
                        {
                            HeaderItemButtons[HeaderItemButtons.Length - 1].Focus();
                        }
                        else {
                            HeaderItemButtons[(int)currentButton.Tag - 1].Focus();
                        }
                    }
                };

                HeaderItemButtons[i] = currentButton;
            }

            // Adjust the tab indexes of buttons intended to be after the property buttons
            optionsMenuDropdownBtn.TabIndex += dcLen;
            MinimizeBtn.TabIndex += dcLen;
            ExitBtn.TabIndex += dcLen;
        }
        #endregion
    }
}
