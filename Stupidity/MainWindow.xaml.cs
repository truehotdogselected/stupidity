using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;

namespace Stupidity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string loginInfoFile = "login.info";
        updateTextDelegate updTxt;
        //ServerLogic gitServerLogic;
        GitServer gitServerLogic;

        string playerName;
        string playerColor;
        string repositoryPath;
        string gitExePath;

        private List<Tuple<string, string>> messages = new List<Tuple<string, string>>();

        public MainWindow()
        {
            InitializeComponent();
            Login();
            Init();
            StartListening();
            updTxt = UpdateTextBlock;
        }

        private void ReadLoginInfoFile()
        {
            if (File.Exists(loginInfoFile))
            {
                var loginInfo = File.ReadAllLines(loginInfoFile);
                string name = loginInfo[0];
                string color = loginInfo[1];
                string gitRepo = loginInfo[2];
                string git = loginInfo[3];
                if (name != "" && File.Exists(git))
                {
                    playerName = name;
                    playerColor = color;
                    repositoryPath = gitRepo;
                    gitExePath = git;
                }
            }
        }

        private void Login()
        {
            ReadLoginInfoFile();
            if (playerName == null || gitExePath == null)
            {
                LoginWindow login = new LoginWindow();
                login.ShowDialog();
                ReadLoginInfoFile();
            }
        }

        private void Init()
        {
            gitServerLogic = new GitServer(repositoryPath, gitExePath);
            gitServerLogic.AddPlayer(playerName, playerColor);
        }


        private void UpdateTextBlock()
        {
            foreach (var message in messages)
            {
                string text = message.Item2;
                Brush brush = (SolidColorBrush) new BrushConverter().ConvertFrom(message.Item1);
                TextRange range = new TextRange(textBlock.Document.ContentEnd, textBlock.Document.ContentEnd);
                range.Text = text;
                range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            }
        }

        private void StartListening()
        {
            gitServerLogic.StartListening(() =>
                {
                    while (!gitServerLogic.ShuttingDown)
                    {
                        messages = gitServerLogic.UpdateChat();
                        try
                        {
                            this.textBlock.Dispatcher.Invoke(updTxt);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            break;
                        }
                        Thread.Sleep(5000);
                    }
                });
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                gitServerLogic.WriteMessage(playerName, textBox.Text);
                textBox.Text = "";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            gitServerLogic.Close();
        }
    }
}
