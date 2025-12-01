using System;

namespace BlueBerryDictionary.Models
{
    /// <summary>
    /// Metadata để detect conflicts giữa local và Drive
    /// </summary>
    public class FileMetadata
    {
        public string FileName { get; set; }
        public DateTime LastModified { get; set; }
        public string Checksum { get; set; } // MD5 hash
        public long FileSize { get; set; }
        public string DriveFileId { get; set; }
        public DateTime? LastSynced { get; set; }
    }
}
