using System;
using System.IO;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;


namespace NaughtyDogDCReader
{
    public partial class OptionsPage : Form
    {
        public OptionsPage() {
            InitializeComponent();
            InitializeAdditionalEventHandlers_OptionsPage(); // Set Event Handlers and Other Form-Related Crap
            

            SidbasePathTextBox.TextChanged += (sender, _) =>
            {
                if (File.Exists(((TextBox) sender).Text))
                {
                    Main.SIDBase = new SIDBase(((TextBox) sender).Text);
                }
            };

            LoadOptions();
        }

        
        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void ShowUnresolvedSIDsCheckBox_CheckedChanged(object sender, EventArgs e) => ShowUnresolvedSIDs = ((CheckBox) sender).Checked;


        
        /// <summary>
        /// 
        /// </summary>
        private void BrowseForSIDBase(object sender, EventArgs e)
        {
            using (var fileBrowser = new OpenFileDialog
            {
                Title = "Select the desired sidbase.bin to use.",
                Filter = "String ID Lookup Table|*.bin"
            })
            {
                if (fileBrowser.ShowDialog() == DialogResult.OK)
                {
                    SidbasePathTextBox.Set(fileBrowser.FileName);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        private void BrowseForDCFile(object sender, EventArgs e)
        {
            using (var fileBrowser = new OpenFileDialog
            {
                Title = "Select the desired DC file to read/edit.",
                Filter = "Naughty Dog DC Script File|*.bin"
            })
            {
                if (fileBrowser.ShowDialog() == DialogResult.OK)
                {
                    DCFilePathTextBox.Set(fileBrowser.FileName);
                }
            }
        }




        /// <summary>
        /// Check for new app version by comparing newest tag to version text
        /// </summary>
        private async void VersionCheckBtn_Click(object sender, EventArgs e)
        {
            try {
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

                            if (tag != Main.Version) {
                                string[]
                                    checkedVersion = tag.Split('.'),
                                    currentVersion = Main.Version.Split('.')
                                ;
                            
                                if (checkedVersion.Length != currentVersion.Length)
                                {
                                    if (checkedVersion.Length < currentVersion.Length) {
                                        echo("Application Up-to-Date");
                                    }
                                    else
                                        echo($@"New Version Available.\nLink: https://github.com/TheMagicalBlob/{nameof(NaughtyDogDCReader)}/releases");
                                    return;
                                }

                                for (var i = 0; i < currentVersion.Length; ++i)
                                {
                                    var currnum = currentVersion[i];
                                    var newnum = checkedVersion[i];

                                    if (int.Parse(currnum) < int.Parse(newnum)) {
                                        echo($"New Version Available. (//! print link or prompt to open in browser)");
                                        return;
                                    }
                                }
                            
                                echo("Application Up-to-Date");
                            }
                        }
                        else
                            echo($"Error checking for newest tag (Status: {reply.StatusCode})");
                    }
                }
            }
            catch (Exception dang) {
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
            else Azem.BringToFront();
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
                        SidbasePathTextBox.Set(ShitBrowser.SelectedPath);
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
                    SidbasePathTextBox.Set(CrapBrowser.FileName.Remove(CrapBrowser.FileName.LastIndexOf('\\')));
                }
            }
        }
        #endregion
        
        //=====================================\\
        //--|   Options-Related Functions   |--\\
        //=====================================\\
        #region [Options-Related Functions]
        
        /// <summary>
        /// Create and subscribe to various event handlers for additional form functionality. (fck your properties panel's event handler window, let me write code)
        /// </summary>
        public void InitializeAdditionalEventHandlers_OptionsPage()
        {
            // Anonomously Create and Set CloseBtn Event Handler
            CloseBtn.Click += new EventHandler((sender, e) =>
            {
                // Hide OptionsPage Form
                Azem.Visible = false;    
                SaveOptions();
            });


            // Set Event Handlers for Form Dragging
            MouseDown += new MouseEventHandler((sender, e) =>
            {
                MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                MouseIsDown = true;

                //Venat.DropdownMenu[1].Visible = Venat.DropdownMenu[0].Visible = false;

            });
            MouseUp += new MouseEventHandler((sender, e) => 
                MouseIsDown = false
            );
            MouseMove += new MouseEventHandler((sender, e) => MoveForm());

            
            foreach(Control item in Controls)
            {
                item.MouseDown += new MouseEventHandler((sender, e) =>
                {
                    MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                    MouseIsDown = true;

                    //Venat.DropdownMenu[1].Visible = Venat.DropdownMenu[0].Visible = false;
                });
                item.MouseUp   += new MouseEventHandler((sender, e) => 
                    MouseIsDown = false
                );
                
                // Avoid Applying MoveForm EventHandler to Text Containters (to retain the ability to drag-select text)
                if (item.GetType() != typeof(TextBox) && item.GetType() != typeof(RichTextBox))
                {
                    item.MouseMove += new MouseEventHandler((sender, e) => MoveForm());
                }
            }

            //Paint += (azem, yoshiP) => DrawFormDecorations(((Form)azem), yoshiP);
        }


        /// <summary>
        /// Mirror Any Non-Default Options to local dc.blb file.
        /// </summary>
        public void SaveOptions()
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
