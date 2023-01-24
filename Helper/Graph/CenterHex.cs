using System;
using System.Collections.Generic;

namespace utopia.Helper.Graph
{
    public class CenterHex
    {
        
        public CenterHex(int x, int y)
        {
            X = x;
            Y = y;
            Z = -x-y;
            if (x+y+Z != 0) throw new ArgumentException("x+y+z must be 0!");
        }
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        

        static public List<CenterHex> Directions = new List<CenterHex>{new CenterHex(1, 0), new CenterHex(1, -1), new CenterHex(0, -1), new CenterHex(-1, 0), new CenterHex(-1, 1), new CenterHex(0, 1)};


        public override string ToString() => $"({X}, {Y}, {Z})";

        public CenterHex Add(CenterHex b)
        {
            return new CenterHex(X+b.X, Y+b.Y);
        }
        public CenterHex Subtract(CenterHex b)
        {
            return new CenterHex(X - b.X, Y - b.Y);
        }


        public CenterHex Scale(int k)
        {
            return new CenterHex(X * k, Y * k);
        }


        public CenterHex RotateLeft()
        {
            return new CenterHex(-Z, -X);
        }


        public CenterHex RotateRight()
        {
            return new CenterHex(-Y, -Z);
        }


        static public CenterHex Direction(int direction)
        {
            return CenterHex.Directions[direction];
        }


        public CenterHex Neighbor(int direction)
        {
            return Add(CenterHex.Direction(direction));
        }

        static public List<CenterHex> diagonals = new List<CenterHex>{new CenterHex(2, -1), new CenterHex(1, -2), new CenterHex(-1, -1), new CenterHex(-2, 1), new CenterHex(-1, 2), new CenterHex(1, 1)};

        public CenterHex DiagonalNeighbor(int direction)
        {
            return Add(CenterHex.diagonals[direction]);
        }


        public int Length()
        {
            return (int)((Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z)) / 2);
        }


        public int Distance(CenterHex b)
        {
            return Subtract(b).Length();
        }
        
        public static void Test() 
        {
            Dictionary<CenterHex, float> heights = new Dictionary<CenterHex, float>();
            CenterHex CenterHexTest = new CenterHex(1, 2);
            heights.Add(CenterHexTest, 4.3f);
            Console.WriteLine(heights[CenterHexTest]);
            Console.WriteLine(CenterHexTest.GetHashCode());

            HashSet<CenterHex> map = new HashSet<CenterHex>();
            int map_radius = 10;
            for (int q = -map_radius; q <= map_radius; q++) 
            {
                int r1 = Math.Max(-map_radius, -q - map_radius);
                int r2 = Math.Min(map_radius, -q + map_radius);
                for (int r = r1; r <= r2; r++) 
                {
                    map.Add(new CenterHex(q, r));
                }
            }

            foreach (var v in map)
            {
                Console.WriteLine("----");
                Console.WriteLine(v);
                Console.WriteLine(v.Neighbor(1));
                Console.WriteLine("----");
                
            }
            
        }
    }
    

}

  