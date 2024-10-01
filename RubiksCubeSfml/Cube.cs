using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace RubiksCubeSfml;

public class CubeModel : IModel
{
    public IReadOnlyList<Vector3> Positions { get; }
    public IReadOnlyList<Vector3> Normals { get; }
    public IReadOnlyList<Vector2> TextureCoords { get; }
    public List<TriangleFace> Faces { get; }
    public Matrix4x4 Transformation { get; set; }

    const uint PosIndexLTF = 1;
    const uint PosIndexLDF = 2;
    const uint PosIndexRTF = 3;
    const uint PosIndexRDF = 4;
    const uint PosIndexLTB = 5;
    const uint PosIndexLDB = 6;
    const uint PosIndexRTB = 7;
    const uint PosIndexRDB = 8;

    const uint NormIndexF = 1;
    const uint NormIndexR = 2;
    const uint NormIndexB = 3;
    const uint NormIndexL = 4;
    const uint NormIndexT = 5;
    const uint NormIndexD = 6;

    public CubeModel(Color[] colors) : this(colors, Matrix4x4.Identity) { }
    public CubeModel(Color[] colors, Matrix4x4 transformation)
    {
        Transformation = transformation;

        uint[] uintColors = colors.Select(c => c.ToInteger()).ToArray();
        Vector3 nan3 = new Vector3(float.NaN);

        TextureCoords = new List<Vector2>() { new Vector2(0f) };

        Vector3 R = new Vector3(0.5f, 0, 0);
        Vector3 T = new Vector3(0, 0.5f, 0);
        Vector3 F = new Vector3(0, 0, 0.5f);
        Vector3 L = -R;
        Vector3 D = -T;
        Vector3 B = -F;

        Positions = new List<Vector3>(9)
        { 
            nan3,
            L + T + F,
            L + D + F,
            R + T + F,
            R + D + F,
            L + T + B,
            L + D + B,
            R + T + B,
            R + D + B,
        };

        Normals = new List<Vector3>(7) 
        { 
            nan3,
            Vector3.UnitZ,  // front
            Vector3.UnitX,  // right
            -Vector3.UnitZ, // back
            -Vector3.UnitX, // left
            Vector3.UnitY,  // top
            -Vector3.UnitY, // back
        };

        Faces = new List<TriangleFace>()
        {
            // Front
            new TriangleFace(
                new Vertex(PosIndexLTF, 0, NormIndexF, uintColors[0 % colors.Length]),
                new Vertex(PosIndexLDF, 0, NormIndexF, uintColors[0 % colors.Length]),
                new Vertex(PosIndexRTF, 0, NormIndexF, uintColors[0 % colors.Length])
            ),
            new TriangleFace(
                new Vertex(PosIndexRDF, 0, NormIndexF, uintColors[6 % colors.Length]),
                new Vertex(PosIndexRTF, 0, NormIndexF, uintColors[6 % colors.Length]),
                new Vertex(PosIndexLDF, 0, NormIndexF, uintColors[6 % colors.Length])
            ),

            // Right
            new TriangleFace(
                new Vertex(PosIndexRTF, 0, NormIndexR, uintColors[1 % colors.Length]),
                new Vertex(PosIndexRDF, 0, NormIndexR, uintColors[1 % colors.Length]),
                new Vertex(PosIndexRTB, 0, NormIndexR, uintColors[1 % colors.Length])
            ),
            new TriangleFace(
                new Vertex(PosIndexRDB, 0, NormIndexR, uintColors[7 % colors.Length]),
                new Vertex(PosIndexRTB, 0, NormIndexR, uintColors[7 % colors.Length]),
                new Vertex(PosIndexRDF, 0, NormIndexR, uintColors[7 % colors.Length])
            ),

            // Back
            new TriangleFace(
                new Vertex(PosIndexRTB, 0, NormIndexB, uintColors[2 % colors.Length]),
                new Vertex(PosIndexRDB, 0, NormIndexB, uintColors[2 % colors.Length]),
                new Vertex(PosIndexLTB, 0, NormIndexB, uintColors[2 % colors.Length])
            ),
            new TriangleFace(
                new Vertex(PosIndexLDB, 0, NormIndexB, uintColors[8 % colors.Length]),
                new Vertex(PosIndexLTB, 0, NormIndexB, uintColors[8 % colors.Length]),
                new Vertex(PosIndexRDB, 0, NormIndexB, uintColors[8 % colors.Length])
            ),
            
            // Left
            new TriangleFace(
                new Vertex(PosIndexLTB, 0, NormIndexL, uintColors[3 % colors.Length]),
                new Vertex(PosIndexLDB, 0, NormIndexL, uintColors[3 % colors.Length]),
                new Vertex(PosIndexLTF, 0, NormIndexL, uintColors[3 % colors.Length])
            ),
            new TriangleFace(
                new Vertex(PosIndexLDF, 0, NormIndexL, uintColors[9 % colors.Length]),
                new Vertex(PosIndexLTF, 0, NormIndexL, uintColors[9 % colors.Length]),
                new Vertex(PosIndexLDB, 0, NormIndexL, uintColors[9 % colors.Length])
            ),
            
            // Top
            new TriangleFace(
                new Vertex(PosIndexLTB, 0, NormIndexT, uintColors[4 % colors.Length]),
                new Vertex(PosIndexLTF, 0, NormIndexT, uintColors[4 % colors.Length]),
                new Vertex(PosIndexRTB, 0, NormIndexT, uintColors[4 % colors.Length])
            ),
            new TriangleFace(
                new Vertex(PosIndexRTF, 0, NormIndexT, uintColors[10 % colors.Length]),
                new Vertex(PosIndexRTB, 0, NormIndexT, uintColors[10 % colors.Length]),
                new Vertex(PosIndexLTF, 0, NormIndexT, uintColors[10 % colors.Length])
            ),
            
            // Down
            new TriangleFace(
                new Vertex(PosIndexLDF, 0, NormIndexD, uintColors[5 % colors.Length]),
                new Vertex(PosIndexLDB, 0, NormIndexD, uintColors[5 % colors.Length]),
                new Vertex(PosIndexRDF, 0, NormIndexD, uintColors[5 % colors.Length])
            ),
            new TriangleFace(
                new Vertex(PosIndexRDB, 0, NormIndexD, uintColors[11 % colors.Length]),
                new Vertex(PosIndexRDF, 0, NormIndexD, uintColors[11 % colors.Length]),
                new Vertex(PosIndexLDB, 0, NormIndexD, uintColors[11 % colors.Length])
            ),
        };
    }
}

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
