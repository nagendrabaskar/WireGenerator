using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireGenerator
{
    public static class Utility
    {
        public static void DeleteDestinationFolder(string destPath)
        {            
            if (Directory.Exists(destPath))
                Directory.Delete(destPath, true);
        }

        public static void CopyAssets(string sourcePath, string destPath)
        {
            Directory.CreateDirectory(destPath);
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest);
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyAssets(folder, dest);
            }
        }


    }
}
