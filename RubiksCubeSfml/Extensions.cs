using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
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

        return new Vector3f(r[0] / r[3], r[1] / r[3], r[2] / r[3]);
    }
    public static Vector3 Multiply(this Matrix4x4 m, Vector3 v)
    {
        float[] r = new float[4];
        for (int i = 0; i < 4; i++)
            r[i] = (m[0, i] * v.X) +
                    (m[1, i] * v.Y) +
                    (m[2, i] * v.Z) +
                    m[3, i];

        return new Vector3(r[0] / r[3], r[1] / r[3], r[2] / r[3]);
    }

    public static string ToPrettyString(this Matrix4x4 m, string format = "0.000")
    {
        string[] vals = new string[16];
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                vals[i * 4 + j] = m[i, j].ToString(format, CultureInfo.InvariantCulture);

        int l = vals.Max(s => s.Length);
        //char[] begin = ['⎡', '⎢', '⎢', '⎣'];
        //char[] end = ['⎤', '⎥', '⎥', '⎦'];

        char[] begin = ['[', '[', '[', '['];
        char[] end = [']', ']', ']', ']'];

        StringBuilder sb = new();
        for (int r = 0; r < 4; r++)
        {
            sb.Append(begin[r]);

            for (int c = 0; c < 4; c++)
                sb.Append(vals[r * 4 + c].PadLeft(l)).Append(", ");

            sb.Remove(sb.Length - 2, 2)
                .Append(end[r])
                .AppendLine();
        }
        return sb.ToString();
    }

    public static void Draw(this RenderTarget target, IPolygon polygon, RenderStates states)
    {
        List<Triangle3f> triangles = new List<Triangle3f>(polygon.GetTriangles());
        VertexArray vertexArray = new VertexArray(PrimitiveType.Triangles);


        //RenderTexture rt = new RenderTexture(200, 200);
        //rt.Draw()
        //Texture tx;

        // draw back to front
        foreach (var triangle in triangles.OrderBy(t => t.MaxZ))
        {
            var p1 = Convert(triangle.P1);
            var p2 = Convert(triangle.P2);
            var p3 = Convert(triangle.P3);

            // prune triangles that are clockwise (seen from behind)
            if (!Triangle3f.IsVisible(p1, p2, p3))
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
