using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static NaughtyDogDCReader.DCModule;

namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //==============================================\\
        //--|   PropertyList Variable Declarations   |--\\
        //==============================================\\
        #region [PropertyList Variable Declarations]

        /// <summary>
        /// The selected/highlighted button out of the loaded header item buttons
        /// </summary>
        private PropertyButton PropertySelection;
        
        
        
        /// <summary>
        /// Used in handling wrapping around the property list
        /// </summary>
        private PropertyButton[] FirstAndLastPropertyButtons;



        /// <summary>
        /// The (vertical) scroll bar used to navigate the buttons populating the PropertyList when they bleed passed the bottom of the group box
        /// </summary>
        public VScrollBar PropertyListScrollBar;


        
        /// <summary>
        /// Made it a variable in case it's needed for scaling. May try and implement that at some point, since I'm designing these on a fairly low-res screen.
        /// </summary>
        private readonly int DefaultPropertyListButtonHeight;
        
        

        /// <summary>
        /// //! I forget what this padding accounts for. I THINK the arrows on each end of the bar.
        /// </summary>
        public int PaddingForPropertyListScrollBar;
        #endregion











        //==============================================\\
        //--|   PropertyList Function Declarations   |--\\
        //==============================================\\
        #region [PropertyList Function Declarations]

        /// <summary>
        /// //!
        /// </summary>
        /// <param name="Module"></param>
        /// <param name="ModuleName"></param>
        /// <exception cref="Exception"></exception>
        public void PopulatePropertyList(DCModule Module, string ModuleName)
        {
            //-# Variable Declarations
            DCModule.DCEntry[] entries;
            PropertyButton currentButton;
            var moduleOrPropertyType = Module.GetType();

            echo($"\nPopulating PropertyList with contents of an item of type \"{moduleOrPropertyType.Name}\".");

            if (Module == null)
            {
                echo($"ERROR: null object provided for population (type: {moduleOrPropertyType})");
                ResetPanels(); // Reset panels to default state
                return;
            }
            if (moduleOrPropertyType == typeof(UnmappedStructure))
            {
                echo($"Aborting Panel population for {Module}, as it has not been mapped.");
                Log("Structure not yet mapped. Please use the hex editor instead (with caution).");
                return;
            }


            if (moduleOrPropertyType != typeof(DCModule))
            {
                throw new Exception($"ERROR: Invalid object passed for PropertyPanel population process. (type provided: {moduleOrPropertyType})");
            }



            // Grab the relevant properties from whatever-the-fuck was passed
            // 0: Struct/Property/Array Item
            // 1: Text for the button (struct/property name)
            // 2: Name of the button (struct/property type)
            entries = Module.Entries;


            if (entries.Length < 1)
            {
                echo($"Aborted panel population, as a struct \"{moduleOrPropertyType.Name}\" contains no properties.");
                Log("Structure contains no mapped properties to load.");
                return;
            }







            //##-> Reset panels to default state
            ResetPanels();

            //##-> Create and add the scroll bar if the controls are going to overflow the group box's height
            CreateScrollBarForGroupBox(PropertySelectionPanel, ref PropertyListScrollBar, entries.Length);

            if (PropertyListScrollBar != null)
            {
                FirstAndLastPropertyButtons = new PropertyButton[2];
            }


            void handleDoubleClickOrEnterInputsOnPropertyButton(DCModule.DCEntry entry)
            {
                Log("Entry Selection Still Unhandled");
            }






            //##-> Create and "style" a button for each property in the provided structure
            for (var i = 0; i < entries.Length; ++i)
            {
                var dcEntry = entries[i];
                currentButton = CreatePropertyListButton();

                PropertySelectionPanel.Controls.Add(currentButton);
                currentButton.Location = new Point(1, currentButton.Height * i);


                // Apply item name as button text
                currentButton.Text = dcEntry.Name.DecodedID;

                // Apply item type id as button name
                currentButton.Name = dcEntry.Struct.GetType().Name;


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

                currentButton.DCEntry = dcEntry;



                // Apply highlight event handler to buttons
                currentButton.GotFocus += (button, _) => HighlightPropertyButton(button as PropertyButton);





                currentButton.PreviewKeyDown += (_, keyEvent) =>
                {
                    if (keyEvent.KeyCode == Keys.Return)
                    {
                        handleDoubleClickOrEnterInputsOnPropertyButton(dcEntry);
                    }
                    if (keyEvent.KeyCode == Keys.Back)
                    {
                        ReturnToParent();
                    }
                };

                currentButton.DoubleClick += (_, __) => handleDoubleClickOrEnterInputsOnPropertyButton(dcEntry);

            }

            if (FirstAndLastPropertyButtons != null)
            {
                var propertyButtons = PropertySelectionPanel.Controls.Cast<PropertyButton>().ToArray();

                FirstAndLastPropertyButtons[0] = propertyButtons[0];
                FirstAndLastPropertyButtons[1] = propertyButtons.Last();
            }



            ForceHighlightDefaultPropertyButton();
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
                Log("New Button was null, you fuckin' dunce, trying to get a default... ");
                newButton = PropertySelectionPanel.Controls.OfType<PropertyButton>().FirstOrDefault();

                if (newButton == default || newButton == null)
                {
                    Log($" {nameof(PropertySelectionPanel)} doesn't contain any {nameof(PropertyButton)} controls!!!");
                    return;
                }

                Log($" Defaulted to the first {nameof(PropertyButton)} in the {nameof(PropertySelectionPanel)}'s controls.");
                PropertySelectionPanel.Focus();
                newButton.Select();
            }



            //##-> Reset the previoud button font and ensure the selected PropertyButton is on screen
            if (PropertySelection != null)
            {
                // "Reset" the previous button
                PropertySelection.Font = new Font(PropertySelection.Font.FontFamily, PropertySelection.Font.Size, PropertySelection.Font.Style ^ FontStyle.Underline);
                

                // Move the scroll bar if we're moving to a button that's outside the groupbox's bounds
                if (PropertyListScrollBar != null)
                {
                    var newScrollBarValue = PropertyListScrollBar.Value;

                    //##-> Handle wrapping from one end of the list to the other
                    if (PropertySelection == FirstAndLastPropertyButtons[1] && newButton == FirstAndLastPropertyButtons[0])
                    {
                        // Wrap to top
                        newScrollBarValue = PropertyListScrollBar.Minimum;
                    }
                    else if (newButton == FirstAndLastPropertyButtons[1] && PropertySelection == FirstAndLastPropertyButtons[0])
                    {
                        // Wrap to bottom
                        newScrollBarValue = PropertyListScrollBar.Maximum - (PropertyListScrollBar.LargeChange - 1);
                    }


                    //##-> Handle moving to slightly-offscreen buttons
                    else {
                        // Scroll up a little
                        if (newButton.Location.Y <= 0)
                        {
                            newScrollBarValue = PropertyListScrollBar.Value + newButton.Location.Y;
                        }
                        else if (newButton.Location.Y + newButton.Height >= PropertySelectionPanel.Size.Height)
                        {
                            // Scroll down a little
                            newScrollBarValue = PropertyListScrollBar.Value + (newButton.Location.Y - PropertySelectionPanel.Height) + newButton.Height + 2; // Why plus 2? I have no fucking idea, everything's just consistently off by a few pixels, and it's driving me insane
                        }



                        // Lazily catch overflow/underflow issues
                        if (newScrollBarValue < 0)
                        {
                            newScrollBarValue = 0;
                        }
                        else if (newScrollBarValue > PropertyListScrollBar.Maximum - (PropertyListScrollBar.LargeChange - 1))
                        {
                            newScrollBarValue = PropertyListScrollBar.Maximum - (PropertyListScrollBar.LargeChange - 1);
                        }
                    }

                    ForceScrollPropertyListScrollBar(newScrollBarValue);
                }
            }



            int itemSize;
            object itemAddress = 0;
            var itemType = newButton.DCEntry.Struct.GetType();
            string typeName;

            // Get the current item's address / offset
            if (itemType == typeof(UnmappedStructure))
            {
                typeName = ((UnmappedStructure) newButton.DCEntry.Struct).TypeID.DecodedID + " (unmapped)";
            }
            else {
                typeName = itemType.Name;
            }


            itemAddress = newButton.DCEntry.StructAddress;
            itemSize    = (int) (itemType.GetField("Size")?.GetValue(newButton.DCEntry.Struct) ?? -1);


            CTUpdateSelectionLabel(
                $"Type: {typeName}\n" +
                $"Address: 0x{itemAddress:X}\n" +
                $"Size: 0x{(itemSize == -1 ? "Unknown" : itemSize.ToString("X"))}"
            );




            PropertySelection = newButton;

            if (newButton != null)
            {
                PopulatePropertyEditor(newButton.DCEntry.Struct);
            }

            PropertySelection.Font = new Font(PropertySelection.Font.FontFamily, PropertySelection.Font.Size, PropertySelection.Font.Style | FontStyle.Underline);
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
        public void ReturnToParent()
        {
/*
            var lastItem = History.LastOrDefault();

            if (lastItem != default)
            {
                if (History.Count == 1)
                {
                    PopulatePropertyList((DCModule) lastItem[1], lastItem[0].ToString());

                    History.Remove(lastItem);
                }
                else if (History.Count > 1)
                {
                    PopulatePropertyList(lastItem[1], lastItem[0].ToString());

                    History.Remove(lastItem);
                }
            }
*/
        }

        #endregion PropertyList-related function declarations
    }
}
