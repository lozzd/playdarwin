using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Principal;
using System.Diagnostics;

namespace PlaydarWin
{
    public partial class frmMain : Form
    {
        private int NumSearches = 0;
        private int Uptime = 0;
        
        public frmMain()
        {
            InitializeComponent();
            this.richTextBox1.Text = "Welcome to Playdar for Windows v0.1." + Environment.NewLine;
            this.richTextBox1.AppendText("If you wish to add files to your library, click Scan Files, otherwise, click Start." + Environment.NewLine);
           
        }

        private ProcessCaller processCaller;

        private void btnOk_Click(object sender, System.EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {

        }


        /// <summary>
        /// Handles the events of StdErrReceived and StdOutReceived.
        /// </summary>
        /// <remarks>
        /// If stderr were handled in a separate function, it could possibly
        /// be displayed in red in the richText box, but that is beyond 
        /// the scope of this demo.
        /// </remarks>
        private void writeStreamInfo(object sender, DataReceivedEventArgs e)
        {
            Font fbold = new Font("Lucida Console", 8);
            richTextBox1.Font = fbold;
            
                if (e.Text.Contains("REJECTED")) {
                    this.richTextBox1.SelectionColor = Color.Red;
                    this.richTextBox1.SelectedText = e.Text + Environment.NewLine;
                }
                if (e.Text.Contains("No matches for:"))
                {
                    this.richTextBox1.SelectionColor = Color.Red;
                    this.richTextBox1.SelectedText = e.Text + Environment.NewLine;
                }
                else if (e.Text.Contains("ACCEPTED")) {
                    this.richTextBox1.SelectionColor = Color.Green;
                    this.richTextBox1.SelectedText = e.Text + Environment.NewLine;
                }
                else if (e.Text.Contains("RESOLVER add_results"))
                {
                    this.richTextBox1.SelectionColor = Color.Green;
                    this.richTextBox1.SelectedText = e.Text + Environment.NewLine;
                }
                else if (e.Text.Contains("LAN_UDP responding for "))
                {
                    this.richTextBox1.SelectionColor = Color.Green;
                    this.richTextBox1.SelectedText = e.Text + Environment.NewLine;
                }
                else if (e.Text.Contains("/api/?method=resolve")) {
                    this.richTextBox1.SelectionColor = Color.Blue;
                    this.richTextBox1.SelectedText = e.Text + Environment.NewLine;
                    notifyIcon1.ShowBalloonTip(1000, "Playdar search", "Search initiated!", ToolTipIcon.Info);
                }
                else if (e.Text.Contains("HTTP GET")) {
                    this.richTextBox1.SelectionColor = Color.Gray;
                    this.richTextBox1.SelectedText = e.Text + Environment.NewLine;        
                }
                else if (e.Text.StartsWith("INFO ")) {
                    this.richTextBox1.SelectionColor = Color.IndianRed;
                    this.richTextBox1.SelectedText = e.Text.ToString().Substring(5) + Environment.NewLine;
                    notifyIcon1.ShowBalloonTip(1000, "Playdar Info", e.Text.ToString().Substring(5), ToolTipIcon.Info);
                }
                else if (e.Text.StartsWith("RESOLVER: dispatch"))
                {
                    this.richTextBox1.SelectionColor = Color.RoyalBlue;
                    this.richTextBox1.SelectedText = e.Text + Environment.NewLine;
                    NumSearches++;
                    lblSearches.Text = NumSearches.ToString();
                }

                else
                {
                    this.richTextBox1.AppendText(e.Text + Environment.NewLine);
                }
        }

        /// <summary>
        /// Handles the events of processCompleted & processCanceled
        /// </summary>
        private void processCompletedOrCanceled(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            this.btnOk.Enabled = true;
            notifyIcon1.ShowBalloonTip(5000, "Playdar Server Quit", "Playdar has ended unexpectedly, or shutdown. Searching no longer available. ", ToolTipIcon.Error);
            tmrUptime.Enabled = false;
        }

        private void btnOk_Click_1(object sender, EventArgs e)
        {
            if (!File.Exists(Environment.GetEnvironmentVariable("ProgramFiles") + @"\Playdar\playdar.exe"))
            {
                MessageBox.Show("The Playdar executable could not be found. "+ Environment.NewLine + "I am currently looking in: "+ Environment.NewLine + Environment.GetEnvironmentVariable("ProgramFiles") + @"\Playdar\playdar.exe"+ Environment.NewLine + Environment.NewLine + "Please ensure the executable is there.","Playdar Critical Error",MessageBoxButtons.OK,MessageBoxIcon.Stop);
                Close();
            }
            this.Cursor = Cursors.AppStarting;
            this.btnOk.Enabled = false;

            processCaller = new ProcessCaller(this);
            processCaller.WorkingDirectory = Environment.GetEnvironmentVariable("ProgramFiles") + @"\Playdar\";
            processCaller.FileName = Environment.GetEnvironmentVariable("ProgramFiles") + @"\Playdar\playdar.exe";
            processCaller.Arguments = "-c playdar.conf";
            processCaller.StdErrReceived += new DataReceivedHandler(writeStreamInfo);
            processCaller.StdOutReceived += new DataReceivedHandler(writeStreamInfo);
            processCaller.Completed += new EventHandler(processCompletedOrCanceled);
            processCaller.Cancelled += new EventHandler(processCompletedOrCanceled);
            // processCaller.Failed += no event handler for this one, yet.

            this.richTextBox1.Text = "Starting Playdar..." + Environment.NewLine;


            notifyIcon1.ShowBalloonTip(5000, "Playdar Start", "Playdar Server is now running and ready to serve search requests.", ToolTipIcon.Info);
            this.Hide();
            // the following function starts a process and returns immediately,
            // thus allowing the form to stay responsive.
            this.Cursor = Cursors.Default;
            Uptime = 0;
            tmrUptime.Enabled = true;
            processCaller.Start();
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            if (processCaller != null)
            {
                processCaller.Cancel();
            }
            richTextBox1.AppendText("Playdar has been shut down." + Environment.NewLine);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void frmMain_Resize(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
            }
        }

        private void frmMain_Close(object sender, System.EventArgs e)
        {
            // TODO: we should probably kill playdar here, 
            // so er, this might work. let's try. 
            Process[] myProcesses;
            myProcesses = Process.GetProcessesByName("playdar");
            myProcesses[0].Kill();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            // Eventually we'll do something nice with the GUI again but 
            // I'm not sure what. So for now, lets just launch notepad. Leet.

            System.Diagnostics.Process.Start(@"notepad.exe", Environment.GetEnvironmentVariable("ProgramFiles") + @"\Playdar\playdar.conf");

            //if (File.Exists(@"C:\Program Files\Playdar\playdarconfig.exe"))
            //{
            //    System.Diagnostics.Process.Start(@"C:\Program Files\Playdar\playdarconfig.exe");
            //}
            //else
            //{
            //    MessageBox.Show("The Playdar Configuration executable could not be found." + Environment.NewLine + "Are you sure it is in the Playdar folder?", "Playdar - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmScan frmScan = new frmScan();
            frmScan.ShowDialog();
        }

        private void tmrUptime_Tick(object sender, EventArgs e)
        {
            Uptime = Uptime + 1;
            lblUptime.Text = TimeSpan.FromSeconds(Uptime).ToString() ;
        }
    }
}