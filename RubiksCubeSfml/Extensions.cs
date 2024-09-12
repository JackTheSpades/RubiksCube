using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RubiksCubeSfml;

public static class Extensions
{
    public static Vector3f Multiply(this Matrix4x4 m, Vector3f v)
    {
        float[] r = new float[4];
        for (int i = 0; i < 4; i++)
            r[i] = (m[0, i] * v.X) +
                    (m[1, i] * v.Y) +
                    (m[2, i] * v.Z) +
                    m[3, i];

        if (float.Abs(r[3] - 1) > 0.0001)
            throw new ArgumentException("Math isn't mathing?");

        return new Vector3f(r[0], r[1], r[2]);
    }

    public static void Draw(this RenderTarget target, IPolygon polygon, RenderStates states)
    {
        List<Triangle> triangles = new List<Triangle>(polygon.GetTriangles());
        VertexArray vertexArray = new VertexArray(PrimitiveType.Triangles);

        Matrix4x4 matrix =
            Matrix4x4.CreateScale(polygon.Scale.X, polygon.Scale.Y, polygon.Scale.Z) *
            Matrix4x4.CreateTranslation(polygon.Position.X, polygon.Position.Y, polygon.Position.Z);

        // draw back to front
        foreach (var triangle in triangles.OrderBy(t => t.MinZ))
        {
            var p1 = Convert(matrix.Multiply(triangle.P1));
            var p2 = Convert(matrix.Multiply(triangle.P2));
            var p3 = Convert(matrix.Multiply(triangle.P3));

            // prune triangles that are clockwise (seen from behind)
            if (!Triangle.IsVisible(p1, p2, p3))
                continue;

            vertexArray.Append(new Vertex(p1, triangle.Col, new Vector2f(0, 0)));
            vertexArray.Append(new Vertex(p2, triangle.Col, new Vector2f(0, 200)));
            vertexArray.Append(new Vertex(p3, triangle.Col, new Vector2f(200, 0)));
        }

        target.Draw(vertexArray, states);
    }


    private static Vector2f Convert(Vector3f v)
    {
        // Z-Axis is facing the camera.
        // So we apply bonus with Z
        const float alpha = 0.01f;
        float factor = MathF.Exp(v.Z * alpha);

        return new Vector2f(v.X * factor, v.Y * factor);
    }
}
