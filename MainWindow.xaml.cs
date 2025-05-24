using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TSPProject.Logic;
using TSPProject.Models;
using TSPProject.Utils;

namespace TSPProject
{
    public partial class MainWindow : Window
    {
        private List<City> cities = new();
        private const double CityRadius = 6;
        private List<TSPResult> lastComparisonResults = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // Додає місто при кліку на канвас
        {
            if (cities.Count >= 100)
            {
                MessageBox.Show("Максимальна кількість міст — 100. Неможливо додати більше.", "Обмеження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ClearRouteLines(); // Очищаємо попередній маршрут

            // Отримуємо координати кліку
            Point p = e.GetPosition(DrawingCanvas);

            // Додаємо місто з унікальним Id
            var city = new City(cities.Count, p.X, p.Y);
            cities.Add(city);

            // Малюємо точку
            Ellipse circle = new()
            {
                Width = CityRadius * 2,
                Height = CityRadius * 2,
                Fill = Brushes.DarkBlue
            };
            Canvas.SetLeft(circle, p.X - CityRadius);
            Canvas.SetTop(circle, p.Y - CityRadius);
            DrawingCanvas.Children.Add(circle);

            // Підпис (номер міста)
            TextBlock label = new()
            {
                Text = city.Id.ToString(),
                FontSize = 12
            };
            Canvas.SetLeft(label, p.X + CityRadius);
            Canvas.SetTop(label, p.Y - CityRadius);
            DrawingCanvas.Children.Add(label);
        }

        private void ClearRouteLines() // Очищає маршрут
        {
            var linesToRemove = DrawingCanvas.Children.OfType<Line>().ToList();
            foreach (var line in linesToRemove)
            {
                DrawingCanvas.Children.Remove(line);
            }
        }
        private void DrawRoute(int[] route) // Малює маршрут між містами
        {
            for (int i = 0; i < route.Length - 1; i++)
            {
                City from = cities[route[i]];
                City to = cities[route[i + 1]];

                Line line = new()
                {
                    X1 = from.X,
                    Y1 = from.Y,
                    X2 = to.X,
                    Y2 = to.Y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };

                DrawingCanvas.Children.Add(line);
            }
        }
        private void ShowAlgorithmResultMessage(TSPResult result) // Виводить результат алгоритму
        {
            MessageBox.Show(
                $"Алгоритм: {result.AlgorithmName}\n" +
                $"Довжина: {result.RouteLength:F2}\n" +
                $"Час виконання: {result.ExecutionTime.TotalMilliseconds:F2} мс\n" +
                $"Шлях: {string.Join(" -> ", result.Route)}",
                $"{result.AlgorithmName} — результат"
            );
        }

        private void SolveGreedyButton_Click(object sender, RoutedEventArgs e) // Виконує жадібний алгоритм
        {
            if (cities.Count < 3)
            {
                MessageBox.Show("Потрібно принаймні 3 міста для побудови маршруту.");
                return;
            }

            ClearRouteLines();

            var matrix = Distance.BuildDistanceMatrix(cities);
            var solver = new TSPSolver(matrix);
            var result = solver.SolveGreedy();
            DrawRoute(result.Route);
            lastComparisonResults = new List<TSPResult> { result };

            ShowAlgorithmResultMessage(result);
        }


        private void SolveNNButton_Click(object sender, RoutedEventArgs e) // Виконує алгоритм найближчого сусіда
        {
            if (cities.Count < 3)
            {
                MessageBox.Show("Потрібно принаймні 3 міста для побудови маршруту.");
                return;
            }

            ClearRouteLines();

            var matrix = Distance.BuildDistanceMatrix(cities);
            var solver = new TSPSolver(matrix);
            var result = solver.SolveNearestNeighbour();
            DrawRoute(result.Route);
            lastComparisonResults = new List<TSPResult> { result };

            ShowAlgorithmResultMessage(result);
        }


        private void SolveSAButton_Click(object sender, RoutedEventArgs e) // Виконує алгоритм симульованого відпалу
        {
            if (cities.Count < 3)
            {
                MessageBox.Show("Потрібно принаймні 3 міста для побудови маршруту.");
                return;
            }

            ClearRouteLines();

            var matrix = Distance.BuildDistanceMatrix(cities);
            var solver = new TSPSolver(matrix);
            var result = solver.SolveSA();
            DrawRoute(result.Route);
            lastComparisonResults = new List<TSPResult> { result };

            ShowAlgorithmResultMessage(result);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) // Очищає канвас
        {
            // 1. Очистити візуальну частину
            DrawingCanvas.Children.Clear();

            // 2. Очистити список міст
            cities.Clear();
        }
        private void SaveResultsButton_Click(object sender, RoutedEventArgs e) // Зберігає результати в файл
        {
            if (lastComparisonResults == null || !lastComparisonResults.Any())
            {
                MessageBox.Show("Спочатку виконайте розв'язання.");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "TSP_Results",
                DefaultExt = ".txt",
                Filter = "Text files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                var matrix = Distance.BuildDistanceMatrix(cities);
                ResultExporter.SaveResults(dialog.FileName, lastComparisonResults, matrix);
                MessageBox.Show("Результати збережено успішно!", "Готово");
            }
        }
        private void CompareAllButton_Click(object sender, RoutedEventArgs e) // Порівнює всі алгоритми
        {
            if (cities.Count < 3)
            {
                MessageBox.Show("Потрібно принаймні 3 міста для побудови маршруту.");
                return;
            }

            ClearRouteLines();

            var matrix = Distance.BuildDistanceMatrix(cities);
            var solver = new TSPSolver(matrix);
            lastComparisonResults = solver.SolveAll();

            var sb = new StringBuilder();
            foreach (var result in lastComparisonResults)
            {
                sb.AppendLine($"Алгоритм: {result.AlgorithmName}");
                sb.AppendLine($"Довжина: {result.RouteLength:F2}");
                sb.AppendLine($"Час: {result.ExecutionTime.TotalMilliseconds:F2} мс");
                sb.AppendLine($"Шлях: {string.Join(" -> ", result.Route)}");
                sb.AppendLine();
            }
            // Знаходимо найкращий маршрут
            var best = lastComparisonResults.OrderBy(r => r.RouteLength).First();
            DrawRoute(best.Route);
            MessageBox.Show(sb.ToString(), "Порівняння алгоритмів");
            MessageBox.Show(
                $"Найкращий маршрут знайдено алгоритмом: {best.AlgorithmName}\n" +
                $"Довжина: {best.RouteLength:F2}\n" +
                $"Час: {best.ExecutionTime.TotalMilliseconds:F2} мс",
                "Найкращий результат"
            );
        }

    }

}