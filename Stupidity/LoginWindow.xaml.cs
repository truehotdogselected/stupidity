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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace Stupidity
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public string gitPath;
        public string playerName;

        private List<Color> allColors = new List<Color>();
        private Color chosenColor;
        public LoginWindow()
        {
            InitializeComponent();
            InitColorBox();
        }
        private void InitColorBox()
        {
            int step = 75;
            for(int r = 0; r < 255; r+= step)
            {
                for (int g = 0; g < 255; g += step)
                {
                    for (int b = 0; b < 255; b += step)
                    {
                        allColors.Add(Color.FromRgb((byte) r, (byte) g, (byte) b));
                    }
                }
            }
            for (int i = 0; i < allColors.Count; i++)
            {
                var item = new ComboBoxItem();
                item.Background = new SolidColorBrush(allColors[i]);
                item.Content = allColors[i].ToString();
                colorComboBox.Items.Add(item);
            }
        }
        private void gitButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            gitPath = dialog.FileName;
            if(gitPath!= null && File.Exists(gitPath))
            {
                gitButton.Content = gitPath;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text != "" && File.Exists(gitPath))
            {
                playerName = textBox.Text;
                File.WriteAllLines("login.info", new string[] {textBox.Text, chosenColor.ToString(), gitRepoTextBox.Text, gitPath });
                Close();
            }
        }

        private void colorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chosenColor = allColors[colorComboBox.SelectedIndex];
            colorComboBox.Background = new SolidColorBrush(allColors[colorComboBox.SelectedIndex]);
        }
    }
}
