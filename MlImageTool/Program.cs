using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MlImageTool
{
    class Program
    {
        /// <summary>
        /// Workflow:
        /// 1. Search: input csv of mappings/search terms -> images per category in folders
        /// 2. Manual vetting
        /// 3. Crops: add square crops in situ
        /// 4. Manual vetting
        /// 5. Flip
        /// 6. Resize
        /// 7. Split into train/test
        /// 
        /// Note: given the infrequent use of this and manual steps, I've not spent the effort parallelising anything
        /// </summary>
        /// <param name="args"></param>
        /// 
        private static void Main(string[] args)
        {
            var commandLineArgs = CommandLineArgs.Parse(args);
            if (commandLineArgs == null) return;

            switch (commandLineArgs.Mode)
            {
                case Mode.Search:
                    Search(commandLineArgs.TargetDirectory, commandLineArgs.CategoriesFile, commandLineArgs.SearchCount);
                    break;
                case Mode.Crop:
                    Crop(commandLineArgs.TargetDirectory);
                    break;
                case Mode.Flip:
                    Flip(commandLineArgs.TargetDirectory);
                    break;
                case Mode.Resize:
                    Resize(commandLineArgs.TargetDirectory, commandLineArgs.ImageSize);
                    break;
                case Mode.Split:
                    Split(commandLineArgs.TargetDirectory, commandLineArgs.TestNumber);
                    break;
            }
        }

        private static void Search(string targetDirectory, string categoriesFile, int searchCount)
        {
            if (!File.Exists(categoriesFile))
            {
                Console.WriteLine("File not found: " + categoriesFile);
                return;
            }
            var searches = LoadClasses(categoriesFile);

            if (string.IsNullOrEmpty(targetDirectory))
            {
                targetDirectory = "results";
            }
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var downloader = new ImageDownloader();

            foreach (var key in searches.AllKeys)
            {
                var keyPath = Path.Combine(targetDirectory, key);
                CreateDirectoryWithRename(keyPath);
                downloader.Download(key, searches[key], keyPath, searchCount);
            }
        }

        private static NameValueCollection LoadClasses(string filePath)
        {
            var result = new NameValueCollection();
            foreach (var parts in File.ReadAllLines(filePath).Select(line => line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)))
            {
                result.Add(parts[0],parts[1]);
            }
            return result;
        }

        private static void Crop(string targetDirectory)
        {
            ProcessDirectory(targetDirectory, ImageResizer.CropImage);
        }

        private static void Flip(string targetDirectory)
        {
            ProcessDirectory(targetDirectory, ImageResizer.FlipImage);
        }

        private static void Resize(string targetDirectory, int size)
        {
            ProcessDirectory(targetDirectory, s => ImageResizer.ResizeImage(s, size));
        }

        private static void ProcessDirectory(string path, Action<string> fileAction)
        {
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                fileAction(file);
            }
        }

        private static void CreateDirectoryWithRename(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Move(path, string.Format("{0}-{1}", path, DateTime.Now.ToString("yyyyMMddTHHmmssfff")));
            }
            Directory.CreateDirectory(path);            
        }

        private static void Split(string path, int testNumber)
        {
            var directories = Directory.GetDirectories(path);
            var testDirectory = Path.Combine(path, "test");
            CreateDirectoryWithRename(testDirectory);
            var trainDirectory = Path.Combine(path, "train");
            CreateDirectoryWithRename(trainDirectory);

            var indexRegex = new Regex(@"^.+-(\d+)\..+\.png$");
            foreach (var directory in directories)
            {
                if (directory.StartsWith(testDirectory) || directory.StartsWith(trainDirectory)) continue;
                var files = Directory.GetFiles(directory);
                foreach (var file in files)
                {
                    var filename = Path.GetFileName(file);
                    Debug.Assert(!string.IsNullOrEmpty(filename));
                    var match = indexRegex.Match(filename);
                    if (match.Success && match.Groups.Count == 2)
                    {
                        // segregate test/train according to the original source image - don't mix derivatives of the
                        // same source image into test and train
                        var index = int.Parse(match.Groups[1].Value);
                        var targetDirectory = index > testNumber ? trainDirectory : testDirectory;
                        File.Copy(file, Path.Combine(targetDirectory, filename));
                    }
                }
            }
        }
    }
}
