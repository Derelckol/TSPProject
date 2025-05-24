namespace TSPProject.Models
{
    public class TSPResult
    {
        public string AlgorithmName { get; set; } = string.Empty; // Назва алгоритму
        public int[] Route { get; set; } = Array.Empty<int>(); // Шлях
        public double RouteLength { get; set; } // Довжина маршруту
        public TimeSpan ExecutionTime { get; set; } // Час виконання алгоритму
    }
}
