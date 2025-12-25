using System;
using System.IO;

namespace BlueBerryDictionary.Helpers
{
    public static class PathHelper
    {
        // Property để lấy base path
        private static string BasePath
        {
            get
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

#if DEBUG
                // Trong môi trường Development (chạy từ Visual Studio)
                DirectoryInfo? dir = new DirectoryInfo(baseDir);
                while (dir != null && dir.Parent != null)
                {
                    if (Directory.GetFiles(dir.FullName, "*.csproj").Length > 0)
                    {
                        return dir.FullName;
                    }
                    dir = dir.Parent;
                }
#endif

                // Trong môi trường Production
                return baseDir;
            }
        }

        /// <summary>
        /// Kết hợp đường dẫn, tự động xử lý ..\..\..\ 
        /// </summary>
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return BasePath;

            // Loại bỏ AppDomain.CurrentDomain.BaseDirectory nếu có
            string[] cleanPaths = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];

                // Bỏ qua nếu là AppDomain.CurrentDomain.BaseDirectory
                if (path == AppDomain.CurrentDomain.BaseDirectory ||
                    path == "AppDomain.CurrentDomain.BaseDirectory")
                {
                    continue;
                }

                // Loại bỏ ..\..\..\ pattern
                path = path.Replace(@"..\..\..\", "")
                          .Replace(@"../../../", "")
                          .Replace(@"..\", "")
                          .Replace(@"../", "")
                          .TrimStart('\\', '/');

                cleanPaths[i] = path;
            }

            // Lọc bỏ các phần tử rỗng
            cleanPaths = Array.FindAll(cleanPaths, s => !string.IsNullOrEmpty(s));

            // Kết hợp với BasePath
            string fullPath = Path.Combine(BasePath, Path.Combine(cleanPaths));

            return fullPath;
        }

        /// <summary>
        /// Đảm bảo thư mục cha của file/folder tồn tại
        /// </summary>
        public static string EnsureDirectory(params string[] paths)
        {
            string fullPath = Combine(paths);

            // Kiểm tra xem path này là file hay folder
            bool isFile = Path.HasExtension(fullPath);

            string? directoryToCreate;
            if (isFile)
            {
                // Nếu là file, tạo thư mục cha
                directoryToCreate = Path.GetDirectoryName(fullPath);
            }
            else
            {
                // Nếu là folder, tạo chính nó
                directoryToCreate = fullPath;
            }

            if (!string.IsNullOrEmpty(directoryToCreate) && !Directory.Exists(directoryToCreate))
            {
                Directory.CreateDirectory(directoryToCreate);
            }

            return fullPath;
        }

        /// <summary>
        /// Đảm bảo thư mục cha của file tồn tại (dành riêng cho file paths)
        /// </summary>
        public static string EnsureDirectoryForFile(string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return filePath;
        }

        // Shortcut methods
        public static string DataPath(params string[] subPaths)
        {
            string[] allPaths = new string[subPaths.Length + 1];
            allPaths[0] = "Data";
            Array.Copy(subPaths, 0, allPaths, 1, subPaths.Length);
            return Combine(allPaths);
        }

        public static string PersistentStoragePath(params string[] subPaths)
        {
            string[] allPaths = new string[subPaths.Length + 2];
            allPaths[0] = "Data";
            allPaths[1] = "PersistentStorage";
            Array.Copy(subPaths, 0, allPaths, 2, subPaths.Length);
            return Combine(allPaths);
        }

        public static string ResourcePath(params string[] subPaths)
        {
            string[] allPaths = new string[subPaths.Length + 1];
            allPaths[0] = "Resources";
            Array.Copy(subPaths, 0, allPaths, 1, subPaths.Length);
            return Combine(allPaths);
        }
    }
}