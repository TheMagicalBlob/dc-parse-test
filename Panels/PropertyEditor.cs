using System;
using System.CodeDom;
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
        /// <summary>
        /// The (vertical) scroll bar used to navigate the rows populating the PropertyEditor when they bleed passed the bottom of the group box
        /// </summary>
        private VScrollBar PropertyEditorScrollBar;
        private int PaddingForPropertyEditorScrollBar;




        //==========================================================\\
        //--|   PropertyEditor-Related Function Declarations   |--\\
        //==========================================================\\
        #region [PropertyEditor-Related Function Declarations]

        /// <summary>
        /// Populate either the PropertyEditor or PropertiesWindow with information about the highlighted PropertyButton's 
        /// </summary>
        /// <param name="property"></param>
        private void LoadPropertyListSelectionIntoPropertyEditor(object property)
        {
            if (property == null)
            {
                echo("Null property.");
                return;
            }



            var type = property.GetType();

            if (ObjectIsStruct(property))
            {
                //-# Object is a struct
                PopulateEditorWithStructProperties(property);
            }
            else {
                //-# Object is an Array of any type
                if (type.IsArray)
                {
                    PopulateEditorWithArrayItems(property as Array);
                    return;
                }



                //-# Object is some Numerical Value
                PopulateEditorWithSingleNumericalValue(property);
            }
        }

        /// <summary>
        /// Create and add a Property Editor row for each of the provided <paramref name="Struct"/>'s properties.<br/>Defaults to showing basic info if the struct is either unknown or contains no properties
        /// </summary>
        /// <param name="Struct"></param>
        /// <exception cref="Exception"></exception>
        private void PopulateEditorWithStructProperties(object Struct)
        {
            // Start with 2 to both account for the GroupBox control's stupid title section at the top, and give the controls a tiny bit of padding
            var totalHeight = 2;
            var type = Struct.GetType();

            // Grab the actual struct if the provided struct obj is a DC Header Entry (//! CLUNKY!)
            if (type == typeof(DCModule.DCEntry))
            {
                Struct = ((DCModule.DCEntry) Struct).Struct;
                type = Struct.GetType();
            }


            // Make sure the passed object is actually a struct
            if (!ObjectIsStruct(Struct))
            {
                throw new Exception($"ERROR: Object of type \"{Struct.GetType().Name}\" is not a struct");
            }





            PropertyEditorPanel.Controls.Clear();


            //##-> Create the applicable buttons
            if (type.GetProperties().Any())
            {
                // Create button for each struct property
                foreach (var property in type.GetProperties())
                {
                    var propertyValue = property.GetValue(Struct);

                    // Create the applicable buttons
                    var newRow = NewPropertyEditorRow(memberValue:propertyValue, memberClickEvent:ObjectIsStruct(propertyValue) ? setupPropertyListPopulation : spawnVariableEditorBox, memberName:property.Name);

                    PropertyEditorPanel.Controls.Add(newRow);
                    newRow.Location = new Point(2, totalHeight);

                    totalHeight += newRow.Height;
                }
            }
            else {
                // Create the default buttons for unmapped or empty structures
                PropertyButton newRow;
                long Address;
                string DecodedTypeID;

                // Get from expected UnmappedStructure struct
                if (Struct.GetType() == typeof(UnmappedStructure))
                {
                    var unmappedStruct = (UnmappedStructure) Struct;

                    Address = unmappedStruct.Address;

                    DecodedTypeID = unmappedStruct.TypeID.DecodedID;
                }
                // Dynamically get from unknown empty structure
                else {
                    Address = ((dynamic) Struct).Address;

                    DecodedTypeID = ((dynamic) Struct).TypeID.DecodedID;
                }


                // Struct Address Row
                PropertyEditorPanel.Controls.Add(newRow = NewPropertyEditorRow(memberValue: Address, memberClickEvent: null, memberName: null));
                newRow.Location = new Point(2, totalHeight);
                totalHeight += newRow.Height;

                // Decoded Type ID Row
                PropertyEditorPanel.Controls.Add(newRow = NewPropertyEditorRow(memberValue: DecodedTypeID, memberClickEvent: null, memberName: null));
                newRow.Location = new Point(2, totalHeight);
            }


            CreateScrollBarForGroupBox(PropertyEditorPanel, ref PropertyEditorScrollBar, DefaultPropertyEditorRowHeight * PropertyEditorPanel.Controls.Count);
        }








        /// <summary>
        /// //!
        /// </summary>
        /// <param name="Array"></param>
        /// <exception cref="Exception"></exception>
        private void PopulateEditorWithArrayItems(Array Array)
        {
            // Start with 2 to both account for the GroupBox control's stupid title section at the top, and give the controls a tiny bit of padding
            var totalHeight = 2;

            var type = Array.GetType();

            PropertyEditorPanel.Controls.Clear();

            if (!type.IsArray)
            {
                throw new Exception($"ERROR: Object of type \"{type.Name}\" is not an array.");
            }




            foreach (var item in Array)
            {
                string propertyName;
                PropertyPanelEventHandler propertyEvent;
                var itemType = item.GetType();

                if (itemType.IsArray || ObjectIsStruct(item))
                {
                    propertyEvent = setupPropertyListPopulation;
                    propertyName = itemType.IsArray ? itemType.GetElementType().Name : itemType.Name;
                }
                else {
                    propertyEvent = spawnVariableEditorBox;
                    propertyName = itemType.Name;
                }



                // Create the applicable buttons
                var newRow = NewPropertyEditorRow(item, propertyEvent, propertyName);

                PropertyEditorPanel.Controls.Add(newRow);

                newRow.Location = new Point(2, totalHeight);

                totalHeight += newRow.Height;
            }


            CreateScrollBarForGroupBox(PropertyEditorPanel, ref PropertyEditorScrollBar, DefaultPropertyEditorRowHeight * PropertyEditorPanel.Controls.Count);
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        private void PopulateEditorWithSingleNumericalValue(object value, string name = null)
        {
            var row = NewPropertyEditorRow(value, spawnVariableEditorBox, name ?? value.GetType().Name);

            PropertyEditorPanel.Controls.Add(row);

            row.Location = new Point(2, 2);
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="memberValue"></param>
        /// <param name="memberClickEvent"></param>
        /// <returns> A new row for the property editor, containing the value of said property. </returns>
        private PropertyButton NewPropertyEditorRow(object memberValue, PropertyPanelEventHandler memberClickEvent, string memberName = null)
        {
            PropertyButton newRow = null;

            var text = FormatPropertyValueAsString(memberValue);

            if (memberName != null && !ObjectIsStruct(memberValue))
            {
                text = $"{memberName}: " + text;
            }

            newRow = new PropertyButton()
            {
                Font = TextFont,
                BackColor = AppColourLight,
                ForeColor = Color.White,
                Padding = Padding.Empty,
                FlatStyle = FlatStyle.Flat,

                Height = DefaultPropertyEditorRowHeight,
                Width = PropertyEditorPanel.Width - (PropertyEditorScrollBar != null ? 20 : 0) - 2,

                Text = text,

                DCProperty = memberValue
            };

            // Assign basic form functionality event handlers
            newRow.MouseDown += MouseDownFunc;
            newRow.MouseUp += MouseUpFunc;

            newRow.DoubleClick += (row, __) => memberClickEvent(row, memberName);

            return newRow;
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
                case var type when type.IsArray:
                {
                    if (PropertyEditorPanel.Visible)
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
                    returnString = $"Unknown Struct {((UnmappedStructure) Value).TypeID.DecodedID} @ 0x{((UnmappedStructure) Value).Address:X}";
                    break;




                // Hopefully structs
                default:
                    returnString = Value.GetType().Name;
                    break;
            }


            return returnString;
        }








        /// <summary>
        /// //!
        /// </summary>
        /// <param name="propertyEditorRow"></param>
        /// <param name="memberName"></param>
        private void SpawnVariableEditorBox(object propertyEditorRow, string memberName = null)
        {
            if (memberName == null)
            {
                memberName = ((PropertyButton) propertyEditorRow).DCProperty.GetType().Name;
            }


            echo($"Creating editor box for {memberName}.");

            var parent = propertyEditorRow as PropertyButton;

            var editor = new TextBox()
            {
                Size = parent.Size,
                TextAlign = HorizontalAlignment.Center,
                Name = "TemporaryVariableEditorBox for " + memberName,
                Text = FormatPropertyValueAsString(parent.DCProperty)
            };

            parent.Controls.Add(editor);

            editor.Location = Point.Empty;

            editor.PreviewKeyDown += ParseEditedVariableString;
        }







        /// <summary>
        /// //!
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="eventArgs"></param>
        private void ParseEditedVariableString(object editor, PreviewKeyDownEventArgs eventArgs)
        {

        }








        /// <summary>
        /// //!
        /// </summary>
        /// <param name="_"></param>
        /// <param name="offset"></param>
        public void ScrollPropertyEditorRows(object _, ScrollEventArgs offset)
        {
            foreach (Control button in PropertyEditorPanel.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            PropertyEditorPanel.Update();
        }








        /// <summary>
        /// //!
        /// </summary>
        /// <param name="Incrementation"></param>
        public void ForceScrollPropertyEditorRows(int Incrementation)
        {
            foreach (Control button in PropertyEditorPanel.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (PropertyEditorScrollBar.Value - (PropertyEditorScrollBar.Value += Incrementation)));
            }
            PropertyEditorPanel.Update();
        }
        #endregion PropertyEditor-related function declarations
    }
}
