    using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using static SFML.Graphics.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public Font Font { get; set; }

    static RenderWindow DefaultWindow(string title)
    {
        var settings = new ContextSettings();
        settings.AntialiasingLevel = 16;

        var window = new RenderWindow(new VideoMode(800, 600), title, SFML.Window.Styles.Default, settings);
        window.SetFramerateLimit(60);

        return window;
    }

    public PolygonWindow(Camera camera, string title, params IPolygon[] polygons) : this(camera, () => DefaultWindow(title), polygons) { }
    public PolygonWindow(Camera camera, Func<RenderWindow> windowFactory, params IPolygon[] polygons)
    {
        Camera = camera;
        Polygons = new List<IPolygon>(polygons);

        string fontFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        Font = new Font(Path.Combine(fontFolder, "Consola.ttf"));

        Window = windowFactory();

        Window.Closed += (_, _) => Window.Close();
        Window.MouseButtonPressed += Window_MouseButtonPressed;
        Window.MouseButtonReleased += Window_MouseButtonReleased;
        Window.MouseMoved += Window_MouseMoved;
        Window.MouseWheelScrolled += Window_MouseWheelScrolled;
    }

    #region Events

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
        var screenCoords = Window.MapPixelToCoords(new Vector2i(e.X, e.Y));

        var clipCoords = new Vector2f(screenCoords.X / (Window.Size.X / 2) - 1, screenCoords.Y / (-Window.Size.Y / 2) - 1);
        Console.WriteLine($"x: {clipCoords.X}, y: {clipCoords.Y}");

        if (e.Button == Mouse.Button.Left && MoveClick is null)
        {
            RotateClick = new ClickInfo(screenCoords, Camera);
        }
        if (e.Button == Mouse.Button.Right && RotateClick is null)
        {
            MoveClick = new ClickInfo(screenCoords, Camera);
        }
    }

#endregion

    public void Add(IPolygon polygon) { Polygons.Add(polygon); }

    public Queue<TimeSpan> RenderTime { get; } = new Queue<TimeSpan>();

    public void Run(Func<Transform, RenderStates> statesFactory, Action? preDraw, Action? postDraw)
    {
        while (Window.IsOpen)
        {
            Window.DispatchEvents();
            Window.Clear(Color.Cyan);


            // general transformation from clip space onto screen
            // clip space is in range [-1, 1] on all three axis with X going right, Y going up and Z going back.
            // screen space has the origin in the top left corner and Y goes downwards.
            Transform transform = Transform.Identity;
            transform.Translate(Window.Size.X / 2, Window.Size.Y / 2);
            transform.Scale(Window.Size.X / 2, -Window.Size.Y / 2);

            preDraw?.Invoke();

            Stopwatch sw = Stopwatch.StartNew();
            DoDraw(statesFactory(transform));
            sw.Stop();

            if (RenderTime.Count >= 20)
                RenderTime.Dequeue();
            RenderTime.Enqueue(sw.Elapsed);


            var tms = RenderTime.Average(ts => ts.TotalMilliseconds);
            var debugText = new Text($"{tms:00.00}ms", Font);
            debugText.OutlineThickness = 0.5f;
            debugText.CharacterSize = 20;
            debugText.Position = new Vector2f(10f, 10f);
            Window.Draw(debugText);

            postDraw?.Invoke();

            Window.Display();
        }
    }


    protected void DoDraw(RenderStates state)
    {
        var viewMatrix = Matrix4x4.CreateLookAt(Camera.Position, Camera.LookAt, Camera.Up);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Camera.FieldOfView, Camera.AspectRatio, 1f, 100f);

        var vp = viewMatrix * projectionMatrix;


        List<Triangle> triangles =
            Polygons
                .SelectMany(p => p.GetTriangles())
                .Select(t => t * vp)
                .Where(t => Triangle.IsVisible(t.P1, t.P2, t.P3))
                .ToList();

        List<Vector3> vertices = triangles.SelectMany(t => t).ToList();

        VertexArray vertexArray = new VertexArray(PrimitiveType.Triangles);

        int[] indizes = SortTriangles(vertices);

        // draw back to front
        foreach (int triIndex in indizes)
        {
            int vertIndex = triIndex * 3;
            if (vertices[vertIndex].Z is < -1 or > 1 ||
                vertices[vertIndex + 1].Z is < -1 or > 1 ||
                vertices[vertIndex + 2].Z is < -1 or > 1)
                        continue;

            var p1 = new Vector2f(vertices[vertIndex].X, vertices[vertIndex].Y);
            var p2 = new Vector2f(vertices[vertIndex + 1].X, vertices[vertIndex + 1].Y);
            var p3 = new Vector2f(vertices[vertIndex + 2].X, vertices[vertIndex + 2].Y);


            vertexArray.Append(new Vertex(p1, triangles[triIndex].Col, new Vector2f(0, 0)));
            vertexArray.Append(new Vertex(p2, triangles[triIndex].Col, new Vector2f(0, 200)));
            vertexArray.Append(new Vertex(p3, triangles[triIndex].Col, new Vector2f(200, 0)));
        }

        Window.Draw(vertexArray, state);


        if (ShowAxis)
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

    public static int[] SortTriangles(List<Vector3> vertices)
    {
        /*
         * Triangle sorting is fairly tricky, since we can't just use the Z coordinates
         * Imagine two slanted triangles (coordinates in clip space after dividing by w)
         * such that the above triangle is positioned behind the bottom triangle
         * but still reaching further into the Z axis and stopping sooner.
         * 
         *   y                          y                       y    
         *   ^                          ^                       ^    
         *   |            /             |        /\             |        /\
         *   |           /              |       /  \            |       /  \
         *   |          /               |      /    \           |      /    \
         *   |         / /              |     /  /\  \          |    \‾‾‾‾‾‾‾‾/
         *   |        / /               |    /__/  \__\         |    /\      /\
         *   |         /                |      /    \           |    ‾‾\    /‾‾
         *   |        /                 |     /      \          |       \  /
         *   |       /                  |    /________\         |        \/
         *   |                          |                       |    
         *   o----------------> z       o--------------> x      o--------------> x
         *           123456                 Front Option 1          Front Option 2
         *   
         * Assuming we have Triangle.MinZ as Min(Triangle.P1.Z, Triangle.P2.Z, Triangle.P3.Z)
         * and a correlated MaxZ, we can see that neither of these would provide the correct sorting
         *   
         * We need to draw the triangles back to front, such that triangles in the front are drawn on top of
         * the earlier ones, as we don't have a Z-Buffer available.
         *   
         * For the above illustration the top triangle would have a MinZ of 2 and MaxZ of 6, while the 
         * bottom triangle has a MinZ of 1 and MaxZ of 5.
         * So regardless what we sort by, the bottom triangle would be sorted before the top triangle.
         * 
         * To sort two triangles we compare all their edges against one another.
         * First, we look for a 2D intersection point in which we ignore the Z coordinate.
         * Since the coordinates are already in clip space and the final drawing will only use X/Y this will match with
         * intersections on screen.
         * 
         * If two edges intersect, we can calculate the X/Y coordinates of this intersection.
         * We can then translate back into 3D and solve for Z on both edges for this intersection.
         * The triangle with the higher z will be in the back.
         * 
         * 
         * 
         * After we also need to check if one triangle is fully inside the other since this would mean their
         * edges don't intersect
         * For this, we check if the average point of triangle i is contained within triangle j
         * and vice versa. We use the average to avoid rounding shenanigans on triangles that share edges
         * 
         * Once we have a point that is inside the triangle (from the 2D screen perspective) we can just take that point's Z coordinate.
         * We can then use ray tracing starting from that point along the Z axis to get the point on the triangle with the same X/Y coordinates.
         * Then we can compare the average point's Z with the triangle intersection point Z.
         * 
        */

        const float epsilon = 0.0001f;

        var triIndizes = Enumerable.Range(0, vertices.Count / 3).ToArray();

        for(int i = 0; i < triIndizes.Length; i++)
            for(int j = i + 1;  j < triIndizes.Length; j++)
            {

                // start index of the triangle in vertices buffer
                int triangle1 = triIndizes[i] * 3;
                int triangle2 = triIndizes[j] * 3;

                if (!BoxIntersect(
                    vertices[triangle1], vertices[triangle1 + 1], vertices[triangle1 + 2],
                    vertices[triangle2], vertices[triangle2 + 1], vertices[triangle2 + 2]
                    ))
                    continue;

                // each triangle has 3 edges [0,1], [1,2] [2,0]
                for (int edge1 = 0; edge1 < 3; edge1++)
                    for(int edge2 = 0; edge2 < 3; edge2++)
                    {
                        var a1 = vertices[triangle1 + edge1];
                        var b1 = vertices[triangle1 + ((edge1 + 1) % 3)];

                        var a2 = vertices[triangle2 + edge2];
                        var b2 = vertices[triangle2 + ((edge2 + 1) % 3)];

                        var dir1 = b1 - a1;
                        var dir2 = b2 - a2;

                        (float u, float v) = RayIntersect2D(a1, dir1, a2, dir2, epsilon);

                        // check if the rays were parallel
                        // or if one of the directions was (0,0,z) aka paralle to the z-axis
                        if (float.IsNaN(u))
                            continue;

                        // equation was solved such that a1 + dir1 * u == a2 + dir2 * v
                        // this intersection could have been anywhere in 2D space.
                        // since dir = b - a
                        // we can make sure that the intersection happened in the [a1,b1] [a2,b2] range
                        if (u is <= epsilon or >= 1-epsilon || v is <= epsilon or >= 1-epsilon)
                            continue;

                        float z1 = (a1 + dir1 * u).Z;
                        float z2 = (a2 + dir2 * v).Z;

                        // higher z value means it needs to be drawn first
                        // meaning it needs to be ahead in the array
                        if (z2 > z1)
                            (triIndizes[j], triIndizes[i]) = (triIndizes[i], triIndizes[j]);

                        // release the velociraptor
                        goto EndCheck;
                    }



                // check if point of triangle 1 is inside triangle 2
                int ptIndex = triangle1;
                int triIndex = triangle2;
                bool swapped = false;
                Vector3 pt = (vertices[ptIndex] + vertices[ptIndex + 1] + vertices[ptIndex + 2]) / 3;

                if (!PointInTriangle2D(pt, vertices[triIndex], vertices[triIndex + 1], vertices[triIndex + 2]))
                {
                    // if not, check if avg(tri-2) is inside tri-1
                    ptIndex = triangle2;
                    triIndex = triangle1;
                    pt = (vertices[ptIndex] + vertices[ptIndex + 1] + vertices[ptIndex + 2]) / 3;
                    swapped = true;

                    if (!PointInTriangle2D(vertices[ptIndex], vertices[triIndex], vertices[triIndex + 1], vertices[triIndex + 2]))
                    {
                        ptIndex = -1;
                        triIndex = -1;
                    }
                }

                if(ptIndex >= 0 && triIndex >= 0)
                {
                    float zPt = pt.Z;

                    // ray tracing using the average point as origin and going along the z axis for direction.
                    // the intersection with the triangle is (O + D * t)
                    float t = MoellerTrumbore(pt, Vector3.UnitZ,
                        vertices[triIndex], vertices[triIndex + 1], vertices[triIndex + 2],
                        epsilon);

                    float zTri = (pt + Vector3.UnitZ * t).Z;

                    // swapped = false -> zPt is triangle[i], zTri is triangle[j]   -> zi = zPt,  zj = zTri
                    // swapped = true  -> zPt is triangle[j], zTri is triangle[i]   -> zi = zTri, zj = zPt
                    // higher z needs to have lower index (be drawn first)

                    float zi = zPt, zj = zTri;
                    if (swapped)
                        (zi, zj) = (zTri, zPt);

                    if (zj > zi)
                        (triIndizes[j], triIndizes[i]) = (triIndizes[i], triIndizes[j]);
                }







            // unpopular opinon:
            // labels and goto are great for breaking out of multiple nested loops
            EndCheck:;
            }

        return triIndizes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <returns></returns>
    /// <remarks><a href="https://stackoverflow.com/a/2049593">Source</a>.</remarks>
    public static bool PointInTriangle2D(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float sign(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        var d1 = sign(pt, v1, v2);
        var d2 = sign(pt, v2, v3);
        var d3 = sign(pt, v3, v1);

        var has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        var has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    static (float, float) MinMax(float a, float b, float c)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        if (a > max) max = a;
        if (b > max) max = b;
        if (c > max) max = c;

        if (a < min) min = a;
        if (b < min) min = b;
        if (c < min) min = c;

        return (min, max);
    }

    static bool BoxIntersect(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 w1, Vector3 w2, Vector3 w3)
    {
        (float left1, float right1) = MinMax(v1.X, v2.X, v3.X);
        (float left2, float right2) = MinMax(w1.X, w2.X, w3.X);
        (float top1, float bottom1) = MinMax(v1.Y, v2.Y, v3.Y);
        (float top2, float bottom2) = MinMax(w1.Y, w2.Y, w3.Y);

        float left = float.Max(left1, left2);
        float right = float.Min(right1, right2);
        float top = float.Max(top1, top2);
        float bottom = float.Min(bottom1, bottom2);

        float width = right - left;
        float height = bottom - top;
        if (width <= 0 || height <= 0)
            return false;
        return true;
    }

    /// <summary>
    /// Calculate the intersection point of two rays in 2D even though 3D vectors are used. The z-coordinate is ignored.
    /// </summary>
    /// <param name="aOrigin">Origin of Ray-A.</param>
    /// <param name="aDirection">Direction of Ray-A.</param>
    /// <param name="bOrigin">Origin of Ray-B.</param>
    /// <param name="bDirection">Direction of Ray-B.</param>
    /// <returns>The floats u and v such that <code>aOrigin + aDirection * u == bOrigin + bDirection * v</code>.
    /// Or <see cref="float.NaN"/> if the rays are parallel.</returns>
    /// <remarks><a href="https://stackoverflow.com/a/2932601/9883041">Source</a></remarks>
    public static (float u, float v) RayIntersect2D(Vector3 aOrigin, Vector3 aDirection, Vector3 bOrigin, Vector3 bDirection, float epsilon = float.Epsilon)
    {
        var dx = bOrigin.X - aOrigin.X;
        var dy = bOrigin.Y - aOrigin.Y;
        var det = bDirection.X * aDirection.Y - bDirection.Y * aDirection.X;
        if (float.Abs(det) < epsilon)
            return (float.NaN, float.NaN);
        var u = (dy * bDirection.X - dx * bDirection.Y) / det;
        var v = (dy * aDirection.X - dx * aDirection.Y) / det;
        return (u, v);
    }

    /// <summary>
    /// Möller–Trumbore Ray-Triangle intersection algorithm
    /// </summary>
    /// <param name="origin">The origin of the ray.</param>
    /// <param name="direction">The direction of the ray.</param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="epsilon">Small float to avoid checking floating-point numbers equaling zero.</param>
    /// <returns>The value t such that origin + direction * t describes the point on the triangle where the ray intersects.
    /// Or <see cref="float.NaN"/> if no intersection is found.</returns>
    /// <remarks><a href="https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm">Wikipedia Source</a>.</remarks>
    public static float MoellerTrumbore(Vector3 origin, Vector3 direction, Vector3 v1, Vector3 v2, Vector3 v3, float epsilon = 0.0001f)
    {
        Vector3 edge1 = v2 - v1;
        Vector3 edge2 = v3 - v1;
        Vector3 rayCrossEdge2 = Vector3.Cross(direction, edge2);
        float det = Vector3.Dot(edge1, rayCrossEdge2);

        // // This ray is parallel to this triangle.
        if (float.Abs(det) < epsilon)
            return float.NaN;

        float invDet = 1f / det;
        Vector3 s = origin - v1;
        float u = invDet * Vector3.Dot(s, rayCrossEdge2);

        if(u is < 0 or > 1)
            return float.NaN;

        Vector3 sCrossEdge1 = Vector3.Cross(s, edge1);
        float v = invDet * Vector3.Dot(direction, sCrossEdge1);

        if (v < 0 || v + u > 1)
            return float.NaN;

        float t = invDet * Vector3.Dot(edge2, sCrossEdge1);

        //if (t > epsilon)
        //    return t;
        //return float.NaN;

        return t;
    }
}
