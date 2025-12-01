using System.Collections.Generic;

namespace BlueBerryDictionary.Models
{
    /// <summary>
    /// Kết quả đồng bộ dữ liệu
    /// </summary>
    public class SyncResult
    {
        /// <summary>
        /// Danh sách file đã download từ Drive
        /// </summary>
        public List<string> Downloaded { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách file đã upload lên Drive
        /// </summary>
        public List<string> Uploaded { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách file đã đồng bộ (không cần upload/download)
        /// </summary>
        public List<string> InSync { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách file bị xung đột
        /// </summary>
        public List<string> Conflicts { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách lỗi
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Tổng số file đã xử lý
        /// </summary>
        public int TotalFiles => Downloaded.Count + Uploaded.Count + InSync.Count;

        /// <summary>
        /// Có xung đột không?
        /// </summary>
        public bool HasConflicts => Conflicts.Count > 0;

        /// <summary>
        /// Có lỗi không?
        /// </summary>
        public bool HasErrors => Errors.Count > 0;
    }
}
