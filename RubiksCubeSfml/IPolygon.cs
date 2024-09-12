namespace RubiksCubeSfml;

public interface IPolygon
{
    public IEnumerable<Triangle3f> GetTriangles();
}

public class PolygonList<T> : List<T>, IPolygon
    where T : IPolygon
{
    public PolygonList() { }
    public PolygonList(IEnumerable<T> collection) : base(collection) { }
    public PolygonList(int capacity) : base(capacity) { }
    public IEnumerable<Triangle3f> GetTriangles() => this.SelectMany(t => t.GetTriangles());
}