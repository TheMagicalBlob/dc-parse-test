using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;
using System.ComponentModel;
using static weapon_data.Main;


namespace weapon_data
{
    public partial class OptionsPage : Form
    {
        public OptionsPage() {
            InitializeComponent();
            InitializeAdditionalEventHandlers(); // Set Event Handlers and Other Form-Related Crap
            

            sidbasePathTextBox.TextChanged += (sender, _) =>
            {
                if (File.Exists(((TextBox)sender).Text))
                {
                    SIDBase = File.ReadAllBytes(((TextBox)sender).Text);
                }
            };

            LoadOptions();
        }

        
        //=====================================\\
        //--|   Options-Related Functions   |--\\
        //=====================================\\
        #region [Options-Related Functions]
        
        /// <summary>
        /// Create and subscribe to various event handlers for additional form functionality. (fck your properties panel's event handler window, let me write code)
        /// </summary>
        public void InitializeAdditionalEventHandlers()
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



        // Check for new app version by comparing newest tag to version text
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
                   
                        if ((reply = await client.GetAsync($"https://api.github.com/repos/TheMagicalBlob/{nameof(weapon_data)}/tags")).IsSuccessStatusCode)
                        {
                            var message = reply.Content.ReadAsStringAsync().Result;
                            var tag = message.Remove(message.IndexOf(',') - 1).Substring(message.IndexOf(':') + 2);
    #if DEBUG
                            PrintNL($"Newest Tag: [{tag}]");
    #endif

                            if (tag != Main.Version) {
                                string[]
                                    checkedVersion = tag.Split('.'),
                                    currentVersion = Main.Version.Split('.')
                                ;
                            
                                if (checkedVersion.Length != currentVersion.Length)
                                {
                                    if (checkedVersion.Length < currentVersion.Length) {
                                        PrintNL("Application Up-to-Date");
                                    }
                                    else
                                        PrintNL($@"New Version Available.\nLink: https://github.com/TheMagicalBlob/{nameof(weapon_data)}/releases");
                                    return;
                                }

                                for (var i = 0; i < currentVersion.Length; ++i)
                                {
                                    var currnum = currentVersion[i];
                                    var newnum = checkedVersion[i];

                                    if (int.Parse(currnum) < int.Parse(newnum)) {
                                        PrintNL($"New Version Available. (//! print link or prompt to open in browser)");
                                        return;
                                    }
                                }
                            
                                PrintNL("Application Up-to-Date");
                            }
                        }
                        else
                            PrintNL($"Error checking for newest tag (Status: {reply.StatusCode})");
                    }
                }
            }
            catch (Exception dang) {
                PrintNL($"Unable to connect to api.github");
            }
        }


        // Prompt user to open their default browser and download the latest source code
        private void DownloadSourceBtn_Click(object sender, EventArgs e)
        {
            PrintNL($"Download Latest Source: https://github.com/TheMagicalBlob/{nameof(weapon_data)}/archive/refs/heads/master.zip\nNo guarantees on stability; I just use the main branch for everything.");
            
            if (MessageBox.Show(
                    "Download the latest source code through this system's default browser?\n\n(Download Will Start Automatically)",
                    "Press \"Ok\" to open in a browser, or copy the link from the Output Window.",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) 
                == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start($"https://github.com/TheMagicalBlob/{nameof(weapon_data)}/archive/refs/heads/master.zip");
            }
            else Azem.BringToFront();
        }


        // Choose a .gp4 Output Path Through Either a FolderBrowserDialogue, or OpenFileDialogue Instance (W/ the hackey Dummy File Method.
        private void UNUSEDBrowseBtn_Click(object sender, EventArgs e)
        {
            // Use the ghastly Directory Tree Dialogue to Choose A Folder
            if (LegacyFolderSelectionDialogue)
            {
                using (var ShitBrowser = new FolderBrowserDialog())
                {
                    if (ShitBrowser.ShowDialog() == DialogResult.OK)
                    {
                        sidbasePathTextBox.Set(ShitBrowser.SelectedPath);
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
                    sidbasePathTextBox.Set(CrapBrowser.FileName.Remove(CrapBrowser.FileName.LastIndexOf('\\')));
                }
            }
        }


        // Search for the Base Application Package Through an OpenFileDialogue Instance.
        private void BasePackagePathBrowseBtn_Click(object sender, EventArgs e)
        {
            using (var Browser = new OpenFileDialog
            {
                Title = "Please Select the Base-Game Package You're Creating a Patch Package for." })
            {
                if (Browser.ShowDialog() == DialogResult.OK)
                {
                    sidbasePathTextBox.Set(Browser.FileName);
                }
            }
        }
        #endregion
    }
}
