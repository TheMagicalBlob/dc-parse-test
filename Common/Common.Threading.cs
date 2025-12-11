using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Security.Cryptography;


namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //#
        //## Threading-Related Variables (threads, delegates, and mammets)
        //#
        #region [Threading-Related Variables]
        private static Thread DCFileHandlerThread;

        /// <summary> Cross-thread form interaction delegate. </summary>
        public delegate void binThreadFormWand(bool args); //! god I need to read about delegates lmao

        /// <summary> //! </summary>
        private delegate void binThreadLabelWand(string[] details);
        private delegate void generalBinThreadWand();

        /// <summary> //! </summary>
        public delegate string[] binThreadFormWandOutputRead();




        private readonly binThreadLabelWand statusLabelMammet = new binThreadLabelWand(SetStatusLabelDetails);

        private readonly generalBinThreadWand statusLabelResetMammet = new generalBinThreadWand(ResetStatusLabelDetails);


        private readonly binThreadLabelWand selectionLabelMammet = new binThreadLabelWand(SetSelectionLabelDetails);

        private readonly generalBinThreadWand selectionLabelResetMammet = new generalBinThreadWand(ResetSelectionLabelDetails);




        private readonly binThreadFormWand setReloadCloseButtonsEnabledStatus = new binThreadFormWand((isEnabled) =>
        {
            if (Venat.CloseBtn == null)
            {
                echo($"ERROR: {nameof(Venat.CloseBtn)} was null!");
                return;
            }

            // Enable/Disable the button, and update the button with the strikeout style property
            Venat.CloseBtn.Enabled = isEnabled;
            Venat.CloseBtn.Font = new Font(MainFont.FontFamily, MainFont.Size, MainFont.Style | (isEnabled ? FontStyle.Regular : FontStyle.Strikeout));


            if (Venat.ReloadScriptBtn == null)
            {
                echo($"ERROR: {nameof(Venat.ReloadScriptBtn)} was null!");
                return;
            }

            Venat.ReloadScriptBtn.Enabled = isEnabled;
            Venat.ReloadScriptBtn.Font = new Font(MainFont.FontFamily, MainFont.Size, MainFont.Style | (isEnabled ? FontStyle.Regular : FontStyle.Strikeout));
        });


        
        private readonly generalBinThreadWand CloseBinFileMammet = new generalBinThreadWand(() =>
        {
            CloseBinFile();
        });
        #endregion
        















        //#
        //## Cross-Thread-Safe Functions
        //#
        #region [Cross-Thread-Safe Functions]

        /// <summary>
        /// I plan to do more here. Not 100 on it yet though.
        /// </summary>
        public static void DCFileHandlerFunction()
        {
            var filePath = ActiveFilePath ?? "C:\\[null path!]";

            #if !DEBUG
            try
            #endif
            {
                //#
                //## Load & Parse provided DC file.
                //#
                LoadProvidedDCFile(filePath);
            }
            #if !DEBUG
            // File in use, probably
            catch (IOException dang)
            {
                echo($"\n{dang.GetType()}: Selected file is either in use, or doesn't exist.\nMessage: [{dang.Message}]");

                CTCloseBinFile();
                UpdateStatusLabel(new[] { "Error loading DC file; file may be in use, or simply not exist.", emptyStr, emptyStr });
            }
            // File in use, probably
            catch (Exception nani)
            {
                echo($"\nERROR: Selected file is either in use, or doesn't exist.\nMessage: [{nani.Message}]");

                CTCloseBinFile();
                UpdateStatusLabel(new[] { "Error loading DC file; file may be in use, or simply not exist.", emptyStr, emptyStr });
            }
            #endif
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetReloadCloseButtonsEnabledStatus(bool enabled)
        {
            Venat?.Invoke(Venat.setReloadCloseButtonsEnabledStatus, new object[] { enabled });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcFileName"></param>
        /// <param name="dcEntries"></param>
        public static void PopulatePropertiesPanel(object dcFileName, DCScript dcEntries)
        {
            Venat?.Invoke(Panels.populatePropertiesPanelWithHeaderItemContents, new object[] { dcFileName, dcEntries });
        }


        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="details">
        /// A string[3] containing the details for the status label.
        /// <br/> 
        /// </param>
        public static void UpdateStatusLabel(string[] details)
        {
            Venat?.Invoke(Venat.statusLabelMammet, new object[] { details });
        }

        public static void ResetStatusLabel()
        {
            Venat?.Invoke(Venat.statusLabelResetMammet);
        }


        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="details">
        /// A string[3] containing the details for the slection label.
        /// <br/> 
        public static void UpdateSelectionLabel(string[] details)
        {
            Venat?.Invoke(Venat.selectionLabelMammet, new[] { details });
        }

        
        public static void ResetSelectionLabel()
        {
            Venat?.Invoke(Venat.selectionLabelResetMammet);
        }
        #endregion [mammet shorthand functions]
    }
}
