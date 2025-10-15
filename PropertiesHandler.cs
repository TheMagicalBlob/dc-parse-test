using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

            propertiesPanelMammet = PopulatePropertiesPanelWithHeaderItems;
            propertiesWindowSameLineMammet = PropertiesWindowSameLineMammetWorker;
            propertiesWindowNewLineMammet = PropertiesWindowNewLineMammetWorker;
            propertiesWindowSpecificLineMammet = propertiesWindowSpecificLineMammetWorker;
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
                    PrintHeaderItemDetailDisplay((int) value.Tag);
                }

                _subItemSelection = value;
            }
        }
        private Button _subItemSelection;



        /// <summary>
        /// The (vertical) scroll bar used to navigate the buttons populating the PropertiesPanel when they bleed passed the bottom of the group box
        /// </summary>
        private ScrollBar scrollBar;

        private int IndentationDepth
        {
            get => Indentation.Length < 8 ? 0 : Indentation.Length / 8; set => Indentation = new string(' ', value * 8);
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


        private readonly PropertiesWindowOutputWand propertiesWindowSameLineMammet;
        private readonly PropertiesWindowOutputWand propertiesWindowNewLineMammet;
        private readonly PropertiesWindowOutputWand propertiesWindowSpecificLineMammet;

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



        private void propertiesWindowSpecificLineMammetWorker(string message, int line = 0)
        {
            PropertiesWindow.UpdateLine(Indentation + message.Replace("\n", "\n" + Indentation), line);
            PropertiesWindow.Update();
        }




        /// <summary>
        /// Replace a specified line in the properties output window with <paramref name="message"/>.
        /// <br/> Clears the line if no message is provided.
        /// </summary>
        public void PrintPropertyDetailSL(object message)
        {
            if (message == null)
            {
                message = " ";
            }

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try
            {
                Venat?.Invoke(propertiesWindowSameLineMammet, new object[] { message?.ToString() ?? "null", null });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailNL Invokation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }



        private void PropertiesWindowSameLineMammetWorker(string message, int _ = 0)
        {
            //! This is a bit of a lazy way of maintaining the indent...
            PropertiesWindow.UpdateLine(Indentation + PropertiesWindow.Lines.Last() + message.Replace("\n", "\n" + Indentation), PropertiesWindow.Lines.Length - 1);
            PropertiesWindow.Update();
        }




        /// <summary>
        /// Replace a specified line in the properties output window with <paramref name="message"/>.
        /// <br/> Clears the line if no message is provided.
        /// </summary>
        public void PrintPropertyDetailNL(object message = null, int indentationOverride = 0)
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
                Venat?.Invoke(propertiesWindowNewLineMammet, new object[] { message?.ToString() ?? "null", null });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailNL Invokation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }

        private void PropertiesWindowNewLineMammetWorker(string message, int _ = 0)
        {
            PropertiesWindow.AppendLine(message.Replace("\n", "\n" + Indentation), false);
            PropertiesWindow.Update();
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
        private string FormatPropertyValue(object value, int? indentationOverride = null)
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

                    }
                    returnString = value.ToString();
                    break;


                // ## Arrays
                case var type when type.ToString().Contains("[]"):
                {
                    returnString = $"{type.ToString().Replace("System.", emptyStr)}: {{\n";

                    foreach (var item in (Array) value)
                    {
                        returnString += $"{FormatPropertyValue(item, 1)},\n";
                    }

                    returnString += '}';
                    break;
                }



                // ## Unknown Struct
                case var type when type == typeof(UnmappedStructure):
                    returnString = $"{((UnmappedStructure) value).Message.Replace("\n", "\n        ")}";
                    break;


                // Hopefully structs
                default:
                    returnString = PrintSubItemDetailDisplay(value);
                    break;
            }

            return indent + returnString;
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
            PrintPropertyDetailNL("Type: " + structType.DecodedID);

            if (dcEntry.Struct == null)
            {
                PrintPropertyDetailNL("Null structure, for some reason.");
                return;
            }


            foreach (var property in dcEntry.Struct.GetType().GetProperties())
            {
                // Print the name of the property
                PrintPropertyDetailSL($" {SpaceOutStructName(property.Name)}: ");

                // Get and format the property value
                var val = property.GetValue(dcEntry.Struct);
                var formattedVal = FormatPropertyValue(val).Replace("\n", "\n" + Indentation);

                // Print the formatted property value
                PrintPropertyDetailNL(formattedVal);
            }
        }


        private string PrintSubItemDetailDisplay(object dcEntry)
        {
            //## clear the current properties window contents 
            PropertiesWindow.Clear();

            // Update Properties Window
            var ret = "Type: " + ((dynamic)dcEntry).Name.DecodedID + '\n';

            foreach (var property in dcEntry.GetType().GetProperties())
            {
                // Print the name of the property
                ret += $"{SpaceOutStructName(property.Name)}: ";

                var val = property.GetValue(dcEntry);
                ret += $"{FormatPropertyValue(val).Replace("\n", "\n" + Indentation)}\n";
            }

            return ret;
        }






        //#
        //## Properties Panel-related funtion declarations
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
            }
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

            HeaderItemButtons = new Button[dcEntries.Length];

            var cumulativeButtonHeight = DefaultPropertyButtonHeight * dcEntries.Length;
            if (cumulativeButtonHeight >= PropertiesPanel.Height - GroupBoxContentsOffset) // minus 7 to half-assedly account for the stupid top border of the group box.
            {
                scrollBar = new VScrollBar()
                {
                    Name = "PropertiesPanelScrollBar",
                    Height = PropertiesPanel.Height,
                    Width = 20, // Default width's a bit fat
                    Maximum = cumulativeButtonHeight
                };

                scrollBar.Location = new Point(PropertiesPanel.Width - (scrollBar.Width + 1), GroupBoxContentsOffset);
                scrollBar.Scroll += ScrollPropertyButtons;

                PropertiesPanel.Controls.Add(scrollBar);
            }


            for (var i = 0; i < dcLen; ++i)
            {
                var dcEntry = dcEntries[i];
                currentButton = NewButton();

                PropertiesPanel.Controls.Add(currentButton);
                currentButton.Location = new Point(1, (currentButton.Height * i) + GroupBoxContentsOffset);

                // Apply item name as button text
                currentButton.Text = dcEntry.Name.DecodedID;

                // Apply item type id as button name
                currentButton.Name = dcEntry.Type.DecodedID;


                // Style the control
                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;
                if (scrollBar != null)
                {
                    currentButton.Width -= scrollBar.Width;
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
            Venat.optionsMenuDropdownBtn.TabIndex += dcLen;
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
            return;


            Button currentButton;
            var dcEntries = (dynamic[])((dynamic)dcArrayObj).Entries;
            var dcLen = dcEntries.Length;

            HeaderItemButtons = new Button[dcEntries.Length];


            for (var i = 0; i < dcLen; ++i)
            {
                var dcEntry = dcEntries[i];
                PropertiesPanel.Controls.Add(currentButton = NewButton());
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
            Venat.optionsMenuDropdownBtn.TabIndex += dcLen;
            Venat.MinimizeBtn.TabIndex += dcLen;
            Venat.ExitBtn.TabIndex += dcLen;
        }



        private Button NewButton()
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

        public void ScrollPropertyButtons(object _, ScrollEventArgs offset)
        {
            foreach (var button in PropertiesPanel.Controls.OfType<Button>())
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            PropertiesPanel.Update();
        }
        #endregion
    }
}
