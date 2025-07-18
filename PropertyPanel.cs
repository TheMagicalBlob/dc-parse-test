﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
                    DisplayHeaderItemDetails(value.TabIndex);
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
                    DisplayHeaderItemDetails(value.TabIndex);
                }

                _subItemSelection = value;
            }
        }
        private Button _subItemSelection;


        public delegate void PropertiesPanelWand(string dcFileName, object[] dcEntries);

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
            var item = DCHeader.HeaderItems[itemIndex - TabIndexBase];
            var itemType = item.Type;

            UpdateSelectionLabel(new[] { null, item.Name });

            // Update Properties Window
            PrintPropertyDetailNL(itemType + (item.Name.Length > 0 ? $" {item.Name}" : string.Empty));
            PrintPropertyDetailNL($"Address: {item.StructAddress:X}\n");

            switch (item.Type)
            {
                case "map":
                    switch (item.Name) //!
                    {
                        case "*weapon-gameplay-defs*":
                            break;

                            
                        default:
                            echo($"unhandled map \"{item.Name}\".");
                    break;
                    }
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
            if (DCHeader.HeaderItems[headerItemIndex].Struct == null)
            {
                headerItemIndex -= TabIndexBase;

                DCHeader.HeaderItems[headerItemIndex].Struct = LoadDCStructByType(DCFile, DCHeader.HeaderItems[headerItemIndex].Type, (int)DCHeader.HeaderItems[headerItemIndex].StructAddress, DCHeader.HeaderItems[headerItemIndex].Name);
            }
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
        private void PopulatePropertiesPanel(string dcFileName, object[] dcEntries)
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
            HeaderItemButtons = new Button[dcEntries.Length];

            var dcLen = dcEntries.Length;
            TabIndexBase = optionsMenuDropdownBtn.TabIndex - 1;
            char[] name = null;

            for (int i = 0; i < dcLen; i++, name = null)
            {
                var dcEntry = dcEntries[i];
                currentButton = newButton();
                PropertiesPanel.Controls.Add(currentButton);
                
                currentButton.Location = new Point(1, 7 + currentButton.Height * i);


                // Format struct name for use in button text
                name = (((dynamic)dcEntry).Name as string).ToArray();
                var tmp = 0;

                for(;tmp < name.Length && name[tmp] < 123 && name[tmp] > 96; ++tmp);

                name[tmp + 1] = $"{name[tmp + 1]}".ToUpper()[0];


                for (var j = tmp + 1; j < name.Length;)
                {
                    if (name[j] == '-')
                    {
                        while (name[++j] == '-' && j < name.Length);
                            
                        name[j - 1] = ' ';
                        name[j] = $"{name[j]}".ToUpper()[0];
                    }

                    j++;
                }

                // Apply formatted name as button text
                currentButton.Text = new string(name);

                currentButton.Name = ((dynamic)dcEntry).Name;

                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;

                currentButton.TabIndex = TabIndexBase + i;

                currentButton.GotFocus += (button, _) => HighlightHeaderButton(button as Button);
                currentButton.Click += (button, _) => HighlightHeaderButton(button as Button);

                currentButton.KeyDown += (sender, arg) =>
                {
                    if (arg.KeyData == Keys.Down)
                    {
                        if (currentButton.TabIndex == HeaderItemButtons.Length - 1)
                        {
                            HeaderItemButtons[0].Focus();
                        }
                        else {
                            HeaderItemButtons[currentButton.TabIndex + 1].Focus();
                        }
                    }
                    else if (arg.KeyData == Keys.Up)
                    {
                        if (currentButton.TabIndex == 0)
                        {
                            HeaderItemButtons[HeaderItemButtons.Length - 1].Focus();
                        }
                        else {
                            HeaderItemButtons[currentButton.TabIndex - 1].Focus();
                        }
                    }
                };

                HeaderItemButtons[i] = currentButton;
            }

            optionsMenuDropdownBtn.TabIndex += dcLen;
            MinimizeBtn.TabIndex += dcLen;
            ExitBtn.TabIndex += dcLen;
        }
        #endregion
    }
}
