using System;
using System.IO;

namespace FilesUpdater
{
    public static class DirectoryManagement
    {
        public static FileInfo[] ListFilesInDirectory(string sourcePath, string filesMask, SearchOption searchOptions = SearchOption.TopDirectoryOnly)
        {
            try
            {
                var di = new DirectoryInfo(sourcePath);
                return di.GetFiles(filesMask, searchOptions);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка поиска файлов в директории: {0}", ex), ex.InnerException);
            }
        }
    }
}
