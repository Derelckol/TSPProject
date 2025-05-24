using System.IO;
using TSPProject.Models;

namespace TSPProject.Utils
{
    public static class ResultExporter
    {
        public static void SaveResults(string filePath, List<TSPResult> results, double[,] matrix) // Збереження рещультатів у файл
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Матриця відстаней:");
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                        writer.Write(matrix[i, j].ToString("F2") + "\t");
                    writer.WriteLine();
                }

                writer.WriteLine();

                foreach (var result in results)
                {
                    writer.WriteLine($"Алгоритм: {result.AlgorithmName}");
                    writer.WriteLine($"Шлях: {string.Join(" -> ", result.Route)}");
                    writer.WriteLine($"Довжина: {result.RouteLength:F2}");
                    writer.WriteLine($"Час виконання: {result.ExecutionTime.TotalMilliseconds:F2} мс");
                    writer.WriteLine();
                }
            }
        }
    }
}
