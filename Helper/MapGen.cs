using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Timers;
using utopia.Helper.Graph;
using utopia.Models;


namespace utopia.Helper
{
    public class Vector2
    {
        
        public Vector2()
        {
        }
        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public override string ToString() => $"({X}, {Y})";
        
        public override bool Equals(object obj)
        {
            Vector2 q = obj as Vector2;
            return q != null && q.X.Equals(X) && q.Y.Equals(Y);
        }
        
        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }
    }

    public struct Vector3
    {
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public override string ToString() => $"({X}, {Y}, {Z})";
    }

    public struct Hex
    {
        public Hex(int x, int y)
        {
            X = x;
            Y = y;
            Z = -x - y;
            if (x + y + Z != 0) throw new ArgumentException("x+y+z must be 0!");
            Water = false;
            Ocean = false;
            Coast = false;
            Border = false;
            Biome = "none";
            Elevation = 0.0;
            Moisture = 0.0;

        }

        public bool Water { get; }
        public bool Ocean { get; }
        public bool Coast { get; }
        public bool Border{ get; set; }
        public string Biome{ get; }
        public double Elevation{ get; }
        public double Moisture{ get; }
        public int X { get; }
        public int Y { get; }
        public int Z { get; }


        static public List<Hex> Directions = new List<Hex>
            {new Hex(1, 0), new Hex(1, -1), new Hex(0, -1), new Hex(-1, 0), new Hex(-1, 1), new Hex(0, 1)};


        public override string ToString() => $"({X}, {Y}, {Z})";

        public bool HexEqual(Hex b)
        {
            return X == b.X && Y == b.Y;
        }

        public Hex Add(Hex b)
        {
            return new Hex(X + b.X, Y + b.Y);
        }

        public Hex Subtract(Hex b)
        {
            return new Hex(X - b.X, Y - b.Y);
        }


        public Hex Scale(int k)
        {
            return new Hex(X * k, Y * k);
        }


        public Hex RotateLeft()
        {
            return new Hex(-Z, -X);
        }


        public Hex RotateRight()
        {
            return new Hex(-Y, -Z);
        }


        static public Hex Direction(int direction)
        {
            return Hex.Directions[direction];
        }


        public Hex Neighbor(int direction)
        {
            return Add(Hex.Direction(direction));
        }

        static public List<Hex> diagonals = new List<Hex>
            {new Hex(2, -1), new Hex(1, -2), new Hex(-1, -1), new Hex(-2, 1), new Hex(-1, 2), new Hex(1, 1)};

        public Hex DiagonalNeighbor(int direction)
        {
            return Add(Hex.diagonals[direction]);
        }


        public int Length()
        {
            return (int) ((Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z)) / 2);
        }


        public int Distance(Hex b)
        {
            return Subtract(b).Length();
        }

        public static int Length(int hexX, int hexY)
        {
            return (int) ((Math.Abs(hexX) + Math.Abs(hexY) + Math.Abs((-hexX)-hexY)) / 2);
        }

        public static int Distance(int hexOneX, int hexOneY, int hexTwoX, int hexTwoY)
        {
            int newX = hexOneX-hexTwoX;
            int newY = hexOneY-hexTwoY;
            return Length(newX, newY);
        }
    }



    

    public class MapGen
    {
        public int MAPSIZE;
        public double MAPSIZEX;
        public double MAPSIZEY;
        public int HEXNUM;
        public double HEXWIDTH;
        public int NumPoints;
        public Island Island;

        public List<Center> Centers;
        public List<Point> Points;
        public List<Vertex> Vertices;
        public List<Vector2> Corners;
        public List<Edge> Edges;
        private Stopwatch _Timer;

        public Dictionary<Vector3, Hex> HexDict;

        public MapGen(int hexNum, int seed)
        {
            MAPSIZEY = Double.MinValue;
            MAPSIZEX = Double.MinValue;
            HEXNUM = hexNum;
            //HEXWIDTH = MAPSIZE/Math.Sqrt(HEXNUM);
            HEXWIDTH = 5;
            NumPoints = 1;
            
            //TODO: finne ut hvordan man lager kvadratisk hexmap, ikke førstepri
            HexDict = GenerateHexDict((int)Math.Floor(Math.Sqrt(HEXNUM)), (int)Math.Floor(Math.Sqrt(HEXNUM)));
            Centers = new List<Center>();
            Vertices = new List<Vertex>();
            Corners = new List<Vector2>();
            Edges = new List<Edge>();
            Island = new Island(seed);
            
            _Timer = new Stopwatch();
            BuildMap();

        }

        //Hjelpefunksjon for å time
        private void TimerHelp(Stopwatch total, Stopwatch timer)
        {
            Console.Write("Total time elapsed: " + total.ElapsedMilliseconds + 
                          "\nTime elapsed for current stage: " + timer.ElapsedMilliseconds + "\n");
            timer.Restart();
        }
        
        
        //Hjelpefunksjon som passer på at alt gjøres i riktig rekkefølge

        public void BuildMap()
        {
            
            var _TotalTimer = new Stopwatch();
            _TotalTimer.Start();
            _Timer.Start();
            
            Console.WriteLine("Building graph...");
            BuildGraph();
            //TimerHelp(_TotalTimer, _Timer);

            Console.WriteLine("Assigning vert elevations...");
            AssignVertElev();
            //TimerHelp(_TotalTimer, _Timer);
            Console.WriteLine("Assigning ocean, coast and land...");
            AssignOceanCoastLand();
            //TimerHelp(_TotalTimer, _Timer);
            Console.WriteLine("Redistributing elevation...");
            RedistributeElevations(LandCorners(Vertices));
            //TimerHelp(_TotalTimer, _Timer);
            
            foreach (var vert in Vertices) 
            {
                if (vert.Ocean || vert.Coast) 
                {
                    vert.Elevation = 0.0;
                }
            }
            
            Console.WriteLine("Assigning center elevations...");
            AssignCenterElevations();
            //TimerHelp(_TotalTimer, _Timer);
            
            Console.WriteLine("Computing downslopes...");
            ComputeDownslopes();
            //TimerHelp(_TotalTimer, _Timer);
            Console.WriteLine("Computing Watersheds...");
            ComputeWatersheds();
            //TimerHelp(_TotalTimer, _Timer);
            Console.WriteLine("Creating rivers...");
            CreateRivers();
            //TimerHelp(_TotalTimer, _Timer);
            
            Console.WriteLine("Assigning vert moisture...");
            AssignVertMoisture();
            //TimerHelp(_TotalTimer, _Timer);
            Console.WriteLine("Redistributing moisture...");
            RedistributeMoisture(LandCorners(Vertices));
            //TimerHelp(_TotalTimer, _Timer);
            Console.WriteLine("Assigning center moisture...");
            AssignCenterMoisture();
            //TimerHelp(_TotalTimer, _Timer);
            
            Console.WriteLine("Assigning biomes...");
            AssignBiomes();
            //TimerHelp(_TotalTimer, _Timer);
            _TotalTimer.Stop();
            _Timer.Stop();
        }

        
        public HashSet<Hex> GenerateHexes(int mapHeight, int mapWidth)
        {
            HashSet<Hex> map = new HashSet<Hex>();
            for (int q = 0; q < mapWidth; q++)
            {
                int qOffset = q / 2;
                for (int r = -qOffset; r < mapHeight - qOffset; r++)
                {
                    Hex hex = new Hex(q, r);
                    if (r == 0 || r == mapHeight - 1 || q == 0 || q == mapWidth - qOffset - 1)
                        hex.Border = true;
                    
                    map.Add(hex);
                }
            }

            return map;
        }
        
        public Dictionary<Vector3, Hex> GenerateHexDict(int mapHeight, int mapWidth)
        {
            Dictionary<Vector3, Hex> map = new Dictionary<Vector3, Hex>();
            for (int q = 0; q < mapWidth; q++)
            {
                int qOffset = q / 2;
                for (int r = -qOffset; r < mapHeight - qOffset; r++)
                {
                    Hex hex = new Hex(q, r);
                    if (r == 0 || r == mapHeight - 1 || q == 0 || q == mapWidth - qOffset - 1)
                        hex.Border = true;
                    
                    map.Add(new Vector3(hex.X, hex.Y, hex.Z), hex);
                }
            }

            return map;
        }

        
        //Brukes ikke
        public List<PointF> generateHexagonPoints(int size, int seed, int numPoints)
        {

            List<PointF> Points = new List<PointF>();

                int N = (int)Math.Floor(Math.Sqrt(numPoints));

                for (int x = 0; x < N; x++) {

                    for (int y = 0; y < N; y++) {

                        Points.Add(new PointF((0.5f + x)/N * size, (0.25f + 0.5f*x%2f + y)/N * size));

                    }

                }

                return Points;

        }

        public void BuildGraph()
        {
            
            //Finner kantpunkter for hver hexagon, lagrer
            foreach (var hexVal in HexDict)
            {
                var p = new Center();
                p.Index = Centers.Count;
                p.Location = ConvertFromHexCoords(hexVal.Value, HEXWIDTH);
                p.TileHex = hexVal.Value;
                p.Neighbors = new List<Center>();
                p.Borders = new List<Edge>();
                var tempV = FetchVertices(ComputeHexVerticesFromFace(p.Location, HEXWIDTH));
                var tempC = ComputeHexVerticesFromFace(p.Location, HEXWIDTH);
                Corners.AddRange(tempC);
                Centers.Add(p);

            }

            Corners = Corners.Distinct().ToList();
            
            
            //Gjør om punkter til vertexer
            foreach (var corner in Corners)
            {
                var tempVert = new Vertex();
                tempVert.Location = corner;
                tempVert.Index = Vertices.Count;
                Vertices.Add(tempVert);

                if (corner.X > MAPSIZEX)
                    MAPSIZEX = corner.X;
                if (corner.Y > MAPSIZEY)
                    MAPSIZEY = corner.Y;
            }

            Vertices = Vertices.Distinct().ToList();
            
            //Setter vertexer og kanter til hexagons
            foreach (var c in Centers)
            {
                var tempC = ComputeHexVerticesFromFace(c.Location, HEXWIDTH);
                foreach (var v in Vertices)
                {
                    var vecFind = tempC.Find(x => x.Equals(v.Location));
                    if (vecFind != null)
                    {
                        v.Touches.Add(c);
                        c.Vertices.Add(v);
                    }
                }
                
                for (int i = 0; i < 6; i++)
                {
                    var current = Vertices.Find(x => x.Location.Equals(tempC[i]));
                    var next = Vertices.Find(x => x.Location.Equals(tempC[(i + 1) % tempC.Count]));
                    var prev = Vertices.Find(x => x.Location.Equals(tempC[(i + 5) % tempC.Count]));
                    var tempEdge = new Edge();
                    
                    tempEdge.v0 = current;
                    tempEdge.v1 = next;
                    current.Adjacent.Add(next);
                    current.Adjacent.Add(prev);

                    c.Borders.Add(tempEdge);
                }
                
            }
            
            //Setter nabohexagons
            foreach (var p in Centers)
            {
   
                for(int i = 0; i < 6; i++)
                {
                    var currentNeigh = p.TileHex.Neighbor(i);
                    if (HexDict.ContainsKey(new Vector3(currentNeigh.X, currentNeigh.Y,currentNeigh.Z)))
                    {

                        var neighPoint = Centers.Find(x => x.TileHex.HexEqual(currentNeigh));
                        if (neighPoint != null)
                        {
                            p.Neighbors.Add(neighPoint);
                        }
                        else
                        {
                            throw new Exception("HexDict and Centers not in sync!");
                        }
                    }
           
                }

       

            }

            //Lagrer alle kanter
            foreach (var vert in Vertices)
            {
                foreach (var neigh in vert.Adjacent)
                {
                    Edge edge = new Edge();
                    edge.v0 = vert;
                    edge.v1 = neigh;
                    edge.Index = Edges.Count;
                    vert.Protrudes.Add(edge);
                    neigh.Protrudes.Add(edge);
                    Edges.Add(edge);
                }
            }
            Edges = Edges.Distinct().ToList();
        }
        
        
        //Returnerer alle vertexer som ikke er hav eller kyst
        public List<Vertex> LandCorners(List<Vertex> vertices)
        {
 
            List<Vertex> locations = new List<Vertex>();
            foreach (var q in vertices) 
            {
                if (!q.Ocean && !q.Coast) 
                {
                    locations.Add(q);
                }
            }
            return locations;
        }
        
        //Returnerer sant om et punkt er inne i øya
        public bool Inside(Vertex Point) 
        {
            Vertex temp = new Vertex();
            temp.Location = new Vector2(2*(Point.Location.X/MAPSIZEX - 0.5), 2*(Point.Location.Y/MAPSIZEY - 0.5));
            return Island.Inside(temp);
        }
        
        //Førster gjennomgang av høyde for vertexer
        public void AssignVertElev()
        {
            Random random = new Random();
            var queue = new Queue<Vertex>();

            //Må loope separat for å initialisere vann; bruker det etterpå
            foreach (var vert in Vertices)
            {
                vert.Water = !Inside(vert);
            }

            foreach (var vert in Vertices)
            {
                if (vert.Adjacent.Count < 3)
                {
                    vert.Border = true;
                  
                }

                if (vert.Border)
                {
                    vert.Elevation = 0.0;
                    queue.Enqueue(vert);
                }
                else
                {
                    vert.Elevation = Double.PositiveInfinity;
                }
            }

            while (queue.Count != 0)
            {
                Vertex v = queue.Dequeue();

                foreach (var neigh in v.Adjacent)
                {
                    double newElevation = 0.01 + v.Elevation;
                    if (!v.Water && !neigh.Water)
                    {
                        newElevation += 1;
                        newElevation += random.NextDouble();
                    }

                    if (newElevation < neigh.Elevation)
                    {
                        neigh.Elevation = newElevation;
                        queue.Enqueue(neigh);
                    }
                }
            }

        }

        //Setter hexagonhøyde
        public void AssignCenterElevations()
        {
            Random random = new Random();
            var queue = new Queue<Vertex>();
            double elevationSum;
            foreach (var center in Centers)
            {
                elevationSum = 0.0;
                foreach (var vert in center.Vertices)
                {
                    elevationSum += vert.Elevation;
                }

                center.Elevation = elevationSum / center.Vertices.Count;

            }
        }

        
        //Normaliserer høyden og setter sannsynligheten for en gitt høyde X som (1-X)
        public void RedistributeElevations(List<Vertex> locations)
        {
            double SCALE_FACTOR = 1.1;

            List<Vertex> sortedLocations = locations.OrderBy(l => l.Elevation).ToList();
            for (int i = 0; i < sortedLocations.Count; i++)
            {
                double y = (double)i / (sortedLocations.Count - 1);
                double x = Math.Sqrt(SCALE_FACTOR) - Math.Sqrt(SCALE_FACTOR * (1 - y));
                if (x > 1.0)
                    x = 1.0;
                
                sortedLocations[i].Elevation = x;
            }
        }      
            
        public void RedistributeMoisture(List<Vertex> locations)
        {
            List<Vertex> sortedLocations = locations.OrderBy(l=> l.Moisture).ToList();
            for (int i = 0; i < sortedLocations.Count; i++) 
            {
                sortedLocations[i].Moisture = (double)i/(sortedLocations.Count-1);
            }
        }
        
        //Setter kyst, hav og land for vertexer
        public void AssignOceanCoastLand()
        {
            var queue = new Queue<Center>();
            int waters;
            foreach (var center in Centers)
            {
                waters = 0;
                foreach (var vert in center.Vertices)
                {
                    if (vert.Border)
                    {
                        center.Border = true;
                        center.Ocean = true;
                        vert.Water = true;
                        queue.Enqueue(center);
                        
                    }

                    if (vert.Water)
                    {
                        waters += 1;
                    }
                }

                center.Water = (center.Ocean || waters > center.Vertices.Count * 0.3);
            }

            while (queue.Count > 0)
            {
                Center center = queue.Dequeue();
                foreach (var neigh in center.Neighbors)
                {
                    if (neigh.Water && !neigh.Ocean)
                    {
                        neigh.Ocean = true;
                        queue.Enqueue(neigh);
                    }
                }
            }
            
            //Setter om tile er kyst eller ikke, ved å se på nabotiles
            foreach (var center in Centers)
            {
                int Oceans = 0;
                int Lands = 0;
                foreach (var neigh in center.Neighbors)
                {
                    if (neigh.Ocean)
                        Oceans += 1;
                    if (!neigh.Water)
                        Lands += 1;
                }

                center.Coast = (Oceans > 0) && (Lands > 0);
            }

            foreach (var vert in Vertices)
            {
                int Oceans = 0;
                int Lands = 0;
                foreach (var neigh in vert.Touches)
                {
                    if (neigh.Ocean)
                        Oceans += 1;
                    if (!neigh.Water)
                        Lands += 1;
                }

                vert.Ocean = (Oceans == vert.Touches.Count);
                vert.Coast = (Oceans > 0) && (Lands > 0);
                vert.Water = vert.Border || ((Lands != vert.Touches.Count) && !vert.Coast);
            }
            
        }
        
        
        //Sjekker hvilken vertex som har størst nedgående høydedifferensial
        public void ComputeDownslopes()
        {
            foreach (var vert in Vertices)
            {
                var r = vert;
                foreach (var s in vert.Adjacent)
                {
                    if (s.Elevation <= r.Elevation)
                    {
                        r = s;
                    }
                }

                vert.Downslope = r;
            }
        }

        
        //Finner ut hvor elver skal renne
        public void ComputeWatersheds()
        {
            foreach (var vert in Vertices)
            {
                vert.Watershed = vert;
                if (!vert.Ocean && !vert.Coast)
                {
                    vert.Watershed = vert.Downslope;
                }
            }
            
            for (int i = 0; i < 100; i++) {
                bool changed = false;
                foreach (var q in Vertices) {
                    if (!q.Ocean && !q.Coast && !q.Watershed.Coast) {
                        var r = q.Downslope.Watershed;
                        if (!r.Ocean) {
                            q.Watershed = r;
                            changed = true;
                        }
                    }
                }
                if (!changed) break;
            }
            // Hvor stort vannskille?
            foreach (var q in Vertices) {
                var r = q.Watershed;
                r.WatershedSize = 1 + (r.WatershedSize);
            }
            
        }
        
        //Danner elver
        public void CreateRivers()
        {
            for (int i = 0; i < MAPSIZEX/2; i++) {
                var q = Vertices[Island.islandRandom.Next(0, Vertices.Count-1)];
                if (q.Ocean || q.Elevation < 0.3 || q.Elevation > 0.9) continue;
                while (!q.Coast) {
                    if (q == q.Downslope) {
                        break;
                    }
                    var edge = LookupEdgeFromVertices(q, q.Downslope);
                    edge.River = edge.River + 1;
                    q.River += 1;
                    q.Downslope.River = q.Downslope.River + 1;  // TODO: fix double count
                    q = q.Downslope;
                }
            }
        }
        
        //Setter fuktighet på vertexer, brukes for å regne hexagonfuktighet
        public void AssignVertMoisture()
        {
            double newMoisture;
            var queue = new Queue<Vertex>();
            
            // Ferskvann (innsjø og elv) gir fuktighet
            foreach (var q in Vertices) {
                if ((q.Water || q.River > 0) && !q.Ocean) {
                    q.Moisture = q.River > 0 ? Math.Min(3.0, (0.2 * q.River)) : 1.0;
                    queue.Enqueue(q);
                } else {
                    q.Moisture = 0.0;
                }
            }
            while (queue.Count > 0) {
                var q = queue.Dequeue();

                foreach (var r in q.Adjacent) {
                    newMoisture = q.Moisture * 0.9;
                    if (newMoisture > r.Moisture) {
                        r.Moisture = newMoisture;
                        queue.Enqueue(r);
                    }
                }
            }
            // Saltvann gir ikke fuktighet
            foreach (var q in Vertices) 
            {
                if (q.Ocean || q.Coast) 
                {
                    q.Moisture = 1.0;
                }
            }
        }
        
        //Setter hexagonfuktigheten utifra vertexfuktighet
        public void AssignCenterMoisture()
        {
            foreach (var p in Centers) 
            {
                double sumMoisture = 0.0;
                foreach (var q in p.Vertices) 
                {
                    if (q.Moisture > 1.0) 
                        q.Moisture = 1.0;
                    sumMoisture += q.Moisture;
                }
                p.Moisture = sumMoisture / p.Vertices.Count;
            }
        }
        
        //Henter biome uifra høyde og fuktighet (Whittaker diagram)
        static public string GetBiome(Center p) {
            if (p.Ocean) {
                return "OCEAN";
            } else if (p.Water) {
                if (p.Elevation < 0.1) return "MARSH";
                if (p.Elevation > 0.8) return "ICE";
                return "LAKE";
            } else if (p.Coast) {
                return "BEACH";
            } else if (p.Elevation > 0.8) {
                if (p.Moisture > 0.50) return "SNOW";
                else if (p.Moisture > 0.33) return "TUNDRA";
                else if (p.Moisture > 0.16) return "BARE";
                else return "SCORCHED";
            } else if (p.Elevation > 0.6) {
                if (p.Moisture > 0.66) return "TAIGA";
                else if (p.Moisture > 0.33) return "SHRUBLAND";
                else return "TEMPERATE_DESERT";
            } else if (p.Elevation > 0.3) {
                if (p.Moisture > 0.83) return "TEMPERATE_RAIN_FOREST";
                else if (p.Moisture > 0.50) return "TEMPERATE_DECIDUOUS_FOREST";
                else if (p.Moisture > 0.16) return "GRASSLAND";
                else return "TEMPERATE_DESERT";
            } else {
                if (p.Moisture > 0.66) return "TROPICAL_RAIN_FOREST";
                else if (p.Moisture > 0.33) return "TROPICAL_SEASONAL_FOREST";
                else if (p.Moisture > 0.16) return "GRASSLAND";
                else return "SUBTROPICAL_DESERT";
            }
        }
        
        //Setter biome
        public void AssignBiomes() 
        {
            foreach (var p in Centers) 
            {
                p.Biome = GetBiome(p);
            }
        }

        
        //Henter kant fra vertexer
        public Edge LookupEdgeFromVertices(Vertex vertOne, Vertex vertTwo)
        {
            foreach (var edge in vertOne.Protrudes) {
                if (edge.v0 == vertTwo || edge.v1 == vertTwo) return edge;
            }
            return null;
        }

        
        //Konverter fra hexagonale akser til kartesianske
        public static Vector2 ConvertFromHexCoords(Hex h, double size)
        {
            double hexagonNarrow = 2 * size * 0.75;
            double hexagonHeight = size * Math.Sqrt(3);
            Vector2 v = new Vector2();
            Vector2 i = new Vector2(hexagonNarrow, 0.5 * hexagonHeight);
            Vector2 j = new Vector2(0, hexagonHeight);

            v.X = i.X * h.X + j.X * h.Y + size;
            v.Y = i.Y * h.X + j.Y * h.Y;
            return v;
        }

        public static List<Vector2> FetchCorners(Hex h, double size)
        {
            List<Vector2> CornerPoints = new List<Vector2>();
            for (int i = 0; i < 6; i++)
            {
                var angle_deg = 60 * i;
                var angle_rad = Math.PI / 180 * angle_deg;
                CornerPoints.Add( new Vector2(h.X + size * Math.Cos(angle_rad),
                    h.Y + size * Math.Sin(angle_rad)));
            }

            return CornerPoints;
        }
        
        
        //Tror ikke jeg bruker denne lenger, men tør ikke fjerne
        public List<Vertex> FetchVertices(List<Vector2> vert)
        {
            List<Vertex> v = new List<Vertex>();

            for(int i = 0; i < vert.Count; i++)
            {
                Vertex temp = new Vertex();
                temp.Location = vert[i];
                v.Add(temp);
                
            }

            return v;
        }
        
        //Regner ut koordinater for kantene til en hexagon
        //koordinater må være standard, ikke hex
        public static List<Vector2> ComputeHexVerticesFromFace(Vector2 h, double size)
        {
            double hexagonHeight = size * Math.Sqrt(3);
            
            var fixY1= Math.Round(h.Y + hexagonHeight / 2, 2);
            var fixY2= Math.Round(h.Y - hexagonHeight / 2, 2);
            var fixY3= Math.Round(h.Y, 2);
            
            var fixX1= Math.Round(h.X - size, 2);
            var fixX2= Math.Round(h.X + size, 2);
            var fixX3= Math.Round(h.X + size / 2, 2);
            var fixX4= Math.Round(h.X - size / 2, 2);
            
            List<Vector2> vertices = new List<Vector2>();
            Vector2 vertexL = new Vector2(fixX1, fixY3);
            Vector2 vertexR = new Vector2(fixX2, fixY3);

           
            Vector2 vertexTR = new Vector2(fixX3, fixY1);
            Vector2 vertexTL = new Vector2(fixX4, fixY1);
            
            Vector2 vertexBR = new Vector2(fixX3, fixY2);
            Vector2 vertexBL = new Vector2(fixX4, fixY2);

            vertices.Add(vertexL);
           
            vertices.Add(vertexBL);
            vertices.Add(vertexBR);
            vertices.Add(vertexR);
            vertices.Add(vertexTR);
            vertices.Add(vertexTL);

            return vertices;
        }
        
        
        
        
        //----
        //Syk testsuite
        public static void Test()
        {
            MapGen m = new MapGen(2500, 89134);
            /*
\
            Console.WriteLine(m.Centers[78]);
            Console.WriteLine(m.Centers[78].Location);
            Console.WriteLine(m.Centers[78].Neighbors[0]);
            foreach (var n in  m.Centers[78].Neighbors)
            {
                Console.WriteLine(n);
            }
            Console.WriteLine("-------VertHash------");
            foreach (var n in  m.Corners)
            {
                Console.WriteLine("(" + n.X + "," + n.Y + ")");
            }
            
            
            Console.WriteLine("-------NewHexCoords------");
            foreach (var n in  m.HexDict)
            {
                var temp = FetchCorners(n.Value, m.HEXWIDTH);
                foreach (var e in temp)
                {
                   // Console.WriteLine("(" + e.X + "," + e.Y + ")");
                }
                
            }
            
            Console.WriteLine("-------VertTest------");
            foreach (var v in m.Centers[78].Vertices)
            {
                Console.WriteLine("(" + v.Location.X + "," + v.Location.Y + ")");
            }
            
            foreach (var v in m.Vertices)
            {
                Console.WriteLine("(" + v.Location.X + "," + v.Location.Y + ")");
            }
            
            Console.WriteLine("-------EdgeTest------");
            foreach (var v in m.Centers[78].Borders)
            {
                Console.WriteLine("(" + v.v0.Location.X + "," + v.v0.Location.Y + ")");
                Console.WriteLine("(" + v.v1.Location.X + "," + v.v1.Location.Y + ")");
            }
            
            Console.WriteLine("-------AdjacentTest------");
            Console.WriteLine("(" + m.Centers[78].Borders[0].v0.Location.X + "," + m.Centers[78].Borders[0].v0.Location.Y + ")");
            foreach (var v in m.Centers[78].Borders[0].v0.Adjacent)
            {
                Console.WriteLine("(" + v.Location.X + "," + v.Location.Y + ")");
            }
            
            
            Console.WriteLine("-------IslandTest------");
            foreach (var v in m.Vertices)
            {
                if (!v.Water)
                {
                    Console.WriteLine("(" + v.Location.X + "," + v.Location.Y + ")");
                }
                
                
            }   
            */
            
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@"Helper\Island.txt"))
            {
                
                foreach (var v in m.Vertices)
                {
                    if (!v.Water)
                    {
                        file.WriteLine("(" + v.Location.X + "," + v.Location.Y + ")");
                    }
                }
            }
            
            Console.WriteLine("-------WaterTest------");
            foreach (var v in m.Vertices)
            {
                if (v.Water)
                {
                    Console.WriteLine("(" + v.Location.X + "," + v.Location.Y + ")");
                }
            }
            
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@"Helper\Water.txt"))
            {
                
                foreach (var v in m.Vertices)
                {
                    if (v.Water)
                    {
                        file.WriteLine("(" + v.Location.X + "," + v.Location.Y + ")");
                    }
                }
            }
            using (System.IO.StreamReader file =
                new System.IO.StreamReader(@"Helper\Water.txt"))
            {
                file.ReadLine();
            }
            
            Console.WriteLine("-------ElevationTest------");
            foreach (var v in m.Vertices)
            {
                if(v.Ocean || v.Coast)
                    Console.WriteLine("(OceanCost: "+v.Elevation+")");
                else
                {
                    Console.WriteLine("(Land: "+v.Elevation+")");
                }

            }
            
            Console.WriteLine("-------RiverTest------");
            foreach (var v in m.Vertices)
            {
                if(v.River > 0)
                    Console.WriteLine("(" + v.Location.X + "," + v.Location.Y + ")");

            }
            
            
            Console.WriteLine("-------MoistureTestVert------");
            foreach (var v in m.Vertices)
            {
                if(!v.Ocean)
                    Console.WriteLine("("+ v.Moisture + ")");

            }
            Console.WriteLine("-------MoistureTestCenter------");
            foreach (var v in m.Centers)
            {
                if(!v.Ocean)
                    Console.WriteLine("("+ v.Moisture + ")");

            }
            
            Console.WriteLine("-------BiomeCheck------");

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@"Helper\Biomes.txt"))
            {
                
                foreach (var v in m.Centers)
                {
                    file.WriteLine("(" + v.Biome + ")");
                }
            }
            



           
        }
    }

    
    //Hjelpeklasse som holder styr på selve øyen
    public class Island
    {

        public Island(int seed)
        {
            islandRandom = new Random(seed);
            bumps = islandRandom.Next(1, 6);
            startAngle = islandRandom.NextDouble() * (2 * Math.PI);
            dipAngle = islandRandom.NextDouble() * (2 * Math.PI);
            dipWidth = (islandRandom.NextDouble()*0.5)-0.2;
        }
        
        private double ISLAND_FACTOR = 1.07;
        public Random islandRandom;
        private double bumps;
        private double startAngle;
        private double dipAngle;
        private double dipWidth;

        
        //Funksjon som sier om et punkt hører hjemme innenfor eller utenfor øyen (basert på overlappende sinusfunksjoner)
        public bool Inside(Vertex point)
        {
            
            //Lengde fra origo til normalisert punkt (mellom -1 og 1, begge akser)
            var pointLength = Math.Sqrt(point.Location.X * point.Location.X + point.Location.Y * point.Location.Y);

            //Vinkel 
            var angle = Math.Atan2(point.Location.Y, point.Location.X);
            
            var length = 0.5 * (Math.Max(Math.Abs(point.Location.X), Math.Abs(point.Location.Y)) + pointLength);

            var r1 = 0.5 + 0.40 * Math.Sin(startAngle + bumps * angle + Math.Cos((bumps + 3) * angle));
            var r2 = 0.7 - 0.20 * Math.Sin(startAngle + bumps * angle - Math.Sin((bumps + 2) * angle));
            if (Math.Abs(angle - dipAngle) < dipWidth
                || Math.Abs(angle - dipAngle + 2 * Math.PI) < dipWidth
                || Math.Abs(angle - dipAngle - 2 * Math.PI) < dipWidth)
            {
                r1 = r2 = 0.2;
            }
            var test = (length < r1 || (length > r1 * ISLAND_FACTOR && length < r2));
            return test;
        }
    }
}