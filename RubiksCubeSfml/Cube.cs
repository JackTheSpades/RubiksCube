using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RubiksCubeSfml;

/// <summary>
/// Unit Cube centered about the origin.
/// </summary>
/// <remarks>Composed of 12 triangles, 2 for each face in the order of front, right, back, left, top, down.</remarks>
public class Cube : ITransformable, IPolygon
{
    Triangle3f[] Triangles;

    Matrix4x4 TranslationScale;
    Matrix4x4 TranslationScaleInverse;

    public Cube(Color[] colors) : this(new Vector3f(0, 0, 0), new Vector3f(1f, 1f, 1f), colors) { }

    public Cube(Vector3f position, Vector3f scale, Color[] colors)
    {
        TranslationScale = Matrix4x4.CreateScale(scale.X, scale.Y, scale.Z) *
            Matrix4x4.CreateTranslation(position.X, position.Y, position.Z);
        if (!Matrix4x4.Invert(TranslationScale, out TranslationScaleInverse))
            throw new ArgumentException($"{nameof(position)} and {nameof(scale)} could not be inverted.");

        if (!(colors.Length is 1 or 6 or 12))
            throw new ArgumentException($"{nameof(colors)} must be an array of length 1, 6, 12", nameof(colors));


        Vector3f R, T, F, L, D, B;

        R = new Vector3f(0.5f, 0, 0);
        T = new Vector3f(0, 0.5f, 0);
        F = new Vector3f(0, 0, 0.5f);

        L = -R;
        D = -T;
        B = -F;

        Triangles = [
            // front
            new(L + T + F,     L + D + F,      R + T + F,  colors[0 % colors.Length]),
            new(R + D + F,     R + T + F,      L + D + F,  colors[6 % colors.Length]),

            // right
            new(R + T + F,     R + D + F,      R + T + B,  colors[1 % colors.Length]),
            new(R + D + B,     R + T + B,      R + D + F,  colors[7 % colors.Length]),

            // back
            new(R + T + B,     R + D + B,      L + T + B,  colors[2 % colors.Length]),
            new(L + D + B,     L + T + B,      R + D + B,  colors[8 % colors.Length]),

            // left
            new(L + T + B,     L + D + B,      L + T + F,  colors[3 % colors.Length]),
            new(L + D + F,     L + T + F,      L + D + B,  colors[9 % colors.Length]),

            // top
            new(L + T + B,     L + T + F,      R + T + B,  colors[4 % colors.Length]),
            new(R + T + F,     R + T + B,      L + T + F,  colors[10 % colors.Length]),

            // down
            new(L + D + F,     L + D + B,      R + D + F,  colors[5 % colors.Length]),
            new(R + D + B,     R + D + F,      L + D + B,  colors[11 % colors.Length]),
        ];


#if DEBUG
        // sanity check of triangle setup
        if (Triangles.SelectMany(t => t).Any(v => float.Abs(v.X) != 0.5f || float.Abs(v.Y) != 0.5f || float.Abs(v.Z) != 0.5f))
            throw new Exception("Huh?");
#endif

        // initial transformation for location and scaling
        for (int i = 0; i < Triangles.Length; i++)
            Triangles[i].Transform(TranslationScale);
    }

    public void Transform(Matrix4x4 matrix)
    {
        for(int i = 0; i < Triangles.Length; i++)
            Triangles[i].Transform(TranslationScaleInverse * matrix * TranslationScale);
    }

    public IEnumerable<Triangle3f> GetTriangles() => Triangles;
}
