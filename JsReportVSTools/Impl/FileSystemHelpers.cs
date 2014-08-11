using System;
using System.IO;

namespace JsReportVSTools.Impl
{
    public class FileSystemHelpers
    {
        public static void Copy(string sourceDirName, string destDirName, string pattern = "*.*")
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles(pattern);
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);

                if (!File.Exists(temppath) || new FileInfo(temppath).LastWriteTime < file.LastWriteTime)
                    try
                    {
                        file.CopyTo(temppath, true);
                    }
                    catch (Exception e)
                    {
                        //ignore file locks
                    }
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                Copy(subdir.FullName, temppath);
            }
        } 
    }
}