using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;

namespace weapon_data
{
    public partial class Main
    {
        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        private List<Button> HeaderButtons;
        private List<Button> StructButtons;

        private Button Selection
        {
            get => _selection;

            set {
                if (value != null)
                    DisplayHItemContents(value.TabIndex);
                _selection = value;
            }
        }

        private Button _selection;

        public delegate void PropertiesPanelWand(string dcFileName, object[] dcEntries);

        public PropertiesPanelWand propertiesPanelMammet;
        #endregion



        
        //=================================\\
        //--|   Function Delcarations   |--\\
        //=================================\\
        #region [Function Delcarations]

        private void DisplayHItemContents(int itemIndex)
        {

        }


        private void PopulatePropertiesPanel(string dcFileName, object[] dcEntries)
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
                

                                
                btn.MouseDown += new MouseEventHandler((sender, e) =>
                {
                    MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                    MouseIsDown = true;
                });
                btn.MouseUp   += new MouseEventHandler((sender, e) =>
                {
                    MouseIsDown = false;
                    if (OptionsPageIsOpen) {
                        Azem.BringToFront();
                    }
                });
                
                btn.MouseMove += new MouseEventHandler((sender, e) => MoveForm());


                return btn;
            }

            void highlightHeaderButton(Button sender)
            {
                if (Selection != null)
                {
                    Selection.Font = new Font(Selection.Font.FontFamily, Selection.Font.Size, (FontStyle) 0);
                }
                    
                (Selection = sender)
                .Font = new Font(Selection.Font.FontFamily, Selection.Font.Size, Selection.Font.Style ^ FontStyle.Underline);
            }

            Button currentButton;
            HeaderButtons = new List<Button>(dcEntries.Length);

            var dcLen = dcEntries.Length;
            var tabIndexBase = optionsMenuDropdownBtn.TabIndex - 1;
            char[] name = null;

            for (int i = 0; i < dcLen; i++, name = null)
            {
                var dcEntry = dcEntries[i];
                currentButton = newButton();
                PropertiesPanel.Controls.Add(currentButton);
                
                currentButton.Location = new Point(1, 7 + currentButton.Height * i);



                name = (((dynamic)dcEntry).Name as string).ToArray();
                var tmp = 0;

                for(;tmp < name.Length && name[tmp] < 123 && name[tmp] > 96; ++tmp);

                name[tmp + 1] = $"{name[tmp + 1]}".ToUpper()[0];


                for (var j = tmp + 1; j < name.Length;)
                {
                    if (name[j] == '-')
                    {
                        while (name[++j] == '-' && j < name.Length);
                            
                        name[j - 1] = ' ';
                        name[j] = $"{name[j]}".ToUpper()[0];
                        //  eat--my-ass
                        //  go---bite-a-dick
                    }

                    j++;
                }
                currentButton.Text = new string(name);

                currentButton.Name = ((dynamic)dcEntry).Name;

                currentButton.FlatAppearance.BorderSize = 0;
                currentButton.Width = currentButton.Parent.Width - 2;

                currentButton.TabIndex = tabIndexBase + i;
                currentButton.GotFocus += (button, _) => highlightHeaderButton(button as Button);
                currentButton.Click += (button, _) => highlightHeaderButton(button as Button);

                currentButton.KeyDown += (sender, arg) =>
                {
                    if (arg.KeyData == Keys.Down)
                    {
                        if ((int)currentButton.Tag == HeaderButtons.Count - 1)
                        {
                            HeaderButtons[0].Focus();
                        }
                        else {
                            HeaderButtons[(int)currentButton.Tag + 1].Focus();
                        }
                    }
                    else if (arg.KeyData == Keys.Up)
                    {
                        if ((int)currentButton.Tag == 0)
                        {
                            HeaderButtons[HeaderButtons.Count - 1].Focus();
                        }
                        else {
                            HeaderButtons[(int)currentButton.Tag - 1].Focus();
                        }
                    }
                };

                HeaderButtons.Add(currentButton);
            }

            optionsMenuDropdownBtn.TabIndex += dcLen;
            MinimizeBtn.TabIndex += dcLen;
            ExitBtn.TabIndex += dcLen;
        }
        #endregion
    }
}
