using SFML.Graphics;
using SFML.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RubiksCubeSfml;

public interface ITransformable
{
    public void Transform(Matrix4x4 matrix);
}
public interface IPolygon
{
    public IEnumerable<Triangle> GetTriangles();
    Vector3f Position { get; set; }
    Vector3f Scale { get; set; }
}

public class Triangle : IEnumerable<Vector3f>, ITransformable
{
    public float MinZ { get; private set; }

    public Color Col { get; }
    public Vector3f P1 { get; private set; }
    public Vector3f P2 { get; private set; }
    public Vector3f P3 { get; private set; }

    public Triangle(Vector3f p1, Vector3f p2, Vector3f p3, Color c)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
        Col = c;

        MinZ = float.Min(p1.Z, float.Min(p2.Z, p3.Z));
    }

    public static bool IsVisible(Vector2f p1, Vector2f p2, Vector2f p3)
    {
        // https://math.stackexchange.com/a/1324213
        float det =
            (p1.X * p2.Y) + (p1.Y * p3.X) + (p2.X * p3.Y)
            - (p2.Y * p3.X) - (p1.X  * p3.Y) - (p1.Y * p2.X);

        // det > 0 -> points are counter clockwise
        // det = 0 -> points are in line
        // det < 0 -> points are clockwise
        return det > 0;
    }

    public void Transform(Matrix4x4 matrix)
    {
        P1 = matrix.Multiply(P1);
        P2 = matrix.Multiply(P2);
        P3 = matrix.Multiply(P3);

        MinZ = float.Min(P1.Z, float.Min(P2.Z, P3.Z));
    }


    public IEnumerator<Vector3f> GetEnumerator()
    {
        yield return P1;
        yield return P2;
        yield return P3;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class Cube : ITransformable, IPolygon
{
    

    Triangle[] Triangles;

    Vector3f Up, Right, Front;

    public Vector3f Position { get; set; }
    public Vector3f Scale { get; set; }

    public Cube(Color[] colors) : this(new Vector3f(0, 0, 0), new Vector3f(1f, 1f, 1f), colors) { }

    public Cube(Vector3f position, Vector3f scale, Color[] colors)
    {
        Position = position;
        Scale = scale;

        Vector3f R, T, F, L, D, B;

        R = new Vector3f(0.5f, 0, 0);
        T = new Vector3f(0, 0.5f, 0);
        F = new Vector3f(0, 0, 0.5f);

        L = -R;
        D = -T;
        B = -F;

        Up = T * 2;
        Right = R * 2;
        Front = F * 2;

        Triangles = new Triangle[]
        {
            // front
            new Triangle(L + T + F,     L + D + F,      R + T + F,  colors[0]),
            new Triangle(R + D + F,     R + T + F,      L + D + F,  colors[0]),

            // right
            new Triangle(R + T + F,     R + D + F,      R + T + B,  colors[1]),
            new Triangle(R + D + B,     R + T + B,      R + D + F,  colors[1]),

            // back
            new Triangle(R + T + B,     R + D + B,      L + T + B,  colors[2]),
            new Triangle(L + D + B,     L + T + B,      R + D + B,  colors[2]),

            // left
            new Triangle(L + T + B,     L + D + B,      L + T + F,  colors[3]),
            new Triangle(L + D + F,     L + T + F,      L + D + B,  colors[3]),

            // top
            new Triangle(L + T + B,     L + T + F,      R + T + B,  colors[4]),
            new Triangle(R + T + F,     R + T + B,      L + T + F,  colors[4]),

            // down
            new Triangle(L + D + F,     L + D + B,      R + D + F,  colors[5]),
            new Triangle(R + D + B,     R + D + F,      L + D + B,  colors[5]),
        };


#if DEBUG
        if (Triangles.SelectMany(t => t).Any(v => float.Abs(v.X) != 0.5f || float.Abs(v.Y) != 0.5f || float.Abs(v.Z) != 0.5f))
            throw new Exception("Huh?");
#endif
    }

    public void Transform(Matrix4x4 matrix)
    {
        for(int i = 0; i < Triangles.Length; i++)
            Triangles[i].Transform(matrix);

        Up = matrix.Multiply(Up);
        Right = matrix.Multiply(Right);
        Front = matrix.Multiply(Front);
    }

#if false
    public void Draw(RenderTarget target, RenderStates states)
    {
        VertexArray vertexArray = new VertexArray(PrimitiveType.Lines);

        Vector2f z = new Vector2f(0, 0);
        vertexArray.Append(new Vertex(z, Color.Red));
        vertexArray.Append(new Vertex(Convert(Up), Color.Red));
        vertexArray.Append(new Vertex(z, Color.Blue));
        vertexArray.Append(new Vertex(Convert(Front), Color.Blue));
        vertexArray.Append(new Vertex(z, Color.Green));
        vertexArray.Append(new Vertex(Convert(Right), Color.Green));
        target.Draw(vertexArray, states);
    }
#endif

    public IEnumerable<Triangle> GetTriangles() => Triangles;
}
