using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL;
using System.Diagnostics;

namespace Paint2D
{
    public enum SelectMode
    {
        LINE, CIRCLE, RECTANGLE, ELLIPSE, TRIANGLE, PENTAGON, HEXAGON, POLYGON, SELECT, SCANLINE, FLOODFILL,
        TRANSLATE, ROTATE, SCALE,APPLY
    }
    public partial class Form1 : Form
    {
        private Stopwatch st = new Stopwatch();
        int selectedID = -1;
        List<Shape> shapes;
        bool startup;
        Affine affine;

        Color lineColor;
        Color fillColor;
        int lineWidth;
        ShapeType shape;
        Point startPoint, endPoint;

        Point selectedPoint;
        bool isRender = false;
        SelectMode selectMode;

        private void SwitchMode()
        {
            System.UInt32[] buffer;
            buffer = new System.UInt32[512];

            OpenGL gl = openGLControl.OpenGL;
            gl.SelectBuffer(512, buffer);
            gl.RenderMode(OpenGL.GL_SELECT);
            isRender = false;

        }

        private void Draw(int except)
        {
            OpenGL gl = openGLControl.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.ClearColor(1, 1, 1, 1);

            Filler fill = new Filler(gl);

            for (int i = 0; i < shapes.Count; i++)
            {
                if (i != except)
                {
                    //gl.Color(shapes[i].lineColor.R / 255.0, shapes[i].lineColor.G / 255.0, shapes[i].lineColor.B / 255.0);
                    
                    shapes[i].Draw(gl);
                    if (shapes[i].isFill)
                    {
                        switch(shapes[i].fillType)
                        {
                            case FillType.FLOODFILL:
                                fill.FloodFill(shapes[i], shapes[i].fillColor);
                                break;
                            default:
                                fill.ScanFill(shapes[i], shapes[i].fillColor);
                                break;
                        }
                    }

                }

            }

        }

        public Form1()
        {
            InitializeComponent();
            shapes = new List<Shape>();
            List<Point> pointsOfShape = new List<Point>();
            lineColor = Color.Black;
            openGLControl.Tag = DrawState.IDLE;
            selectMode = SelectMode.SELECT;
            startup = true;  //
            selectedPoint = new Point();
            affine = new Affine();
            isRender = false;
        }

        private void openGLControl_Load(object sender, EventArgs e)
        {

        }

        private void openGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            // Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;
            // Set the clear color.
            gl.ClearColor(1, 1, 1, 1);
            // Set the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            // Load the identity.
            gl.LoadIdentity();
        }

        private void openGLControl_Resized(object sender, EventArgs e)
        {
            // Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;
            // Set the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            // Load the identity.
            gl.LoadIdentity();
            // Create a perspective transformation.
            gl.Viewport(0, 0, openGLControl.Width, openGLControl.Height);
            gl.Ortho2D(0, openGLControl.Width, 0, openGLControl.Height);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.ClearColor(1, 1, 1, 1);
        }

        private void openGLControl_OpenGLDraw(object sender, RenderEventArgs e)
        {
            OpenGL gl = openGLControl.OpenGL;
            if (startup)
            {
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                gl.ClearColor(1, 1, 1, 1);

                startup = false;
                return;
            }
            if ((int)openGLControl.Tag == (int)DrawState.IDLE || isRender == false)
                return;
            // Clear the color and depth buffer.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            Draw(-1);

            if (selectMode==SelectMode.APPLY)
            {
                openGLControl.Tag = (int)DrawState.IDLE;
                SwitchMode();
                selectMode = SelectMode.SELECT;
                return;
            }

            Shape newShape;
            Filler fill = new Filler(gl);

            if (selectMode==SelectMode.TRANSLATE)
            {
                newShape = shapes[selectedID].Clone(gl);
                newShape.Transform(affine, gl);
                Draw(selectedID);
                newShape.Draw(gl);


                if (newShape.isFill)
                {
                    switch (newShape.fillType)
                    {
                        case FillType.FLOODFILL:
                            fill.FloodFill(newShape, newShape.fillColor);
                            break;
                        default:
                            fill.ScanFill(newShape, newShape.fillColor);
                            break;
                    }
                }

                if ((int)openGLControl.Tag == (int)DrawState.DONE)
                {
                    shapes[selectedID].startPoint = newShape.startPoint;
                    shapes[selectedID].endPoint = newShape.endPoint;
                    shapes[selectedID] = newShape.Clone(gl);
                    Draw(-1);
                    affine = new Affine();
                    openGLControl.Tag = (int)DrawState.IDLE;
                }
                return;
            }


            switch (shape)
            {
                case ShapeType.LINE:
                    newShape = new Line()
                    {
                        type = shape
                    };
                    break;
                case ShapeType.CIRCLE:
                    newShape = new Circle()
                    {
                        type = shape
                    };
                    break;
                case ShapeType.RECTANGLE:
                    newShape = new Rectangle()
                    {
                        type = shape
                    };
                    break;
                case ShapeType.ELLIPSE:
                    newShape = new Ellipse()
                    {
                        type = shape
                    };
                    break;

                case ShapeType.TRIANGLE:
                    newShape = new Triangle()
                    {
                        type = shape
                    };
                    break;
                case ShapeType.PENTAGON:
                    newShape = new Pentagon()
                    {
                        type = shape
                    };
                    break;
                case ShapeType.POLYGON:
                    newShape = new Polygon();
                    newShape = shapes.Last();
                    if (newShape.type != ShapeType.POLYGON || newShape.isCreated == true)
                    {
                        openGLControl.Tag = DrawState.IDLE;

                        return;
                    }
                    newShape.lineColor = lineColor;
                    newShape.fillColor = fillColor;
                    newShape.lineWidth = (int)nmudLineWidth.Value;
                    return;

                default:
                    newShape = new Hexagon()
                    {
                        type = shape
                    };
                    break;
            }

            newShape.lineColor = lineColor;
            newShape.fillColor = fillColor;
            newShape.lineWidth = (int)nmudLineWidth.Value;

            newShape.startPoint = new Point(startPoint.X, startPoint.Y);
            newShape.endPoint = new Point(endPoint.X, endPoint.Y);

            newShape.Draw(gl);
            if ((int)openGLControl.Tag == (int)DrawState.DONE)
            {
                newShape.Create(gl);
                shapes.Add(newShape);
                openGLControl.Tag = DrawState.IDLE;

            }
        }

        private void openGLControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (selectMode == SelectMode.SELECT)
            {
                openGLControl.Tag = DrawState.IDLE;
                return;
            }
            if (selectMode == SelectMode.POLYGON)
                return;
            if (selectMode == SelectMode.TRANSLATE)
            {
                selectedPoint = e.Location;
                openGLControl.Tag = DrawState.DRAWING;
                return;
            }
            openGLControl.Tag = DrawState.DRAWING;
            st.Reset();
            st.Start();
            startPoint = e.Location;
            endPoint = startPoint;
        }

        private void openGLControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (isRender == false)
            {
                openGLControl.Tag = DrawState.IDLE;
                return;
            }
            if (selectMode == SelectMode.POLYGON)
                return;
            if (selectMode == SelectMode.TRANSLATE)
            {
                openGLControl.Tag = DrawState.DONE;

                affine.Translate(e.Location.X - selectedPoint.X, selectedPoint.Y - e.Location.Y);
                if (shapes[selectedID].type == ShapeType.POLYGON)
                    affine.Translate(e.Location.X - selectedPoint.X, selectedPoint.Y - e.Location.Y);
                return;
            }
            st.Stop();
            txtTime.Text = st.Elapsed.ToString() + "ms";

            openGLControl.Tag = DrawState.DONE;
            endPoint = e.Location;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            selectMode = SelectMode.SELECT;
            SwitchMode();
        }

        private void btnLine_Click(object sender, EventArgs e)
        {
            shape = ShapeType.LINE;
            selectMode = SelectMode.LINE;
            OpenGL gl = openGLControl.OpenGL;
            gl.RenderMode(OpenGL.GL_RENDER);
            isRender = true;
        }

        private void btnCircle_Click(object sender, EventArgs e)
        {
            shape = ShapeType.CIRCLE;
            selectMode = SelectMode.CIRCLE;
            OpenGL gl = openGLControl.OpenGL;
            gl.RenderMode(OpenGL.GL_RENDER);
            isRender = true;
        }

        private void btnEllipse_Click(object sender, EventArgs e)
        {
            shape = ShapeType.ELLIPSE;
            selectMode = SelectMode.ELLIPSE;
            OpenGL gl = openGLControl.OpenGL;
            gl.RenderMode(OpenGL.GL_RENDER);
            isRender = true;
        }

        private void btnTriangle_Click(object sender, EventArgs e)
        {
            shape = ShapeType.TRIANGLE;
            selectMode = SelectMode.TRIANGLE;
            OpenGL gl = openGLControl.OpenGL;
            gl.RenderMode(OpenGL.GL_RENDER);
            isRender = true;
        }

        private void btnRectangle_Click(object sender, EventArgs e)
        {
            shape = ShapeType.RECTANGLE;
            selectMode = SelectMode.RECTANGLE;
            OpenGL gl = openGLControl.OpenGL;
            gl.RenderMode(OpenGL.GL_RENDER);
            isRender = true;
        }

        private void btnPentagon_Click(object sender, EventArgs e)
        {
            shape = ShapeType.PENTAGON;
            selectMode = SelectMode.PENTAGON;
            OpenGL gl = openGLControl.OpenGL;
            gl.RenderMode(OpenGL.GL_RENDER);
            isRender = true;
        }

        private void btnHexagon_Click(object sender, EventArgs e)
        {
            shape = ShapeType.HEXAGON;
            selectMode = SelectMode.HEXAGON;
            OpenGL gl = openGLControl.OpenGL;
            gl.RenderMode(OpenGL.GL_RENDER);
            isRender = true;
        }

        private void btnPolygon_Click(object sender, EventArgs e)
        {
            shape = ShapeType.POLYGON;
            selectMode = SelectMode.POLYGON;
            OpenGL gl = openGLControl.OpenGL;
            gl.RenderMode(OpenGL.GL_RENDER);
            isRender = true;
        }

        private void btnScanFill_Click(object sender, EventArgs e)
        {
            if (isRender == true || selectedID == -1) return;
            selectMode = SelectMode.SCANLINE;
            openGLControl.OpenGL.RenderMode(OpenGL.GL_RENDER);
            shapes[selectedID].fillColor = fillColor;
            shapes[selectedID].isFill = true;
            shapes[selectedID].fillType = FillType.SCANLINE;
            Filler fill = new Filler(openGLControl.OpenGL);
            double time=fill.ScanFill(shapes[selectedID], shapes[selectedID].fillColor);
            SwitchMode();

            txtTime.Text = time.ToString() + "ms";
;        }

        private void btnFloodFill_Click(object sender, EventArgs e)
        {
            if (isRender == true || selectedID == -1) return;
            selectMode = SelectMode.FLOODFILL;
            openGLControl.OpenGL.RenderMode(OpenGL.GL_RENDER);
            shapes[selectedID].fillColor = fillColor;
            shapes[selectedID].isFill = true;
            shapes[selectedID].fillType = FillType.FLOODFILL;
            Filler fill = new Filler(openGLControl.OpenGL);
            double time =fill.FloodFill(shapes[selectedID], shapes[selectedID].fillColor);
            SwitchMode();

            txtTime.Text = time.ToString() + "ms";
        }

        private void btnTranslate_Click(object sender, EventArgs e)
        {
            if (selectMode == SelectMode.SELECT)
            {
                selectMode = SelectMode.TRANSLATE;
                OpenGL gl = openGLControl.OpenGL;
                gl.RenderMode(OpenGL.GL_RENDER);
                isRender = true;
            }
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {

        }

        private void btnScale_Click(object sender, EventArgs e)
        {

        }

        private void btnLineColor_Click(object sender, EventArgs e)
        {
            if (cdiagLine.ShowDialog() == DialogResult.OK)
            {
                lineColor = cdiagLine.Color;
                btnLineColor.BackColor = lineColor;
            }
        }

        private void btnFillColor_Click(object sender, EventArgs e)
        {
            if (cdiagFill.ShowDialog() == DialogResult.OK)
            {
                fillColor = cdiagFill.Color;
                btnFillColor.BackColor = fillColor;
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (isRender == true || selectedID == -1) return;
            //selectMode = SelectMode.FLOODFILL;
            lineWidth = (int)nmudLineWidth.Value;
            shapes[selectedID].fillColor = fillColor;
            shapes[selectedID].lineColor = lineColor;
            shapes[selectedID].lineWidth = lineWidth;
            shapes[selectedID].isFill = true;
            shapes[selectedID].fillType = FillType.SCANLINE;

            openGLControl.OpenGL.RenderMode(OpenGL.GL_RENDER);
            Draw(-1);
            SwitchMode();
        }

        private void openGLControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (isRender == false)
            {
                double epsilon = 30;
                double minDist = epsilon;
                int id = -1;
                for (int i = 0; i < shapes.Count; i++)
                {
                    for (int j = 0; j < shapes[i].controlPoints.Count; j++)
                    {
                        int x = shapes[i].controlPoints[j].X;
                        int y = shapes[i].controlPoints[j].Y;
                        double d = Math.Sqrt((e.Location.X - x) * (e.Location.X - x)
                        + (openGLControl.OpenGL.RenderContextProvider.Height - e.Location.Y - y) * (openGLControl.OpenGL.RenderContextProvider.Height - e.Location.Y - y));
                        if (d <= epsilon)
                        {
                            if (d < minDist)
                            {
                                minDist = d;
                                id = i;
                            }
                        }
                    }
                }

                if (id != selectedID)
                {
                    openGLControl.OpenGL.RenderMode(OpenGL.GL_RENDER);
                    Draw(-1);
                    if (id != -1)
                    {
                        shapes[id].DrawControlPoints(openGLControl.OpenGL);
                    }
                    selectedID = id;
                    SwitchMode();
                }
                return;
            }
            if (selectMode == SelectMode.POLYGON)
            {
                if (e.Button == MouseButtons.Right)
                {
                    if ((int)openGLControl.Tag == (int)DrawState.DRAWING)
                    {

                        Shape tmp = new Polygon();
                        tmp = shapes.Last();
                        tmp.isCreated = true;
                        tmp.startPoint = tmp.controlPoints.Last();
                        tmp.endPoint = tmp.controlPoints[0];
                        endPoint = tmp.controlPoints.Last();

                    }


                }
                if (e.Button == MouseButtons.Left)
                {
                    startPoint = e.Location;
                    openGLControl.Tag = DrawState.DRAWING;
                    Shape tmp;
                    if (shapes.Count == 0 || shapes.Last().type != ShapeType.POLYGON || shapes.Last().isCreated)
                    {
                        tmp = new Polygon()
                        {
                            type = ShapeType.POLYGON,
                            isCreated = false,
                            startPoint = new Point(startPoint.X, startPoint.Y),
                            endPoint = new Point(startPoint.X, startPoint.Y)

                        };
                        shapes.Add(tmp);


                        endPoint = e.Location;
                    }

                    Point t = new Point(e.Location.X, openGLControl.OpenGL.RenderContextProvider.Height - e.Location.Y);
                    shapes.Last().controlPoints.Add(t);
                    shapes.Last().startPoint = startPoint;
                    shapes.Last().endPoint = endPoint;

                    endPoint = startPoint;
                }
                return;
            }
            if (selectMode == SelectMode.TRANSLATE)
            {

                return;
            }
        }

        private void openGLControl_MouseMove(object sender, MouseEventArgs e)
        {
            txtPosition.Text = e.Location.ToString();
            if ((int)openGLControl.Tag == (int)DrawState.DRAWING)
            {

                if (selectMode == SelectMode.POLYGON)
                {
                    return;
                }
                if (selectMode == SelectMode.TRANSLATE)
                {
                    //sua o day
                    affine.Translate(e.Location.X - selectedPoint.X, -e.Location.Y + selectedPoint.Y);
                    if (shapes[selectedID].type == ShapeType.POLYGON)
                        affine.Translate(e.Location.X - selectedPoint.X, selectedPoint.Y - e.Location.Y);
                    return;
                }
                endPoint = e.Location;
            }
        }

    }
}
