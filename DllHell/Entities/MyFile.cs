using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DllHell.Entities
{
    public class MyFile
    {
        public FileInfo File { get; set; }

        public string Name
        {
            get
            {
                if (File != null)
                    return File.FullName;

                return "No file";
            }
        }
        private string _version;
        public Version Version
        {
            get
            {
                if (string.IsNullOrEmpty(_version))
                    _version = FileVersionInfo.GetVersionInfo(Name).FileVersion;

                return new Version(_version??"0.0.0.0");
            }
        }
        public bool Exclude { get; set; }

        public MyFile(FileInfo fileInfo)
        {
            this.File = fileInfo;
        }

        public Brush ErrorColor { get; set; }
    }
}
