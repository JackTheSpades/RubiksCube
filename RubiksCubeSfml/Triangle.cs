using SFML.Graphics;
using SFML.System;
using System.Collections;
using System.Numerics;

namespace RubiksCubeSfml;

public struct Triangle : IEnumerable<Vector3>, IPolygon
{
    public Color Col { get; }

    public Vector3 P1 { get; }
    public Vector3 P2 { get; }
    public Vector3 P3 { get; }
    public Matrix4x4 Transformation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, Color c)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
        Col = c;
    }

    public static bool IsVisible(Vector3 p1, Vector3 p2, Vector3 p3)
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

    public static Triangle operator*(Triangle triangle, Matrix4x4 matrix)
    {
        return new Triangle(
            matrix.Multiply(triangle.P1),
            matrix.Multiply(triangle.P2),
            matrix.Multiply(triangle.P3),
            triangle.Col
        );
    }

    public IEnumerator<Vector3> GetEnumerator()
    {
        yield return P1;
        yield return P2;
        yield return P3;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<Triangle> GetTriangles()
    {
        yield return this;
    }
}
