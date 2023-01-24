using System.Collections.Generic;
using System.Drawing;

namespace utopia.Helper.Graph
{
    public class Vertex
    {
        public Vertex()
        {
            Index = -1;
            Location = new Vector2(0, 0);
            Water = default;
            Ocean = default;
            Coast = default;
            Border = default;
            Elevation = default;
            Moisture = default;
            Touches = default;
            Protrudes = default;
            Adjacent = new HashSet<Vertex>();
            Touches = new List<Center>();
            Protrudes = new List<Edge>();
        }

        public int Index;
        public Vector2 Location; //Sted
        public bool Water;
        public bool Ocean;
        public bool Coast;
        public bool Border;
        public double Elevation;
        public double Moisture;
        public int River;

        public List<Center> Touches;
        public List<Edge> Protrudes;
        public HashSet<Vertex> Adjacent;
        

        public Vertex Watershed;
        public Vertex Downslope;
        public int WatershedSize;

        public static void Test()
        {
        }

        public override string ToString() => $"(Vertex: # {Index} Location: {Location.X}, {Location.Y}";

        public override bool Equals(object obj)
        {
            Vertex q = obj as Vertex;
            return q != null && Index == q.Index && Location == q.Location;
        }

        public override int GetHashCode()
        {
            return this.Location.X.GetHashCode() ^ this.Location.Y.GetHashCode() * this.Index;
        }
    }
}