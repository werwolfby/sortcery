// See https://aka.ms/new-console-template for more information

using Mono.Unix;

// Get home dir

var homeDir = Environment.GetEnvironmentVariable("HOME") 
              ?? Environment.GetEnvironmentVariable("USERPROFILE")
              ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
var dir1 = new UnixDirectoryInfo(Path.Combine(homeDir, "Documents/Private/SortceryTest/1"));
var dir2 = new UnixDirectoryInfo(Path.Combine(homeDir, "Documents/Private/SortceryTest/2"));

// Find all files and collect them into dictionary by ino
var files1 = TraverseDirectory(dir1);
var files2 = TraverseDirectory(dir2);

// Find new files in dir1
var newFiles = files1
    .Where(f => !files2.ContainsKey(f.Key))
    .Select(f => f.Value)
    .ToList();

// Find stale files in dir2
var staleFiles = files2
    .Where(f => !files1.ContainsKey(f.Key))
    .Select(f => f.Value)
    .ToList();

// Print new files
Console.WriteLine("New files:");
foreach (var file in newFiles)
{
    Console.WriteLine(file.FullName);
}

// Print stale files
Console.WriteLine("Stale files:");
foreach (var file in staleFiles)
{
    Console.WriteLine(file.FullName);
}

// Create hardlink to new files into dir2
foreach (var file in newFiles)
{
    file.CreateLink(Path.Combine(dir2.FullName, file.Name));
}

Dictionary<long, UnixFileInfo> TraverseDirectory(UnixDirectoryInfo dir)
{
    var result = new Dictionary<long, UnixFileInfo>();
    foreach (var entry in dir.GetFileSystemEntries())
    {
        if (entry is UnixDirectoryInfo subDir)
        {
            foreach (var (inode, file) in TraverseDirectory(subDir))
            {
                result.Add(inode, file);
            }
        }
        else if (entry is UnixFileInfo file)
        {
            result.Add(file.Inode, file);
        }
    }

    return result;
}