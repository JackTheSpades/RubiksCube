using System.Numerics;

namespace RubiksCubeSfml;

public interface ITransformable
{
    public void Transform(Matrix4x4 matrix);
}
