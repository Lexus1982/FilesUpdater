using System;
using System.IO;
using System.Text;

namespace FilesUpdater
{
    internal static class Logger
    {
        public static void WriteLog(string fileName, string message)
        {
            try
            {
                // Проверим корректность исходных данных
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentNullException("fileName", string.Format("Не задано имя лог файла."));
                }

                var directory = Path.GetDirectoryName(fileName);

                if (string.IsNullOrEmpty(directory))
                {
                    throw new ArgumentNullException("fileName", string.Format("Не определен путь к лог файлу."));
                }

                // Создадим директорию, если не существует.
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Запишем строку данных в файл
                using (var outfile = new StreamWriter(fileName, true, Encoding.Default))
                {
                    outfile.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка записи данных в лог файл ({0}): {1}", fileName, ex), ex.InnerException);
            }
        }

        public static string WrapMessage(string hostName, string message)
        {
            return $"{DateTime.Now:dd.MM.yyyy HH:mm:ss}\t{hostName}\t{message}";
        }
    }
}
