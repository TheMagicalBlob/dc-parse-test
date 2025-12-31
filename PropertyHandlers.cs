using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            DefaultPropertiesPanelButtonHeight = 23;
            DefaultPropertiesEditorRowHeight = 23;

            Changes = new List<object[]>();
            History = new List<object[]>();



            //#
            //## Create the various delegates for the Properties Handler, so we can do shit across multiple threads
            //#

            populatePropertiesPanelWithHeaderItemContents = pp_SetupPropertiesPanelPopulation;

            populatePropertiesPanelWithClickedItemsContents = pp_SetupPropertiesPanelPopulation;

            spawnVariableEditorBox = SpawnVariableEditorBox;



            // Newline
            propertiesWindowNewLineMammet = (message) =>
            {
                PropertiesWindow.AppendLine(message, false);
                PropertiesWindow.Update();
            };


            // Newline W/ Indent
            propertiesWindowMammet = (message) =>
            {
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
                    DisplayHighlightedPropertyDetails(value.DCProperty);
                }

                _propertySelection = value;
            }
        }
        private PropertyButton _propertySelection;





        /// <summary>
        /// The (vertical) scroll bar used to navigate the buttons populating the PropertiesPanel when they bleed passed the bottom of the group box
        /// </summary>
        public VScrollBar PropertiesPanelScrollBar;
        public int PaddingForPropertiesPanelScrollBar;

        /// <summary>
        /// The (vertical) scroll bar used to navigate the rows populating the PropertiesEditor when they bleed passed the bottom of the group box
        /// </summary>
        private VScrollBar PropertiesEditorScrollBar;
        private int PaddingForPropertiesEditorScrollBar;



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







        //#
        //## Threading-Related Variables (threads, delegates, and mammets)
        //#

        /// <summary> //! </summary>
        public delegate void PropertiesWindowOutputWand(string msg);

        /// <summary> //! </summary>
        public delegate void InitialPropertiesPanelPopulation(string dcFileName, DCModule dcEntries);
        public delegate void SubsequentPropertiesPanelPopulation(string structName, object structProperty);

        public delegate void PropertyEditorClickHandler(string propertyName, object property);


        private readonly PropertiesWindowOutputWand propertiesWindowMammet;
        private readonly PropertiesWindowOutputWand propertiesWindowNewLineMammet;


        public readonly InitialPropertiesPanelPopulation populatePropertiesPanelWithHeaderItemContents;

        public PropertyEditorClickHandler spawnVariableEditorBox;
        public PropertyEditorClickHandler populatePropertiesPanelWithClickedItemsContents;
        #endregion

















        //=================================\\
        //--|   Function Declarations   |--\\
        //=================================\\
        #region [Function Declarations]
        //#
        //## Miscellaneous Function Declarations
        //#
        #region [Miscellaneous Function Declarations]


        private void SpawnVariableEditorBox(string propertyName, object property)
        {

        }




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

                    hostBoxScrollBarReference.Scroll += (_, args) => ScrollPropertiesPanelButtons(groupBox, args);
                }

                Venat.Controls.Add(hostBoxScrollBarReference);
            }


            hostBoxScrollBarReference.BringToFront();
                
            hostBoxScrollBarReference.Maximum = cumulativeButtonHeight - groupBox.Height + DefaultPropertiesPanelButtonHeight + 2;
            hostBoxScrollBarReference.SmallChange = DefaultPropertiesPanelButtonHeight;
        }






        /// <summary>
        /// Populate either the PropertiesEditor or PropertiesWindow with information about the highlighted PropertyButton's 
        /// </summary>
        /// <param name="property"></param>
        private void DisplayHighlightedPropertyDetails(object property)
        {
            if (property == null)
            {
                echo("Null property.");
                return;
            }



            var type = property.GetType();

            if (ObjectIsStruct(property))
            {
                #if DEBUG
                if (type == typeof(DCModule.DCEntry))
                {
                    echo($"button property is DCHeaderItem {((DCModule.DCEntry) property).Name.DecodedID} of type {type}.");
                }
                else {
                    echo($"button property is unnamed struct of type {type}.");
                }
                #endif

                if (PropertiesEditor.Visible)
                {
                    if (type == typeof(DCModule.DCEntry))
                    {
                        pe_PopulatePropertiesEditorWithStructItems(((DCModule.DCEntry) property).Struct);
                    }
                    else {
                        pe_PopulatePropertiesEditorWithStructItems(property);
                    }
                }
                else {
                    if (type == typeof(DCModule.DCEntry))
                    {
                        pw_PrintHeaderItemDetails((DCModule.DCEntry) property);
                    }
                    else {
                        pw_PrintStructPropertyDetails(property);
                    }
                }

            }
            else {
                //-# Object is an Array of any type
                if (type.IsArray)
                {
                    pe_PopulatePropertiesEditorWithArrayItems(property as Array);
                    return;
                }


                
                //-# Object is some Numerical Value
                echo($"Button property is \"{type}\"");
                LogWindow.AppendLine("Non.");
                pe_PopulatePropertiesEditorWithSingleNumericalValue(property);
            }
        }






        /// <summary>
        /// Reset all instance members in the current PropertiesHandler (clear all added controls, reset static ones to default states, clear variables)
        /// </summary>
        public void ResetPanels()
        {
            PropertiesWindow.Clear();
            PropertiesPanel.Controls.Clear();
            PropertiesEditor.Controls.Clear();

            FirstAndLastPropertyButtons = null;
            PropertySelection = null;

            Venat.Controls.Remove(PropertiesPanelScrollBar);
            PropertiesPanelScrollBar = null;

            Venat.Controls.Remove(PropertiesEditorScrollBar);
            PropertiesEditorScrollBar = null;

            IndentationDepth = 0;
        }
        #endregion












        //#
        //## PropertiesPanel-Related Function Declarations
        //#
        #region [PropertiesPanel-Related Function Declarations]

        private void pp_SetupPropertiesPanelPopulation(string SelectionName, object ModuleOrProperty)
        {
            //-# Variable Declarations
            object[][] entries;
            PropertyButton currentButton;
            int entryCount, cumulativeButtonHeight;
            var moduleOrPropertyType = ModuleOrProperty.GetType();

            echo($"\nPopulating PropertiesPanel with contents of an item of type \"{moduleOrPropertyType.Name}\".");

            if (ModuleOrProperty == null)
            {
                echo($"ERROR: null object provided for population (type: {moduleOrPropertyType})");
                ResetPanels(); // Reset panels to default state
                return;
            }


            
            // Grab the relevant properties from whatever-the-fuck was passed
            // 0: Struct/Property/Array Item
            // 1: Text for the button (struct/property name)
            // 2: Name of the button (struct/property type)
            if (moduleOrPropertyType == typeof(DCModule))
            {
                entries = (ModuleOrProperty as DCModule).Entries.Select(entry => new object[]
                {
                    entry.Struct,
                    entry.Name.DecodedID,
                    entry.Type.DecodedID

                }).ToArray();
            }
            else if (moduleOrPropertyType.IsArray)
            {
                entries = (ModuleOrProperty as Array).Cast<object>().Select(arrayItem =>
                {
                    var type = arrayItem.GetType();
                    return new object[]
                    {
                        arrayItem,
                        $"Unnamed {type.Name}",
                        type.Name

                    };

                }).ToArray();
            }
            else if (ObjectIsStruct(ModuleOrProperty))
            {
                entries = moduleOrPropertyType.GetProperties().Select(property =>
                {
                    var type = property.GetType();
                    return new object[]
                    {
                        property.GetValue(ModuleOrProperty),
                        property.Name,
                        property.GetType().Name
                    };

                }).ToArray();
            }
            else
            {
                throw new Exception($"ERROR: Invalid object passed for PropertyPanel population process. (type provided: {moduleOrPropertyType})");
            }

            entryCount = entries.Length;
            cumulativeButtonHeight = DefaultPropertiesPanelButtonHeight * entryCount;





            

            //-## Reset panels to default state
            ResetPanels();

            //-## Create and add the scroll bar if the controls are going to overflow the group box's height
            if (cumulativeButtonHeight >= PropertiesPanel.Height)
            {
                CreateScrollBarForGroupBox(PropertiesPanel, ref PropertiesPanelScrollBar, cumulativeButtonHeight:cumulativeButtonHeight);
                
                FirstAndLastPropertyButtons = new PropertyButton[2];
            }


            
            void handleDoubleClickOrEnterInputsOnPropertyButton(object[] entry)
            {
                var entryPropertyOrObject = entry[0];

                if (ObjectIsStruct(entryPropertyOrObject) || entryPropertyOrObject.GetType().IsArray)
                {
                    History.Add(new object[] { SelectionName, ModuleOrProperty });

                    pp_SetupPropertiesPanelPopulation(entry[1].ToString(), entryPropertyOrObject);
                }
                else
                {
                    LogWindow.AppendLine("unhandled");
                }
            }



            // Create and "style" a button for each property in the provided structure
            for (var i = 0; i < entryCount; ++i)
            {
                var entry = entries[i];
                currentButton = CreatePropertiesPanelButton();

                PropertiesPanel.Controls.Add(currentButton);
                currentButton.Location = new Point(1, currentButton.Height * i);

                
                // Apply item name as button text
                currentButton.Text = entry[1].ToString();

                // Apply item type id as button name
                currentButton.Name = entry[2].ToString();


                // Style the control
                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;

                if (Venat.Controls.Contains(PropertiesPanelScrollBar))
                {
                    // Account for the scroll bar's width by shrinking the buttons a bit if it's been added to the form
                    currentButton.Width -= PropertiesPanelScrollBar.Width;
                }


                // Save the index of the header item tied to the control via the button's TabIndex property
                currentButton.TabIndex = i;

                currentButton.DCProperty = entry[0];



                // Apply highlight event handler to buttons
                currentButton.GotFocus += (button, _) => PropertyButtonHighlighted(button as PropertyButton);





                currentButton.PreviewKeyDown += (_, keyEvent) =>
                {
                    if (keyEvent.KeyCode == Keys.Return)
                    {
                        handleDoubleClickOrEnterInputsOnPropertyButton(entry);
                    }
                };

                currentButton.DoubleClick += (_, __) => handleDoubleClickOrEnterInputsOnPropertyButton(entry);

            }
            if (FirstAndLastPropertyButtons != null)
            {
                var propertyButtons = PropertiesPanel.Controls.Cast<PropertyButton>().ToArray();

                FirstAndLastPropertyButtons[0] = propertyButtons[0];
                FirstAndLastPropertyButtons[1] = propertyButtons[1];
            }

             

            Panels.ForceHighlightDefaultPropertyButton();
        }


        /// <summary>
        /// Fill the PropertiesPanel with the <paramref name="LoadedModule"/>'s header structures.
        /// </summary>
        /// <param name="LoadedModuleName"> The name of the loaded module; specifically the file name, since the modules' names aren't stored anywhere inside them. </param>
        /// <param name="LoadedModule">  </param>

        


        /// <summary>
        /// Fill the PropertiesPanel with entries for the <paramref name="currentStruct"/>'s Properties.
        /// </summary>
        /// <param name="currentStuctName"></param>
        /// <param name="currentStruct"></param>
        


        
        /// <summary>
        /// //!
        /// </summary>
        /// <returns> Home with the milk </returns>
        private PropertyButton CreatePropertiesPanelButton()
        {
            var btn = new PropertyButton()
            {
                // Set button styling
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

            btn.MouseClick += new MouseEventHandler((_, eventArgs) =>
            {
                //! Right-click dropdown menu functionality maybe
            });


            return btn;
        }


        


        /// <summary>
        /// Highlight the selected/active property button, after removing said highlight from the previous selection's button
        /// </summary>
        private void PropertyButtonHighlighted(PropertyButton newButton)
        {
            if (newButton == PropertySelection)
            {
                return;
            }


            // Default to the first Property Button if any are present
            if (newButton == null)
            {
                newButton = PropertiesPanel.Controls.OfType<PropertyButton>().FirstOrDefault();

                if (newButton == default || newButton == null)
                {
                    return;
                }

                PropertiesPanel.Focus();
                newButton.Select();
            }

            if (PropertySelection != null)
            {
                // "Reset" the previous button
                PropertySelection.Font = new Font(PropertySelection.Font.FontFamily, PropertySelection.Font.Size, PropertySelection.Font.Style ^ FontStyle.Underline);

                // Move the scroll bar if we're moving to a button that's outside the groupbox's bounds
                if (PropertiesPanelScrollBar != null)
                {
                    var newValue = PropertiesPanelScrollBar.Value;

                    // Handle wrapping from one end to another
                    if (PropertySelection == FirstAndLastPropertyButtons[1] && newButton == FirstAndLastPropertyButtons[0]) // Wrap to top
                    {
                        newValue = PropertiesPanelScrollBar.Minimum;
                    }
                    else if (newButton == FirstAndLastPropertyButtons[1] && PropertySelection == FirstAndLastPropertyButtons[0]) // Wrap to bottom
                    {
                        newValue = PropertiesPanelScrollBar.Maximum - (PropertiesPanelScrollBar.LargeChange - 1);
                    }
                    else
                    {
                        // Handle moving to slightly-offscreen buttons
                        if (newButton.Location.Y <= 0)
                        {
                            newValue = PropertiesPanelScrollBar.Value + newButton.Location.Y; // Scroll up a little
                        }
                        else if (newButton.Location.Y + newButton.Height >= PropertiesPanel.Size.Height)
                        {
                            newValue = PropertiesPanelScrollBar.Value + (newButton.Location.Y - PropertiesPanel.Height) + newButton.Height + 2; // Scroll down a little
                        }



                        // Lazily catch overflow/underflow issues
                        if (newValue < 0)
                        {
                            newValue = 0;
                        }
                        else if (newValue >= PropertiesPanelScrollBar.Maximum - (PropertiesPanelScrollBar.LargeChange - 1))
                        {
                            newValue = PropertiesPanelScrollBar.Maximum - (PropertiesPanelScrollBar.LargeChange - 1);
                        }
                    }
                    
                    ForceScrollPropertiesPanelScrollBar(newValue); 
                }
            }


            UpdateSelectionLabel(new[] { null, newButton.Text });


            (PropertySelection = newButton)
            .Font = new Font(PropertySelection.Font.FontFamily, PropertySelection.Font.Size, PropertySelection.Font.Style | FontStyle.Underline);
        }



        /// <summary>
        /// Highlight the first/top PropertyButton in the panel
        /// </summary>
        public void ForceHighlightDefaultPropertyButton()
        {
            var newButton = PropertiesPanel.Controls.OfType<PropertyButton>().FirstOrDefault();

            if (newButton == default)
            {
                echo ("No property buttons are on the form, so none were highlighted.");
                return;
            }



            PropertyButtonHighlighted(newButton);

            // Highlight the button as if it were clicked, so we can continue using the arrow keys without needing to reimplement that functionality we've already deleted
            PropertiesPanel.Focus();
            newButton.Select();
        }






        





        public void ScrollPropertiesPanelButtons(Control hostBox, ScrollEventArgs offset)
        {
            foreach (Control button in hostBox.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            hostBox.Update();
        }
        



        public void ForceScrollPropertiesPanelScrollBar(int NewValue)
        {
            ScrollEventType scrollEventType;

            if (NewValue < PropertiesPanelScrollBar.Value)
            {
                scrollEventType = ScrollEventType.SmallDecrement; // Going Up
            }
            else if (NewValue > PropertiesPanelScrollBar.Value)
            {
                scrollEventType = ScrollEventType.SmallIncrement; // Going Down
            }
            else {
                return;
            }


            ScrollPropertiesPanelButtons(PropertiesPanel, new ScrollEventArgs(scrollEventType, PropertiesPanelScrollBar.Value, PropertiesPanelScrollBar.Value = NewValue));
            PropertiesPanel.Update();
        }


        public void GoBack()
        {
            var lastItem = History.LastOrDefault();

            if (lastItem != default)
            {
                if (History.Count == 1)
                {
                    pp_SetupPropertiesPanelPopulation(lastItem[0].ToString(), (DCModule) lastItem[1]);

                    History.Remove(lastItem);
                }
                else if (History.Count > 1)
                {
                    pp_SetupPropertiesPanelPopulation(lastItem[0].ToString(), lastItem[1]);
                                
                    History.Remove(lastItem);
                }
                else {
                    echo("Why the hell is the history empty?");
                }
            }
        }

        
        /// <summary>
        /// Load the property (or struct, but I'm not going to clarify that literally every time...) for the highlighted property button, as if enter was pressed on it.
        /// </summary>
        public void LoadPropertyForHighlightedPropertyButton()
        {
            if (PropertySelection == null || PropertySelection.DCProperty == null)
            {
                return;
            }
            pp_SetupPropertiesPanelPopulation(PropertySelection.Name, PropertySelection.DCProperty);
        }
        #endregion propertiespanel-related function declarations















        //==========================================================\\
        //--|   PropertiesEditor-Related Function Declarations   |--\\
        //==========================================================\\
        #region [PropertiesEditor-Related Function Declarations]

        private void pe_PopulatePropertiesEditorWithStructItems(object Struct)
        {
            Control NewPropertiesEditorRow(string propertyName, object propertyValue, PropertyEditorClickHandler propertyEvent)
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
                    Width = PropertiesEditor.Width - (PropertiesEditorScrollBar != null ? 20 : 0) - 2,

                    Text = $"{propertyName}: {FormatPropertyValueAsString(propertyValue)}"
                };

                // Assign basic form functionality event handlers
                newRow.MouseDown += MouseDownFunc;
                newRow.MouseUp += MouseUpFunc;
                
                newRow.DoubleClick += (_, __) => propertyEvent(propertyName, propertyValue);



                return newRow;
            }


            // Start with 2 to both account for the GroupBox control's stupid title section at the top, and give the controls a tiny bit of padding
            var totalHeight = 2;

            var type = Struct.GetType();

            PropertiesEditor.Controls.Clear();

            if (!ObjectIsStruct(Struct))
            {
                throw new Exception($"ERROR: Object of type \"{Struct.GetType().Name}\" is not a struct");
            }


            

            foreach (var property in type.GetProperties().Select(property => new object [] { property.Name, property.GetValue(Struct), ObjectIsStruct(property.GetValue(Struct)) ? populatePropertiesPanelWithClickedItemsContents : spawnVariableEditorBox }))
            {
                // Create the applicable buttons
                var newRow = NewPropertiesEditorRow(property[0]?.ToString() ?? null, property[1], (PropertyEditorClickHandler) property[2]);
                    
                PropertiesEditor.Controls.Add(newRow);
                newRow.Location = new Point(2, totalHeight);

                totalHeight += newRow.Height;
            }
            
            
            CreateScrollBarForGroupBox(PropertiesEditor, ref PropertiesEditorScrollBar, DefaultPropertiesEditorRowHeight * PropertiesEditor.Controls.Count);
        }

        
        private void pe_PopulatePropertiesEditorWithArrayItems(Array Array)
        {

        }

        private void pe_PopulatePropertiesEditorWithSingleNumericalValue(object number)
        {

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
        #endregion propertiesEditor-related function declarations



        








        //#
        //## Output-related function declarations
        //#
        #region [Properties Window-related function declarations]

        /// <summary>
        /// Replace a specified line in the properties output window with <paramref name="message"/>.
        /// <br/> Clears the line if no message is provided.
        /// </summary>
        public void pw_PrintPropertyDetailNL(object message = null)
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
                var err = $"Missed {nameof(pw_PrintPropertyDetailNL)} Invocation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }






        /// <summary>
        /// 
        /// </summary>
        public void pw_PrintPropertyDetail(object message = null, int? indentationOverride = null)
        {
            var previousIndentation = -1;
            
            if (message == null)
            {
                message = "null";
            }
            if (indentationOverride != null)
            {
                previousIndentation = IndentationDepth;
                IndentationDepth = (int) indentationOverride;
            }


#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(propertiesWindowMammet, new object[] { message });
            }
            catch (Exception dang)
            {
                var err = $"Missed {nameof(pw_PrintPropertyDetail)} Invocation due to a(n) {dang.GetType()}.";
                echo(err);
            }
            finally {
                if (indentationOverride != null)
                {
                    IndentationDepth = previousIndentation;
                }
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
        private void pw_PrintHeaderItemDetails(DCModule.DCEntry @struct)
        {
            //#
            //## Grab basic data about the current item and clear the current properties window contents 
            //#
            var structType = @struct.Type;

            PropertiesWindow.Clear();
            UpdateSelectionLabel(new[] { null, @struct.Name.DecodedID });




            // Update Properties Window
            pw_PrintPropertyDetailNL("\nType: " + structType.DecodedID);

            if (@struct.Struct == null)
            {
                pw_PrintPropertyDetailNL("Null structure, for some reason.");
                return;
            }

            IndentationDepth = 1;
            foreach (var property in @struct.Struct.GetType().GetProperties())
            {
                // Print the name of the property
                pw_PrintPropertyDetailNL($"{SpaceOutStructName(property.Name)}: [");

                // Get and format the property value
                var rawValue = property.GetValue(@struct.Struct);
                var formattedValue = FormatPropertyValueAsString(rawValue);

                // Print the formatted property value
                pw_PrintPropertyDetail(Indentation + formattedValue.Replace("\n", "\n" + Indentation));

                pw_PrintPropertyDetailNL("]");
            }
        }






        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcEntry"></param>
        /// <returns></returns>
        private void pw_PrintStructPropertyDetails(object dcEntry)
        {
            //## clear the current properties window contents 
            PropertiesWindow.Clear();

            // Update Properties Window
            var str = FormatPropertyValueAsString(dcEntry);

#if DEBUG
            echo(str);
#endif

            pw_PrintPropertyDetail(str);
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
        private string FormatPropertyValueAsString(object Value)
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
                    returnString = $"{{{((SID) Value).DecodedID}}}";
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
                case var type when type.Name.Contains("[]"):
                {
                    if (PropertiesEditor.Visible)
                    {
                        var ind = type.Name.IndexOf(']');
                        returnString = type.Name.Remove(ind) + ((Array) Value).Length + type.Name.Substring(ind); // insert the array length into the type name
                    }
                    else
                    {
                        var depth = type.ToString().Count(item => item == '[');
                        var len = ((Array) Value).Length;
                        var indentIndex = 1;
                        
                        string printArrayContents(object item)
                        {
                            var itemType = item.GetType();
                            var retStr = string.Empty;

                            echo($"Item {itemType} base type: [{itemType.BaseType}]");

                            if (itemType.BaseType == typeof(Array))
                            {
                                foreach (var subItem in (Array) item)
                                {
                                    ++indentIndex;
                                    retStr += printArrayContents(subItem);
                                    --indentIndex;
                                }
                            }

                            retStr += $"{new string(' ', 4 * indentIndex)}{FormatPropertyValueAsString(item)},\n";

                            return retStr;
                        }

                        returnString = type.Name + " {\n";
                        foreach (var item in (Array) Value)
                        {
                            returnString += printArrayContents(item);
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
                        var str = "\nType: " + ((dynamic)Value).Name.DecodedID + '\n';

                        foreach (var property in Value.GetType().GetProperties())
                        {
                            // Print the name of the property
                            str += $"{SpaceOutStructName(property.Name)}: [\n";

                            var val = property.GetValue(Value);
                            str += $"{FormatPropertyValueAsString(val).Replace("\n", "\n" + Indentation)}\n";
                        }

                        returnString = str;
                    }
                    break;
            }


            return returnString;
        }

        #endregion
        #endregion (function declarations)
    }
}
