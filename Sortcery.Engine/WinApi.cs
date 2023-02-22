using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Sortcery.Engine;

#if _WINDOWS
public class WinApi
{
    public static HardLinkId FromFileInfo(FileInfo fileInfo)
    {
        var fileHandle = CreateFile(fileInfo.FullName, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
        if (fileHandle == INVALID_HANDLE_VALUE)
        {
            throw new Win32Exception("Failed to open file");
        }

        try
        {
            var fileIndex = new ByHandleFileInformation();
            if (!GetFileInformationByHandle(fileHandle, ref fileIndex))
            {
                throw new Win32Exception("Failed to get file information");
            }

            return new HardLinkId(fileIndex.FileIndexLow, fileIndex.FileIndexHigh, fileIndex.VolumeSerialNumber);
        }
        finally
        {
            CloseHandle(fileHandle);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, FileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetFileInformationByHandle(IntPtr hFile, ref ByHandleFileInformation lpFileInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private const int INVALID_HANDLE_VALUE = -1;

    [StructLayout(LayoutKind.Sequential)]
    private struct ByHandleFileInformation
    {
        public uint FileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }
}
#endif
