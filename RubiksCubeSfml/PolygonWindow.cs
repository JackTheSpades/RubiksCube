    using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RubiksCubeSfml;

internal class PolygonWindow
{
    Vector2f? click = null, drag = null;
    Matrix4x4? currentTransform = null;
    float zoomExponent = 0;

    public readonly IPolygon Polygon;
    public readonly RenderWindow Window;

    public PolygonWindow(IPolygon polygon)
    {
        Polygon = polygon;

        var settings = new ContextSettings();
        settings.AntialiasingLevel = 16;

        Window = new RenderWindow(new VideoMode(800, 600), "Rubik's Cube", Styles.Default, settings);
        Window.SetFramerateLimit(60);

        Window.Closed += (_, _) => Window.Close();
        Window.MouseButtonPressed += Window_MouseButtonPressed;
        Window.MouseButtonReleased += Window_MouseButtonReleased;
        Window.MouseMoved += Window_MouseMoved;
        Window.MouseWheelScrolled += Window_MouseWheelScrolled;
    }

    private void Window_MouseWheelScrolled(object? sender, MouseWheelScrollEventArgs e)
    {
        //zoomExponent = float.Clamp(zoomExponent + e.Delta / 10f, 0.01f, 10f);
        zoomExponent += e.Delta / 10f;
    }

    void Window_MouseMoved(object? sender, MouseMoveEventArgs e)
    {
        if (click is null)
            return;

        

        drag = Window.MapPixelToCoords(new Vector2i(e.X, e.Y));


        if (currentTransform is null)
            currentTransform = Polygon.Transformation;

        var dir = drag.Value - click.Value;

        // normalized so that dragging across the whole window is 1 (or -1 if dragged in oposite direction)
        float normX = dir.X / Window.Size.X;
        float normY = dir.Y / Window.Size.Y;

        float angle = MathF.Atan2(-normY, normX);   // y goes top to bottom, for angle we want y to go up.
        float length = MathF.Sqrt(normX * normX + normY * normY);


        angle += MathF.PI / 2f; // we want angle orthagonal to our drawn line

        //unit vector in the XY-Plane that is 90° counter clockwise to our dragged line
        Vector3 unit = new(MathF.Cos(angle), MathF.Sin(angle), 0);

        Polygon.Transformation = currentTransform.Value *
            Matrix4x4.CreateFromAxisAngle(unit, length * MathF.Tau);
    }

    void Window_MouseButtonReleased(object? sender, MouseButtonEventArgs e)
    {
        currentTransform = null;
        click = null;
        drag = null;
    }
    void Window_MouseButtonPressed(object? sender, MouseButtonEventArgs e)
    {
        click = Window.MapPixelToCoords(new Vector2i(e.X, e.Y));
    }

    public void Run(Func<Transform, RenderStates> statesFactory, Action? preDraw, Action? postDraw)
    {
        Transform transform = Transform.Identity;
        transform.Translate(Window.Size.X / 2, Window.Size.Y / 2);
        transform.Scale(1, -1);

        while (Window.IsOpen)
        {
            Window.DispatchEvents();
            Window.Clear(Color.Cyan);

            preDraw?.Invoke();
            Transform transformLocal = transform;
            float zoomFactor = MathF.Exp(zoomExponent);
            transformLocal.Scale(zoomFactor, zoomFactor);
            Window.Draw(Polygon, statesFactory(transformLocal));
            postDraw?.Invoke();

            if (click is not null && drag is not null)
            {
                var vertArray = new VertexArray(PrimitiveType.Lines, 2);
                vertArray[0] = new Vertex(click.Value);
                vertArray[1] = new Vertex(drag.Value);

                Window.Draw(vertArray);
            }

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
}
