using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WeChatHook
{
    /// <summary>
    /// DialogSetApiUrl.xaml 的交互逻辑
    /// </summary>
    public partial class DialogSetDatabaseFolder : Window
    {
        public string Text { get; private set; }
        public DialogSetDatabaseFolder(string text)
        {
            InitializeComponent();
            InputTextBox.Text = text;
            Text = text;
            var defaultDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\xwechat_files";
            if (Directory.Exists(defaultDocsPath))
            {
                AddToAutoDetectList(defaultDocsPath);
            }
            foreach (var drive in Environment.GetLogicalDrives())
            {
                if (Directory.Exists(drive + "xwechat_files"))
                {
                    AddToAutoDetectList(drive + "xwechat_files");
                }
            }
        }

        private void AddToAutoDetectList(string xwechat_files)
        {
            try
            {
                if (!Directory.Exists(xwechat_files)) return;
                var directory = new DirectoryInfo(xwechat_files);
                foreach (var item in directory.GetDirectories())
                {
                    string dbPath = directory.FullName + "\\" + item.Name + "\\db_storage\\message";
                    if (Directory.Exists(dbPath))
                    {
                        AutoDetectList.Items.Add(new ListBoxItem
                        {
                            Content = item.Name,
                            Tag = dbPath
                        });
                    }
                }
            }
            catch { }
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            Text = InputTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void Button_Browse_Click(object sender, RoutedEventArgs e)
        {
            var defaultDocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\xwechat_files";
            var initialDir = !string.IsNullOrEmpty(Text) && Directory.Exists(Text)
                ? Text
                : (Directory.Exists(defaultDocsPath) ? defaultDocsPath : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            var ofd = new OpenFolderDialog
            {
                Title = "选择 “xwechat_files\\(用户文件夹)\\db_storage\\message”",
                InitialDirectory = initialDir,
                Multiselect = false
            };
            if (ofd.ShowDialog() == true)
            {
                InputTextBox.Text = ofd.FolderName;
            }
        }

        private void AutoDetectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AutoDetectList.SelectedItem is ListBoxItem item)
            {
                var tag = item.Tag;
                if (tag is string dbPath && dbPath != string.Empty)
                {
                    InputTextBox.Text = dbPath;
                }
            }
        }
    }
}
