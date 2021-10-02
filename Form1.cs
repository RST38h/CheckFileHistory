using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckFileHistory
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        delegate void AddItemDelegate(string text);

        private void AddItem(string text)
        {
            listView1.Items.Add(text);
        }

        private async void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reloadToolStripMenuItem.Enabled = false;
            toolStripStatusLabel1.Text = "更新中...";
            listView1.Items.Clear();

            await Task.Run(() =>
            {
                var list = new HashSet<string>();

                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var enumerationOptions = new EnumerationOptions();
                enumerationOptions.RecurseSubdirectories = true;
                var files = Directory.GetFileSystemEntries(path, "*", enumerationOptions);

                foreach (var file in files)
                {
                    var file1 = Strings.StrConv(file, VbStrConv.Wide);
                    file1 = Strings.StrConv(file1, VbStrConv.Uppercase);
                    file1 = Strings.StrConv(file1, VbStrConv.Hiragana);

                    if (list.Contains(file1))
                    {
                        Invoke(new AddItemDelegate(AddItem), file);
                    }
                    else
                    {
                        list.Add(file1);

                        if (file.Length >= 250)
                        {
                            Invoke(new AddItemDelegate(AddItem), file);
                        }
                    }
                }
            });

            reloadToolStripMenuItem.Enabled = true;
            toolStripStatusLabel1.Text = "準備完了";
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var process = new Process();
            process.StartInfo.Verb = "RunAs";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "eventvwr.exe";
            process.StartInfo.Arguments = "/c:Microsoft-Windows-FileHistory-Engine/BackupLog";
            process.Start();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var path = listView1.SelectedItems[0].Text;

            while (path.Length >= 260)
            {
                path = Path.GetDirectoryName(path);
            }

            Process.Start("explorer.exe", "/select,\"" + path + '"');
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Opacity = 100;
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }
    }
}
