using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileManager
{

    class Program
    {
        // поля для вывода каталога
        public static int CountLevel { get; set; }

        public static string DirNow {get; set;}
        public static string FileNow { get; set; }
        public static List<string> FileDir { get; set; }
        // размер директории
        public static long SizeDir { get; set; }
        // проверка на директорию возвращает True - если это директорию False - если это файл
        public static bool IsDirectory(FileSystemInfo fsItem)
        {
            return (fsItem.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }
        // расчет размера каталога
        public static long DirSize(DirectoryInfo di)
        {
            long size = 0;
            FileInfo[] fis = null;
            try
            {
                fis = di.GetFiles();
            }
            catch (DirectoryNotFoundException ex)
            {
                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                return 0;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                return 0;
            }
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo d in dis)
            {
                size += DirSize(d);
            }
            return size;
        }


        // Вывод страницы
        public static void OutputPage(int minCountFile, int maxCountFile, DirectoryInfo file)
        {
            //выводим надпапку
            int j = minCountFile;
            while (FileDir[j].Contains("│   ├──"))
            {
                j--;
            }
            if (minCountFile != j)
            {
                Console.WriteLine(FileDir[j]);
            }
            for (int i = minCountFile; i < FileDir.Count && i < maxCountFile; i++)
            {
                Console.WriteLine(FileDir[i]);
            }
        }


        public static void Paging(DirectoryInfo file, int index)
        {
            int minCountFile = 0;
            int maxCountFile = 10;
            Console.SetCursorPosition(0, 2);
            for (int i = minCountFile; i < FileDir.Count && i < maxCountFile; i++)
            {
                Console.WriteLine(FileDir[i]);
            }
            if (index == 0)
            {
                OutputInfoDir(file);
            }
            Console.SetCursorPosition(0, 25);
            while (true)
            {
                var consoleKey = Console.ReadKey();

                switch (consoleKey.Key)
                {
                    case ConsoleKey.RightArrow:
                        minCountFile += 10;
                        maxCountFile += 10;
                        if (minCountFile < FileDir.Count)
                        {
                            ClearLine(3, 12);
                            Console.SetCursorPosition(0, 2);
                            OutputPage(minCountFile, maxCountFile, file);
                            Console.SetCursorPosition(0, 25);
                        }
                        else
                        {
                            minCountFile -= 10;
                            maxCountFile -= 10;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        minCountFile -= 10;
                        maxCountFile -= 10;

                        if (minCountFile >= 0)
                        {
                            ClearLine(3, 12);
                            Console.SetCursorPosition(0, 2);
                            OutputPage(minCountFile, maxCountFile, file);
                            Console.SetCursorPosition(0, 25);
                        }
                        else
                        {
                            minCountFile += 10;
                            maxCountFile += 10;
                        }
                        break;
                    case ConsoleKey.Enter:
                        ClearLine(25, 5);
                        Console.SetCursorPosition(0, 25);
                        return;
                }
            }
        }
        //очистить строку в консоли
        public static void ClearLine(int line, int count)
        {
            for (int i = line; i < line + count; i++)
            {
                Console.MoveBufferArea(0, i, Console.BufferWidth, 1, Console.BufferWidth, i, ' ', Console.ForegroundColor, Console.BackgroundColor);
            }
        }

        //Вывод информации о файле
        public static void OutputInfoFile(string filename)
        {
            var fileInfo = new FileInfo(filename);
            Console.SetCursorPosition(0, 16);
            Console.WriteLine($"Имя файла: {fileInfo.FullName}");
            Console.WriteLine($"Расширение: {fileInfo.Extension}");
            Console.WriteLine($"Размер (в байтах): {fileInfo.Length}");
            Console.WriteLine($"Размер (в Кб): {fileInfo.Length / 1024}");
            Console.WriteLine($"Атрибуты: {fileInfo.Attributes}");
            Console.WriteLine($"Время создания: {fileInfo.CreationTime}");
            Console.WriteLine($"Время последней записи: {fileInfo.LastWriteTime}");
            if (fileInfo.IsReadOnly == true)
            {
                Console.WriteLine($"Доступен только для чтения: да");
            }
            else
            {
                Console.WriteLine($"Доступен только для чтения: нет");
            }
        }

        //Вывод информации о директории
        public static void OutputInfoDir(DirectoryInfo di)
        {
            Console.SetCursorPosition(0, 15);
            Console.Write("-------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("ИНФОРМАЦИЯ О КАТАЛОГЕ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"Имя каталога: {di.FullName}");
            Console.WriteLine($"Размер (в байтах): {DirSize(di)}");
            Console.WriteLine($"Размер (в Кб): {DirSize(di) / 1024}");
            Console.WriteLine($"Атрибуты: {di.Attributes}");
            Console.WriteLine($"Время создания: {di.CreationTime}");
            Console.WriteLine($"Время последней записи: {di.LastWriteTime}");
            Console.SetCursorPosition(0, 24);
            Console.Write("---------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("КОМАНДНАЯ СТРОКА");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-----------------------------------------------------");
            Console.SetCursorPosition(0, 25);
        }


        // Создание List из всех папок и подпапок директории + символы для красивого вывода на консоль
        public static void ShowFileDir(string filename, string prefix = " ")
        {
            var di = new DirectoryInfo(filename);
            List<FileSystemInfo> fsItems = null;
            try
            {
                fsItems = di.GetFileSystemInfos().ToList();
            }
            catch (System.UnauthorizedAccessException ex)
            {
                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                return;
            }

            for (int i = 0; i < fsItems.Count; i++)
            {
                FileDir.Add($"{prefix}├── {fsItems[i].Name}");

                if (IsDirectory(fsItems[i]) && CountLevel != 1)
                {
                    CountLevel = 1;
                    ShowFileDir(fsItems[i].FullName, prefix + "│   ");
                }
            }
            CountLevel = 0;
        }

        // копирование каталога
        public static void DirCopy(string sourcePath, string destinationPath)
        {
            var di = new DirectoryInfo(sourcePath);
            
            Directory.CreateDirectory(destinationPath + "\\" + di.Name);
            List<FileSystemInfo> fsItems = null;
            try
            {
                fsItems = di.GetFileSystemInfos().ToList();
            }
            catch (System.UnauthorizedAccessException ex)
            {
                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                return;
            }

            for (int i = 0; i < fsItems.Count; i++)
            {

                if (Directory.Exists(fsItems[i].FullName))
                {
                    DirCopy(fsItems[i].FullName, destinationPath + "\\" + di.Name);
                }
                else
                {
                    File.Create(destinationPath + "\\" + di.Name + "\\" + fsItems[i].Name);
                    try
                    {
                        File.Copy(fsItems[i].FullName, destinationPath + "\\" + di.Name + "\\" + fsItems[i].Name, true);
                    }
                    catch (System.IO.IOException ex)
                    {
                        File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                        File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                        File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                    }
                }
            }

        }

        static void Main(string[] args)
        {
            Console.Title = "Файловый менеджер";
            Console.Write("---------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("ДЕРЕВО С ФАЙЛАМИ И КАТАЛОГАМИ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("----------------------------------------------");

            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            BinaryFormatter formatter = new BinaryFormatter();
            // считываем директорию к которой последний раз обращались
            using (FileStream fs = new FileStream("settings.dat", FileMode.OpenOrCreate))
            {
                try 
                { 
                    string str = ((string)formatter.Deserialize(fs));
                    if (str != null && Directory.Exists(str.Trim()))
                    {
                        filename = String.Empty;
                        filename = str;
                    }
                }
                catch
                {

                }
            }

            DirNow = filename;
            using (FileStream fs = new FileStream("settings.dat", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, DirNow);
            }

            var file = new DirectoryInfo(filename);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(file);
            Console.ForegroundColor = ConsoleColor.White;
            FileDir = new List<string>();
            ShowFileDir(filename);
            Paging(file, 0);
            while (true)
            {
                //Console.SetCursorPosition(0, 25);
                string cmd = Console.ReadLine();
                if (cmd.Contains("ls"))
                {
                    cmd.Trim();

                    cmd = cmd.Remove(0, 2);
                    if (Directory.Exists(cmd.Trim()))
                    {
                        DirNow = null;
                        DirNow = cmd.Trim();
                        // записываем директорию к которой последний раз обратились
                        using (FileStream fs = new FileStream("settings.dat", FileMode.OpenOrCreate))
                        {
                            formatter.Serialize(fs, DirNow);
                        }

                        var di = new DirectoryInfo(cmd.Trim());

                        ClearLine(1, 14);
                        ClearLine(25, 5);
                        Console.SetCursorPosition(0, 1);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(di);
                        Console.ForegroundColor = ConsoleColor.White;
                        FileDir.Clear();
                        FileDir = new List<string>();
                        ShowFileDir(cmd.Trim());
                        Paging(di, 0);
                    }
                    else
                    {
                        ClearLine(25, 5);
                        Console.SetCursorPosition(0, 25);
                        Console.WriteLine("Директория не существует");
                        Console.SetCursorPosition(0, 26);
                    }
                }
                else if (cmd.Contains("cp"))
                {
                    cmd = cmd.Trim();
                    cmd = cmd.Remove(0, 2);
                    cmd = cmd.Trim();

                    if(cmd.Length > 1 && cmd.Substring(0, 2) == "*f")
                    {
                        if (File.Exists(FileNow))
                        {
                            string destDir = cmd.Remove(0, 2).Trim();
                            string sourceDir = FileNow;
                            var path = new FileInfo(sourceDir).Name;
                            try
                            {
                                File.Copy(sourceDir, destDir + "\\" + path);
                            }
                            catch(System.IO.IOException ex)
                            {
                                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                                ClearLine(25, 5);
                                Console.SetCursorPosition(0, 25);
                                Console.WriteLine("Не удалось скопировать файл");
                                Console.SetCursorPosition(0, 26);
                            }
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine($"Файл скопирован и находится по пути: {destDir}");
                            Console.SetCursorPosition(0, 26);
                        }
                        else
                        {
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Файл не существует");
                            Console.SetCursorPosition(0, 26);
                        }
                    }
                    else if(cmd.Length > 1 && cmd[0] == '*')
                    {
                        if (Directory.Exists(DirNow))
                        {
                            string sourceDir = DirNow;
                            var path = new DirectoryInfo(sourceDir).Name;
                            string destDir = cmd.Remove(0, 1).Trim();

                            DirCopy(sourceDir,destDir);
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine($"Каталог скопирован и находится по пути: {destDir + "\\" + path}");
                            Console.SetCursorPosition(0, 26);
                        }
                        else
                        {
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Директория не существует");
                            Console.SetCursorPosition(0, 26);
                        }
                    }
                    else if (cmd.Contains("->"))
                    {
                        int index = cmd.IndexOf("->");
                        string sourceDir = cmd.Substring(0, index);
                        string destDir = cmd.Substring(index + 3);
                        if (Directory.Exists(sourceDir))
                        {
                            var path = new DirectoryInfo(sourceDir).Name;
                            DirCopy(sourceDir, destDir);
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine($"Каталог скопирован и находится по пути: {destDir + "\\" + path}");
                            Console.SetCursorPosition(0, 26);
                        }
                        else if (File.Exists(sourceDir))
                        {
                            var path = new FileInfo(sourceDir).Name;
                            try
                            {
                                File.Copy(sourceDir, destDir + "\\" + path);
                            }
                            catch (System.IO.IOException ex)
                            {
                                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                                ClearLine(25, 5);
                                Console.SetCursorPosition(0, 25);
                                Console.WriteLine("Не удалось скопировать файл");
                                Console.SetCursorPosition(0, 26);
                            }
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine($"Файл скопирован и находится по пути: {destDir}");
                            Console.SetCursorPosition(0, 26);
                        }
                        else
                        {
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine($"Каталог или файл не найден по адресу: {sourceDir}");
                            Console.SetCursorPosition(0, 26);
                        }
                    }
                    else
                    {
                        ClearLine(25, 5);
                        Console.SetCursorPosition(0, 25);
                        Console.WriteLine("Команда не найдена");
                        Console.SetCursorPosition(0, 26);
                    }

                }
                else if (cmd.Contains("rm"))
                {
                    cmd = cmd.Trim();
                    cmd = cmd.Remove(0, 2);
                    cmd = cmd.Trim();
                  
                    if(cmd[0].ToString() + cmd[1].ToString() == "*f")
                    {
                        cmd = cmd.Remove(0, 2);
                        if (File.Exists(FileNow))
                        {
                            try
                            {
                                File.Delete(FileNow);
                            }
                            catch(Exception ex)
                            {
                                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                                ClearLine(25, 5);
                                Console.SetCursorPosition(0, 25);
                                Console.WriteLine("Не удалось удалить файл");
                                Console.SetCursorPosition(0, 26);
                                break;
                            }
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Файл удален");
                            Console.SetCursorPosition(0, 26);
                        }
                        else
                        {
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Файла по предыдущему пути не существует");
                            Console.SetCursorPosition(0, 26);
                        }
                    }
                    else if (cmd[0] == '*')
                    {
                        cmd = cmd.Remove(0, 1);
                        if (Directory.Exists(DirNow))
                        {
                            try
                            {
                                Directory.Delete(DirNow, true);
                            }
                            catch(Exception ex)
                            {
                                File.AppendAllText("errorsrandom_name_exception.txt", $"{DateTime.Now} Сообщение об ошибке: {ex.Message}");
                                File.AppendAllText("errorsrandom_name_exception.txt", Environment.NewLine);
                                ClearLine(25, 5);
                                Console.SetCursorPosition(0, 25);
                                Console.WriteLine("Не удалось удалить каталог");
                                Console.SetCursorPosition(0, 26);
                                break;
                            }
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Каталог удален");
                            Console.SetCursorPosition(0, 26);
                        }
                        else
                        {
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Каталога по предыдущему пути не существует");
                            Console.SetCursorPosition(0, 26);

                        }
                    }
                    else
                    {
                        if (Directory.Exists(cmd))
                        {
                            Directory.Delete(cmd, true);
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Каталог удален");
                            Console.SetCursorPosition(0, 26);
                        }
                        else if (File.Exists(cmd))
                        {
                            File.Delete(cmd);
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Файл удален");
                            Console.SetCursorPosition(0, 26);
                        }
                        else
                        {
                            ClearLine(25, 5);
                            Console.SetCursorPosition(0, 25);
                            Console.WriteLine("Файла или каталога по такому пути не существует");
                            Console.SetCursorPosition(0, 26);
                        }
                    }
                }
                else if (cmd.Contains("file"))
                {
                    cmd = cmd.Trim();
                    cmd = cmd.Remove(0, 4);
                    cmd = cmd.Trim();
                    if (cmd != "")
                    {
                        if (cmd[0] == '*')
                        {
                            cmd = cmd.Remove(0, 1);
                            if (File.Exists(DirNow + "\\" + cmd.Trim()))
                            {
                                var di = new DirectoryInfo(DirNow);
                                ClearLine(16, 7);
                                FileNow = String.Empty;
                                FileNow = DirNow + "\\" + cmd.Trim();
                                // записываем директорию к которой последний раз обратились
                                using (FileStream fs = new FileStream("settings.dat", FileMode.OpenOrCreate))
                                {
                                    formatter.Serialize(fs, DirNow);
                                }

                                OutputInfoFile(DirNow + "\\" + cmd.Trim());
                                Paging(di, 1);
                            }
                            else
                            {
                                ClearLine(25, 5);
                                Console.SetCursorPosition(0, 25);
                                Console.WriteLine("Файл не найден");
                                Console.SetCursorPosition(0, 26);
                            }
                        }
                        else
                        {
                            if (File.Exists(cmd.Trim()))
                            {
                                ClearLine(16, 7);
                                OutputInfoFile(cmd.Trim());
                                FileNow = String.Empty;
                                FileNow = cmd.Trim();
                                // записываем директорию к которой последний раз обратились
                                using (FileStream fs = new FileStream("settings.dat", FileMode.OpenOrCreate))
                                {
                                    formatter.Serialize(fs, DirNow);
                                }

                                cmd = cmd.Substring(0, cmd.LastIndexOf('\\'));
                                var di = new DirectoryInfo(cmd);
                                ClearLine(1, 14);
                                ClearLine(25, 5);
                                Console.SetCursorPosition(0, 1);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(di);
                                Console.ForegroundColor = ConsoleColor.White;
                                FileDir.Clear();
                                FileDir = new List<string>();
                                DirNow = cmd;
                                ShowFileDir(cmd);
                                Paging(di, 1);
                            }
                            else
                            {
                                ClearLine(25, 5);
                                Console.SetCursorPosition(0, 25);
                                Console.WriteLine("Файл не найден");
                                Console.SetCursorPosition(0, 26);
                            }
                        }
                    }
                    else
                    {
                        ClearLine(25, 5);
                        Console.SetCursorPosition(0, 25);
                        Console.WriteLine("Укажите файл");
                        Console.SetCursorPosition(0, 26);
                    }
                }
                else
                {
                    ClearLine(25, 5);
                    Console.SetCursorPosition(0, 25);
                    Console.WriteLine("Команда не найдена");
                    Console.SetCursorPosition(0, 26);
                }
            }
        }
    }
}
