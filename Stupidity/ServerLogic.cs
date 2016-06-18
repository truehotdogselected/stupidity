using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows;

namespace Stupidity
{
    public delegate void updateTextDelegate();
    public class GitServer
    {
        List<Player> players;
        string gitExePath;
        string repositoryPath;
        string repositoryName;
        string workDirectory;

        Thread updateThread;
        bool shuttingDown;
        bool dontListen;
        public bool ShuttingDown { get { return shuttingDown; } }

        public GitServer(string gitRepository, string gitExe)
        {
            players = new List<Player>();
            repositoryPath = gitRepository;
            repositoryName = Path.GetFileName(gitRepository); //getfilename will give us the last thing after last /
            gitExePath = gitExe;
            workDirectory = Directory.GetCurrentDirectory();
            string newWorkDirectory = workDirectory + Path.DirectorySeparatorChar + repositoryName;
            if (Directory.Exists(newWorkDirectory))
            {
                Directory.Delete(newWorkDirectory, true);
            }
            InitChat();
            workDirectory = newWorkDirectory;
            dontListen = false;
        }

        public void Close()
        {
            shuttingDown = true;
            updateThread.Join();
        }

        void InitChat()
        {
            if (!File.Exists(gitExePath))
            {
                return;
            }

            ProcessStartInfo procInfo = new ProcessStartInfo
            {
                FileName = gitExePath,
                Arguments = "clone " + repositoryPath,
                WorkingDirectory = workDirectory
            };

            var proc = Process.Start(procInfo);
            proc.WaitForExit();
        }

        void InitIfDirectoryExists()
        {
            if (!File.Exists(gitExePath))
            {
                return;
            }

            ProcessStartInfo procInfo = new ProcessStartInfo
            {
                FileName = gitExePath,
                WorkingDirectory = workDirectory, // Directory.GetCurrentDirectory() + "\\" + repositoryName,
                Arguments = "pull" //"pull " + gitRepositoryPath;
            };

            var proc = Process.Start(procInfo);
            proc.WaitForExit();

        }

        public void AddPlayer(string playerName, string playerColor)
        {
            if (!players.Exists(p=>p.Name == playerName))
            {
                players.Add(new Player(playerName, playerColor));
            }
        }

        public void WriteMessage(string playerName, string messageText)
        {
            dontListen = true;

            string playerColor = players.Find(p => p.Name == playerName).ColorString;
            string playerFile = playerName + ".txt";

            File.WriteAllText(workDirectory + "\\" + playerFile, playerColor + Environment.NewLine + messageText);

            var procInfo = new ProcessStartInfo
            {
                Arguments = "add " + playerFile,
                WorkingDirectory = workDirectory,
                FileName = gitExePath
            };
            var proc = Process.Start(procInfo);
            proc.WaitForExit();

            procInfo.Arguments = "commit -m \"commit " + DateTime.Now.ToShortTimeString() + "\"";
            proc = Process.Start(procInfo);
            proc.WaitForExit();

            procInfo.Arguments = "push"; //"push -f --set-upstream " + repositoryPath + " master";
            procInfo.UseShellExecute = false;
            procInfo.RedirectStandardOutput = true;

            proc = Process.Start(procInfo);
            proc.WaitForExit();

            dontListen = false;
        }

        public void StartListening(ThreadStart monitorMessageUpdates)
        {
            updateThread = new Thread(monitorMessageUpdates);
            updateThread.Name = "listeningThread";
            updateThread.Start();
        }

        private void UpdatePlayers()
        {
            string[] playersFiles = Directory.GetFiles(workDirectory);
            foreach (var playerFile in playersFiles)
            {
                string playerName = Path.GetFileNameWithoutExtension(playerFile);
                if(playerName == null && playerName == "")
                {
                    continue;
                }
                string playerColor;
                using (var file = File.OpenText(playerFile))
                {
                      playerColor = file.ReadLine();
                }
                if (!players.Exists(player=>player.Name == playerName))
                {
                    players.Add(new Player(playerName, playerColor));
                }
                players.Find(p => p.Name == playerName).NewMessage(PlayerMessage(playerName));
            }
        }

        private string PlayerMessage(string playerName)
        {
            StringBuilder message = new StringBuilder(playerName + ": ");
            bool wasText = false;
            using (var file = File.OpenText(workDirectory + Path.DirectorySeparatorChar+ playerName+".txt"))
            {
                file.ReadLine(); // skip color
                var line = file.ReadLine();
                while(line != null)
                {
                    wasText = true;
                    message.AppendLine(line);
                    line = file.ReadLine();
                }
            }
            if (!wasText)
            {
                message.AppendLine();   // message must end with newline
            }
            return message.ToString();
        }

        public List<Tuple<string, string>> UpdateChat()
        {
            if (dontListen)
            {
                return new List<Tuple<string, string>>();
            }

            var procInfo = new ProcessStartInfo
            {
                Arguments = "pull",
                WorkingDirectory = workDirectory,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = gitExePath,
                CreateNoWindow = true
            };

            var proc = Process.Start(procInfo);
            proc.WaitForExit();

            UpdatePlayers();
            List<Tuple<string, string>> updatedMessages = new List<Tuple<string, string>>();
            foreach(var player in players)
            {
                if (player.Updated)
                {
                    updatedMessages.Add(new Tuple<string, string> (player.ColorString, player.LastMessage.MessageText));
                }
            }
            return updatedMessages;
        }
    }
    
}
