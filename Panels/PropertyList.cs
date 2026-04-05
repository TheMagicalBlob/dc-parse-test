using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;

namespace NaughtyDogDCReader
{
    public partial class PropertyPanels
    {
        //==============================================\\
        //--|   PropertyList Variable Declarations   |--\\
        //==============================================\\
        #region [PropertyList Variable Declarations]

        /// <summary>
        /// Used in handling wrapping around the property list
        /// </summary>
        private PropertyButton[] FirstAndLastPropertyButtons;


        /// <summary>
        /// The selected/highlighted button out of the loaded header item buttons
        /// </summary>
        private PropertyButton PropertySelection
        {
            get => _propertySelection;

            set
            {
                if (value != null)
                {
                    LoadPropertyListSelectionIntoPropertyEditor(value.DCProperty);
                }
                else
                {
                    //! bitch & moan
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
        #endregion












        //==============================================\\
        //--|   PropertyList Function Declarations   |--\\
        //==============================================\\
        #region [PropertyList Function Declarations]
        
        /// <summary>
        /// Highlight the selected/active property button, after removing said highlight from the previous selection's button
        /// </summary>
        private void HighlightPropertyButton(PropertyButton newButton)
        {
            if (newButton == PropertySelection)
            {
                return;
            }


            // Default to the first Property Button if any are present
            if (newButton == null)
            {
                LogWindow.AppendLine("New Button was null, you fuckin' dunce");
                newButton = PropertySelectionPanel.Controls.OfType<PropertyButton>().FirstOrDefault();

                if (newButton == default || newButton == null)
                {
                    return;
                }

                PropertySelectionPanel.Focus();
                newButton.Select();
            }

            if (PropertySelection != null)
            {
                // "Reset" the previous button
                PropertySelection.Font = new Font(PropertySelection.Font.FontFamily, PropertySelection.Font.Size, PropertySelection.Font.Style ^ FontStyle.Underline);

                // Move the scroll bar if we're moving to a button that's outside the groupbox's bounds
                if (PropertyListScrollBar != null)
                {
                    var newScrollBarValue = PropertyListScrollBar.Value;

                    // Wrap to top
                    if (PropertySelection == FirstAndLastPropertyButtons[1] && newButton == FirstAndLastPropertyButtons[0])
                    {
                        newScrollBarValue = PropertyListScrollBar.Minimum;
                    }
                    // Wrap to bottom
                    else if (newButton == FirstAndLastPropertyButtons[1] && PropertySelection == FirstAndLastPropertyButtons[0])
                    {
                        newScrollBarValue = PropertyListScrollBar.Maximum - (PropertyListScrollBar.LargeChange - 1);
                    }

                    // Handle moving to slightly-offscreen buttons
                    else {
                        // Scroll up a little
                        if (newButton.Location.Y <= 0)
                        {
                            newScrollBarValue = PropertyListScrollBar.Value + newButton.Location.Y;
                        }
                        else if (newButton.Location.Y + newButton.Height >= PropertySelectionPanel.Size.Height)
                        {
                            // Scroll down a little
                            newScrollBarValue = PropertyListScrollBar.Value + (newButton.Location.Y - PropertySelectionPanel.Height) + newButton.Height + 2; // Why plus 2? I have no fucking idea, everything's jsut consistently off by a few pixels, and it's driving me insane
                        }



                        // Lazily catch overflow/underflow issues
                        if (newScrollBarValue < 0)
                        {
                            newScrollBarValue = 0;
                        }
                        else if (newScrollBarValue >= PropertyListScrollBar.Maximum - (PropertyListScrollBar.LargeChange - 1))
                        {
                            newScrollBarValue = PropertyListScrollBar.Maximum - (PropertyListScrollBar.LargeChange - 1);
                        }
                    }

                    ForceScrollPropertyListScrollBar(newScrollBarValue);
                }
            }

            var variableType = newButton.DCProperty.GetType();
            CTUpdateSelectionLabel($"Type: {variableType.Name}\nAddress: 0x{variableType.GetProperty("Address", variableType).GetValue(newButton.DCProperty):X}");


            PropertySelection = newButton;
            PropertySelection.Font = new Font(PropertySelection.Font.FontFamily, PropertySelection.Font.Size, PropertySelection.Font.Style | FontStyle.Underline);
        }

        /// <summary>
        /// //!
        /// </summary>
        /// <param name="ModuleOrProperty"></param>
        /// <param name="SelectionName"></param>
        /// <exception cref="Exception"></exception>
        private void SetupPropertyListPopulation(object ModuleOrProperty, string SelectionName)
        {
            //-# Variable Declarations
            object[][] entries;
            PropertyButton currentButton;
            int entryCount, cumulativeButtonHeight;
            var moduleOrPropertyType = ModuleOrProperty.GetType();

            echo($"\nPopulating PropertyList with contents of an item of type \"{moduleOrPropertyType.Name}\".");

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
                var ind = 0;
                entries = (ModuleOrProperty as Array).Cast<object>().Select(arrayItem =>
                {
                    var type = arrayItem.GetType();
                    return new object[]
                    {
                        arrayItem,
                        $"{type.Name} #{ind++}",
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
            cumulativeButtonHeight = (DefaultPropertyListButtonHeight * entryCount) - 1; // I don't know why it's off by a pixel and I'm sick of fucking with it.







            //##-> Reset panels to default state
            ResetPanels();

            //##-> Create and add the scroll bar if the controls are going to overflow the group box's height
            if (cumulativeButtonHeight >= PropertySelectionPanel.Height)
            {
                CreateScrollBarForGroupBox(PropertySelectionPanel, ref PropertyListScrollBar, cumulativeButtonHeight: cumulativeButtonHeight);

                FirstAndLastPropertyButtons = new PropertyButton[2];
            }



            void handleDoubleClickOrEnterInputsOnPropertyButton(object[] entry)
            {
                var entryPropertyOrObject = entry[0];

                if (ObjectIsStruct(entryPropertyOrObject) || entryPropertyOrObject.GetType().IsArray)
                {
                    History.Add(new object[] { SelectionName, ModuleOrProperty });

                    SetupPropertyListPopulation(entryPropertyOrObject, entry[1].ToString());
                }
                else {
                    LogWindow.AppendLine("unhandled doubleclick bs");
                }
            }






            //##-> Create and "style" a button for each property in the provided structure
            for (var i = 0; i < entryCount; ++i)
            {
                var entry = entries[i];
                currentButton = CreatePropertyListButton();

                PropertySelectionPanel.Controls.Add(currentButton);
                currentButton.Location = new Point(1, currentButton.Height * i);


                // Apply item name as button text
                currentButton.Text = entry[1].ToString();

                // Apply item type id as button name
                currentButton.Name = entry[2].ToString();


                // Style the control
                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;

                if (Venat.Controls.Contains(PropertyListScrollBar))
                {
                    // Account for the scroll bar's width by shrinking the buttons a bit if it's been added to the form
                    currentButton.Width -= PropertyListScrollBar.Width;
                }


                // Save the index of the header item tied to the control via the button's TabIndex property
                currentButton.TabIndex = i;

                currentButton.DCProperty = entry[0];



                // Apply highlight event handler to buttons
                currentButton.GotFocus += (button, _) => HighlightPropertyButton(button as PropertyButton);





                currentButton.PreviewKeyDown += (_, keyEvent) =>
                {
                    if (keyEvent.KeyCode == Keys.Return)
                    {
                        handleDoubleClickOrEnterInputsOnPropertyButton(entry);
                    }
                    if (keyEvent.KeyCode == Keys.Back)
                    {
                        GoBack();
                    }
                };

                currentButton.DoubleClick += (_, __) => handleDoubleClickOrEnterInputsOnPropertyButton(entry);

            }

            if (FirstAndLastPropertyButtons != null)
            {
                var propertyButtons = PropertySelectionPanel.Controls.Cast<PropertyButton>().ToArray();

                FirstAndLastPropertyButtons[0] = propertyButtons[0];
                FirstAndLastPropertyButtons[1] = propertyButtons.Last();
            }



            Panels.ForceHighlightDefaultPropertyButton();
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <returns> Home with the milk </returns>
        private PropertyButton CreatePropertyListButton()
        {
            var btn = new PropertyButton()
            {
                // Set button styling
                Font = MainFont,
                BackColor = AppColour,
                ForeColor = Color.White,

                FlatStyle = 0,
                Height = DefaultPropertyListButtonHeight
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
        /// Highlight the first/top PropertyButton in the panel
        /// </summary>
        public void ForceHighlightDefaultPropertyButton()
        {
            var newButton = PropertySelectionPanel.Controls.OfType<PropertyButton>().FirstOrDefault();

            if (newButton == default)
            {
                echo("No property buttons are on the form, so none were highlighted.");
                return;
            }



            HighlightPropertyButton(newButton);

            // Highlight the button as if it were clicked, so we can continue using the arrow keys without needing to reimplement that functionality we've already deleted
            PropertySelectionPanel.Focus();
            newButton.Select();
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <param name="hostBox"></param>
        /// <param name="offset"></param>
        public void ScrollPropertyListButtons(Control hostBox, ScrollEventArgs offset)
        {
            foreach (Control button in hostBox.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            hostBox.Update();
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <param name="NewValue"></param>
        public void ForceScrollPropertyListScrollBar(int NewValue)
        {
            ScrollEventType scrollEventType;

            if (NewValue < PropertyListScrollBar.Value)
            {
                scrollEventType = ScrollEventType.SmallDecrement; // Going Up
            }
            else if (NewValue > PropertyListScrollBar.Value)
            {
                scrollEventType = ScrollEventType.SmallIncrement; // Going Down
            }
            else {
                return;
            }


            ScrollPropertyListButtons(PropertySelectionPanel, new ScrollEventArgs(scrollEventType, PropertyListScrollBar.Value, PropertyListScrollBar.Value = NewValue));
            PropertySelectionPanel.Update();
        }






        /// <summary>
        /// Return to the previously displayed structure in the Property List
        /// </summary>
        public void GoBack()
        {
            var lastItem = History.LastOrDefault();

            if (lastItem != default)
            {
                if (History.Count == 1)
                {
                    SetupPropertyListPopulation((DCModule) lastItem[1], lastItem[0].ToString());

                    History.Remove(lastItem);
                }
                else if (History.Count > 1)
                {
                    SetupPropertyListPopulation(lastItem[1], lastItem[0].ToString());

                    History.Remove(lastItem);
                }
            }
        }






        /// <summary>
        /// Load the property (or struct, but I'm not going to clarify that literally every time...) for the highlighted property button, as if enter was pressed on it.
        /// </summary>
        public void LoadPropertyForHighlightedPropertyButton()
        {
            if (PropertySelection != null && PropertySelection.DCProperty != null)
            {
                SetupPropertyListPopulation(PropertySelection.DCProperty, PropertySelection.Name);
            }
        }
        #endregion PropertyList-related function declarations
    }
}
