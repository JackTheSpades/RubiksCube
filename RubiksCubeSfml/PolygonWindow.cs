    using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SFML.Graphics.Text;

namespace RubiksCubeSfml;


record struct Camera(Vector3 Position, Vector3 LookAt, Vector3 Up, float FieldOfView, float AspectRatio = 16f/9f);

/// <summary></summary>
/// <param name="ClickPosition">Click on the 2d screen.</param>
/// <param name="Camera">Camera at the time of the click.</param>
record ClickInfo(Vector2f ClickPosition, Camera Camera);

internal class PolygonWindow
{
    ClickInfo? RotateClick;
    ClickInfo? MoveClick;

    public readonly List<IPolygon> Polygons;
    public readonly RenderWindow Window;

    public Camera Camera { get; private set; }

    public bool ShowAxis { get; set; }

    public PolygonWindow(Camera camera, params IPolygon[] polygons)
    {
        Camera = camera;
        Polygons = new List<IPolygon>(polygons);

        var settings = new ContextSettings();
        settings.AntialiasingLevel = 16;

        Window = new RenderWindow(new VideoMode(800, 600), "Rubik's Cube", SFML.Window.Styles.Default, settings);
        Window.SetFramerateLimit(60);

        Window.Closed += (_, _) => Window.Close();
        Window.MouseButtonPressed += Window_MouseButtonPressed;
        Window.MouseButtonReleased += Window_MouseButtonReleased;
        Window.MouseMoved += Window_MouseMoved;
        Window.MouseWheelScrolled += Window_MouseWheelScrolled;
    }

    private void Window_MouseWheelScrolled(object? sender, MouseWheelScrollEventArgs e)
    {
        var posDirection = Camera.Position - Camera.LookAt;
        float length = posDirection.Length();

        float lnLength = MathF.Log(length);
        lnLength -= e.Delta / 10f;

        Camera = Camera with
        {
            Position = (Vector3.Normalize(posDirection) * MathF.Exp(lnLength)) + Camera.LookAt
        };
    }

    void Window_MouseMoved(object? sender, MouseMoveEventArgs e)
    {
        if (RotateClick is not null)
        {
            var drag = Window.MapPixelToCoords(new Vector2i(e.X, e.Y));
            var direction = drag - RotateClick.ClickPosition;

            // To calculate the rotation we make the assumption, that the camera is facing along the Z-axis and is looking at the XY-plane
            // This makes it relate almost 1:1 to the screen coordinates for our direction, except that on the screen Y goes downwards.

            // We can create a rotation matrix around an arbitrary vector (not just the unit x/y/z ones)
            // so for our rotation we can create an XY unit vector that is 90° counter clockwise to our dragged line
            // and then use the length of the dragged line as the amount (angle) in our rotation matrix.


            // normalized so that dragging across the whole window is 1 (or -1 if dragged in oposite direction)
            float normX = direction.X / Window.Size.X;
            float normY = direction.Y / Window.Size.Y;

            float angle = MathF.Atan2(-normY, normX);   // y goes top to bottom, for angle we want y to go up.
            float length = MathF.Sqrt(normX * normX + normY * normY);


            angle += MathF.PI / 2f; // we want angle orthagonal to our drawn line

            //unit vector in the XY-Plane that is 90° counter clockwise to our dragged line
            Vector3 unit = new(MathF.Cos(angle), MathF.Sin(angle), 0);


            // Our calculation so far assumes that the drag is happening on the XY-plane, with the center of the screen being at (0,0,_)
            // In reality, our camera could be anywhere and as such, we need to transform the calculated unit vector
            // The built in View-Matrix generation is of help here. It takes the camera parameters and generates the view matrix that
            // transforms the camera to be at (0,0,0) and looking into the Z-axis.
            // This is the exact opposite of what we need, so we can simply use the inverse on our unit vector so it is transformed onto
            // the plane the camera is looking at.

            // The Matrix4x4.CreateLookAt takes the camera position, look-at and up vector to calculated the view-matrix
            // however, we only need the rotational aspect of this matrix.
            // Since the translation comes from moving the camera to (0,0,0), we can just do that prior to calling the function
            // CreateLookAt(pos, lookat, up) -> CreateLookAt(pos - pos, lookat - pos, up) -> CreateLookAt(0, lookat - pos, up)
            // Note that we don't need to adjust the up vector

            var camMatrix = Matrix4x4.CreateLookAt(Vector3.Zero, RotateClick.Camera.LookAt - RotateClick.Camera.Position, RotateClick.Camera.Up);
            Matrix4x4.Invert(camMatrix, out camMatrix);
            unit = camMatrix.Multiply(unit);

            // rotate around Look-At by first translating it to (0,0,0) then rotating, then translating it back
            Matrix4x4 t =
                (RotateClick.Camera.LookAt == Vector3.Zero) ?
                Matrix4x4.CreateFromAxisAngle(-unit, length * MathF.Tau) :
                (
                    //Matrix4x4.CreateTranslation(-RotateClick.Camera.LookAt) *
                    Matrix4x4.CreateFromAxisAngle(-unit, length * MathF.Tau) //*
                    //Matrix4x4.CreateTranslation(RotateClick.Camera.LookAt)
                );

            // update the camera
            Camera = RotateClick.Camera with
            {
                Position = t.Multiply(RotateClick.Camera.Position),
                Up = t.Multiply(RotateClick.Camera.Up),
            };
        }
        else if(MoveClick is not null)
        {
            // see above documentation for explanation on the transformations

            var drag = Window.MapPixelToCoords(new Vector2i(e.X, e.Y));
            var direction = drag - MoveClick.ClickPosition;
            float normX = direction.X / Window.Size.X;
            float normY = direction.Y / Window.Size.Y;

            // on screen, Y goes downwards so we need negative Y.
            // In addition, since we move the camera, not the scene we have to negate both X and Y.
            // since moving the camer down/right is the same as moving the scene up/left etc.
            var move = new Vector3(-normX, normY, 0);

            var camMatrix = Matrix4x4.CreateLookAt(Vector3.Zero, MoveClick.Camera.LookAt - MoveClick.Camera.Position, MoveClick.Camera.Up);
            Matrix4x4.Invert(camMatrix, out camMatrix);
            move = camMatrix.Multiply(move);


            float scale = (MoveClick.Camera.Position - MoveClick.Camera.LookAt).Length();

            //Console.WriteLine(unit * scale);
            var t = Matrix4x4.CreateTranslation(move * scale * 2);

            // update the camera
            Camera = MoveClick.Camera with
            {
                Position = t.Multiply(MoveClick.Camera.Position),
                LookAt = t.Multiply(MoveClick.Camera.LookAt),
            };

        }
    }

    void Window_MouseButtonReleased(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Left)
            RotateClick = null;
        if (e.Button == Mouse.Button.Right)
            MoveClick = null;
    }
    void Window_MouseButtonPressed(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Left && MoveClick is null)
        {
            RotateClick = new ClickInfo(
                Window.MapPixelToCoords(new Vector2i(e.X, e.Y)),
                Camera
            );
        }
        if (e.Button == Mouse.Button.Right && RotateClick is null)
        {
            MoveClick = new ClickInfo(
                Window.MapPixelToCoords(new Vector2i(e.X, e.Y)),
                Camera
            );
        }
    }

    public void Run(Func<Transform, RenderStates> statesFactory, Action? preDraw, Action? postDraw)
    {
        Transform transform = Transform.Identity;
        transform.Translate(Window.Size.X / 2, Window.Size.Y / 2);
        transform.Scale(Window.Size.X / 2, -Window.Size.Y / 2);

        while (Window.IsOpen)
        {
            Window.DispatchEvents();
            Window.Clear(Color.Cyan);

            preDraw?.Invoke();
            DoDraw(statesFactory(transform));
            postDraw?.Invoke();

            //if (click is not null && drag is not null)
            //{
            //    var vertArray = new VertexArray(PrimitiveType.Lines, 2);
            //    vertArray[0] = new Vertex(click.Value);
            //    vertArray[1] = new Vertex(drag.Value);

            //    Window.Draw(vertArray);
            //}

            // Test for rotation drag
            //if(unit is not null)
            //{
            //    var vertArray = new VertexArray(PrimitiveType.Lines, 2);
            //    var center = new Vector2f(Window.Size.X / 2f, Window.Size.Y / 2f);
            //    vertArray[0] = new Vertex(center, Color.Magenta);
            //    vertArray[1] = new Vertex(center + new Vector2f(unit.Value.X * Window.Size.X / 2f, -unit.Value.Y * Window.Size.Y / 2f), Color.Magenta);

            //    Window.Draw(vertArray);
            //}

            Window.Display();
        }
    }

    protected void DoDraw(RenderStates state)
    {
        var viewMatrix = Matrix4x4.CreateLookAt(Camera.Position, Camera.LookAt, Camera.Up);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Camera.FieldOfView, Camera.AspectRatio, 1f, 100f);

        var vp = viewMatrix * projectionMatrix;


        List<Triangle3f> triangles = new List<Triangle3f>(
            Polygons
                .SelectMany(p => p.GetTriangles())
                .Select(t => t * vp)
                );

        VertexArray vertexArray = new VertexArray(PrimitiveType.Triangles);


        // draw back to front
        foreach (var triangle in triangles.OrderByDescending(t => t.MaxZ))
        {
            if (triangle.P1.X is < -1 or > 1 || triangle.P1.Y is < -1 or > 1 || triangle.P1.Z is < -1 or > 1)
                continue;

            var p1 = new Vector2f(triangle.P1.X, triangle.P1.Y);
            var p2 = new Vector2f(triangle.P2.X, triangle.P2.Y);
            var p3 = new Vector2f(triangle.P3.X, triangle.P3.Y);

            // prune triangles that are clockwise (seen from behind)
            if (!Triangle3f.IsVisible(p1, p2, p3))
                continue;

            vertexArray.Append(new Vertex(p1, triangle.Col, new Vector2f(0, 0)));
            vertexArray.Append(new Vertex(p2, triangle.Col, new Vector2f(0, 200)));
            vertexArray.Append(new Vertex(p3, triangle.Col, new Vector2f(200, 0)));
        }

        Window.Draw(vertexArray, state);

        if(ShowAxis)
        {
            var lines = new VertexArray(PrimitiveType.Lines);

            var zero = vp.Multiply(Vector3.Zero);

            var zero2f = new Vector2f(zero.X, zero.Y);

            var x = vp.Multiply(Vector3.UnitX);
            var y = vp.Multiply(Vector3.UnitY);
            var z = vp.Multiply(Vector3.UnitZ);

            lines.Append(new Vertex(zero2f, Color.Red));
            lines.Append(new Vertex(new Vector2f(x.X, x.Y), Color.Red));
            lines.Append(new Vertex(zero2f, Color.Blue));
            lines.Append(new Vertex(new Vector2f(y.X, y.Y), Color.Blue));
            lines.Append(new Vertex(zero2f, Color.Green));
            lines.Append(new Vertex(new Vector2f(z.X, z.Y), Color.Green));

            Window.Draw(lines, state);
        }
    }
}
