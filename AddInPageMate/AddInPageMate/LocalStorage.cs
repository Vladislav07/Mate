using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddInPageMate
{
    public static class LocalStorage
    {
        static string  path= @"D:\local.txt";
        public static void WriteComponents(List<ElementSW> elementSWs)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (ElementSW item in elementSWs)
                {
                    writer.Write("Matrix:");
                    for (int i = 0; i < item.matrixSw.Length; i++)
                    {
                        writer.Write(item.matrixSw[i] + " ");
                    }
                    writer.WriteLine();

                    writer.Write("Planes:");
                    foreach (string plane in item.planes)
                    {
                        writer.Write(plane + " ");
                    }
                    writer.WriteLine();
                    writer.Write("Name: " + item.nameSwComponent);
                    writer.WriteLine();
                }
            }
        }
 /*       public static List<ElementSW> ReadComponents()
        {
            List<ElementSW> dataList = new List<ElementSW>();

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i += 6)
            {
                double[] matrix = Array.ConvertAll(lines[i + 1].Split(' '), Double.Parse);
                string[] planes = lines[i + 3].Split(',');
                string name = lines[i + 5].Substring(lines[i + 5].LastIndexOf(':') + 1).Trim();

                dataList.Add(new ElementSW(name, planes, matrix));
            }

            return dataList;
        }*/
        public static List<ElementSW> ReadComponents()
        {
            List<ElementSW> dataList = new List<ElementSW>();

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i += 3)
            {
                if (i + 5 >= lines.Length)
                {
                    Console.WriteLine("Недостаточно данных для чтения элемента. Пропуск записи.");
                    continue;
                }

                string name = lines[i + 2].Substring(lines[i + 2].LastIndexOf(':') + 1).Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Некорректное имя элемента. Пропуск записи.");
                    continue;
                }

                double[] matrix;
                try
                {
                    matrix = lines[i + 1].Split(' ', (char)StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => double.TryParse(s, out double result) ? result : 0.0)
                        .ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при чтении матрицы элемента: {ex.Message}. Пропуск записи.");
                    continue;
                }

                string[] planes = lines[i + 1].Split(',');

                dataList.Add(new ElementSW(name, planes, matrix));
            }

            return dataList;
        }

    }
}
