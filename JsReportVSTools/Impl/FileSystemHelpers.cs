using System;
using System.IO;

namespace JsReportVSTools.Impl
{
    public class FileSystemHelpers
    {
        public static void Copy(string sourceDirName, string destDirName, string pattern = "*.*", bool forceOverwrite = false)
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

                if (!File.Exists(temppath) || new FileInfo(temppath).LastWriteTime < file.LastWriteTime || forceOverwrite)
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

        public static void DeleteFilesInDirectory(string target_dir, string pattern)
        {
            string[] files = Directory.GetFiles(target_dir, pattern);
            string[] dirs = Directory.GetDirectories(target_dir);


            foreach (string f in files)
            {
                File.Delete(f);
            }

            foreach (var dir in dirs)
            {
                DeleteFilesInDirectory(dir, pattern);
            }
        }

        public static void DeleteDirectory(string target_dir)
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    DeleteDirectoryInner(target_dir);
                    return;
                }
                catch (Exception e)
                {
                    
                }
            }
        }

        public static void DeleteDirectoryInner(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}