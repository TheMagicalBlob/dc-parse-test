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

        //==========================================================\\
        //--|   PropertiesEditor-Related Function Declarations   |--\\
        //==========================================================\\
        #region [PropertiesEditor-Related Function Declarations]

        private void pe_PopulatePanelWithStructItems(object Struct)
        {
            // Start with 2 to both account for the GroupBox control's stupid title section at the top, and give the controls a tiny bit of padding
            var totalHeight = 2;
            var type = Struct.GetType();

            if (type == typeof(DCModule.DCEntry))
            {
                Struct = ((DCModule.DCEntry) Struct).Struct;
                type = Struct.GetType();
            }


            if (!ObjectIsStruct(Struct))
            {
                throw new Exception($"ERROR: Object of type \"{Struct.GetType().Name}\" is not a struct");
            }





            PropertyEditorPanel.Controls.Clear();

            foreach (var property in type.GetProperties())
            {
                var propertyValue = property.GetValue(Struct);

                // Create the applicable buttons
                var newRow = NewPropertiesEditorRow(memberValue:propertyValue, memberClickEvent:ObjectIsStruct(propertyValue) ? setupPropertiesPanelPopulation : spawnVariableEditorBox, memberName:property.Name);

                PropertyEditorPanel.Controls.Add(newRow);
                newRow.Location = new Point(2, totalHeight);

                totalHeight += newRow.Height;
            }


            CreateScrollBarForGroupBox(PropertyEditorPanel, ref PropertiesEditorScrollBar, DefaultPropertiesEditorRowHeight * PropertyEditorPanel.Controls.Count);
        }



        private void pe_PopulatePanelWithArrayItems(Array Array)
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
                    propertyEvent = setupPropertiesPanelPopulation;
                    propertyName = itemType.IsArray ? itemType.GetElementType().Name : itemType.Name;
                }
                else
                {
                    propertyEvent = spawnVariableEditorBox;
                    propertyName = itemType.Name;
                }



                // Create the applicable buttons
                var newRow = NewPropertiesEditorRow(item, propertyEvent, propertyName);

                PropertyEditorPanel.Controls.Add(newRow);

                newRow.Location = new Point(2, totalHeight);

                totalHeight += newRow.Height;
            }


            CreateScrollBarForGroupBox(PropertyEditorPanel, ref PropertiesEditorScrollBar, DefaultPropertiesEditorRowHeight * PropertyEditorPanel.Controls.Count);
        }



        private void pe_PopulatePanelWithSingleNumericalValue(object value, string name = null)
        {
            var row = NewPropertiesEditorRow(value, spawnVariableEditorBox, name ?? value.GetType().Name);

            PropertyEditorPanel.Controls.Add(row);

            row.Location = new Point(2, 2);
        }







        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="memberValue"></param>
        /// <param name="memberClickEvent"></param>
        /// <returns> A new row for the property editor, containing the value of said property. </returns>
        private PropertyButton NewPropertiesEditorRow(object memberValue, PropertyPanelEventHandler memberClickEvent, string memberName = null)
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

                Height = DefaultPropertiesEditorRowHeight,
                Width = PropertyEditorPanel.Width - (PropertiesEditorScrollBar != null ? 20 : 0) - 2,

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
        /// 
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







        private void ParseEditedVariableString(object editor, PreviewKeyDownEventArgs eventArgs)
        {

        }







        public void ScrollPropertyEditorRows(object _, ScrollEventArgs offset)
        {
            foreach (Control button in PropertyEditorPanel.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (offset.NewValue - offset.OldValue));
            }
            PropertyEditorPanel.Update();
        }



        public void ForceScrollPropertyEditorRows(int Incrementation)
        {
            foreach (Control button in PropertyEditorPanel.Controls)
            {
                button.Location = new Point(button.Location.X, button.Location.Y - (PropertiesEditorScrollBar.Value - (PropertiesEditorScrollBar.Value += Incrementation)));
            }
            PropertyEditorPanel.Update();
        }
        #endregion propertiesEditor-related function declarations
    }
}
