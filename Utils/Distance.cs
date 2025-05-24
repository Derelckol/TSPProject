using TSPProject.Models;

namespace TSPProject.Utils
{
    public static class Distance
    {
        public static double CalculateDistance(City city1, City city2) // Обчислення відстані між двома містами
        {
            double dx = city1.X - city2.X;
            double dy = city1.Y - city2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public static double[,] BuildDistanceMatrix(List<City> cities) // Створення матриці відстаней між містами
        {
            int n = cities.Count;
            var matrix = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 0;
                    }
                    else
                    {
                        matrix[i, j] = CalculateDistance(cities[i], cities[j]);
                    }
                }
            }
            return matrix;
        }
    }
}
