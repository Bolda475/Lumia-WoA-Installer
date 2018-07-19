using ByteSizeLib;
using Installer.Core.FileSystem;

namespace Installer.ViewModels.Raspberry
{
    public class DiskViewModel
    {
        private readonly Disk disk;

        public DiskViewModel(Disk disk)
        {
            this.disk = disk;
        }

        public uint Number => disk.Number + 1;
        public string FriendlyName => disk.FriendlyName;
        public ByteSize Size => disk.Size;
        public bool IsUsualTarget => Size > ByteSize.FromGigaBytes(14) && Size < ByteSize.FromGigaBytes(255);
        public Disk Disk => disk;
    }
}