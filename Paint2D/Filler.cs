using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SharpGL;
using System.Drawing;
using System.Diagnostics;

namespace Paint2D
{
    public struct AEL
    {
        public int yUpper, yLower;
        public double xIntersect, reciSlope;
    }

    public struct RGBColor
    {
        public byte r;
        public byte g;
        public byte b;
    }
    class Filler
    {
        public OpenGL gl { set; get; }
        public Filler(OpenGL GL)
        {
            gl = GL;
        }

        public bool checkColor(RGBColor colorX, RGBColor colorY)
        {
            colorX.r = (byte)(colorX.r >> 1);
            colorX.g = (byte)(colorX.g >> 1);
            colorX.b = (byte)(colorX.b >> 1);
            colorY.r = (byte)(colorY.r >> 1);
            colorY.g = (byte)(colorY.g >> 1);
            colorY.b = (byte)(colorY.b >> 1);
            return (colorX.r == colorY.r) && (colorX.g == colorY.g) && (colorX.b == colorY.b);
        }

        public RGBColor GetPixel(int x, int y)
        {
            byte[] ptr = new byte[3];
            RGBColor color;
            gl.ReadPixels(x, gl.RenderContextProvider.Height - y, 1, 1, format: OpenGL.GL_RGB, type: OpenGL.GL_BYTE, ptr);
            color.r = (byte)((ptr[0]) << 1);
            color.g = (byte)((ptr[1]) << 1);
            color.b = (byte)((ptr[2]) << 1);
            return color;
        }

        public void PSetPixeltPixel(int x, int y, RGBColor color)
        {
            byte[] ptr = { color.r, color.g, color.b };
            gl.RasterPos(x, gl.RenderContextProvider.Height - y);
            gl.DrawPixels(1, 1, OpenGL.GL_RGB, ptr);
            gl.Flush();
        }
        
        
        public double FloodFill(Shape shape, Color fillColor)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            TimeSpan ts;

            shape.fillType = FillType.FLOODFILL;
            shape.Draw(gl);

            int x = 0, y = 0;
            foreach (var v in shape.vertexs)
            {
                x += v.X;
                y += v.Y;
            }

            x /= shape.vertexs.Count();
            y = gl.RenderContextProvider.Height - (y / shape.vertexs.Count());

            RGBColor rgbcolor;
            rgbcolor.r = fillColor.R;
            rgbcolor.g = fillColor.G;
            rgbcolor.b = fillColor.B;
            BFSFlood(x, y, rgbcolor);

            stopWatch.Stop();
            ts = stopWatch.Elapsed;

            return ts.TotalMilliseconds;
        }

        bool isValid(Point p)
        {
            if (p.X < 0 || p.Y < 0 || p.X > gl.RenderContextProvider.Width || p.Y > gl.RenderContextProvider.Height)
                return false;
            return true;
        }
        private void BFSFlood(int x, int y, RGBColor fillColor)
        {
            Queue<Point> Q = new Queue<Point>();
            Point curPoint = new Point();
            Point tempPoint = new Point();
            RGBColor curColor = GetPixel(x, y);

            PSetPixeltPixel(x, y, fillColor);
            curPoint.X = x;
            curPoint.Y = y;

            Q.Enqueue(curPoint); 
            while (Q.Count() != 0)   
            {
                curPoint = Q.Dequeue();

                //Xet cac diem lan can cua 1 diem
                tempPoint.X = curPoint.X - 1;
                tempPoint.Y = curPoint.Y;
                if (isValid(tempPoint) && checkColor(curColor, GetPixel(tempPoint.X, tempPoint.Y)))
                {
                    PSetPixeltPixel(tempPoint.X, tempPoint.Y, fillColor);
                    Q.Enqueue(tempPoint);
                }

                tempPoint.X = curPoint.X + 1;
                tempPoint.Y = curPoint.Y;
                if (isValid(tempPoint) && checkColor(curColor, GetPixel(tempPoint.X, tempPoint.Y)))
                {
                    PSetPixeltPixel(tempPoint.X, tempPoint.Y, fillColor);
                    Q.Enqueue(tempPoint);
                }

                tempPoint.X = curPoint.X;
                tempPoint.Y = curPoint.Y - 1;
                if (isValid(tempPoint) && checkColor(curColor, GetPixel(tempPoint.X, tempPoint.Y)))
                {
                    PSetPixeltPixel(tempPoint.X, tempPoint.Y, fillColor);
                    Q.Enqueue(tempPoint);
                }

                tempPoint.X = curPoint.X;
                tempPoint.Y = curPoint.Y + 1;
                if (isValid(tempPoint) && checkColor(curColor, GetPixel(tempPoint.X, tempPoint.Y)))
                {
                    PSetPixeltPixel(tempPoint.X, tempPoint.Y, fillColor);
                    Q.Enqueue(tempPoint);
                }

            }
        }

        public double ScanFill(Shape shape, Color fillColor)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            TimeSpan ts;

            shape.fillType = FillType.SCANLINE;
            int yMin, yMax;
            if (shape.type == ShapeType.CIRCLE || shape.type == ShapeType.ELLIPSE) 
            {
                List<Point> fillVertex = new List<Point>();
                fillVertex.AddRange(shape.vertexs);

                yMin = fillVertex[0].Y;
                yMax = yMin;
                foreach (var v in fillVertex)
                {
                    if (v.Y > yMax)
                        yMax = v.Y;
                    if (v.Y < yMin)
                        yMin = v.Y;
                }

                for (int y = yMin; y <= yMax; y++)
                {
                    List<Point> line = new List<Point>();
                    line.AddRange(fillVertex.Where(v => v.Y == y));
                    line = line.OrderBy(v => v.X).ToList();

                    int xmin = -1, xmax = -1;
                    for (int i = 0; i < line.Count - 1; i++)
                    {
                        if (Math.Abs(line[i].X - line[i + 1].X) > 1)
                        {
                            xmin = line[i].X;
                            xmax = line[i + 1].X;
                            break;
                        }
                    }

                    if (xmax < 0)
                        continue;

                    gl.Color(fillColor.R / 255.0, fillColor.G / 255.0, fillColor.B / 255.0);
                    gl.Begin(OpenGL.GL_LINES);
                    gl.Vertex(xmin, y);
                    gl.Vertex(xmax, y);
                    gl.End();
                }
                gl.Flush();
                shape.Draw(gl);

                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                
                return ts.TotalMilliseconds;
            }
            List<List<AEL>> edgeTable = new List<List<AEL>>();
            List<AEL> begList = new List<AEL>();
            List<AEL> edges = new List<AEL>();


            VertexToEdge(ref edges, shape.vertexs, out yMin, out yMax);


            //Tạo Edge Table
            for (int y = yMin; y <= yMax; y++)
            {
                List<AEL> subList = new List<AEL>();
                edgeTable.Add(subList);
            }

            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].yLower != edges[i].yUpper)
                {
                    int id = edges[i].yLower - yMin;
                    edgeTable[id].Add(edges[i]);
                }
            }

            for (int y = yMin; y <= yMax; y++)
            {
                begList.AddRange(edgeTable[y - yMin]);

                //Sắp xếp các giao điểm theo x
                begList = begList.OrderBy(v => v.xIntersect).ToList();
                
                //Tô màu cho dòng hiện tại
                if (y != yMin && y != yMax)
                {
                    gl.Color(fillColor.R / 255.0, fillColor.G / 255.0, fillColor.B / 255.0);
                    gl.Begin(OpenGL.GL_LINES);
                    for (int i = 0; i < begList.Count; i += 2)
                    {
                        if (i + 1 < begList.Count)
                        {
                            gl.Vertex(begList[i].xIntersect, y);
                            gl.Vertex(begList[i + 1].xIntersect, y);

                        }
                    }
                    gl.End();
                }
                gl.Flush();

                //Xoá các cạnh đã ở dưới scan line
                int j = 0;
                while (j < begList.Count)
                {
                    if (begList[j].yUpper == y)
                    {

                        begList.RemoveAt(j);
                    }
                    else j++;
                }

                
                for (int i = 0; i < begList.Count; i++)
                {
                    AEL t = new AEL();
                    t.reciSlope = begList[i].reciSlope;
                    t.yLower = begList[i].yLower;
                    t.yUpper = begList[i].yUpper;
                    t.xIntersect = begList[i].xIntersect + begList[i].reciSlope;
                    begList[i] = t;

                }

            }
            shape.Draw(gl);

            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            return ts.TotalMilliseconds;
        }

        private void VertexToEdge(ref List<AEL> Edges, List<Point> Vertex, out int ymin, out int ymax)
        {
            AEL temp;
            ymin = Vertex[0].Y;
            ymax = Vertex[0].Y;
            Vertex.Add(Vertex[0]);
            Vertex.Add(Vertex[1]);
            Vertex.Insert(0, Vertex[Vertex.Count - 3]);

            for (int i = 1; i <= Vertex.Count - 3; i++)
            {
                temp = new AEL();
                temp.yUpper = Math.Max(Vertex[i].Y, Vertex[i + 1].Y);
                temp.yLower = Math.Min(Vertex[i].Y, Vertex[i + 1].Y);
                if (Vertex[i].Y > Vertex[i + 1].Y)
                    temp.xIntersect = Vertex[i + 1].X;
                else temp.xIntersect = Vertex[i].X;

                if (temp.yLower == temp.yUpper)
                {
                    temp.reciSlope = 0;
                }
                else
                {
                    temp.reciSlope = (Vertex[i].X - Vertex[i + 1].X) * 1.0 / (Vertex[i].Y - Vertex[i + 1].Y) * 1.0;
                }


                //Trường hợp điểm giao nhau không phải là cực trị
                if (Vertex[i + 1].Y < Vertex[i].Y && Vertex[i].Y < Vertex[i - 1].Y)
                {
                    temp.yUpper--;
                }
                else
                if (Vertex[i + 2].Y > Vertex[i + 1].Y && Vertex[i + 1].Y > Vertex[i].Y)
                {
                    temp.yUpper--;
                }

                Edges.Add(temp);
                ymin = Math.Min(ymin, Vertex[i + 1].Y);
                ymax = Math.Max(ymax, Vertex[i + 1].Y);

            }
            Vertex.RemoveAt(0);
            Vertex.RemoveAt(Vertex.Count - 1);
            Vertex.RemoveAt(Vertex.Count - 1);
        }

    }
}