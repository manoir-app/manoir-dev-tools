using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Manoir.DevTools
{
    public class BuildData : ObservableCollection<BuildComponentKind>
    {
    }

    public class BuildComponentKind : ObservableCollection<BuildComponent>
    {
        private string _name = null;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }
    }

    public class BuildComponent : ObservableCollection<BuildResult>
    {
        private string _name = null;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }

        private string _folder = null;
        public string Folder
        {
            get { return _folder; }
            set
            {
                _folder = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Folder"));
            }
        }

        private string _imageName = null;
        public string ImageName
        {
            get { return _imageName; }
            set
            {
                _imageName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ImageName"));
            }
        }

        private string _dockerFile = null;
        public string DockerFile
        {
            get { return _dockerFile; }
            set
            {
                _dockerFile = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DockerFile"));
            }
        }

    }

    public class BuildResult : INotifyPropertyChanged
    {
        BuildComponent _parent = null;
        public BuildResult(BuildComponent parent)
        {
            _parent = parent;
        }

        public BuildComponent Parent { get { return _parent; } }

        private string _env = null;
        public string Environment
        {
            get { return _env; }
            set
            {
                _env = value;
                OnPropertyChanged("Environment");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var evt = PropertyChanged;
            if (evt != null)
                evt(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class BuildBll
    {
        public static BuildData Get()
        {
            BuildData dt = new BuildData();
            var sett = ToolsBll.Load();
            if (!Directory.Exists(sett.Local.RootForManoirRepo))
                return dt;

            foreach (var dir in Directory.GetDirectories(sett.Local.RootForManoirRepo))
            {
                AddToData(dt, dir, sett.Local.RootForManoirRepo, sett);
            }

            return dt;
        }


        public class ManoirDeployJson
        {
            public string Kind { get; set; }
            public string Name { get; set; }
            public string ImageName { get; set; }
            public string DockerFile { get; set; }
        }


        private static void AddToData(BuildData dt, string dir, string rootForManoirRepo, ToolsBll.Settings sett)
        {
            string pth = Path.GetFileNameWithoutExtension(dir);
            if (pth.Equals("Release") || pth.Equals("Debug"))
                return;

            foreach (var subdir in Directory.GetDirectories(dir))
            {
                AddToData(dt, subdir, rootForManoirRepo, sett);
            }

            pth = Path.Combine(dir, "manoir-deploy.json");
            if (File.Exists(pth))
            {
                var t = JsonSerializer.Deserialize<ManoirDeployJson>(File.ReadAllText(pth));
                if (t == null || t.Kind == null)
                    return;

                var k = (from z in dt where z.Name.Equals(t.Kind, System.StringComparison.InvariantCultureIgnoreCase) select z).FirstOrDefault();
                if (k == null)
                {
                    k = new BuildComponentKind() { Name = t.Kind };
                    dt.Add(k);
                }

                var c = (from z in k where z.Name.Equals(k.Name, System.StringComparison.InvariantCultureIgnoreCase) select z).FirstOrDefault();
                if (c == null)
                {
                    c = new BuildComponent() { Name = t.Name, Folder = dir, ImageName = t.ImageName };
                    if (File.Exists(Path.Combine(dir, t.DockerFile)))
                        c.DockerFile = Path.Combine(dir, t.DockerFile);
                    k.Add(c);
                }

                foreach (var env in sett)
                {
                    c.Add(new BuildResult(c)
                    {
                        Environment = env.Name
                    });
                }
            }
        }

        public static bool BuildAndDeploy(BuildResult br)
        {
            string pthTmp = Path.Combine(Path.GetTempPath(), "manoir-build");
            if (Directory.Exists(pthTmp))
                Directory.Delete(pthTmp, true);
            Directory.CreateDirectory(pthTmp);

            // on build le csproj
            foreach (var c in Directory.GetFiles(br.Parent.Folder, "*.csproj"))
            {
                BuildCsProj(c, pthTmp);
            }

            BuildDocker(pthTmp, br);

            return true;
        }

        private static bool BuildDocker(string pthTmp, BuildResult br)
        {
            var sett = ToolsBll.Load();
            var leEnv = (from z in sett where z.Name.Equals(br.Environment, StringComparison.InvariantCultureIgnoreCase) select z).FirstOrDefault();
            if (leEnv == null)
                return false;

            string batFile = Path.Combine(pthTmp, "build-docker.ps1");
            if (File.Exists(batFile))
                File.Delete(batFile);

            var bat = new StringBuilder();
            bat.Append(CleanUpRepoUrl(leEnv.DockerRepositoyUrl));
            bat.Append("/");
            bat.Append(br.Parent.ImageName);
            bat.Append(":");
            bat.Append(leEnv.DockerTagForImages);

            var imageFullName = bat.ToString();

            string localDockerFile = Path.Combine(pthTmp, Path.GetFileName(br.Parent.DockerFile));
            File.Copy(br.Parent.DockerFile, localDockerFile);

            bat = new StringBuilder();
            bat.AppendLine("$dt = Get-Date");
            bat.Append("docker build \"");
            bat.Append(pthTmp);
            bat.Append("\" -f \"");
            bat.Append(Path.GetFileName(br.Parent.DockerFile));
            bat.Append("\" -t ");
            bat.AppendLine(imageFullName);

            bat.Append("docker push ");
            bat.AppendLine(imageFullName);

            bat.Append("docker image rm ");
            bat.AppendLine(imageFullName);

            File.WriteAllText(batFile, bat.ToString());

            ProcessStartInfo pci = new ProcessStartInfo();
            pci.WorkingDirectory = pthTmp;
            pci.UseShellExecute = false;
            pci.ArgumentList.Add("-NonInteractive");
            pci.ArgumentList.Add(batFile);
            pci.FileName = "powershell.exe";
            var p = Process.Start(pci);
            p.WaitForExit();



            File.Delete(batFile);

            if (p.ExitCode == 0)
                return true;
            else
                return false;
        }

        private static string CleanUpRepoUrl(string dockerRepositoyUrl)
        {
            try
            {
                var uri = new Uri(dockerRepositoyUrl);
                return uri.Host + ":" + uri.Port;
            }
            catch
            {
                return dockerRepositoyUrl;
            }
        }

        private static bool BuildCsProj(string csprojFile, string pthTmp)
        {
            string batFile = Path.Combine(pthTmp, "build.bat");
            var bat = new StringBuilder();
            bat.Append("dotnet publish ");
            bat.Append("\"");
            bat.Append(csprojFile);
            bat.Append("\" ");
            bat.Append("--output \"");
            bat.Append(pthTmp);
            bat.Append("\" ");
            bat.Append("--configuration Release");
            File.WriteAllText(batFile, bat.ToString());

            ProcessStartInfo pci = new ProcessStartInfo();
            pci.WorkingDirectory = pthTmp;
            pci.UseShellExecute = false;
            pci.FileName = batFile;
            //pci.RedirectStandardError = true;
            //pci.RedirectStandardOutput = true;
            var p = Process.Start(pci);
            p.WaitForExit();
            //while (!p.HasExited)
            //{
            //    var outSt = p.StandardOutput.ReadToEnd();
            //    var errSt = p.StandardError.ReadToEnd();
            //    Debug.WriteLine(outSt);
            //    Debug.WriteLine("ERR:" + errSt);
            //    Thread.Sleep(200);
            //}

            File.Delete(batFile);

            if (p.ExitCode == 0)
                return true;
            else
                return false;

        }

        private static void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
        }

        private static void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine("ERR:" + e.Data);
        }
    }
}
