﻿using SFML.Graphics;
using SFML.System;
using System.Collections;
using System.Numerics;

namespace RubiksCubeSfml;

public struct Triangle3f : IEnumerable<Vector3f>, IPolygon
{
    public float MinZ { get; }
    public float MaxZ { get; }
    public Color Col { get; }

    public Vector3f P1 { get; }
    public Vector3f P2 { get; }
    public Vector3f P3 { get; }
    public Matrix4x4 Transformation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Triangle3f(Vector3f p1, Vector3f p2, Vector3f p3, Color c)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
        Col = c;

        MinZ = float.Min(p1.Z, float.Min(p2.Z, p3.Z));
        MaxZ = float.Max(p1.Z, float.Max(p2.Z, p3.Z));
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

    public static Triangle3f operator*(Triangle3f triangle, Matrix4x4 matrix)
    {
        return new Triangle3f(
            matrix.Multiply(triangle.P1),
            matrix.Multiply(triangle.P2),
            matrix.Multiply(triangle.P3),
            triangle.Col
        );
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
