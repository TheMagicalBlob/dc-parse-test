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
            DefaultPropertyButtonHeight = 23;

            Changes = new List<object[]>();


            propertiesPanelMammet = PopulatePropertiesPanelWithStructItems;

            // Newline
            propertiesWindowNewLineMammet = (message, _) =>
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
            propertiesWindowMammet = (message, _) =>
            {
                if (message[0] != '\n')
                {
                    message = Indentation + message;
                }
             
                PropertiesWindow.AppendLine(message.Replace("\n", "\n" + Indentation));
                PropertiesWindow.Update();
            };
        }






        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]


        //#
        //## Properties Panels Functionality Variables
        //#
        private Button[] HeaderItemButtons;

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
        private ScrollBar ScrollBar;



        private int IndentationDepth
        {
            get => Indentation.Length < 4 ? 0 : Indentation.Length / 4;
            
            set => Indentation = new string(' ', value > 0 ? value * 4 : 0);
        }


        private string Indentation = emptyStr;




        private readonly Type[] WholeNumericalTypes = new[]
        {
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(byte),
            typeof(sbyte),
        };


        private readonly Type[] AdvancedNumericalTypes = new[]
        {
            typeof(decimal),
            typeof(double),
            typeof(float),
        };



        /// <summary>
        /// Made it a variable in case it's needed for scaling. May try and implement that at some point, since I'm designing these on a fairly low-res screen.
        /// </summary>
        private readonly int DefaultPropertyButtonHeight;

        /// <summary>
        /// The offset of the actual contents of the group box from the top of the control. (why the hell does it need that thing?)
        /// </summary>
        private readonly int GroupBoxContentsOffset;




        //#
        //## Threading-Related Variables (threads, delegates, and mammets)
        //#

        /// <summary> //! </summary>
        public delegate void PropertiesWindowOutputWand(string msg, int line);

        /// <summary> //! </summary>
        public delegate void PropertiesPanelWand(string dcFileName, DCFileHeader dcEntries);


        private readonly PropertiesWindowOutputWand propertiesWindowNewLineMammet;
        private readonly PropertiesWindowOutputWand propertiesWindowSpecificLineMammet;
        private readonly PropertiesWindowOutputWand propertiesWindowMammet;

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
                var err = $"Missed PrintPropertyDetailSL Invokation due to a(n) {dang.GetType()}.";
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
                var err = $"Missed PrintPropertyDetailNL Invokation due to a(n) {dang.GetType()}.";
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
                var err = $"Missed PrintPropertyDetailNL Invokation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }



        public void Reset()
        {
            PropertiesPanel.Controls.Clear();
            PropertiesWindow.Clear();

            HeaderItemButtons = null;
            SubItemButtons = null;
            HeaderSelection = null;
            SubItemSelection = null;
            ScrollBar = null;
        }



        /// <summary>
        /// Prepend a space to any capitalized letter that follows a lowercase one.
        /// </summary>
        /// <returns> The provided <paramref name="name">, now spaced out rather than camel/pascal-case. </returns>
        private string SpaceOutStructName(string name)
        {
            var str = emptyStr;

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
        /// //!
        /// </summary>
        /// <param name="value"></param>
        /// <param name="indentationOverride"></param>
        /// <returns></returns>
        private string FormatPropertyValueAsString(object value, int? indentationOverride = null)
        {
            if (value == null)
            {
                return "null";
            }
            string
                indent,
                returnString
            ;


            indent = indentationOverride != null ? new string(' ', (indentationOverride * 8) ?? IndentationDepth) : Indentation;


            switch (value.GetType())
            {
                // ## Booleans
                case var val when val == typeof(bool):
                    returnString = val.ToString();
                    break;


                // ## Basic Numerical Values
                case var val when WholeNumericalTypes.Contains(val):
                    returnString = $"0x{value:X}";
                    break;

                // ## "Advanced" Numerical Values
                case var val when AdvancedNumericalTypes.Contains(val):
                    returnString = $"{value:F}";
                    break;


                // ## String ID's
                case var type when type == typeof(SID):
                    returnString = ((SID) value).DecodedID;
                    break;


                // ## Strings
                case var type when type == typeof(string):
                    if (value.ToString().Length < 1)
                    {
                        //! the fuck was I going to do here, again?
                    }
                    returnString = value.ToString();
                    break;


                // ## Arrays
                case var type when type.ToString().Contains("[]"):
                {
                    returnString = $"{type.ToString().Replace("System.", emptyStr)}: {{\n";

                    foreach (var item in (Array) value)
                    {
                        returnString += $"{FormatPropertyValueAsString(item, 1)},\n";
                    }

                    returnString += '}';
                    break;
                }



                // ## Unknown Struct
                case var type when type == typeof(UnmappedStructure) && !PropertiesEditor.Visible:
                    returnString = $"{((UnmappedStructure) value).Message}";
                    break;


                // Hopefully structs
                default:
                    if (PropertiesEditor.Visible)
                    {
                        returnString = value.GetType().Name;
                    }
                    else {
                        returnString = PrintStructPropertyDetails(value);
                    }
                    break;
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
                var formattedVal = FormatPropertyValueAsString(val);

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
                ret += $"{FormatPropertyValueAsString(val).Replace("\n", "\n" + Indentation)}\n";
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
            // "Reset" the previous button if applicable
            if (HeaderSelection != null)
            {
                HeaderSelection.Font = new Font(HeaderSelection.Font.FontFamily, HeaderSelection.Font.Size, HeaderSelection.Font.Style ^ FontStyle.Underline);
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
                Height = DefaultPropertyButtonHeight
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

            var cumulativeButtonHeight = DefaultPropertyButtonHeight * dcEntries.Length;
            if (cumulativeButtonHeight >= PropertiesPanel.Height - GroupBoxContentsOffset) // minus 7 to half-assedly account for the stupid top border of the group box.
            {
                ScrollBar = new VScrollBar()
                {
                    Name = "PropertiesPanelScrollBar",
                    Height = PropertiesPanel.Height,
                    Width = 20, // Default width's a bit fat
                    Maximum = cumulativeButtonHeight
                };

                ScrollBar.Location = new Point(PropertiesPanel.Width - (ScrollBar.Width + 1), GroupBoxContentsOffset);
                ScrollBar.Scroll += ScrollPropertyPanelButtons;

                PropertiesPanel.Controls.Add(ScrollBar);
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
                if (ScrollBar != null)
                {
                    currentButton.Width -= ScrollBar.Width;
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
            //! Finish this copy-pasted code
            MessageBox.Show("WRONG FUNCTION CALLED, DUMBASS");
            return;

            Button currentButton;
            var dcEntries = (dynamic[])((dynamic)dcArrayObj).Entries;
            var dcLen = dcEntries.Length;

            HeaderItemButtons = new Button[dcEntries.Length];


            for (var i = 0; i < dcLen; ++i)
            {
                var dcEntry = dcEntries[i];
                PropertiesPanel.Controls.Add(currentButton = NewPropertiesPanelButton());
                currentButton.Location = new Point(1, 7 + (currentButton.Height * i));


                // Apply header item name as button text
                currentButton.Text = dcEntry.Name.DecodedID;

                // Apply header item type name as button name
                currentButton.Name = dcEntry.Type.DecodedID;


                // Style the control
                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;


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


        public void ScrollPropertyPanelButtons(object _, ScrollEventArgs offset)
        {
            foreach (var button in PropertiesPanel.Controls.OfType<Button>())
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            PropertiesPanel.Update();
        }









        
        //#
        //## PropertiesEditor-related funtion declarations
        //#

        private void PopulatePropertiesEditorWithStructItems(object dcStruct)
        {
            var totalHeight = 8; // Start with 8 to both account for the GroupBox control's stupid title section at the top, and give the controls a tiny bit of padding

            var type = dcStruct.GetType();
            object[][] properties;
            PropertiesEditor.Controls.Clear();


            if (type == typeof(Map))
            {
                properties = ((Map) dcStruct).Items.Select(item => new object [] { ((SID) ((dynamic) item[1]).Name).DecodedID, item[1] }).ToArray();
            }
            else {
                properties = type.GetProperties().Select(property => new object [] { property.Name, property.GetValue(dcStruct) }).ToArray();
            }



            foreach (var property in properties)
            {
                var newRow = NewPropertiesEditorRow(property[0], property[1]);
                    
                PropertiesEditor.Controls.Add(newRow[0]);
                newRow[0].Location = new Point(2, totalHeight);

                PropertiesEditor.Controls.Add(newRow[1]);
                newRow[1].Location = new Point(newRow[0].Width, totalHeight);


                totalHeight += newRow[0].Height;
            }
        }

        
        private void PopulatePropertiesEditorWithArrayItems(string ArrayName, object ArrayStruct)
        {
            UpdateSelectionLabel(new[] { null, null, ArrayName });

            int dcLen = ((dynamic) ArrayStruct).Entries.Length;

            for (var i = 0; i < dcLen; ++i)
            {

            }
        }


        

        private Control[] NewPropertiesEditorRow(object propertyName, object propertyValue)
        {
            var nameBox = new TextBox()
            {
                Font = TextFont,
                BackColor = AppColourLight,
                ForeColor = Color.White,
                Padding = Padding.Empty,
                
                Height = DefaultPropertyButtonHeight,
                Width = (PropertiesEditor.Width / 3) - 2,
              
                Text = propertyName as string
            };

            // Assign basic form functionality event handlers
            nameBox.MouseDown += MouseDownFunc;
            nameBox.MouseUp += MouseUpFunc;
            nameBox.Click += (_, __) =>
            {

            };




            var valueBox = new TextBox()
            {
                Font = TextFont,
                BackColor = AppColourLight,
                ForeColor = Color.White,
                Padding = Padding.Empty,

                Height = DefaultPropertyButtonHeight,
                Width = PropertiesEditor.Width - nameBox.Width - 2,

                Text = FormatPropertyValueAsString(propertyValue, 0)
            };

            // Assign basic form functionality event handlers
            valueBox.MouseDown += MouseDownFunc;
            valueBox.MouseUp += MouseUpFunc;



            return new Control [] { nameBox, valueBox };
        }

        #endregion
    }
}
