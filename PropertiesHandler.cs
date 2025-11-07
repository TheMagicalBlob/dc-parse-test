using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;

namespace NaughtyDogDCReader
{
    public class PropertiesHandler
    {

        //================================\\
        //--|   Class Initialization   |--\\
        //================================\\
        /// <summary>
        /// Initialize a new instance of the PropertiesHandler class.<br/><br/>
        /// 
        /// Used for management of the PropertiesPanel and PropertiesWindow (struct buttons & details display).
        /// </summary>
        public PropertiesHandler()
        {
            GroupBoxContentsOffset = 7;
            DefaultPropertiesPanelButtonHeight = 23;
            DefaultPropertiesEditorRowHeight = 23;

            Changes = new List<object[]>();


            populatePropertiesPanel = PopulatePropertiesPanelWithStructItems;

            // Newline
            propertiesWindowNewLineMammet = (message) =>
            {
                PropertiesWindow.AppendLine(message, false);
                PropertiesWindow.Update();
            };

            // Specific-line
            propertiesWindowSpecificLineMammet = (message, line) =>
            {
                PropertiesWindow.UpdateLine(message.Replace("\n", "\n" + Indentation), line);
                PropertiesWindow.Update();
            };

            // Default W/ Indent
            propertiesWindowMammet = (message) =>
            {
                if (message[0] != '\n')
                {
                    message = Indentation + message;
                }
             
                PropertiesWindow.AppendLine(message.Replace("\n", "\n" + Indentation));
                PropertiesWindow.Update();
            };

            spawnVariableEditorBox = SpawnVariableEditorBox;
            displayStructContents = DisplayStructContents;
        }




        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]


        //#
        //## Properties Panels Functionality Variables
        //#

        /// <summary>
        /// 
        /// </summary>
        private Button[] HeaderItemButtons;

        /// <summary>
        /// 
        /// </summary>
        private List<Button> SubItemButtons;


        /// <summary>
        /// The selected/highlighted button out of the loaded header item buttons
        /// </summary>
        private Button HeaderSelection
        {
            get => _headerSelection;

            set
            {
                if (value != null)
                {
                    PopulatePropertiesEditorWithStructItems(DCScript.Entries[(int) value.Tag].Struct);
                    PrintHeaderItemDetailDisplay((int) value.Tag);
                }

                _headerSelection = value;
            }
        }
        private Button _headerSelection;


        /// <summary>
        /// The selected/highlighted button out of the loaded 
        /// </summary>
        private Button SubItemSelection
        {
            get => _subItemSelection;

            set
            {
                if (value != null)
                {
                    PopulatePropertiesEditorWithStructItems(DCScript.Entries[(int) value.Tag].Struct);
                    PrintHeaderItemDetailDisplay((int) value.Tag);
                }

                _subItemSelection = value;
            }
        }
        private Button _subItemSelection;




        /// <summary>
        /// The (vertical) scroll bar used to navigate the buttons populating the PropertiesPanel when they bleed passed the bottom of the group box
        /// </summary>
        private ScrollBar PropertiesPanelScrollBar;

        /// <summary>
        /// The (vertical) scroll bar used to navigate the rows populating the PropertiesEditor when they bleed passed the bottom of the group box
        /// </summary>
        private ScrollBar PropertiesEditorScrollBar;




        private int IndentationDepth
        {
            get => Indentation.Length < 4 ? 0 : Indentation.Length / 4;
            
            set => Indentation = new string(' ', value > 0 ? value * 4 : 0);
        }

        private string Indentation = emptyStr;




        /// <summary>
        /// Made it a variable in case it's needed for scaling. May try and implement that at some point, since I'm designing these on a fairly low-res screen.
        /// </summary>
        private readonly int DefaultPropertiesPanelButtonHeight;

        

        /// <summary>
        /// Made it a variable in case it's needed for scaling. May try and implement that at some point, since I'm designing these on a fairly low-res screen.
        /// </summary>
        private readonly int DefaultPropertiesEditorRowHeight;


        /// <summary>
        /// The offset of the actual contents of the group box from the top of the control. (why the hell does it need that thing?)
        /// </summary>
        private readonly int GroupBoxContentsOffset;




        //#
        //## Threading-Related Variables (threads, delegates, and mammets)
        //#

        /// <summary> //! </summary>
        public delegate void PropertiesWindowOutputWand(string msg);
        public delegate void PropertiesWindowOutputWandSpecificLine(string msg, int line);

        /// <summary> //! </summary>
        public delegate void PropertiesPanelPopulation(string dcFileName, DCFileHeader dcEntries);
        public delegate void PropertiesPanelInteractionWand(object property);


        private readonly PropertiesWindowOutputWand propertiesWindowMammet;
        private readonly PropertiesWindowOutputWand propertiesWindowNewLineMammet;
        private readonly PropertiesWindowOutputWandSpecificLine propertiesWindowSpecificLineMammet;


        public readonly PropertiesPanelPopulation populatePropertiesPanel;

        public PropertiesPanelInteractionWand spawnVariableEditorBox;
        public PropertiesPanelInteractionWand displayStructContents;
        #endregion




        //=================================\\
        //--|   Function Delcarations   |--\\
        //=================================\\
        #region [Function Delcarations]

        //#
        //## Event Handler Declarations
        //#
        #region [event handlers]
        private void LoadArrayContentsIntoPropertiesWindow(object sender, PreviewKeyDownEventArgs args)
        {
            if (args.KeyCode == Keys.Return)
            {
                LoadArrayContentsForDisplay((int) ((Control) sender).Tag);

                // Unsubscribe from the event once the struct's been loaded
                ((Button) sender).DoubleClick -= LoadHeaderItemContentsOnEnterIfUnloaded;
            }
        }

        private void LoadHeaderItemContentsOnEnterIfUnloaded(object sender, EventArgs args)
        {
            LoadArrayContentsForDisplay((int) ((Control) sender).Tag);

            // Unsubscribe from the event
            ((Button) sender).DoubleClick -= LoadHeaderItemContentsOnEnterIfUnloaded;
        }
        #endregion [event handlers]








        //#
        //## Properties Window-related funtion declarations
        //#

        /// <summary>
        /// Overrite a specific line in the properties output window with the provided <paramref name="message"/>
        /// <br/> Appends an empty new line if no message is provided.
        /// </summary>
        public void PrintPropertyDetailSpL(object message = null, int line = 0)
        {
            if (message == null)
            {
                message = emptyStr;
            }

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try
            {
                Venat?.Invoke(propertiesWindowSpecificLineMammet, new object[] { message?.ToString() ?? "null", line < 0 ? 0 : line });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailSL Invocation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }



        /// <summary>
        /// Replace a specified line in the properties output window with <paramref name="message"/>.
        /// <br/> Clears the line if no message is provided.
        /// </summary>
        public void PrintPropertyDetailNL(object message = null)
        {
            if (message == null)
            {
                message = emptyStr;
            }

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(propertiesWindowNewLineMammet, new object[] { message?.ToString() ?? "null", null });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailNL Invocation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public void PrintPropertyDetail(object message = null, int indentationOverride = 0)
        {
            if (message == null)
            {
                message = emptyStr;
            }

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(propertiesWindowMammet, new object[] { message?.ToString() ?? "null", null });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailNL Invocation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }





        /// <summary>
        /// Prepend a space to any capitalized letter that follows a lowercase one.
        /// </summary>
        /// <returns> The provided <paramref name="name">, now spaced out rather than camel/pascal-case. </returns>
        private string SpaceOutStructName(string name)
        {
            var str = string.Empty;

            for (var i = 0; i < name.Length; i++)
            {
                if (name[i] <= 122u && name[i] >= 97u)
                {
                    if (i + 1 != name.Length)
                    {
                        if (name[i + 1] >= 65u && name[i + 1] <= 90u)
                        {
                            str += $"{name[i]} ";
                            continue;
                        }
                    }
                }

                str += name[i];
            }

            return str;
        }




        /// <summary>
        /// Get a formatted string representation for the property's value.
        /// <br/><br/>
        /// 
        /// Whole Numerical Values: <br/>
        /// - formats as a hex number if <paramref name="PropertyName"/> indicates the property's a pointer <br/>
        /// - otherwise formats as a basic integer value <br/>
        /// <br/><br/>
        /// 
        /// Advanced Numerical Values: <br/>
        /// - formats as a float. Meh, it works.
        /// <br/><br/>
        /// 
        /// Strings: <br/>
        /// - Either returns the string, or (empty string) / (null string) if applicable.
        /// <br/><br/>
        /// 
        /// Advanced Numerical Values: <br/>
        /// - formats as a float. Meh, it works.
        /// <br/><br/>
        /// 
        /// <br/>
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="IndentationOverride"></param>
        /// <returns></returns>
        private string FormatPropertyValueAsString(string PropertyName, object Value, int? IndentationOverride = null)
        {
            if (Value == null)
            {
                return "null";
            }
            string returnString;



            switch (Value.GetType())
            {
                // ## Booleans
                case var val when val == typeof(bool):
                    returnString = val.ToString();
                    break;


                // ## Basic Numerical Values
                case var val when WholeNumericalTypes.Contains(val):
                    returnString = $"0x{Value:X}";
                    break;

                // ## "Advanced" Numerical Values
                case var val when AdvancedNumericalTypes.Contains(val):
                    returnString = $"{Value:F}";
                    break;


                // ## String ID's
                case var type when type == typeof(SID):
                    returnString = ((SID) Value).DecodedID;
                    break;


                // ## Strings
                case var type when type == typeof(string):
                    if (Value == null)
                    {
                        returnString = "(null string)";
                        break;
                    }

                    if (Value.ToString().Length < 1)
                    {
                        returnString = "(empty string)";
                        break;
                    }

                    returnString = Value.ToString();
                    break;


                // ## Arrays
                case var type when type.ToString().Contains("[]"):
                {
                    returnString = $"{type.ToString().Replace("System.", emptyStr)}: {{\n";

                    foreach (var item in (Array) Value)
                    {
                        returnString += $"{FormatPropertyValueAsString("unnamed", item, 1)},\n";
                    }

                    returnString += '}';
                    break;
                }



                // ## Unknown Struct
                case var type when type == typeof(UnmappedStructure):
                    if (PropertiesEditor.Visible)
                    {
                        returnString = $"{((UnmappedStructure) Value).LongMessage}";
                    }
                    else {
                        returnString = $"{((UnmappedStructure) Value).ShortMessage}";
                    }
                    break;


                // Hopefully structs
                default:
                    if (PropertiesEditor.Visible)
                    {
                        returnString = Value.GetType().Name;
                    }
                    else {
                        returnString = PrintStructPropertyDetails(Value);
                    }
                    break;
            }


            if (PropertiesWindow.Visible)
            {
                var indent = IndentationOverride != null ? new string(' ', (int) IndentationOverride * 8) : Indentation;

                returnString = indent + returnString.Replace("\n", indent);
            }

            return returnString;
        }




        /// <summary>
        /// Populate the property window with details about the highlighted header item
        /// </summary>
        /// <param name="itemIndex"> The index of the item in the HeaderItems array or whatever the fuck I named it, fight me. </param>
        private void PrintHeaderItemDetailDisplay(int itemIndex)
        {
            //#
            //## Grab basic data about the current item and clear the current properties window contents 
            //#
            var dcEntry = DCScript.Entries[itemIndex];
            var structType = dcEntry.Type;

            PropertiesWindow.Clear();
            UpdateSelectionLabel(new[] { null, dcEntry.Name.DecodedID });




            // Update Properties Window
            PrintPropertyDetailNL("\nType: " + structType.DecodedID);

            if (dcEntry.Struct == null)
            {
                PrintPropertyDetailNL("Null structure, for some reason.");
                return;
            }

            IndentationDepth = 1;
            foreach (var property in dcEntry.Struct.GetType().GetProperties())
            {
                // Print the name of the property
                PrintPropertyDetailNL($"{SpaceOutStructName(property.Name)}: [");

                // Get and format the property value
                var val = property.GetValue(dcEntry.Struct);
                var formattedVal = FormatPropertyValueAsString(property.Name, val);

                // Print the formatted property value
                PrintPropertyDetail(val);

                PrintPropertyDetailNL("]");
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcEntry"></param>
        /// <returns></returns>
        private string PrintStructPropertyDetails(object dcEntry)
        {
            //## clear the current properties window contents 
            PropertiesWindow.Clear();

            // Update Properties Window
            var ret = "\nType: " + ((dynamic)dcEntry).Name.DecodedID + '\n';

            foreach (var property in dcEntry.GetType().GetProperties())
            {
                // Print the name of the property
                ret += $"{SpaceOutStructName(property.Name)}: ";

                var val = property.GetValue(dcEntry);
                ret += $"{FormatPropertyValueAsString(property.Name, val).Replace("\n", "\n" + Indentation)}\n";
            }

            return ret;
        }











        //#
        //## PropertiesPanel-related funtion declarations
        //#


        /// <summary>
        /// Highlight the selected/active property button, after removing said highlight from the previous selection's button
        /// </summary>
        private void HighlightHeaderButton(Button sender)
        {
            if (HeaderSelection != null)
            {
                // "Reset" the previous button
                HeaderSelection.Font = new Font(HeaderSelection.Font.FontFamily, HeaderSelection.Font.Size, HeaderSelection.Font.Style ^ FontStyle.Underline);

                if (PropertiesPanelScrollBar != null)
                {
                    // Move the scroll bar if we're wrapping around from one end to the other
                    if (HeaderSelection == HeaderItemButtons.Last() && sender == HeaderItemButtons.First())
                    {
                        ForceScrollPropertyPanelButtons(PropertiesPanelScrollBar.Minimum);
                    }
                    else if (sender == HeaderItemButtons.Last() && HeaderSelection == HeaderItemButtons.First())
                    {
                        ForceScrollPropertyPanelButtons(PropertiesPanelScrollBar.Maximum);
                    }
                }
            }



            (HeaderSelection = sender)
            .Font = new Font(HeaderSelection.Font.FontFamily, HeaderSelection.Font.Size, HeaderSelection.Font.Style | FontStyle.Underline);
        }



        /// <summary>
        /// //! WRITE ME
        /// </summary>
        private void LoadArrayContentsForDisplay(int headerItemIndex)
        {
            if (DCScript.Entries[headerItemIndex].Struct == null)
            {
                UpdateStatusLabel(new[] { null, "Loading Array Contents..." });

                // Update the properties window's displayed contents with the newly loaded struct properties
                PrintHeaderItemDetailDisplay(headerItemIndex);
                PopulatePropertiesEditorWithStructItems(DCScript.Entries[headerItemIndex].Struct);
            }
        }

        

        private Button NewPropertiesPanelButton()
        {
            var btn = new Button()
            {
                Font = MainFont,
                BackColor = AppColour,
                ForeColor = Color.White,

                FlatStyle = 0,
                Height = DefaultPropertiesPanelButtonHeight
            };

            // Assign basic form functionality event handlers
            btn.MouseDown += MouseDownFunc;
            btn.MouseUp += MouseUpFunc;
            btn.MouseMove += new MouseEventHandler((sender, e) => MoveForm());

            btn.DoubleClick += new EventHandler((sender, e) => { }); //!


            return btn;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcFileName"></param>
        /// <param name="dcScript"></param>
        private void PopulatePropertiesPanelWithStructItems(string dcFileName, DCFileHeader dcScript)
        {
            Button currentButton;

            var dcEntries = dcScript.Entries;
            var dcLen = dcEntries.Length;

            HeaderItemButtons = new Button[dcEntries.Length];

            
            //-## Check whether or not the ScrollBar needs to be added 
            var cumulativeButtonHeight = DefaultPropertiesPanelButtonHeight * dcEntries.Length;

            if (cumulativeButtonHeight >= PropertiesPanel.Height - GroupBoxContentsOffset) // minus 7 to half-assedly account for the stupid top border of the group box.
            {
                if (PropertiesPanelScrollBar == null)
                {
                    PropertiesPanelScrollBar = new VScrollBar()
                    {
                        Name = "PropertiesPanelScrollBar",
                        Height = PropertiesPanel.Height - GroupBoxContentsOffset,
                        Width = 20, // Default width's a bit fat
                        Maximum = cumulativeButtonHeight - (PropertiesPanel.Height - DefaultPropertiesPanelButtonHeight) - GroupBoxContentsOffset,
                        SmallChange = DefaultPropertiesPanelButtonHeight
                        //LargeChange = DefaultPropertyButtonHeight * 4, // Not sure which context the LargeChange is even used in, honestly
                    };

                    PropertiesPanelScrollBar.Location = new Point((PropertiesPanel.Location.X + PropertiesPanel.Width) - (PropertiesPanelScrollBar.Width + 1), PropertiesPanel.Location.Y + GroupBoxContentsOffset);
                    PropertiesPanelScrollBar.Scroll += ScrollPropertiesPanelButtons;

                    Venat.Controls.Add(PropertiesPanelScrollBar);

                    PropertiesPanelScrollBar.BringToFront();
                }
            }


            for (var i = 0; i < dcLen; ++i)
            {
                var dcEntry = dcEntries[i];
                currentButton = NewPropertiesPanelButton();

                PropertiesPanel.Controls.Add(currentButton);
                currentButton.Location = new Point(1, (currentButton.Height * i) + GroupBoxContentsOffset);

                // Apply item name as button text
                currentButton.Text = dcEntry.Name.DecodedID;

                // Apply item type id as button name
                currentButton.Name = dcEntry.Type.DecodedID;


                // Style the control
                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;
                if (PropertiesPanelScrollBar != null)
                {
                    currentButton.Width -= PropertiesPanelScrollBar.Width;
                }

                // Save the index of the header item tied to the control via the button's Tag property
                currentButton.Tag = i;


                // Apply event handlers to the control
                //currentButton.Click += (button, _) => HighlightHeaderButton(button as Button);
                currentButton.GotFocus += (button, _) => HighlightHeaderButton(button as Button);

                currentButton.PreviewKeyDown += LoadArrayContentsIntoPropertiesWindow;
                currentButton.DoubleClick += LoadHeaderItemContentsOnEnterIfUnloaded;

                HeaderItemButtons[i] = currentButton;
            }

            // Adjust the tab indexes of buttons intended to be after the property buttons
            Venat.OptionsMenuDropdownBtn.TabIndex += dcLen;
            Venat.MinimizeBtn.TabIndex += dcLen;
            Venat.ExitBtn.TabIndex += dcLen;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcArrayName"></param>
        /// <param name="dcArrayObj"></param>
        private void PopulatePropertiesPanelWithArrayItems(string dcArrayName, object dcArrayObj)
        {
            MessageBox.Show("WRONG FUNCTION CALLED, DUMBASS");
        }


        public void ScrollPropertiesPanelButtons(object _, ScrollEventArgs offset)
        {
            foreach (Button button in PropertiesPanel.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            PropertiesPanel.Update();
        }
        
        public void ForceScrollPropertyPanelButtons(int NewValue)
        {
            int scrollEventType;

            if (NewValue < PropertiesPanelScrollBar.Value)
            {
                scrollEventType = 0;
            }
            else if (NewValue > PropertiesPanelScrollBar.Value)
            {
                scrollEventType = 1;
            }
            else
            {
                return;
            }


            ScrollPropertiesPanelButtons(null, new ScrollEventArgs((ScrollEventType) scrollEventType, PropertiesPanelScrollBar.Value, PropertiesPanelScrollBar.Value = NewValue));
            PropertiesPanel.Update();
        }








        
        //#
        //## PropertiesEditor-related function declarations
        //#

        private void PopulatePropertiesEditorWithStructItems(object DCStruct)
        {
            var totalHeight = GroupBoxContentsOffset + 1; // Start with 8 (assuming GroupBoxContentsOffset's still 7) to both account for the GroupBox control's stupid title section at the top, and give the controls a tiny bit of padding

            var type = DCStruct.GetType();
            object[][] properties;

            PropertiesEditor.Controls.Clear();


            if (type == typeof(map))
            {
                var dcSruct = (map) DCStruct;
                properties = new object[dcSruct.Length][];

                for (var i = 0; i < dcSruct.Length; i++)
                {
                    properties[i] = new object[] { dcSruct.StructNames[i].DecodedID, dcSruct.Structs[i], displayStructContents };
                }
            }
            else {
                properties = type.GetProperties().Select(property => new object [] { property.Name, property.GetValue(DCStruct), new[] { WholeNumericalTypes, AdvancedNumericalTypes }.Any(datatypes => datatypes.Contains(property.GetType())) ? spawnVariableEditorBox : displayStructContents}).ToArray();
            }

            


            //-## Check whether or not the ScrollBar needs to be added 
            var cumulativeButtonHeight = DefaultPropertiesEditorRowHeight * properties.Length;

            if (cumulativeButtonHeight >= PropertiesEditor.Height - GroupBoxContentsOffset) // minus ~7 to half-assedly account for the stupid top border of the group box.
            {
                if (PropertiesEditorScrollBar == null)
                {
                    PropertiesEditorScrollBar = new VScrollBar()
                    {
                        Name = "PropertiesEditorScrollBar",
                        Height = PropertiesEditor.Height - GroupBoxContentsOffset,
                        Width = 20, // Default width's a bit fat
                        Maximum = cumulativeButtonHeight - (PropertiesEditor.Height - DefaultPropertiesEditorRowHeight) - GroupBoxContentsOffset,
                        SmallChange = DefaultPropertiesEditorRowHeight
                        //LargeChange = DefaultPropertyEditorRowHeight * 4, // Not sure which context the LargeChange is even used in, honestly
                    };

                    PropertiesEditorScrollBar.Location = new Point((PropertiesEditor.Location.X + PropertiesEditor.Width) - PropertiesEditorScrollBar.Width, PropertiesEditor.Location.Y + GroupBoxContentsOffset);
                    PropertiesEditorScrollBar.Scroll += ScrollPropertyEditorRows;

                    Venat.Controls.Add(PropertiesEditorScrollBar);

                    PropertiesEditorScrollBar.BringToFront();
                }
            }


            foreach (var property in properties)
            {
                var newRow = NewPropertiesEditorRow(property[0], property[1], (PropertiesPanelInteractionWand) property[2]);
                    
                PropertiesEditor.Controls.Add(newRow[0]);
                newRow[0].Location = new Point(2, totalHeight);

                PropertiesEditor.Controls.Add(newRow[1]);
                newRow[1].Location = new Point(newRow[0].Width, totalHeight);


                totalHeight += newRow[0].Height;
            }
        }

        
        private void PopulatePropertiesEditorWithArrayItems(string ArrayName, object ArrayStruct)
        {
            UpdateSelectionLabel(new string[] { null, null, ArrayName });

            int dcLen = ((dynamic) ArrayStruct).Entries.Length;

            for (var i = 0; i < dcLen; ++i)
            {

            }
        }


        

        private Control[] NewPropertiesEditorRow(object propertyName, object propertyValue, PropertiesPanelInteractionWand propertyEvent)
        {
            var nameBtn = new Button()
            {
                Font = TextFont,
                BackColor = AppColourLight,
                ForeColor = Color.White,
                Padding = Padding.Empty,
                FlatStyle = FlatStyle.Flat,

                Height = DefaultPropertiesEditorRowHeight,
                Width = ((PropertiesEditor.Width - (PropertiesEditorScrollBar != null ? 20 : 0)) / 3) - 2,

                Text = propertyName as string
            };

            // Assign basic form functionality event handlers
            nameBtn.MouseDown += MouseDownFunc;
            nameBtn.MouseUp += MouseUpFunc;

                
            var valueBtn = new Button()
            {
                Font = TextFont,
                BackColor = AppColourLight,
                ForeColor = Color.White,
                Padding = Padding.Empty,
                FlatStyle = FlatStyle.Flat,

                Height = DefaultPropertiesEditorRowHeight,
                Width = (PropertiesEditor.Width - (PropertiesEditorScrollBar != null ? 20 : 0)) - nameBtn.Width - 2,

                Text = FormatPropertyValueAsString(propertyName as string, propertyValue, 0)
            };

            // Assign basic form functionality event handlers
            valueBtn.MouseDown += MouseDownFunc;
            valueBtn.MouseUp += MouseUpFunc;

            valueBtn.Click += (_, __) => propertyEvent(propertyValue);



            return new Control[] { nameBtn, valueBtn };
        }

        public void ScrollPropertyEditorRows(object _, ScrollEventArgs offset)
        {
            foreach (Control button in PropertiesEditor.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            PropertiesEditor.Update();
        }

        
        public void ForceScrollPropertyEditorRows(int Incrementation)
        {
            foreach (Control button in PropertiesEditor.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (PropertiesEditorScrollBar.Value - (PropertiesEditorScrollBar.Value += Incrementation)));
            }
            PropertiesEditor.Update();
        }






        //#
        //## Other Miscellaneous Function Declarations
        //#
        #region [Other Miscellaneous Function Declarations]
        
        private void SpawnVariableEditorBox(object property)
        {

        }

        private void DisplayStructContents(object property)
        {

        }



        /// <summary>
        /// Reset all instance members in the current PropertiesHandler (clear all added controls, reset static ones to default states, clear variables)
        /// </summary>
        public void Reset()
        {
            PropertiesWindow.Clear();
            PropertiesPanel.Controls.Clear();
            PropertiesEditor.Controls.Clear();

            HeaderItemButtons = null;
            SubItemButtons = null;
            HeaderSelection = null;
            SubItemSelection = null;

            Venat.Controls.Remove(PropertiesPanelScrollBar);
            PropertiesPanelScrollBar = null;

            Venat.Controls.Remove(PropertiesEditorScrollBar);
            PropertiesEditorScrollBar = null;


            IndentationDepth = 0;
        }
        #endregion
        #endregion (function declarations)
    }
}
