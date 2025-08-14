using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static weapon_data.Main;

namespace weapon_data
{
    public class StructBSIdkNameItLater
    {
        public StructBSIdkNameItLater()
        {
            
            
            propertiesPanelMammet = new PropertiesPanelWand(PopulatePropertiesPanel);
        }

        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        private Button[] HeaderItemButtons;
        private List<Button> SubItemButtons;

        /// <summary> Offset used for adjusting the tab index of buttons after the property buttons, as well as get the item index from the button's tab index. </summary>
        private int TabIndexBase;
        private int Indentation = 0;


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

        //#
        //## Event Handler Declarations
        //#
        #region [event handlers]
        private void LoadHeaderItemContentsOnEnterIfUnloaded(object sender, PreviewKeyDownEventArgs args)
        {
            if (args.KeyCode == Keys.Return)
            {
                LoadHeaderItemContents((int) ((Control)sender).Tag);

                // Unsubscribe from the event
                ((Button)sender).DoubleClick -= LoadHeaderItemContentsOnEnterIfUnloaded;
            }
        }
        
        private void LoadHeaderItemContentsOnEnterIfUnloaded(object sender, EventArgs args)
        {
            LoadHeaderItemContents((int) ((Control)sender).Tag);

            // Unsubscribe from the event
            ((Button)sender).DoubleClick -= LoadHeaderItemContentsOnEnterIfUnloaded;
        }
        #endregion [event handlers]






        //#
        //## Properties Window-related funtion declarations
        //#
        
        private void PrintToPW(string message = emptyStr, int? indentationOverride = null)
        {

        }

        public void Reset()
        {
            HeaderItemButtons = null;
            SubItemButtons = null;
            HeaderSelection = null;
        }


        /// <summary>
        /// Prepend a space to any capitalized letter that follows a lowercase one.
        /// </summary>
        /// <returns> The provided <paramref name="name">, now spaced out rather than camel/pascal-case. </returns>
        private string SpaceOutStructName(string name)
        {
            var str = string.Empty;

            for (var i = 0; i < name.Length;) {

                if (name[i] <= 122u && name[i] >= 97u) {

                    if (i + 1 != name.Length) {

                        if (name[i + 1] >= 65u && name[i + 1] <= 90u)
                        {
                            str += $"{name[i]} ";
                            continue;
                        }
                    }
                }

                str += name[i++];
            }

            return str;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="indentation"></param>
        /// <returns></returns>
        private string FormatPropertyValue(object value, int indentation = 0)
        {
            if (value == null)
            {
                return "null";
            }

                
            var indent = new string(' ', 8 * indentation);

            switch (value.GetType())
            {
                case var val when val == typeof(long) || val == typeof(ulong) || val == typeof(byte):
                    return $"{indent}0x{value:X}";


                // ## String ID's
                case var type when type == typeof(SID):
                    var id = ((SID)value).DecodedID;

                    if (id == "UNKNOWN_SID_64")
                    {
                        id = ((SID)value).EncodedID;
                    }
                    #if DEBUG
                    else if (id == "INVALID_SID_64")
                    {
                        id = ((SID)value).EncodedID;
                    }
                    #endif

                    return indent + id;



                // ## Arrays
                case var type when type.ToString().Contains("[]"):
                    var str = $"{type}: {{\n";
                    foreach (var item in (Array) value)
                    {
                        str += $"        {indent}{FormatPropertyValue(item, 1)},\n";
                    }
                    str += indent + '}';
                    return str;

                        

                // ## Unknown Struct
                case var type when type == typeof(UnknownStruct):
                    return $"{indent}{((UnknownStruct)value).Message.Replace("\n", "\n" + indent)}";

                default: return indent + value.ToString();
            }
        }


        /// <summary>
        /// Populate the property window with details about the highlighted header item
        /// </summary>
        /// <param name="itemIndex"> The index of the item in the HeaderItems array or whatever the fuck I named it, fight me. </param>
        private void DisplayHeaderItemDetails(int itemIndex)
        {
            //#
            //## Clear the current properties window contents and grab basic data about the current item
            //#
            PropertiesWindow.Clear();

            var dcEntry = DCScript.Entries[itemIndex];
            var structType = dcEntry.Type;


            UpdateSelectionLabel(new[] { null, dcEntry.Name.DecodedID == "UNKNOWN_SID_64" ? dcEntry.Name.EncodedID : dcEntry.Name.DecodedID });

            // Update Properties Window
            PrintPropertyDetailNL($"{structType.DecodedID}");

            if (dcEntry.Struct != null)
            {
                echo("Stuct Has been initialized...");
                echo($"    Iterating through \"{dcEntry.Name}\".");
                foreach (var property in dcEntry.Struct.GetType().GetProperties())
                {
                    echo("PROPERTY: " + property);
                    PrintPropertyDetailNL($"# {SpaceOutStructName(property.Name)}: {FormatPropertyValue(property.GetValue(dcEntry.Struct))}");
                }
            }
            else {
                PrintPropertyDetailNL("Press Enter or Double-Click Entry to Load Struct");
            }
            return;
        }






        //#
        //## Properties Panel-related funtion declarations
        //#

        private void HighlightHeaderButton(Button sender)
        {
            if (HeaderSelection != null) // "Reset" the previous button if applicable
            {
                HeaderSelection.Font = new Font(HeaderSelection.Font.FontFamily, HeaderSelection.Font.Size, HeaderSelection.Font.Style ^ FontStyle.Underline);
            }

            (HeaderSelection = sender)
            .Font = new Font(HeaderSelection.Font.FontFamily, HeaderSelection.Font.Size, HeaderSelection.Font.Style | FontStyle.Underline);
        }


        
        /// <summary>
        /// 
        /// </summary>
        private void LoadHeaderItemContents(int headerItemIndex)
        {
            if (DCScript.Entries[headerItemIndex].Struct == null)
            {
                UpdateStatusLabel(new[] { null, "Loading Struct Contents..." });
                DCScript.Entries[headerItemIndex].LoadItemStruct();

                UpdateStatusLabel(new[] { null, $"{DCScript.Entries[headerItemIndex].Name.DecodedID} Loaded" });

                // Update the properties window's displayed contents with the newly loaded struct properties
                DisplayHeaderItemDetails(headerItemIndex);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcFileName"></param>
        /// <param name="dcScript"></param>
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

                // Assign basic form functionality event handlers
                btn.MouseDown += MouseDownFunc;
                btn.MouseUp   += MouseUpFunc;
                btn.MouseMove += new MouseEventHandler((sender, e) => MoveForm());

                btn.DoubleClick += new EventHandler((sender, e) => { });


                return btn;
            }


            Button currentButton;
            var dcEntries = dcScript.Entries;
            var dcLen = dcEntries.Length;

            HeaderItemButtons = new Button[dcEntries.Length];
            TabIndexBase = Venat.optionsMenuDropdownBtn.TabIndex - 1;


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
                //currentButton.Click += (button, _) => HighlightHeaderButton(button as Button);
                currentButton.GotFocus += (button, _) => HighlightHeaderButton(button as Button);

                currentButton.PreviewKeyDown += LoadHeaderItemContentsOnEnterIfUnloaded;
                currentButton.DoubleClick += LoadHeaderItemContentsOnEnterIfUnloaded;
                
                HeaderItemButtons[i] = currentButton;
            }

            // Adjust the tab indexes of buttons intended to be after the property buttons
            Venat.optionsMenuDropdownBtn.TabIndex += dcLen;
            //Venat.MinimizeBtn.TabIndex += dcLen;
            //Venat.ExitBtn.TabIndex += dcLen;
        }
        #endregion
    }
}
