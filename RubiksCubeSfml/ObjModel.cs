using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace RubiksCubeSfml;

public record struct Vertex(uint PosIndex, uint TextureIndex, uint NormalIndex, uint Color = uint.MaxValue);
public record TriangleFace(Vertex A, Vertex B, Vertex C)
{
    public Vertex this[int index] => (index % 3) switch
    {
        0 => A,
        1 => B,
        2 => C,
        _ => throw new IndexOutOfRangeException(nameof(index)),
    };
}

public interface IModel
{
    IReadOnlyList<Vector3> Positions { get; }
    IReadOnlyList<Vector3> Normals { get; }
    IReadOnlyList<Vector2> TextureCoords { get; }
    List<TriangleFace> Faces { get; }

    Matrix4x4 Transformation { get; set; }
}

internal class ObjModel : IModel
{
    public IReadOnlyList<Vector3> Positions { get; }
    public IReadOnlyList<Vector3> Normals { get; }
    public IReadOnlyList<Vector2> TextureCoords { get; }
    public Matrix4x4 Transformation { get; set; }

    public List<TriangleFace> Faces { get; }

    static float ReadFloat(ReadOnlySpan<char> chars)
    {
        int end = 0;
        while (end < chars.Length && !char.IsWhiteSpace(chars[end]))
            end++;
        return float.Parse(chars[..end], CultureInfo.InvariantCulture);
    }
    static (uint data, int end) ReadUint(ReadOnlySpan<char> chars)
    {
        int end = 0;
        while (end < chars.Length && char.IsAsciiDigit(chars[end]))
            end++;
        if (end == 0)
            return (0u, end);
        return (uint.Parse(chars[..end]), end);
    }

    // Example: 123/456/789
    //          123/456
    //          123//456
    //          123
    //          pos/tex/norm

    static Vertex ReadVertex(ReadOnlySpan<char> chars)
    {
        int start = 0, end;
        uint v, vt, vn;
        (v, end) = ReadUint(chars);
        start += end + 1;
        (vt, end) = ReadUint(chars[start..]);
        start += end + 1;
        (vn, _) = ReadUint(chars[start..]);

        // random color
        // uint c = ((uint)Random.Shared.Next(0x1_00_00_00) << 8) | 0xFFu;
        uint c = Color.White.ToInteger();

        return new Vertex(v, vt, vn, c);
    }

    public ObjModel(string filename) : this(File.OpenRead(filename), (x, y) => new Vector2(x, y), false) { }

    public ObjModel(string filename, Func<float, float, Vector2> textureConfigure) : this(File.OpenRead(filename), textureConfigure, false) { }

    public ObjModel(Stream stream, bool leaveOpen = false) : this(stream, (x,y) => new Vector2(x, y), leaveOpen) { }

    public ObjModel(Stream stream, Func<float, float, Vector2> textureConfigure, bool leaveOpen = false)
    {
        Transformation = Matrix4x4.Identity;

        using StreamReader sr = new StreamReader(stream, leaveOpen: leaveOpen);

        Vector3 nan3 = new Vector3(float.NaN);
        Vector2 nan2 = new Vector2(float.NaN);

        var positions = new List<Vector3>() { nan3 };
        var normals = new List<Vector3>() { nan3 };
        var texCoords = new List<Vector2>() { nan2 };


        Faces = new List<TriangleFace>();

        string? line = null;
        float[] floatBuffer = new float[3];
        Vertex[] vertexBuffer = new Vertex[3];

        void ReadFloatBuffer(ReadOnlySpan<char> span)
        {
            int start = 0;
            int count = 0;
            int index = 0;
            while (true)
            {
                index = span[start..].IndexOf(' ');
                if (index == -1)
                    break;
                start += index + 1;
                floatBuffer[count++] = ReadFloat(span[start..]);
            }
        }
        void ReadVertexBuffer(ReadOnlySpan<char> span)
        {
            int start = 0;
            int count = 0;
            int index = 0;
            while (true)
            {
                index = span[start..].IndexOf(' ');
                if (index == -1)
                    break;
                start += index + 1;
                vertexBuffer[count++] = ReadVertex(span[start..]);
            }
        }

        while ((line = sr.ReadLine()) != null)
        {
            var span = line.AsSpan();

            // # This is a comment
            if (span.StartsWith("#"))
                continue;

            // v -0.409820 -0.755594 1.335047
            if (span.StartsWith("v "))
            {
                ReadFloatBuffer(span);
                positions.Add(new Vector3(floatBuffer[0], floatBuffer[1], floatBuffer[2]));
            }
            // vn 0.505417 0.771142 0.387127
            else if (span.StartsWith("vn "))
            {
                ReadFloatBuffer(span);
                normals.Add(new Vector3(floatBuffer[0], floatBuffer[1], floatBuffer[2]));
            }
            // vt 0.851163 0.438962
            else if (span.StartsWith("vt "))
            {
                ReadFloatBuffer(span);
                texCoords.Add(textureConfigure(floatBuffer[0], floatBuffer[1]));
            }
            else if (span.StartsWith("f "))
            {
                ReadVertexBuffer(span);
                Faces.Add(new TriangleFace(vertexBuffer[0], vertexBuffer[1], vertexBuffer[2]));
            }
        }
        Positions = positions;
        Normals = normals;
        TextureCoords = texCoords;
    }

}
