using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using CefSharp;
using CefSharp.WinForms;
using System.Threading;

namespace InstaRefollow
{
    public partial class Form1 : Form
    {
        public ChromiumWebBrowser chromeBrowser;

        Thread thread;

        // Form reference for Anti-minimize.
        static Form myForm;

        // WindowState for Anti-minimize.
        static FormWindowState preWindowState;

        // Delegae for Anti-minimize.
        public delegate void UpWindowDelegate();
        
        // Anti-minimize. (Because CefSharp doesn't work in minimize-window.)
        private void UpWindow()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpWindowDelegate(this.UpWindow));
                return;
            }

            if (this.WindowState == FormWindowState.Minimized)
            {
                myForm.WindowState = preWindowState;

                int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

                this.SetBounds(0, height - 10, 0, 0, BoundsSpecified.Y);
            }

        }

        // Delegate for setText()
        public delegate void setTextDelegate();

        // Message for setText()
        string message = "";

        // Set text.
        private void setText()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new setTextDelegate(this.setText));
                return;
            }

            this.Text = "AutoFav3 - " + message;

        }



        public Form1()
        {
            InitializeComponent();

            // Start the browser after initialize global component
            InitializeChromium();

            // Remember current WindowState.
            myForm = this;
            preWindowState = this.WindowState;


            try
            {
                
                thread = new Thread(new ThreadStart(() =>
                {
                    


                    // Load keyword to search.
                    StreamReader sr = new StreamReader("keyword.ini", Encoding.GetEncoding("Shift_JIS"));
                    string keyword = sr.ReadToEnd();
                    sr.Close();

                    // Load interval seconds.
                    StreamReader sre = new StreamReader("interval.ini", Encoding.GetEncoding("Shift_JIS"));
                    int intBase = int.Parse(sre.ReadToEnd());
                    sre.Close();
                    if(intBase > 10)
                    {
                        // Decliment offset.
                        intBase -= 10;
                    }

                    // Load limitter.
                    StreamReader srl = new StreamReader("limitter.ini", Encoding.GetEncoding("Shift_JIS"));
                    int limitter = int.Parse(srl.ReadToEnd());
                    srl.Close();


                    // Wait until login.
                    Thread.Sleep(60000);
                    //Thread.Sleep(3000);
                    


                    
                    // Sring of JavaScript code.
                    string jsScript;

                    for (int count = 0; count < limitter; count++)
                    {
                        // Reload webpage.
                        UpWindow();
                        Thread.Sleep(1000);
                        chromeBrowser.Load("https://twitter.com/search?q=%23" + keyword + "&src=trend_click&f=live");

                        

                        // Wait until inerval + random.
                        Random rnd = new System.Random();
                        int interval = (intBase + rnd.Next(0, 20)) * 1000;
                        Thread.Sleep(interval);


                        // Click "Like" button of the latest tweet.
                        UpWindow();
                        jsScript =  "var buttons = document.getElementsByClassName('css-1dbjc4n r-xoduu5'); " +
                                    "for (var i = 0; i < buttons.length; i++) {" +
                                    "   childList = buttons[i].childNodes; " +
                                    "   if(childList[1] != null){" +
                                    "       if(childList[1].innerHTML != null){" +
                                    //"           alert(childList[1].innerHTML);" +
                                    "           if(childList[1].innerHTML.indexOf('965z') > 0){" +
                                    "               buttons[i].click();" +
                                    "               break;" +
                                    "           }"+
                                    "       }" +
                                    "   }"+                                    
                                    "}";
                        chromeBrowser.ExecuteScriptAsync(jsScript);                        
                        

                        // Set message.
                        message = "(Working... " + (count+1) + "/"+limitter+")";
                        setText(); 

                    }

                    message = "Finished!";
                    setText();

                    



                }));

                // Run the thread above.
                thread.Start();               


            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() => { MessageBox.Show(this, ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error); }));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chromeBrowser.ShowDevTools();
        }

        // Chromium initializer.
        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            settings.Locale = "ja";
            settings.AcceptLanguageList = "ja-JP";
            settings.CachePath = "cache";
            settings.PersistSessionCookies = true;

            // Initialize cef with the provided settings
            Cef.Initialize(settings);

            //The Global CookieManager has been initialized, you can now set cookies
            var cookieManager = Cef.GetGlobalCookieManager();
            //cookieManager.SetStoragePath("cookies", true);

            // Create a browser component
            chromeBrowser = new ChromiumWebBrowser("https://twitter.com");

            // Add it to the form and fill it to the form window.
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;

            // Allow the use of local resources in the browser
            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            chromeBrowser.BrowserSettings = browserSettings;
            
        }
        



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();

            thread.Abort();
        }
    }
}
