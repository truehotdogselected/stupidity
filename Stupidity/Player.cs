using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
namespace Stupidity
{
    public class Player
    {
        public string Name { get; private set; }
        string color;
        public Color Color { get { return (Color) ColorConverter.ConvertFromString(color); } }
        public Brush Brush { get { return (SolidColorBrush) new BrushConverter().ConvertFrom(color); } }
        public string ColorString { get { return color; } }
        public Message LastMessage { get; private set; }
        public bool Updated { get; private set; }
        public Player(string name, string color)
        {
            Name = name;
            if (CorrectColorString(color))
            {
                this.color = color;
            }
            else
            {
                this.color = Colors.Black.ToString();
            }
            LastMessage = new Message("");
            Updated = false;
        }

        private bool CorrectColorString(string colorString)
        {
            int colorStringLength = 9;  // #AARRGGBB
            
            if (colorString == null || colorString == "" || colorString[0] != '#') 
            {
                return false;
            }
            colorString = colorString.Trim(new char[] { '\r', '\n' });
            if(colorString.Length < colorStringLength)
            {
                return false;
            }

            var color = colorString.Substring(1, colorStringLength - 1); // skip '#'
            foreach (var letter in color)
            {
                if (!"01234567890ABCDEF".Contains(letter))
                {
                    return false;
                }
            }
            return true;
        }

        public void NewMessage(string messageText)
        {
            if (LastMessage.MessageText == messageText)
            {
                // logic may change if we add time for messages
                Updated = false;
            }
            else
            {
                LastMessage = new Message(messageText);
                Updated = true;
            }
        }
    }

    public class Message
    {
        // later i plan to add time
        public string MessageText { get; private set; }
        public Message(string text)
        {
            MessageText = text;
        }
    }
}
