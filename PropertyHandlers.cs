using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;

namespace NaughtyDogDCReader
{
    public class PropertyHandlers
    {

        //================================\\
        //--|   Class Initialization   |--\\
        //================================\\
        /// <summary>
        /// Initialize a new instance of the PropertiesHandler class.<br/><br/>
        /// 
        /// Used for management of the PropertiesPanel and PropertiesWindow (struct buttons & details display).
        /// </summary>
        public PropertyHandlers()
        {
            //#
            //## Properties Handler Variable Declarations
            //#
            GroupBoxContentsOffset = 7;
            DefaultPropertiesPanelButtonHeight = 23;
            DefaultPropertiesEditorRowHeight = 23;

            Changes = new List<object[]>();
            History = new List<object[]>();




            //#
            //## Create the various delegates for the Properties Handler, so we can do shit across multiple threads
            //#

            populatePropertiesPanel = PopulatePropertiesPanelWithHeaderItems;


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
        /// //!
        /// </summary>
        private Button[] PropertyButtons;


        /// <summary>
        /// The selected/highlighted button out of the loaded header item buttons
        /// </summary>
        private Button PropertySelection
        {
            get => _headerSelection;

            set {
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
        /// The (vertical) scroll bar used to navigate the buttons populating the PropertiesPanel when they bleed passed the bottom of the group box
        /// </summary>
        private VScrollBar PropertiesPanelScrollBar;

        /// <summary>
        /// The (vertical) scroll bar used to navigate the rows populating the PropertiesEditor when they bleed passed the bottom of the group box
        /// </summary>
        private VScrollBar PropertiesEditorScrollBar;



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
        //--|   Function Declarations   |--\\
        //=================================\\
        #region [Function Declarations]

        //#
        //## Event Handler Declarations
        //#
        #region [Event Handler Declarations]
        #endregion [event handlers]







        //#
        //## Properties Window-related function declarations
        //#

        /// <summary>
        /// Overwrite a specific line in the properties output window with the provided <paramref name="message"/>
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
                var err = $"Missed {nameof(PrintPropertyDetailSpL)} Invocation due to a(n) {dang.GetType()}.";
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
                Venat?.Invoke(propertiesWindowNewLineMammet, new object[] { message?.ToString() ?? "null" });
            }
            catch (Exception dang)
            {
                var err = $"Missed {nameof(PrintPropertyDetailNL)} Invocation due to a(n) {dang.GetType()}.";
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
                Venat?.Invoke(propertiesWindowMammet, new object[] { message?.ToString() ?? "null" });
            }
            catch (Exception dang)
            {
                var err = $"Missed {nameof(PrintPropertyDetail)} Invocation due to a(n) {dang.GetType()}.";
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
                var rawValue = property.GetValue(dcEntry.Struct);
                var formattedValue = FormatPropertyValueAsString(property.Name, rawValue);

                // Print the formatted property value
                PrintPropertyDetail(Indentation + formattedValue.Replace("\n", "\n" + Indentation));

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
        //## PropertiesPanel-related function declarations
        //#


        /// <summary>
        /// Highlight the selected/active property button, after removing said highlight from the previous selection's button
        /// </summary>
        private void HighlightPropertyButton(Button newButton)
        {
            if (PropertySelection != null)
            {
                // "Reset" the previous button
                PropertySelection.Font = new Font(PropertySelection.Font.FontFamily, PropertySelection.Font.Size, PropertySelection.Font.Style ^ FontStyle.Underline);

                // Move the scroll bar if we're wrapping around from one end to the other
                if (PropertiesPanelScrollBar != null)
                {
                    if (PropertySelection == PropertyButtons.Last() && newButton == PropertyButtons.First())
                    {
                        ForceScrollPropertyPanelButtons(PropertiesPanelScrollBar.Minimum);
                    }
                    else if (newButton == PropertyButtons.Last() && PropertySelection == PropertyButtons.First())
                    {
                        ForceScrollPropertyPanelButtons(PropertiesPanelScrollBar.Maximum);
                    }
                }
            }



            (PropertySelection = newButton)
            .Font = new Font(PropertySelection.Font.FontFamily, PropertySelection.Font.Size, PropertySelection.Font.Style | FontStyle.Underline);
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
        private void PopulatePropertiesPanelWithHeaderItems(string dcFileName, DCFileHeader dcScript)
        {
            Button currentButton;

            var dcEntries = dcScript.Entries;
            var dcLen = dcEntries.Length;

            PropertyButtons = new Button[dcEntries.Length];
            PropertiesPanel.Controls.Clear();

            
            //-## Create and add the scroll bar if the controls are going to overflow the group box's height
            CreateScrollBarForGroupBox(PropertiesPanel, ref PropertiesPanelScrollBar, cumulativeButtonHeight: DefaultPropertiesPanelButtonHeight * dcLen);


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

                if (Venat.Controls.Contains(PropertiesPanelScrollBar))
                {
                    // Account for the scroll bar's width by shrinking the buttons a bit if it's been added to the form
                    currentButton.Width -= PropertiesPanelScrollBar.Width;
                }


                // Save the index of the header item tied to the control via the button's Tag property
                currentButton.Tag = i;




                // Apply highlight event handler to buttons
                currentButton.GotFocus += (button, _) => HighlightPropertyButton(button as Button);


                currentButton.PreviewKeyDown += (_, keyEvent) =>
                {
                    if (keyEvent.KeyCode == Keys.Return)
                    {
                        History.Add(new object[] { dcFileName, dcScript });

                        PopulatePropertiesPanelWithStructContents(dcEntry.Name.DecodedID, dcEntry.Struct);
                    }
                };

                PropertyButtons[i] = currentButton;
            }

            PropertySelection = PropertyButtons.First(); 
        }



        


        private void PopulatePropertiesPanelWithStructContents(string structureName, object structure)
        {
            Button currentButton;

            var structType = structure.GetType();
            var structProperties = structType.GetProperties();
            var propertyCount = structProperties.Length;

            PropertyButtons = new Button[propertyCount];
            
            PropertiesPanel.Controls.Clear();
            

            //-## Create and add the scroll bar if the controls are going to overflow the group box's height
            CreateScrollBarForGroupBox(PropertiesPanel, ref PropertiesPanelScrollBar, cumulativeButtonHeight: DefaultPropertiesPanelButtonHeight * propertyCount);


            // Create and add a button for each property of the provided structure
            for (var i = 0; i < propertyCount; ++i)
            {
                var property = structProperties[i];
                currentButton = NewPropertiesPanelButton();


                PropertiesPanel.Controls.Add(currentButton);
                currentButton.Location = new Point(1, (currentButton.Height * i) + GroupBoxContentsOffset);

                
                // Apply item name as button text
                currentButton.Text = property.Name;

                // Apply item type id as button name
                currentButton.Name = property.GetType().Name;


                // Style the control
                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;

                if (Venat.Controls.Contains(PropertiesPanelScrollBar))
                {
                    // Account for the scroll bar's width by shrinking the buttons a bit if it's been added to the form
                    currentButton.Width -= PropertiesPanelScrollBar.Width;
                }


                // Save the index of the header item tied to the control via the button's Tag property
                currentButton.Tag = i;




                // Apply highlight event handler to buttons
                currentButton.GotFocus += (button, _) => HighlightPropertyButton(button as Button);

                currentButton.PreviewKeyDown += (_, keyEvent) =>
                {
                    if (keyEvent.KeyCode == Keys.Return && ObjectIsStruct(property.PropertyType))
                    {
                        History.Add(new object[] { structureName, structure });

                        PopulatePropertiesPanelWithStructContents(property.Name, FormatPropertyValueAsString(property.Name, property.GetValue(structure)));
                    }

                    // Go back to displaying the properties of the current structure's declaring struct
                    else if (keyEvent.KeyCode == Keys.Back)
                    {
                        if (History.Count == 1)
                        {
                            PopulatePropertiesPanelWithHeaderItems(History.Last()[0].ToString(), (DCFileHeader) History.Last()[1]);

                            History.Remove(History.Last());
                        }
                        else if (History.Count > 1)
                        {
                            PopulatePropertiesPanelWithStructContents(History.Last()[0].ToString(), History.Last()[1]);
                                
                            History.Remove(History.Last());
                        }
                    }
                };

                PropertyButtons[i] = currentButton;
            }

            if (PropertyButtons.Any())
            {
                PropertySelection = PropertyButtons.First();
            }
        }


        public void ScrollPropertiesPanelButtons(GroupBox hostBox, ScrollEventArgs offset)
        {
            foreach (Button button in hostBox.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            hostBox.Update();
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


            ScrollPropertiesPanelButtons(PropertiesPanel, new ScrollEventArgs((ScrollEventType) scrollEventType, PropertiesPanelScrollBar.Value, PropertiesPanelScrollBar.Value = NewValue));
            PropertiesPanel.Update();
        }


        private void CreateScrollBarForGroupBox(GroupBox groupBox, ref VScrollBar hostBoxScrollBarReference, int cumulativeButtonHeight)
        {
            if (cumulativeButtonHeight >= groupBox.Height - GroupBoxContentsOffset) // minus 7 to half-assedly account for the stupid top border of the group box.
            {
                if (!Venat.Controls.Contains(hostBoxScrollBarReference))
                {
                    if (hostBoxScrollBarReference == null)
                    {
                        hostBoxScrollBarReference = new VScrollBar()
                        {
                            Name = "PropertiesPanelScrollBar",
                            Height = groupBox.Height - GroupBoxContentsOffset,
                            Width = 20, // Default width's a bit fat
                            //LargeChange = DefaultPropertyButtonHeight * 4, // Not sure which context the LargeChange is even used in, honestly
                        };
                        

                        hostBoxScrollBarReference.Location = new Point((groupBox.Location.X + groupBox.Width) - (hostBoxScrollBarReference.Width + 1), groupBox.Location.Y + GroupBoxContentsOffset);

                        hostBoxScrollBarReference.Scroll += (_, args) => ScrollPropertiesPanelButtons(groupBox, args);
                    }

                    Venat.Controls.Add(hostBoxScrollBarReference);
                }


                hostBoxScrollBarReference.BringToFront();
                
                hostBoxScrollBarReference.Maximum = cumulativeButtonHeight - (groupBox.Height - DefaultPropertiesPanelButtonHeight) - GroupBoxContentsOffset;
                hostBoxScrollBarReference.SmallChange = DefaultPropertiesPanelButtonHeight;
            }
            else {
                Venat.Controls.Remove(hostBoxScrollBarReference);
                hostBoxScrollBarReference = null;
            }
        }







        
        //#
        //## PropertiesEditor-related function declarations
        //#

        private void PopulatePropertiesEditorWithStructItems(object Struct)
        {
            var totalHeight = GroupBoxContentsOffset + 1; // Start with 8 (assuming GroupBoxContentsOffset's still 7) to both account for the GroupBox control's stupid title section at the top, and give the controls a tiny bit of padding

            var type = Struct.GetType();
            object[][] properties;

            PropertiesEditor.Controls.Clear();

            if (ObjectIsStruct(type))
            {
                if (type == typeof(map))
                {
                    var dcStruct = (map) Struct;
                    properties = new object[dcStruct.Length][];

                    for (var i = 0; i < properties.Length; i++)
                    {
                        properties[i] = new object[] { dcStruct.StructNames[i].DecodedID, dcStruct.Structs[i], displayStructContents };
                    }
                }
                if (type == typeof(symbol_array))
                {
                    var dcStruct = (symbol_array) Struct;
                    properties = new object[dcStruct.Symbols.Length][];

                    for (var i = 0; i < properties.Length; i++)
                    {
                        properties[i] = new object[] { null, dcStruct.Symbols[i].DecodedID, spawnVariableEditorBox };
                    }
                }
                else {
                    properties = type.GetProperties().Select(property => new object [] { property.Name, property.GetValue(Struct), ObjectIsStruct(property.GetType()) ? displayStructContents : spawnVariableEditorBox }).ToArray();
                }
            }
            else {
                properties = new object[][] { new object[] { type, Struct } };
            }


            

            CreateScrollBarForGroupBox(PropertiesEditor, ref PropertiesEditorScrollBar, DefaultPropertiesEditorRowHeight * properties.Length);



            foreach (var property in properties)
            {
                // Create the applicable buttons
                var newRow = NewPropertiesEditorRow(property[0], property[1], (PropertiesPanelInteractionWand) property[2]);
                    
                PropertiesEditor.Controls.Add(newRow);
                newRow.Location = new Point(2, totalHeight);

                totalHeight += newRow.Height;
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


        

        private Control NewPropertiesEditorRow(object propertyName, object propertyValue, PropertiesPanelInteractionWand propertyEvent)
        {
            Button newRow = null;

            newRow = new Button()
            {
                Font = TextFont,
                BackColor = AppColourLight,
                ForeColor = Color.White,
                Padding = Padding.Empty,
                FlatStyle = FlatStyle.Flat,

                Height = DefaultPropertiesEditorRowHeight,
                Width = (PropertiesEditor.Width - (PropertiesEditorScrollBar != null ? 20 : 0)) - 2,

                Text = $"{propertyValue.GetType().Name}::{propertyName ?? "unnamed"}: [{FormatPropertyValueAsString(propertyValue?.ToString() ?? "null", propertyValue, 0)}]"
            };

            // Assign basic form functionality event handlers
            newRow.MouseDown += MouseDownFunc;
            newRow.MouseUp += MouseUpFunc;
                
            newRow.Click += (_, __) => propertyEvent(propertyValue);



            return newRow;
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
            var returnString = string.Empty;



            switch (Value.GetType())
            {
                // ## Booleans
                case var val when val == typeof(bool):
                    returnString = val.ToString();
                    break;



                // ## Basic Numerical Values
                case var val when BasicNumericalTypes.Contains(val):
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
                    if (PropertiesEditor.Visible)
                    {

                    }
                    else
                    {
                        returnString = "{{\n";

                        foreach (var item in (Array) Value)
                        {
                            returnString += $"{FormatPropertyValueAsString("unnamed", item, IndentationDepth + 1)},\n";
                        }

                        returnString += '}';
                    }
                    break;
                }



                // ## Unknown Struct
                case var type when type == typeof(UnmappedStructure):
                    if (PropertiesEditor.Visible)
                    {
                        returnString = $"{((UnmappedStructure) Value).ShortMessage}";
                    }
                    else {
                        returnString = $"{((UnmappedStructure) Value).LongMessage}";
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

                returnString = indent + returnString.Replace("\n", "\n" + indent);
            }

            return returnString;
        }



        /// <summary>
        /// Reset all instance members in the current PropertiesHandler (clear all added controls, reset static ones to default states, clear variables)
        /// </summary>
        public void Reset()
        {
            PropertiesWindow.Clear();
            PropertiesPanel.Controls.Clear();
            PropertiesEditor.Controls.Clear();

            PropertyButtons = null;
            PropertySelection = null;

            Venat.Controls.Remove(PropertiesPanelScrollBar);
            PropertiesPanelScrollBar = null;

            Venat.Controls.Remove(PropertiesEditorScrollBar);
            PropertiesEditorScrollBar = null;
            

            if (false) //(DCScript.Entries != null)
            {
                var len = DCScript.Entries.Length;

                echo ($"Decrementing relavant button tab indexes by [{len}].");
                Venat.OptionsMenuDropdownBtn.TabIndex -= len;
                Venat.MinimizeBtn.TabIndex -= len;
                Venat.ExitBtn.TabIndex -= len;
            }

            IndentationDepth = 0;
        }
        #endregion
        #endregion (function declarations)
    }
}
