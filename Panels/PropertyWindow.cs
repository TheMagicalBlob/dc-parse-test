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
        //=========================================================\\
        //--|   PropertiesWindow-Related Function Declarations   |--\\
        //=========================================================\\
        #region [PropertiesWindow-related Function Declarations]

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

        #endregion
    }
}
