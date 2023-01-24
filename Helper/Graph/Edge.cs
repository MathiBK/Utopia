using System.Drawing;

namespace utopia.Helper.Graph
{
    public class Edge
    {

        public Edge()
        {
            Index = -1;
            d0 = new Center();
            d1 = new Center();
            v1 = new Vertex();
            v0 = new Vertex();
        }
        public int Index;
        public Center d0;
        public Center d1;
        public Vertex v0;
        public Vertex v1;

        public Point Midpoint;
        public int River;

        public static void Test()
        {
          
            
        }
    }
}