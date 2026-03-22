using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NaughtyDogDCReader.Main;

namespace NaughtyDogDCReader
{
    public partial class PropertyPanels
    {

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
            try
            {
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
            try
            {
                Venat?.Invoke(propertiesWindowMammet, new object[] { message });
            }
            catch (Exception dang)
            {
                var err = $"Missed {nameof(pw_PrintPropertyDetail)} Invocation due to a(n) {dang.GetType()}.";
                echo(err);
            }
            finally
            {
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

            PropertyWindow.Clear();
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
            PropertyWindow.Clear();

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
                    returnString = $"{((UnmappedStructure) Value).Name}//!";
                    break;




                // Hopefully structs
                default:
                    if (PropertyEditorPanel.Visible)
                    {
                        returnString = Value.GetType().Name;
                    }
                    else
                    {
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
    }
}
