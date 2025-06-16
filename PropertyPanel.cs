using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static weapon_data.Main;

namespace weapon_data
{
    public partial class PropertyPanel : Form
    {
        public PropertyPanel(DCFileHeader header)
        {
            InitializeComponent();

            Title.Text = Title.Text.Replace("%s", ActiveFileName);
            HeaderButtons = new List<Button>();

            SuspendLayout();
            foreach (var item in header.HeaderItems)
            {
                HeaderButtons.Add(CreateHeaderButtoon(item.Name));
                Controls.Add(HeaderButtons.Last());
            }
            ResumeLayout(true);
        }
        


        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        private List<Button> HeaderButtons;
        private List<Button> StructButtons;
        #endregion




        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void CloseBtn_Click(object sender, EventArgs e) => Dispose(true);
        #endregion



        
        //=================================\\
        //--|   Function Delcarations   |--\\
        //=================================\\
        #region [Function Delcarations]

        private Button CreateHeaderButtoon(string headerEntryName)
        {
            return new Button()
            {
                Text = headerEntryName
            };
        }
        #endregion
    }
}
