using System;
using System.Collections.Generic;
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
        #endregion



        
        //=================================\\
        //--|   Function Delcarations   |--\\
        //=================================\\
        #region [Function Delcarations]

        private Button PopulatePropertiesPanel(string headerEntryName)
        {
            return new Button()
            {
                Text = headerEntryName
            };
        }
        #endregion
    }
}
