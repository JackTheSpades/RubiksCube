using System.Numerics;

namespace RubiksCubeSfml;

public interface IPolygon
{
    public Matrix4x4 Transformation { get; }
    public IEnumerable<Triangle3f> GetTriangles();
}

public class PolygonList<T> : List<T>, IPolygon
    where T : IPolygon
{
    public Matrix4x4 Transformation { get; set; } = Matrix4x4.Identity;

    public PolygonList() { }
    public PolygonList(Matrix4x4 transformation) { Transformation = transformation; }
    public PolygonList(IEnumerable<T> collection) : base(collection) { }
    public PolygonList(int capacity) : base(capacity) { }

    public IEnumerable<Triangle3f> GetTriangles() =>
        this
        .SelectMany(t => t.GetTriangles())
        .Select(t => t.Transform(Transformation));
}