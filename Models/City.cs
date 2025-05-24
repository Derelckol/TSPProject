namespace TSPProject.Models
{
    public class City
    {
        public int Id { get; } // Ідентифікатор міста
        public double X { get; } // Координата X
        public double Y { get; } // Координата Y
        public City(int id, double x, double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

    }
}
