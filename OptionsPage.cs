using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;


namespace NaughtyDogDCReader
{
    public partial class OptionsPage : Form
    {
        public OptionsPage()
        {
            InitializeComponent();
            InitializeAdditionalEventHandlers(this, CloseBtn, new SubformExitFunction((_, __) => { SaveOptions(); Visible = false; }), ref HSeparatorLines, ref VSeparatorLines); // Set Event Handlers and Other Form-Related Crap

            LoadOptions();
        }


        
        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        public Point[][] HSeparatorLines;

        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        public Point[][] VSeparatorLines;
        #endregion (variable declarations)



        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void ShowUnresolvedSIDsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowUnresolvedSIDs = ((CheckBox) sender).Checked;
        }



        /// <summary>
        /// Check for new app version by comparing newest tag to version text
        /// </summary>
        private async void VersionCheckBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (var clientHandler = new HttpClientHandler())
                {
                    clientHandler.UseDefaultCredentials = true;
                    clientHandler.UseProxy = false;

                    using (var client = new HttpClient(clientHandler))
                    {
                        HttpResponseMessage reply;
                        client.DefaultRequestHeaders.Add("User-Agent", "Other"); // Set request headers to avoid error 403

                        if ((reply = await client.GetAsync($"https://api.github.com/repos/TheMagicalBlob/{nameof(NaughtyDogDCReader)}/tags")).IsSuccessStatusCode)
                        {
                            var message = reply.Content.ReadAsStringAsync().Result;
                            var tag = message.Remove(message.IndexOf(',') - 1).Substring(message.IndexOf(':') + 2);
#if DEBUG
                            echo($"Newest Tag: [{tag}]");
#endif

                            if (tag != Main.Version)
                            {
                                string[]
                                    checkedVersion = tag.Split('.'),
                                    currentVersion = Main.Version.Split('.')
                                ;

                                if (checkedVersion.Length != currentVersion.Length)
                                {
                                    if (checkedVersion.Length < currentVersion.Length)
                                    {
                                        echo("Application Up-to-Date");
                                    }
                                    else
                                    {
                                        echo($@"New Version Available.\nLink: https://github.com/TheMagicalBlob/{nameof(NaughtyDogDCReader)}/releases");
                                    }

                                    return;
                                }

                                for (var i = 0; i < currentVersion.Length; ++i)
                                {
                                    var currnum = currentVersion[i];
                                    var newnum = checkedVersion[i];

                                    if (int.Parse(currnum) < int.Parse(newnum))
                                    {
                                        echo($"New Version Available. (//! print link or prompt to open in browser)");
                                        return;
                                    }
                                }

                                echo("Application Up-to-Date");
                            }
                        }
                        else
                        {
                            echo($"Error checking for newest tag (Status: {reply.StatusCode})");
                        }
                    }
                }
            }
            catch (Exception dang)
            {
                echo($"Unable to connect to api.github ({dang.GetType()})");
            }
        }


        // Prompt user to open their default browser and download the latest source code
        private void DownloadSourceBtn_Click(object sender, EventArgs e)
        {
            echo($"Download Latest Source: https://github.com/TheMagicalBlob/{nameof(NaughtyDogDCReader)}/archive/refs/heads/master.zip\nNo guarantees on stability; I just use the main branch for everything.");

            if (MessageBox.Show(
                    "Download the latest source code through this system's default browser?\n\n(Download Will Start Automatically)",
                    "Press \"Ok\" to open in a browser, or copy the link from the Output Window.",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                )
                == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start($"https://github.com/TheMagicalBlob/{nameof(NaughtyDogDCReader)}/archive/refs/heads/master.zip");
            }
            else
            {
                Azem.BringToFront();
            }
        }



        private void UNUSEDBrowseBtn_Click(object sender, EventArgs e)
        {
            // Use the ghastly Directory Tree Dialogue to Choose A Folder
            if (LegacyFolderSelectionDialogue)
            {
                using (var ShitBrowser = new FolderBrowserDialog())
                {
                    if (ShitBrowser.ShowDialog() == DialogResult.OK)
                    {
                        SidbasePathTextBox.Text = ShitBrowser.SelectedPath;
                    }
                }
            }
            // Use The Newer "Hackey" Method
            else {
                var CrapBrowser = new OpenFileDialog()
                {
                    ValidateNames = false,
                    CheckPathExists = false,
                    CheckFileExists = false,
                    Title = "Do not highlight any files, press open once inside.",
                    FileName = "Press 'Open' Once Inside The Desired Folder.",
                    Filter = "Folder Selection|*."
                };

                if (CrapBrowser.ShowDialog() == DialogResult.OK)
                {
                    SidbasePathTextBox.Text = CrapBrowser.FileName.Remove(CrapBrowser.FileName.LastIndexOf('\\'));
                }
            }
        }
        #endregion




        //=====================================\\
        //--|   Options-Related Functions   |--\\
        //=====================================\\
        #region [Options-Related Functions]


        /// <summary>
        /// Mirror Any Non-Default Options to local dc.blb file.
        /// </summary>
        private void SaveOptions()
        {
            using (var settings = File.Open($"{Directory.GetCurrentDirectory()}\\dc.blb", FileMode.OpenOrCreate, FileAccess.Write))
            {

            }
        }



        /// <summary>
        /// Load any saved options from the local dc.blb file.
        /// </summary>
        private void LoadOptions()
        {
            if (File.Exists($@"{Directory.GetCurrentDirectory()}\dc.blb"))
            {
                using (var settings = File.Open($@"{Directory.GetCurrentDirectory()}\dc.blb", FileMode.OpenOrCreate, FileAccess.Read))
                {

                }
            }
        }
        #endregion
    }
}
