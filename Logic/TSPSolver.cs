using System.Diagnostics;
using TSPProject.Models;
using TSPProject.Utils;

namespace TSPProject.Logic
{
    public class TSPSolver
    {
        private readonly double[,] distanceMatrix; // Матриця відстаней між містами
        private readonly int cityCount; // Кількість міст
        public TimeSpan LastExecutionTime { get; private set; } // Час виконання останнього алгоритму

        public TSPSolver(double[,] matrix)
        {
            distanceMatrix = matrix;
            cityCount = matrix.GetLength(0);
        }

        public TSPResult SolveGreedy() // Жадібний алгоритм
        {
            Stopwatch sw = Stopwatch.StartNew();
            List<(int from, int to, double dist)> edges = new();

            // Зібрання всіх можливих ребер
            for (int i = 0; i < cityCount; i++)
            {
                for (int j = i + 1; j < cityCount; j++)
                {
                    edges.Add((i, j, distanceMatrix[i, j]));
                }
            }

            // Сортування ребер за довжиною
            edges.Sort((a, b) => a.dist.CompareTo(b.dist));

            // Стеження за ступенями вершин і об'єднання компонент
            int[] degree = new int[cityCount];
            List<(int, int)> tourEdges = new();
            DisjointSetUnion dsu = new(cityCount);

            foreach (var (from, to, _) in edges)
            {
                if (degree[from] >= 2 || degree[to] >= 2)
                    continue;

                if (dsu.Find(from) == dsu.Find(to))
                {
                    // Уникаємо передчасних циклів
                    if (tourEdges.Count < cityCount - 1)
                        continue;
                }

                tourEdges.Add((from, to));
                degree[from]++;
                degree[to]++;
                dsu.Union(from, to);

                if (tourEdges.Count == cityCount)
                    break;
            }

            // Побудова маршруту з ребер
            Dictionary<int, List<int>> graph = new();
            foreach (var (a, b) in tourEdges)
            {
                if (!graph.ContainsKey(a)) graph[a] = new();
                if (!graph.ContainsKey(b)) graph[b] = new();
                graph[a].Add(b);
                graph[b].Add(a);
            }

            // Обхід циклу
            int[] route = new int[cityCount + 1]; // +1 для замикання циклу
            bool[] visited = new bool[cityCount];

            int current = 0, prev = -1;

            for (int i = 0; i < cityCount; i++)
            {
                route[i] = current;
                visited[current] = true;

                // Переходимо до наступного сусіда, що не є попереднім
                int next = graph[current].First(x => x != prev);
                prev = current;
                current = next;
            }

            route[cityCount] = route[0];
            sw.Stop();
            LastExecutionTime = sw.Elapsed;
            return new TSPResult
            {
                AlgorithmName = "Жадібний",
                Route = route,
                RouteLength = CalculateRouteLength(route),
                ExecutionTime = sw.Elapsed
            };
        }
        public TSPResult SolveNearestNeighbour(int start = 0) // Алгоритм найближчого сусіда
        {
            Stopwatch sw = Stopwatch.StartNew();
            bool[] visited = new bool[cityCount];
            int[] route = new int[cityCount + 1]; // +1 бо повертаємось в початок

            int current = start;
            visited[current] = true;
            route[0] = current;

            for (int i = 1; i < cityCount; i++) // Проходимо по всіх містах
            {
                int next = -1;
                double minDist = double.PositiveInfinity;

                for (int j = 0; j < cityCount; j++)
                {
                    if (!visited[j] && distanceMatrix[current, j] < minDist)
                    {
                        minDist = distanceMatrix[current, j];
                        next = j;
                    }
                }

                current = next;
                visited[current] = true;
                route[i] = current;
            }

            // Повертаємось до початкового міста
            route[cityCount] = start;
            sw.Stop();
            LastExecutionTime = sw.Elapsed;
            return new TSPResult
            {
                AlgorithmName = "Найближчий сусід",
                Route = route,
                RouteLength = CalculateRouteLength(route),
                ExecutionTime = sw.Elapsed
            };
        }
        public TSPResult SolveSA(double initialTemp = 10000, double coolingRate = 0.995, int iterationsPerTemp = 100) // Симульований відпал
        {
            Stopwatch sw = Stopwatch.StartNew();
            Random rand = new Random();

            // Початковий маршрут (0 → 1 → 2 → ... → n-1 → 0)
            int[] currentRoute = Enumerable.Range(0, cityCount).ToArray();
            currentRoute = currentRoute.Append(currentRoute[0]).ToArray(); // замкнене коло

            int[] bestRoute = (int[])currentRoute.Clone();
            double bestLength = CalculateRouteLength(bestRoute);
            double currentTemp = initialTemp;

            while (currentTemp > 1) // Поки температура не зменшиться до 1
            {
                for (int i = 0; i < iterationsPerTemp; i++)
                {
                    int[] newRoute = SwapCities(currentRoute); // Створення сусіднього маршруту
                    double newLength = CalculateRouteLength(newRoute);
                    double currentLength = CalculateRouteLength(currentRoute);
                    double delta = newLength - currentLength;

                    if (delta < 0 || rand.NextDouble() < Math.Exp(-delta / currentTemp))
                    {
                        currentRoute = newRoute;
                        if (newLength < bestLength)
                        {
                            bestRoute = (int[])newRoute.Clone();
                            bestLength = newLength;
                        }
                    }
                }

                currentTemp *= coolingRate;
            }
            sw.Stop();
            LastExecutionTime = sw.Elapsed;
            return new TSPResult
            {
                AlgorithmName = "Симульований відпал",
                Route = bestRoute,
                RouteLength = bestLength,
                ExecutionTime = sw.Elapsed
            };
        }
        public List<TSPResult> SolveAll() // Запуск всіх алгоритмів
        {
            List<TSPResult> results = new();

            results.Add(SolveNearestNeighbour());
            results.Add(SolveSA());
            results.Add(SolveGreedy());

            return results;
        }
        private int[] SwapCities(int[] route) // Генерація нового маршруту перестановкою двох міст
        {
            Random rand = new Random();
            int[] newRoute = (int[])route.Clone();

            // Вибираємо два випадкові індекси, але не перше і останнє (0 і n) — вони мають бути однакові
            int i = rand.Next(1, cityCount);
            int j = rand.Next(1, cityCount);
            while (i == j) j = rand.Next(1, cityCount);

            (newRoute[i], newRoute[j]) = (newRoute[j], newRoute[i]);
            return newRoute;
        }

        public double CalculateRouteLength(int[] route) // Обчислення довжини маршруту
        {
            double sum = 0;
            for (int i = 0; i < route.Length - 1; i++)
            {
                sum += distanceMatrix[route[i], route[i + 1]];
            }
            return sum;
        }
    }
}
