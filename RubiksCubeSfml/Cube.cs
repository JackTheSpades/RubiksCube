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
public class Cube : IPolygon
{
    Triangle[] Triangles;
    public Matrix4x4 Transformation { get; set; }


    public Cube(Color[] colors) : this(colors, Matrix4x4.Identity) { }
    public Cube(Color[] colors, Matrix4x4 transformation)
    {
        Transformation = transformation;

        if (!(colors.Length is 1 or 6 or 12))
            throw new ArgumentException($"{nameof(colors)} must be an array of length 1, 6, 12", nameof(colors));


        Vector3 R, T, F, L, D, B;

        R = new Vector3(0.5f, 0, 0);
        T = new Vector3(0, 0.5f, 0);
        F = new Vector3(0, 0, 0.5f);

        L = -R;
        D = -T;
        B = -F;

        Triangles = [
            // front
            new(L + T + F, L + D + F, R + T + F, colors[0 % colors.Length]),
            new(R + D + F, R + T + F, L + D + F, colors[6 % colors.Length]),

            // right
            new(R + T + F, R + D + F, R + T + B, colors[1 % colors.Length]),
            new(R + D + B, R + T + B, R + D + F, colors[7 % colors.Length]),

            // back
            new(R + T + B, R + D + B, L + T + B, colors[2 % colors.Length]),
            new(L + D + B, L + T + B, R + D + B, colors[8 % colors.Length]),

            // left
            new(L + T + B, L + D + B, L + T + F, colors[3 % colors.Length]),
            new(L + D + F, L + T + F, L + D + B, colors[9 % colors.Length]),

            // top
            new(L + T + B, L + T + F, R + T + B, colors[4 % colors.Length]),
            new(R + T + F, R + T + B, L + T + F, colors[10 % colors.Length]),

            // down
            new(L + D + F, L + D + B, R + D + F, colors[5 % colors.Length]),
            new(R + D + B, R + D + F, L + D + B, colors[11 % colors.Length]),
        ];

        // Simplified object for debugging purpose
        // Just contains the triangles needed
        //Triangles = [
        //    // right
        //    new(R + T + F, R + D + F, R + T + B, colors[1 % colors.Length]),
        //    // top
        //    new(R + T + F, R + T + B, L + T + F, colors[10 % colors.Length]),
        //];


#if DEBUG
        // sanity check of triangle setup
        if (Triangles.SelectMany(t => t).Any(v => float.Abs(v.X) != 0.5f || float.Abs(v.Y) != 0.5f || float.Abs(v.Z) != 0.5f))
            throw new Exception("Huh?");
#endif
    }


    public IEnumerable<Triangle> GetTriangles() => Triangles.Select(t => t * Transformation);
}
