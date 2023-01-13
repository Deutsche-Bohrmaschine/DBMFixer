using Steamworks;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DBMFixer
{
    public partial class Form1 : Form
    {
        private string dayZLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DayZ");
        private string dayZInstallPath { get; set; }
        private const long OneKb = 1024;
        private const long OneMb = OneKb * 1024;
        private const long OneGb = OneMb * 1024;
        private const long OneTb = OneGb * 1024;


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Task.Factory.StartNew(() => { StartAction(); });
            }
            catch
            {
                WriteLog("A Error has been occured!");
            }
        }

        private void StartAction()
        {
            if (checkBox1.Checked)
            {
                DeleteAllLogs();
            }

            if (checkBox2.Checked)
            {
                ReinstallBattleye();
            }
        }

        private Task ReinstallBattleye()
        {
            var battleyeDir = Path.Combine(dayZInstallPath, "BattlEye");
            if (Directory.Exists(battleyeDir))
            {
                WriteLog($"found battleye on {battleyeDir}");
                var consolePath = Path.Combine(battleyeDir, "Uninstall_BattlEye.bat");
                var installPathConsole = Path.Combine(battleyeDir, "Install_BattlEye.bat");
                WriteLog($"start uninstall BE!");
                Process.Start(consolePath);
                WriteLog($"BE -> Uninstalled!");
                WriteLog($"Start Install ....");
                Process.Start(installPathConsole);
                WriteLog("BE Reinstalled!");
            }
            return Task.CompletedTask;
        }

        private void WriteLog(string log)
        {

            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(WriteLog), new object[] { log });
                return;
            }


            if (richTextBox1.Text == "")
                richTextBox1.Text = log;
            else
                richTextBox1.Text += Environment.NewLine + log;
        }

        private void InvokeProgressbar(int value, int max)
        {

            if (InvokeRequired)
            {
                this.Invoke(new Action<int, int>(InvokeProgressbar), new object[] { value, max });
                return;
            }


            progressBar1.Maximum = max;
            progressBar1.Value = value;
        }

        private string ToPrettySize(long value, int decimalPlaces = 0)
        {
            var asTb = Math.Round((double)value / OneTb, decimalPlaces);
            var asGb = Math.Round((double)value / OneGb, decimalPlaces);
            var asMb = Math.Round((double)value / OneMb, decimalPlaces);
            var asKb = Math.Round((double)value / OneKb, decimalPlaces);
            string chosenValue = asTb > 1 ? string.Format("{0}Tb", asTb)
                : asGb > 1 ? string.Format("{0}Gb", asGb)
                : asMb > 1 ? string.Format("{0}Mb", asMb)
                : asKb > 1 ? string.Format("{0}Kb", asKb)
                : string.Format("{0}B", Math.Round((double)value, decimalPlaces));
            return chosenValue;
        }

        private Task DeleteAllLogs()
        {
            if (Directory.Exists(dayZLogPath))
            {
                WriteLog($"Successfully found dayz log path on: {dayZLogPath}");
                var files = Directory.GetFiles(dayZLogPath);
                var logCount = 0;
                long takenDiskSize = 0;

                foreach (var file in files)
                {
                    FileInfo info = new FileInfo(file);
                    if(info != null && info.Exists)
                    {
                        if(info.Extension == ".log" || info.Extension == ".RPT" || info.Extension == ".mdmp")
                        {
                            logCount++;
                            takenDiskSize += info.Length;
                        }
                    }
                }
                WriteLog($"Found: {logCount} to delete.");
                WriteLog($"Takes: {ToPrettySize(takenDiskSize)} disk size.");

                WriteLog("Starting to delete files.");
                InvokeProgressbar(0, logCount);
                int counter = 0;
                foreach (var file in files)
                {
                    FileInfo info = new FileInfo(file);
                    if (info != null && info.Exists)
                    {
                        if (info.Extension == ".log" || info.Extension == ".RPT" || info.Extension == ".mdmp")
                        {
                            info.Delete();
                            counter++;
                            InvokeProgressbar(counter, logCount);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Steamworks.SteamClient.Init(221100, true);
                label3.Text = "Alive and Running!";
                dayZInstallPath = Steamworks.SteamApps.AppInstallDir();
                if (Steamworks.SteamApps.IsAppInstalled(221100))
                {
                    label5.Text = "Yes";
                    MessageBox.Show(dayZInstallPath);
                }
                else
                {
                    label5.Text = "No";
                }


                label7.Text = SteamClient.Name;
            }
            catch (Exception exception)
            {
                // Something went wrong! Steam is closed?
                MessageBox.Show($"{exception.Message} \nDBM Fixer will exit now!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label3.Text = "Dead!";
                label5.Text = "No";
                label7.Text = "Unkown";
                Environment.Exit(0);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Steamworks.SteamClient.SteamId.Value.ToString());
        }
    }
}
