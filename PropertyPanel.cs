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
            IndentationDepth = 0;

            
            propertiesPanelMammet = PopulatePropertiesPanel;
            propertiesWindowSameLineMammet = _printPropertyDetailSL;
            propertiesWindowNewLineMammet = _printPropertyDetailNL;
            propertiesWindowSpecificLineMammet = _printPropertyDetailSpL;
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




        private int IndentationDepth
        {
            get {
                if (Indentation.Length < 8)
                {
                    return 0;
                }

                return Indentation.Length / 8;
            }
            set {
                Indentation = new string(' ', value * 8);
            }
        }

        private string Indentation = emptyStr;



        
        private readonly Type[] NumericalTypes = new []
        {
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(byte),
            typeof(sbyte),
        };


        


        //#
        //## Threading-Related Variables (threads, delegates, and mammets)
        //#
        
        /// <summary> //! </summary>
        public delegate void PropertiesWindowOutputWand(string msg, int line);
        
        /// <summary> //! </summary>
        public delegate void PropertiesPanelWand(string dcFileName, DCFileHeader dcEntries);


        private PropertiesWindowOutputWand propertiesWindowSameLineMammet;
        private PropertiesWindowOutputWand propertiesWindowNewLineMammet;
        private PropertiesWindowOutputWand propertiesWindowSpecificLineMammet;

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

                // Unsubscribe from the event once the struct's been loaded
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
        

        /// <summary>
        /// Overrite a specific line in the properties output window with the provided <paramref name="message"/>
        /// <br/> Appends an empty new line if no message is provided.
        /// </summary>
        public void PrintPropertyDetailSpL(object message = null, int line = 0)
        {
            if (message == null)
                message = emptyStr;

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(propertiesWindowSpecificLineMammet, new object[] { message?.ToString() ?? "null", line < 0 ? 0 : line });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailSL Invokation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }
        private void _printPropertyDetailSpL(string message, int line = 0)
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
                message = " ";

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(propertiesWindowSameLineMammet, new object[] { message?.ToString() ?? "null", null });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailNL Invokation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }
        private void _printPropertyDetailSL(string message, int _ = 0)
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
                message = emptyStr;

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

        private void _printPropertyDetailNL(string message, int _ = 0)
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

            for (var i = 0; i < name.Length; i++) {

                if (name[i] <= 122u && name[i] >= 97u) {

                    if (i + 1 != name.Length) {

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

                
            //var indent = new string(' ', 8 * indentationOverride ?? Indentation);

            switch (value.GetType())
            {
                // ## Basic Numerical Values
                case var val when NumericalTypes.Contains(val):
                    return $"0x{value:X}";

                // ## Booleans
                case var val when val == typeof(bool):
                    return val.ToString();


                // ## String ID's
                case var type when type == typeof(SID):
                    var id = ((SID)value).DecodedID;

                    if (id == "UNKNOWN_SID_64" && ShowUnresolvedSIDs)
                    {
                        id = ((SID)value).EncodedID;
                    }
                    #if DEBUG
                    else if (id == "INVALID_SID_64" && ShowInvalidSIDs)
                    {
                        id = ((SID)value).EncodedID;
                    }
                    #endif

                    return id;



                // ## Arrays
                case var type when type.ToString().Contains("[]"):
                    var str = $"{type.ToString().Replace("System.", emptyStr)}: {{\n";
                    foreach (var item in (Array) value)
                    {
                        str += $"        {FormatPropertyValue(item, 1)},\n";
                    }
                    str += '}';
                    return str;

                        

                // ## Unknown Struct
                case var type when type == typeof(UnknownStruct):
                    return $"{((UnknownStruct)value).Message.Replace("\n", "\n        ")}";


                default: return value.ToString();
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
                echo($"    Iterating through \"{dcEntry.Name.DecodedID}\".");
                foreach (var property in dcEntry.Struct.GetType().GetProperties())
                {
                    echo("Property: " + property);
                    PrintPropertyDetailSL($"# {SpaceOutStructName(property.Name)}: ");

                    
                    echo ("Getting property value...");
                    
                    var val = property.GetValue(dcEntry.Struct);
                    PrintPropertyDetailNL($"{FormatPropertyValue(val).Replace("\n", "\n" + Indentation)}");
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
            Venat.MinimizeBtn.TabIndex += dcLen;
            Venat.ExitBtn.TabIndex += dcLen;
        }
        #endregion
    }
}
