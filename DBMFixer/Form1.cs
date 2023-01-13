using Steamworks;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
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

        private async void StartAction()
        {
            if (checkBox1.Checked)
            {
                DeleteAllLogs();
            }

            if (checkBox2.Checked)
            {
                await ReinstallBattleye();
            }
        }

        private async Task ReinstallBattleye()
        {
            var battleyeDir = Path.Combine(dayZInstallPath, "BattlEye");
            InvokeProgressbar(0, 100);
            if (Directory.Exists(battleyeDir))
            {
                WriteLog($"found battleye on {battleyeDir}");
                var files = Directory.GetFiles(battleyeDir);
                var subDirs = Directory.GetDirectories(battleyeDir);
                InvokeProgressbar(5, 100);
                foreach (var dir in subDirs)
                {
                    Directory.Delete(dir, true);
                }
                foreach (var file in files)
                {
                    File.Delete(file);
                }

                InvokeProgressbar(10, 100);

                WriteLog("BE -> Deleted!");

                using (HttpClient client = new HttpClient())
                {
                    WriteLog("BE -> Start downloading from Git!");
                    client.BaseAddress = new Uri("https://github.com/Deutsche-Bohrmaschine/");
                    var result = await client.GetAsync("DBMFixer/releases/download/0.0.0/BattleeyeBack.zip");
                    WriteLog("BE -> Successfully downladed backup file!");
                    var backFile = Path.Combine(battleyeDir, "BattleyeBack.zip");
                    InvokeProgressbar(90, 100);
                    using (var fs = new FileStream(backFile, FileMode.CreateNew))
                    {
                        WriteLog("BE -> Start Writing Zip!");
                        await result.Content.CopyToAsync(fs);
                        WriteLog($"BE -> Successfully Written file to {backFile}");
                    }

                    InvokeProgressbar(95, 100);
                    WriteLog("BE -> Start extraction of BE files!");
                    ZipFile.ExtractToDirectory(backFile, battleyeDir);
                    WriteLog("BE -> Successfully Reinstalled start delete of backup files!");
                    File.Delete(backFile);
                    InvokeProgressbar(100, 100);
                    WriteLog("BE -> Successfully Deleted backup file! Reinstall process done!");
                }
            }
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
