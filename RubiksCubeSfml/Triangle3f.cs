using SFML.Graphics;
using SFML.System;
using System.Collections;
using System.Numerics;

namespace RubiksCubeSfml;

public class Triangle3f : IEnumerable<Vector3f>, IPolygon
{
    public float MinZ { get; private set; }

    public Color Col { get; }
    public Vector3f P1 { get; private set; }
    public Vector3f P2 { get; private set; }
    public Vector3f P3 { get; private set; }

    public Triangle3f(Vector3f p1, Vector3f p2, Vector3f p3, Color c)
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

    public IEnumerable<Triangle3f> GetTriangles()
    {
        yield return this;
    }
}
