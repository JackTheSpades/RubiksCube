using System.Numerics;

namespace RubiksCubeSfml;

public interface IPolygon
{
    public IEnumerable<Triangle3f> GetTriangles();
    public void Transform(Matrix4x4 matrix);
}

public class PolygonList<T> : List<T>, IPolygon
    where T : IPolygon
{
    public PolygonList() { }
    public PolygonList(IEnumerable<T> collection) : base(collection) { }
    public PolygonList(int capacity) : base(capacity) { }

    public IEnumerable<Triangle3f> GetTriangles() => this.SelectMany(t => t.GetTriangles());
    public void Transform(Matrix4x4 matrix) => ForEach(p => p.Transform(matrix));
}