using System.Windows;

namespace WeChatHook
{
    /// <summary>
    /// DialogSetToken.xaml 的交互逻辑
    /// </summary>
    public partial class DialogSetToken : Window
    {
        public string Text { get; private set; }
        public DialogSetToken(string text)
        {
            InitializeComponent();
            InputTextBox.Text = text;
            Text = text;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            Text = InputTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}
