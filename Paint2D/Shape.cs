using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using SharpGL;

public enum ShapeType
{
    LINE, CIRCLE, ELLIPSE, TRIANGLE, RECTANGLE, PENTAGON, HEXAGON, POLYGON
}

public enum DrawState
{
    IDLE, DRAWING, DONE
}

public enum FillType
{
    SCANLINE, FLOODFILL
}

namespace Paint2D
{
    public abstract class Shape
    {
        public ShapeType type;
        public bool isCreated, isFill, isDrawn;
        public Color fillColor, lineColor;
        public int lineWidth;
        public FillType fillType;

        public Point startPoint { get; set; }
        public Point endPoint { get; set; }
        public List<Point> controlPoints;
        public List<Point> vertexs;

        public abstract void Draw(OpenGL gl);
        public abstract void Create(OpenGL gl);

        public Shape()
        {
            isCreated = isFill = isDrawn = false;
            lineWidth = 1;
            controlPoints = new List<Point>();
        }

        public void Transform(Affine at, OpenGL gl)
        {
            for (int i = 0; i < controlPoints.Count; i++)
            {
                controlPoints[i] = at.Transform(controlPoints[i]);
            }
            for (int i = 0; i < vertexs.Count; i++)
            {
                vertexs[i] = at.Transform(vertexs[i]);
            }
            startPoint = at.Transform(startPoint);
            endPoint = at.Transform(endPoint);

            
        }/*aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa*/

        public virtual Shape Clone(OpenGL gl)
        {
            Shape temp;
            switch (type)
            {
                case ShapeType.LINE:
                    temp = new Line();
                    break;
                case ShapeType.CIRCLE:
                    temp = new Circle();
                    break;
                case ShapeType.RECTANGLE:
                    temp = new Rectangle();
                    break;
                case ShapeType.ELLIPSE:
                    temp = new Ellipse();
                    break;
                case ShapeType.TRIANGLE:
                    temp = new Triangle();
                    break;
                case ShapeType.PENTAGON:
                    temp = new Pentagon();
                    break;
                case ShapeType.HEXAGON:
                    temp = new Hexagon();
                    break;
                default:
                    temp = new Polygon();
                    break;
            }

            temp.controlPoints.AddRange(controlPoints);
            temp.vertexs = new List<Point>();
            temp.vertexs.AddRange(vertexs);

            temp.isDrawn = isDrawn;
            temp.lineColor = lineColor;
            temp.fillColor = fillColor;
            temp.lineWidth = lineWidth;
            temp.isFill = isFill;
            temp.isCreated = isCreated;
            temp.fillType = fillType;
            temp.type = type;
            temp.startPoint = new Point(startPoint.X, startPoint.Y);
            temp.endPoint = new Point(endPoint.X, endPoint.Y);
            return temp;
        }

        virtual public void DrawControlPoints(OpenGL gl)
        {
            foreach(var v in controlPoints)
            {
                gl.Color(Color.Red.R, Color.Red.G, Color.Red.B);
                gl.PointSize(5);
                gl.Begin(OpenGL.GL_POINTS);
                Point a = v;
                gl.Vertex(a.X, a.Y);
                gl.End();
            }
            gl.Flush();
            gl.PointSize(lineWidth);
        }

    }

    public class Line : Shape
    {
        public override void Draw(OpenGL gl)
        {
            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.LineWidth(lineWidth);
            if (isDrawn)
            {
                gl.Begin(OpenGL.GL_LINES);                                         
                foreach(var v in vertexs)
                {
                    gl.Vertex(v.X, v.Y);
                }
                gl.End();
                gl.Flush();
                return;
            }
            isDrawn = true;
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(startPoint.X, gl.RenderContextProvider.Height - startPoint.Y);
            gl.Vertex(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y);
            gl.End();
            gl.Flush();
        }
        public override void Create(OpenGL gl)
        {
            vertexs = new List<Point>();
            controlPoints = new List<Point>();
            vertexs.Add(new Point(startPoint.X, gl.RenderContextProvider.Height - startPoint.Y));
            vertexs.Add(new Point(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y));
            controlPoints.AddRange(vertexs);
        }
    }

    public class Circle : Shape
    {

        public override void Draw(OpenGL gl)
        {
            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.PointSize(lineWidth);
            if (isDrawn)
            {
                gl.Begin(OpenGL.GL_POINTS);
                foreach (var v in vertexs)
                {
                    gl.Vertex(v.X, v.Y);
                }
                gl.End();
                gl.Flush();
                return;
            }
            isDrawn = true;
            vertexs = new List<Point>();
            gl.Begin(OpenGL.GL_POINTS);

            int r = Math.Abs(startPoint.X - endPoint.X);
            int centerX = startPoint.X, centerY = startPoint.Y;

            int x = 0, y = r;
            int x2 = 2 * x;
            int y2 = 2 * y;
            int p = (int)Math.Round(5.0 / 4.0 - (double)r);
            /* Vẽ đối xứng qua tâm */
            gl.Vertex(centerX + x, gl.RenderContextProvider.Height - (centerY + y));
            gl.Vertex(centerX - x, gl.RenderContextProvider.Height - (centerY - y));
            gl.Vertex(centerX - r, gl.RenderContextProvider.Height - (centerY - 0));
            gl.Vertex(centerX + r, gl.RenderContextProvider.Height - (centerY + 0));
            /* Thêm vào list Vertex */
            vertexs.Add(new Point(centerX + x, gl.RenderContextProvider.Height - (centerY + y)));
            vertexs.Add(new Point(centerX - x, gl.RenderContextProvider.Height - (centerY - y)));
            vertexs.Add(new Point(centerX - r, gl.RenderContextProvider.Height - (centerY - 0)));
            vertexs.Add(new Point(centerX + r, gl.RenderContextProvider.Height - (centerY + 0)));
            /* Tiến hành vẽ các điểm trên đường tròn */
            while (x < y)
            {
                x++;
                x2 = 2 * x;
                if (p < 0)
                {
                    p = p + x2 + 1;
                }
                else
                {
                    y--;
                    y2 = 2 * y;
                    p = p + x2 - y2 + 1;
                }
                /* Vẽ đối xứng qua tâm*/
                gl.Vertex(centerX + x, gl.RenderContextProvider.Height - (centerY + y));
                gl.Vertex(centerX - x, gl.RenderContextProvider.Height - (centerY + y));
                gl.Vertex(centerX + x, gl.RenderContextProvider.Height - (centerY - y));
                gl.Vertex(centerX - x, gl.RenderContextProvider.Height - (centerY - y));
                /* điểm đối xứng với (x,y) qua đường thẳng y = x */
                int xSym = (x + y) - x;
                int ySym = (x + y) - y;
                gl.Vertex(centerX + xSym, gl.RenderContextProvider.Height - (centerY + ySym));
                gl.Vertex(centerX - xSym, gl.RenderContextProvider.Height - (centerY + ySym));
                gl.Vertex(centerX + xSym, gl.RenderContextProvider.Height - (centerY - ySym));
                gl.Vertex(centerX - xSym, gl.RenderContextProvider.Height - (centerY - ySym));
                vertexs.Add(new Point(centerX + x, gl.RenderContextProvider.Height - (centerY + y)));
                vertexs.Add(new Point(centerX - x, gl.RenderContextProvider.Height - (centerY + y)));
                vertexs.Add(new Point(centerX + x, gl.RenderContextProvider.Height - (centerY - y)));
                vertexs.Add(new Point(centerX - x, gl.RenderContextProvider.Height - (centerY - y)));
                vertexs.Add(new Point(centerX + xSym, gl.RenderContextProvider.Height - (centerY + ySym)));
                vertexs.Add(new Point(centerX - xSym, gl.RenderContextProvider.Height - (centerY + ySym)));
                vertexs.Add(new Point(centerX + xSym, gl.RenderContextProvider.Height - (centerY - ySym)));
                vertexs.Add(new Point(centerX - xSym, gl.RenderContextProvider.Height - (centerY - ySym)));
            }

            gl.End();
            gl.Flush();
        }
        public override void Create(OpenGL gl)
        {

            controlPoints = new List<Point>();

            int r = Math.Abs(startPoint.X - endPoint.X);
            int centerX = startPoint.X, centerY = startPoint.Y;
            int h = gl.RenderContextProvider.Height;
            controlPoints.Add(new Point(centerX - r, h - (centerY - r)));
            controlPoints.Add(new Point(centerX - r, h - (centerY)));
            controlPoints.Add(new Point(centerX - r, h - (centerY + r)));
            controlPoints.Add(new Point(centerX, h - (centerY - r)));
            controlPoints.Add(new Point(centerX, h - (centerY + r)));
            controlPoints.Add(new Point(centerX + r, h - (centerY - r)));
            controlPoints.Add(new Point(centerX + r, h - (centerY)));
            controlPoints.Add(new Point(centerX + r, h - (centerY + r)));
        }


    }

    public class Rectangle : Shape
    {
        public override void Draw(OpenGL gl)
        {
            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.LineWidth(lineWidth);
            if (isDrawn)
            {
                gl.Begin(OpenGL.GL_LINE_LOOP);
                foreach (var v in vertexs)
                {
                    gl.Vertex(v.X, v.Y);
                }
                gl.End();
                gl.Flush();
                return;
            }
            isDrawn = true;
            gl.Begin(OpenGL.GL_LINES);
            //Canh 1
            gl.Vertex(startPoint.X, gl.RenderContextProvider.Height - startPoint.Y);
            gl.Vertex(endPoint.X, gl.RenderContextProvider.Height - startPoint.Y);
            //Canh 2
            gl.Vertex(endPoint.X, gl.RenderContextProvider.Height - startPoint.Y);
            gl.Vertex(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y);
            //Canh 3
            gl.Vertex(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y);
            gl.Vertex(startPoint.X, gl.RenderContextProvider.Height - endPoint.Y);
            //Canh 4
            gl.Vertex(startPoint.X, gl.RenderContextProvider.Height - endPoint.Y);
            gl.Vertex(startPoint.X, gl.RenderContextProvider.Height - startPoint.Y);

            gl.End();
            gl.Flush();
        }
        public override void Create(OpenGL gl)
        {
            vertexs = new List<Point>();
            controlPoints = new List<Point>();
           
            /*Thêm các đỉnh*/
            vertexs.Add(new Point(startPoint.X, gl.RenderContextProvider.Height - startPoint.Y));
            vertexs.Add(new Point(endPoint.X, gl.RenderContextProvider.Height - startPoint.Y));
            vertexs.Add(new Point(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y));
            vertexs.Add(new Point(startPoint.X, gl.RenderContextProvider.Height - endPoint.Y));

            /*Thêm các control point*/
            controlPoints.Add(new Point(startPoint.X, (gl.RenderContextProvider.Height - startPoint.Y + gl.RenderContextProvider.Height - endPoint.Y) / 2));
            controlPoints.Add(new Point(endPoint.X, (gl.RenderContextProvider.Height - startPoint.Y + gl.RenderContextProvider.Height - endPoint.Y) / 2));
            controlPoints.Add(new Point((startPoint.X + endPoint.X) / 2, gl.RenderContextProvider.Height - startPoint.Y));
            controlPoints.Add(new Point((startPoint.X + endPoint.X) / 2, gl.RenderContextProvider.Height - endPoint.Y));

            controlPoints.AddRange(vertexs);
        }
    }

    public class Ellipse : Shape
    {
        public override void Draw(OpenGL gl)
        {
            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.PointSize(lineWidth);
            if (isDrawn)
            {
                gl.Begin(OpenGL.GL_POINTS);
                foreach (var v in vertexs)
                {
                    gl.Vertex(v.X, v.Y);
                }
                gl.End();
                gl.Flush();
                return;
            }
            isDrawn = true;
            vertexs = new List<Point>();
            gl.Begin(OpenGL.GL_POINTS);
            /* Xác định các đại lượng cơ bản của hình ellipse */
            /* Bán kính rx, ry*/
            int rX = Math.Abs(startPoint.X - endPoint.X) / 2;
            int rY = Math.Abs(startPoint.Y - endPoint.Y) / 2;
            int rX2 = rX * rX;
            int rY2 = rY * rY;
            int rX22 = 2 * rX2, rY22 = 2 * rY2;
            /* Tâm hình ellipse*/
            int xCenter = (startPoint.X + endPoint.X) / 2;
            int yCenter = (startPoint.Y + endPoint.Y) / 2;
            int x = 0, y = rY;
            int pX = 0, pY = rX22 * y;
            /* Vẽ đối xứng qua tâm*/
            gl.Vertex(xCenter + x, gl.RenderContextProvider.Height - (yCenter + y));
            gl.Vertex(xCenter - x, gl.RenderContextProvider.Height - (yCenter + y));
            gl.Vertex(xCenter + x, gl.RenderContextProvider.Height - (yCenter - y));
            gl.Vertex(xCenter - x, gl.RenderContextProvider.Height - (yCenter - y));
            vertexs.Add(new Point(xCenter + x, gl.RenderContextProvider.Height - (yCenter + y)));
            vertexs.Add(new Point(xCenter - x, gl.RenderContextProvider.Height - (yCenter + y)));
            vertexs.Add(new Point(xCenter + x, gl.RenderContextProvider.Height - (yCenter - y)));
            vertexs.Add(new Point(xCenter - x, gl.RenderContextProvider.Height - (yCenter - y)));
            /* Vùng 1: dy/dx <= 1*/
            int p = (int)(Math.Round(rY2 - rX2 * rY + 0.25 * rX2));
            while (pX < pY)
            {
                x++;
                pX += rY22;
                if (p < 0)
                {
                    p += rY2 + pX;
                }
                else
                {
                    y--;
                    pY -= rX22;
                    p += rY2 + pX - pY;
                }
                /* Vẽ đối xứng qua tâm*/
                gl.Vertex(xCenter + x, gl.RenderContextProvider.Height - (yCenter + y));
                gl.Vertex(xCenter - x, gl.RenderContextProvider.Height - (yCenter + y));
                gl.Vertex(xCenter + x, gl.RenderContextProvider.Height - (yCenter - y));
                gl.Vertex(xCenter - x, gl.RenderContextProvider.Height - (yCenter - y));
                vertexs.Add(new Point(xCenter + x, gl.RenderContextProvider.Height - (yCenter + y)));
                vertexs.Add(new Point(xCenter - x, gl.RenderContextProvider.Height - (yCenter + y)));
                vertexs.Add(new Point(xCenter + x, gl.RenderContextProvider.Height - (yCenter - y)));
                vertexs.Add(new Point(xCenter - x, gl.RenderContextProvider.Height - (yCenter - y)));
            }

            /* Vùng 2*/
            p = (int)(Math.Round(rY2 * (x + 0.5) * (x + 0.5) + rX2 * (y - 1) * (y - 1) - rX2 * rY2));
            while (y > 0)
            {
                y--;
                pY -= rX22;
                if (p > 0)
                {
                    p += rX2 - pY;
                }
                else
                {
                    x++;
                    pX += rY22;
                    p += rX2 - pY + pX;
                }
                /* Vẽ đối xứng qua tâm*/
                gl.Vertex(xCenter + x, gl.RenderContextProvider.Height - (yCenter + y));
                gl.Vertex(xCenter - x, gl.RenderContextProvider.Height - (yCenter + y));
                gl.Vertex(xCenter + x, gl.RenderContextProvider.Height - (yCenter - y));
                gl.Vertex(xCenter - x, gl.RenderContextProvider.Height - (yCenter - y));
                vertexs.Add(new Point(xCenter + x, gl.RenderContextProvider.Height - (yCenter + y)));
                vertexs.Add(new Point(xCenter - x, gl.RenderContextProvider.Height - (yCenter + y)));
                vertexs.Add(new Point(xCenter + x, gl.RenderContextProvider.Height - (yCenter - y)));
                vertexs.Add(new Point(xCenter - x, gl.RenderContextProvider.Height - (yCenter - y)));
            }

            gl.End();
            gl.Flush();
        }

        public override void Create(OpenGL gl)
        {
            controlPoints = new List<Point>();
            /**Thêm các điểm control point*/
            controlPoints.Add(new Point(startPoint.X, gl.RenderContextProvider.Height - startPoint.Y));
            controlPoints.Add(new Point(endPoint.X, gl.RenderContextProvider.Height - startPoint.Y));
            controlPoints.Add(new Point(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y));
            controlPoints.Add(new Point(startPoint.X, gl.RenderContextProvider.Height - endPoint.Y));
            controlPoints.Add(new Point(startPoint.X, (gl.RenderContextProvider.Height - startPoint.Y + gl.RenderContextProvider.Height - endPoint.Y) / 2));
            controlPoints.Add(new Point(endPoint.X, (gl.RenderContextProvider.Height - startPoint.Y + gl.RenderContextProvider.Height - endPoint.Y) / 2));
            controlPoints.Add(new Point((startPoint.X + endPoint.X) / 2, gl.RenderContextProvider.Height - startPoint.Y));
            controlPoints.Add(new Point((startPoint.X + endPoint.X) / 2, gl.RenderContextProvider.Height - endPoint.Y));
            
        }
    }

    public class Triangle : Shape
    {
        public Point p3;
        public override void Draw(OpenGL gl)
        {
            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.LineWidth(lineWidth);
            if (isDrawn)
            {
                gl.Begin(OpenGL.GL_LINE_LOOP);
                foreach (var v in vertexs)
                {
                    gl.Vertex(v.X, v.Y);
                }
                gl.End();
                gl.Flush();
                return;
            }
            isDrawn = true;

            double a = Math.Sqrt((startPoint.X - endPoint.X) ^ 2 + (startPoint.Y - endPoint.Y) ^ 2);
            double c = Math.Sin(60 * Math.PI / 180);
            int deltaY = startPoint.Y - endPoint.Y;
            int deltaX = startPoint.X - endPoint.X;

            /* xác định các đỉnh của tam giác đều */
            p3.X = startPoint.X + endPoint.X;
            p3.X /= 2;
            p3.Y = (int)(startPoint.Y + (deltaX * c - deltaY));
            //!! Chua sap xep cac dinh cua tam giac theo chieu nguoc chieu kim dong ho
            gl.Begin(OpenGL.GL_LINES);
            //Canh 1
            // cùng chiều
            gl.Vertex(startPoint.X, gl.RenderContextProvider.Height - endPoint.Y); //Đỉnh 1
                                                                                   // ngược chiều
            gl.Vertex(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y); //Đỉnh 2
                                                                                 //Canh 2
            gl.Vertex(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y); //Đỉnh 2
            gl.Vertex(p3.X, gl.RenderContextProvider.Height - p3.Y); //Đỉnh 3
                                                                     //Canh 3
                                                                     // còn lại
            gl.Vertex(p3.X, gl.RenderContextProvider.Height - p3.Y); //Đỉnh 3
            gl.Vertex(startPoint.X, gl.RenderContextProvider.Height - endPoint.Y); //Đỉnh 1

            gl.End();
            gl.Flush();
        }
        public override void Create(OpenGL gl)
        {
            vertexs = new List<Point>();
            controlPoints = new List<Point>();
            
            vertexs.Add(new Point(startPoint.X, gl.RenderContextProvider.Height - endPoint.Y));
            vertexs.Add(new Point(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y));
            vertexs.Add(new Point(p3.X, gl.RenderContextProvider.Height - p3.Y));

            controlPoints.AddRange(vertexs);
        }
    }

    public class Pentagon : Shape
    {
        public override void Draw(OpenGL gl)
        {
            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.LineWidth(lineWidth);
            if (isDrawn)
            {
                gl.Begin(OpenGL.GL_LINE_LOOP);                                         
                foreach (var v in vertexs)
                {
                    gl.Vertex(v.X, v.Y);
                }
                gl.End();
                gl.Flush();
                return;
            }
            isDrawn = true;
            vertexs = new List<Point>();
            double[,] pt = new double[5, 2];
            double const_cos = Math.Cos(72 * Math.PI / 180);
            double const_sin = Math.Sin(72 * Math.PI / 180);

            pt[0, 0] = startPoint.X;
            pt[0, 1] = startPoint.Y;

            pt[1, 0] = endPoint.X;
            pt[1, 1] = endPoint.Y;

            for (int i = 2; i < 5; i++)
            {
                double xA = pt[i - 2, 0], yA = pt[i - 2, 1];
                double xB = pt[i - 1, 0], yB = pt[i - 1, 1];
                pt[i, 0] = xB + const_cos * (xB - xA) + const_sin * (yA - yB);
                pt[i, 1] = yB + const_cos * (yB - yA) + const_sin * (xB - xA);
            }


            gl.Begin(OpenGL.GL_LINE_LOOP);
            for (int i = 0; i < 5; i++)
            {
                gl.Vertex((int)pt[i, 0], (int)(gl.RenderContextProvider.Height - pt[i, 1]));
                vertexs.Add(new Point((int)pt[i, 0], (int)(gl.RenderContextProvider.Height - pt[i, 1])));
            }
            gl.End();
            gl.Flush();
        }
        public override void Create(OpenGL gl)
        {
            controlPoints = new List<Point>();
            controlPoints.AddRange(vertexs);
        }
    }

    public class Hexagon : Shape
    {
        public override void Draw(OpenGL gl)
        {
            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.LineWidth(lineWidth);

            if (isDrawn)
            {
                gl.Begin(OpenGL.GL_LINE_LOOP);                                       
                foreach (var v in vertexs)
                {
                    gl.Vertex(v.X, v.Y);
                }
                gl.End();
                gl.Flush();
                return;
            }
            isDrawn = true;
            vertexs = new List<Point>();
            double[,] pt = new double[6, 2];
            double const_cos = Math.Cos(60 * Math.PI / 180);
            double const_sin = Math.Sin(60 * Math.PI / 180);
            //get 2 points for 2 first vertices
            pt[0, 0] = startPoint.X;
            pt[0, 1] = startPoint.Y;

            pt[1, 0] = endPoint.X;
            pt[1, 1] = endPoint.Y;

            for (int i = 2; i < 6; i++)
            {
                double xA = pt[i - 2, 0], yA = pt[i - 2, 1];
                double xB = pt[i - 1, 0], yB = pt[i - 1, 1];
                pt[i, 0] = xB + const_cos * (xB - xA) + const_sin * (yA - yB);
                pt[i, 1] = yB + const_cos * (yB - yA) + const_sin * (xB - xA);
            }

            gl.Begin(OpenGL.GL_LINE_LOOP);                                        
            for (int i = 0; i < 6; i++)
            {
                gl.Vertex((int)pt[i, 0], (int)(gl.RenderContextProvider.Height - pt[i, 1]));
                vertexs.Add(new Point((int)pt[i, 0], (int)(gl.RenderContextProvider.Height - pt[i, 1])));
            }
            gl.End();
            gl.Flush();
        }
        public override void Create(OpenGL gl)
        {
            controlPoints = new List<Point>();
            controlPoints.AddRange(vertexs);
        }


    }

    public class Polygon : Shape
    {

        public override void Draw(OpenGL gl)
        {
            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.LineWidth(lineWidth);

            gl.Begin(OpenGL.GL_LINES);
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                //gl.Begin(OpenGL.GL_LINES);
                gl.Vertex(controlPoints[i].X, controlPoints[i].Y);
                gl.Vertex(controlPoints[i + 1].X, controlPoints[i + 1].Y);
                //gl.End();
                //gl.Flush();
            }
            gl.Vertex(controlPoints[0].X, controlPoints[0].Y);
            gl.Vertex(controlPoints.Last().X, controlPoints.Last().Y);
            gl.End();
            gl.Flush();

            if (isCreated)
            {
                vertexs = new List<Point>();
                vertexs.AddRange(controlPoints);
                return;
            }

            gl.Color(lineColor.R / 255.0, lineColor.G / 255.0, lineColor.B / 255.0);
            gl.LineWidth(lineWidth);
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(startPoint.X, gl.RenderContextProvider.Height - startPoint.Y);
            gl.Vertex(endPoint.X, gl.RenderContextProvider.Height - endPoint.Y);
            gl.End();
            gl.Flush();
        }
        public override void Create(OpenGL gl)
        {

        }
    }
}