using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;


namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //=====================================================================================\\
        //--|   Threading-Related Variable Declarations (threads, delegates, and mammets)   |--\\
        //=====================================================================================\\
        #region [Threading-Related Variables]
        
        private static Thread DCFileHandlerThread;

        
        public delegate void binThreadFormWand(bool arg); //! god I need to read about delegates lmao

        
        private delegate void binThreadLabelWand(string details);


        private delegate void generalBinThreadWand();



        /// <summary>
        /// Delegate for handling the editing of values upon clicking their corresponding PropertyEditor row
        /// </summary>
        /// <param name="MemberValue"></param>
        /// <param name="MemberName"></param>
        public delegate void PropertyPanelEventHandler(object MemberValue, string MemberName);


        /// <summary>
        /// Delegate for handling the creation hex editor window for advanced editing of structures.
        /// </summary>
        /// <param name="Struct"> The struct to load the raw data of in to the hex editor. </param>
        public delegate void HexEditorSomethingSomething(object Struct, string StructName);




        public readonly PropertyPanelEventHandler setupPropertyListPopulation;

        public readonly PropertyPanelEventHandler spawnVariableEditorBox;

        public readonly HexEditorSomethingSomething editStructureInHexEditor;

        
        private readonly binThreadLabelWand selectionLabelMammet;

        private readonly generalBinThreadWand selectionLabelResetMammet;

        private readonly binThreadLabelWand LogUpdateMammet;
            
        private readonly binThreadLabelWand LogSameLineMammet;

        
        private readonly binThreadFormWand setReloadCloseButtonStatus;
        
        private readonly generalBinThreadWand CloseBinFileMammet;
        #endregion












        //===================================================\\
        //--|   Cross-Thread-Safe Function Declarations   |--\\
        //===================================================\\
        #region [Cross-Thread-Safe Function Declarations]

        /// <summary>
        /// I plan to do more here. Not 100 on it yet though.
        /// </summary>
        public static void CTLoadProvidedDCFile()
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
            if (Venat == null)
            {
                echo($"{nameof(setReloadCloseButtonStatus)} Invocation attempted while {nameof(Venat)} was still null.");
                return;
            }

            Venat?.Invoke(Venat.setReloadCloseButtonStatus, new object[] { enabled });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcFileName"></param>
        /// <param name="dcEntries"></param>
        public static void PopulatePropertiesPanelWithHeaderItemContents(string dcFileName, DCModule dcEntries)
        {
            if (Venat == null)
            {
                echo($"{nameof(setupPropertyListPopulation)} Invocation attempted while {nameof(Venat)} was still null.");
                return;
            }

            Venat.Invoke(Venat.setupPropertyListPopulation, new object [] { dcEntries, dcFileName });
        }






        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="details">
        /// A string[3] containing the details for the slection label.
        /// <br/> 
        public static void CTUpdateSelectionLabel(string details)
        {
            if (Venat == null)
            {
                echo($"{nameof(selectionLabelMammet)} Invocation attempted while {nameof(Venat)} was still null.");
                return;
            }

            Venat.Invoke(Venat.selectionLabelMammet, new[] { details });
        }


        public static void CTResetSelectionLabel()
        {
            if (Venat == null)
            {
                echo($"{nameof(selectionLabelResetMammet)} Invocation attempted while {nameof(Venat)} was still null.");
                return;
            }

            Venat.Invoke(Venat.selectionLabelResetMammet);
        }






        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="details">
        /// A string[3] containing the details for the slection label.
        /// <br/> 
        public static void CTLog(string message = "")
        {
            if (Venat == null)
            {
                echo($"{nameof(LogUpdateMammet)} Invocation attempted while {nameof(Venat)} was still null.");
                return;
            }

            Venat?.Invoke(Venat.LogUpdateMammet, new[] { message });
        }


        public static void CT_Log(string message = "")
        {
            if (Venat == null)
            {
                echo($"{nameof(LogSameLineMammet)} Invocation attempted while {nameof(Venat)} was still null.");
                return;
            }

            Venat.Invoke(Venat.LogSameLineMammet, message);
        }

        public static void _CTLog(string message = "", int line = -1) => CT_Log(message);

        #endregion [mammet shorthand functions]
    }
}
