using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceModPluginsBuilder
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Start! " + args.Length);
            Console.WriteLine("3!" + Environment.CurrentDirectory);
            Console.WriteLine("mod: " + Path.GetFileName(Environment.CurrentDirectory));

            var pluginName = Path.GetFileName(Environment.CurrentDirectory).ToLower();
            var buildPath = Path.Combine(Environment.CurrentDirectory, "build");
            var buildSourceModPath = Path.Combine(buildPath, "addons", "sourcemod");
            var pluginSourcePath = Path.Combine(buildSourceModPath, "scripting", pluginName + ".sp");
            if (!File.Exists(pluginSourcePath))
            {
                Console.WriteLine("File '" + pluginSourcePath + "' not found!");
                return;
            }

            var parentDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;
            var compilerPath = Path.Combine(parentDirectory, "SourceModCompiler", "spcomp.exe");

            if (!File.Exists(compilerPath))
            {
                Console.WriteLine("SourcePawn Compiler '" + compilerPath + "' not found!");
                return;
            }

            var compileProcess = new Process();

            compileProcess.StartInfo.FileName = compilerPath;
            compileProcess.StartInfo.Arguments = pluginSourcePath;
            compileProcess.StartInfo.UseShellExecute = false;
            compileProcess.StartInfo.RedirectStandardOutput = true;
            Console.WriteLine("start " + compileProcess.StartInfo.FileName);
            compileProcess.Start();

            string compileOutput = compileProcess.StandardOutput.ReadToEnd();
            Console.WriteLine(compileOutput);

            compileProcess.WaitForExit();

            var newPluginFilePath = Path.Combine(Environment.CurrentDirectory, pluginName + ".smx");
            if (!File.Exists(newPluginFilePath))
            {
                Console.WriteLine("Compiled file '" + newPluginFilePath + "' not found!");
                return;
            }

            var pluginDestinationDirectoryPath = Path.Combine(buildSourceModPath, "plugins");
            var pluginDestinationPath = Path.Combine(pluginDestinationDirectoryPath, pluginName + ".smx");

            if (File.Exists(pluginDestinationPath))
            {
                try
                {
                    File.Delete(pluginDestinationPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Deleting file " + pluginDestinationPath + " failed! (" + ex.Message + ")");
                }
            }

            if (!Directory.Exists(pluginDestinationDirectoryPath))
            {
                Directory.CreateDirectory(pluginDestinationDirectoryPath);
            }

            File.Move(newPluginFilePath, pluginDestinationPath);

            for (int i = 0; i < args.Length; i++)
            {
                var gamePath = args[i];
                if (Directory.Exists(gamePath))
                {
                    Console.WriteLine("copy plugin to '" + gamePath + "'...");
                    DirectoryCopy(buildPath, gamePath, true);
                }
                else
                {
                    Console.WriteLine("game path " + gamePath + " not found!");
                }
            }
            Console.WriteLine("Complited!");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
