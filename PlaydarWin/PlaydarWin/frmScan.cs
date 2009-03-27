using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PlaydarWin
{
    public partial class frmScan : Form
    {
        public frmScan()
        {
            InitializeComponent();

        }

        private ProcessCaller processCaller;

        private void writeStreamInfo(object sender, DataReceivedEventArgs e)
        {
                this.richTextBox1.AppendText(e.Text + Environment.NewLine);
        }

        /// <summary>
        /// Handles the events of processCompleted & processCanceled
        /// </summary>
        private void processCompletedOrCanceled(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            frmMain.notifyIcon1.ShowBalloonTip(5000, "Playdar Scanner", "File scanning has completed. You may now launch Playdar.", ToolTipIcon.Info);
            this.richTextBox1.AppendText(Environment.NewLine + "Playdar scanner completed. Review these results if you wish, then click 'Start Playdar'");
        }

        private void frmScan_Load(object sender, EventArgs e)
        {
            this.StartScan();
        }

        private void StartScan()
        {
            folderBrowserDialog1.ShowDialog();

            processCaller = new ProcessCaller(this);
            processCaller.WorkingDirectory = Environment.GetEnvironmentVariable("ProgramFiles") + @"\Playdar\";
            processCaller.FileName = Environment.GetEnvironmentVariable("ProgramFiles") + @"\Playdar\playdar-scanner.exe";
            processCaller.Arguments = "collection.db \"" + folderBrowserDialog1.SelectedPath + "\"";
            processCaller.StdErrReceived += new DataReceivedHandler(writeStreamInfo);
            processCaller.StdOutReceived += new DataReceivedHandler(writeStreamInfo);
            processCaller.Completed += new EventHandler(processCompletedOrCanceled);
            processCaller.Cancelled += new EventHandler(processCompletedOrCanceled);
            // processCaller.Failed += no event handler for this one, yet.

            this.richTextBox1.Text = "Start scanning process in folder " + folderBrowserDialog1.SelectedPath + Environment.NewLine;
            frmMain.notifyIcon1.ShowBalloonTip(5000, "Playdar Scanner", "The Playdar Scanner is now adding your mp3 files the Playdar library.", ToolTipIcon.Info);
            processCaller.Start();
        }

    }
}