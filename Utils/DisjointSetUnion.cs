namespace TSPProject.Utils
{
    public class DisjointSetUnion
    {
        private int[] parent;

        public DisjointSetUnion(int size)
        {
            parent = new int[size];
            for (int i = 0; i < size; i++)
                parent[i] = i;
        }

        public int Find(int x) // Знаходить корінь множини, до якої належить x
        {
            if (parent[x] != x)
                parent[x] = Find(parent[x]);
            return parent[x];
        }

        public void Union(int x, int y) // Об'єднує множини, до яких належать x і y
        {
            int rx = Find(x);
            int ry = Find(y);
            if (rx != ry)
                parent[rx] = ry;
        }
    }

}
