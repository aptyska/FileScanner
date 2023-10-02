using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner
{
    public class FileSearch
    {

        public FileSearch(IEnumerable<string> extensions)
        {
            Extensions = extensions.ToList();
        }
    
        public List<string>? Extensions { get; set; }
        public List<string>? Filenames { get; set; }

        public string[] GetFiles(string directory)
        {
            return Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                    .Where(f => Extensions.IndexOf(Path.GetExtension(f)) >= 0).ToArray();
        }

    }
}
