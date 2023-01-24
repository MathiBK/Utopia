using System.Drawing;
using System.Collections.Generic;

namespace utopia.Helper.Graph
{
    public class Center
    {
        public Center(int index, Vector2 location, Hex tileHex, bool water, bool ocean, bool coast, bool border, string biome, double elevation, double moisture, List<Center> neighbors, List<Edge> borders, List<Vertex> vertices)
        {
            Index = index;
            Location = location;
            TileHex = tileHex;
            Water = water;
            Ocean = ocean;
            Coast = coast;
            Border = border;
            Biome = biome;
            Elevation = elevation;
            Moisture = moisture;
            Neighbors = neighbors;
            Borders = borders;
            Vertices = vertices;
        }
        
        public Center()
        {
            Index = -1;
            Location =default;
            TileHex = default;
            Water = default;
            Ocean = default;
            Coast = default;
            Border = default;
            Biome = default;
            Elevation = default;
            Moisture = default;
            Neighbors = new List<Center>();
            Borders = new List<Edge>();
            Vertices = new List<Vertex>();
        }

        
        public int Index;
        public Vector2 Location; //Sted
        public Hex TileHex;
        public bool Water;
        public bool Ocean;
        public bool Coast;
        public bool Border;
        public string Biome;
        public double Elevation;
        public double Moisture;

        public List<Center> Neighbors;
        public List<Edge> Borders;
        public List<Vertex> Vertices;

        public static void Test()
        {
          
            
        }
        
        public override string ToString() => $"(Center # {Index} Location: {Location.X}, {Location.Y} HexCoords: {TileHex.X}, {TileHex.Y}, {TileHex.Z})";
    }
}