using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RubiksCubeSfml;

public enum CubeMove
{
    Front = 1,
    Right = 2,
    Back = 3,
    Left = 4, 
    Top = 5,
    Down = 6,

    FrontInverted = 11,
    RightInverted = 12,
    BackInverted = 13,
    LeftInverted = 14,
    TopInverted = 15,
    DownInverted = 16,
}

public class Rubiks : IPolygon
{
    enum CubeX { Left = 0, Center = 1, Right = 2};
    enum CubeY { Down = 0, Center = 1, Top = 2 };
    enum CubeZ { Back = 0, Center = 1, Front = 2 };
    enum CubieFace { Front = 0, Right, Back, Left, Top, Down };

    private Cube[] CubieStore;


    public static Color[] CubieFaceColors { get; }
    /// <summary>Contains a replacement tuply-array of indices into the <see cref="CubieStore"/> for all the non-inverting moves.</summary>
    private static Dictionary<CubeMove, (int from, int to)[]> Replace;



    static Rubiks()
    {
        CubieFaceColors =
        [
            new(0, 0, 255),     // front  / blue
            new(255, 106, 0),   // right  / orange
            new(0, 200, 0),     // back   / green
            new(255, 0, 0),     // left   / red
            new(255, 255, 255), // white  / top
            new(242, 242, 0),   // yellow / down
        ];

        Replace = new();

        Replace[CubeMove.Right] = [
            (GetIndex(CubeX.Right, CubeY.Top, CubeZ.Front),     GetIndex(CubeX.Right, CubeY.Top, CubeZ.Back)),
            (GetIndex(CubeX.Right, CubeY.Top, CubeZ.Center),    GetIndex(CubeX.Right, CubeY.Center, CubeZ.Back)),
            (GetIndex(CubeX.Right, CubeY.Top, CubeZ.Back),      GetIndex(CubeX.Right, CubeY.Down, CubeZ.Back)),
            (GetIndex(CubeX.Right, CubeY.Center, CubeZ.Back),   GetIndex(CubeX.Right, CubeY.Down, CubeZ.Center)),
            (GetIndex(CubeX.Right, CubeY.Down, CubeZ.Back),     GetIndex(CubeX.Right, CubeY.Down, CubeZ.Front)),
            (GetIndex(CubeX.Right, CubeY.Down, CubeZ.Center),   GetIndex(CubeX.Right, CubeY.Center, CubeZ.Front)),
            (GetIndex(CubeX.Right, CubeY.Down, CubeZ.Front),    GetIndex(CubeX.Right, CubeY.Top, CubeZ.Front)),
            (GetIndex(CubeX.Right, CubeY.Center, CubeZ.Front),  GetIndex(CubeX.Right, CubeY.Top, CubeZ.Center)),
        ];

        Replace[CubeMove.Front] = [
            (GetIndex(CubeX.Left, CubeY.Top, CubeZ.Front),      GetIndex(CubeX.Right, CubeY.Top, CubeZ.Front)),
            (GetIndex(CubeX.Center, CubeY.Top, CubeZ.Front),    GetIndex(CubeX.Right, CubeY.Center, CubeZ.Front)),
            (GetIndex(CubeX.Right, CubeY.Top, CubeZ.Front),     GetIndex(CubeX.Right, CubeY.Down, CubeZ.Front)),
            (GetIndex(CubeX.Right, CubeY.Center, CubeZ.Front),  GetIndex(CubeX.Center, CubeY.Down, CubeZ.Front)),
            (GetIndex(CubeX.Right, CubeY.Down, CubeZ.Front),    GetIndex(CubeX.Left, CubeY.Down, CubeZ.Front)),
            (GetIndex(CubeX.Center, CubeY.Down, CubeZ.Front),   GetIndex(CubeX.Left, CubeY.Center, CubeZ.Front)),
            (GetIndex(CubeX.Left, CubeY.Down, CubeZ.Front),     GetIndex(CubeX.Left, CubeY.Top, CubeZ.Front)),
            (GetIndex(CubeX.Left, CubeY.Center, CubeZ.Front),   GetIndex(CubeX.Center, CubeY.Top, CubeZ.Front))
        ];

        Replace[CubeMove.Top] = [
            (GetIndex(CubeX.Left, CubeY.Top, CubeZ.Back),       GetIndex(CubeX.Right, CubeY.Top, CubeZ.Back)),
            (GetIndex(CubeX.Center, CubeY.Top, CubeZ.Back),     GetIndex(CubeX.Right, CubeY.Top, CubeZ.Center)),
            (GetIndex(CubeX.Right, CubeY.Top, CubeZ.Back),      GetIndex(CubeX.Right, CubeY.Top, CubeZ.Front)),
            (GetIndex(CubeX.Right, CubeY.Top, CubeZ.Center),    GetIndex(CubeX.Center, CubeY.Top, CubeZ.Front)),
            (GetIndex(CubeX.Right, CubeY.Top, CubeZ.Front),     GetIndex(CubeX.Left, CubeY.Top, CubeZ.Front)),
            (GetIndex(CubeX.Center, CubeY.Top, CubeZ.Front),    GetIndex(CubeX.Left, CubeY.Top, CubeZ.Center)),
            (GetIndex(CubeX.Left, CubeY.Top, CubeZ.Front),      GetIndex(CubeX.Left, CubeY.Top, CubeZ.Back)),
            (GetIndex(CubeX.Left, CubeY.Top, CubeZ.Center),     GetIndex(CubeX.Center, CubeY.Top, CubeZ.Back)),
        ];

        Replace[CubeMove.Left] = [
            (GetIndex(CubeX.Left, CubeY.Top, CubeZ.Back),       GetIndex(CubeX.Left, CubeY.Top, CubeZ.Front)),
            (GetIndex(CubeX.Left, CubeY.Center, CubeZ.Back),    GetIndex(CubeX.Left, CubeY.Top, CubeZ.Center)),
            (GetIndex(CubeX.Left, CubeY.Down, CubeZ.Back),      GetIndex(CubeX.Left, CubeY.Top, CubeZ.Back)),
            (GetIndex(CubeX.Left, CubeY.Down, CubeZ.Center),    GetIndex(CubeX.Left, CubeY.Center, CubeZ.Back)),
            (GetIndex(CubeX.Left, CubeY.Down, CubeZ.Front),     GetIndex(CubeX.Left, CubeY.Down, CubeZ.Back)),
            (GetIndex(CubeX.Left, CubeY.Center, CubeZ.Front),   GetIndex(CubeX.Left, CubeY.Down, CubeZ.Center)),
            (GetIndex(CubeX.Left, CubeY.Top, CubeZ.Front),      GetIndex(CubeX.Left, CubeY.Down, CubeZ.Front)),
            (GetIndex(CubeX.Left, CubeY.Top, CubeZ.Center),     GetIndex(CubeX.Left, CubeY.Center, CubeZ.Front)),
        ];

        Replace[CubeMove.Back] = [
            (GetIndex(CubeX.Right, CubeY.Top, CubeZ.Back),      GetIndex(CubeX.Left, CubeY.Top, CubeZ.Back)),
            (GetIndex(CubeX.Right, CubeY.Center, CubeZ.Back),   GetIndex(CubeX.Center, CubeY.Top, CubeZ.Back)),
            (GetIndex(CubeX.Right, CubeY.Down, CubeZ.Back),     GetIndex(CubeX.Right, CubeY.Top, CubeZ.Back)),
            (GetIndex(CubeX.Center, CubeY.Down, CubeZ.Back),    GetIndex(CubeX.Right, CubeY.Center, CubeZ.Back)),
            (GetIndex(CubeX.Left, CubeY.Down, CubeZ.Back),      GetIndex(CubeX.Right, CubeY.Down, CubeZ.Back)),
            (GetIndex(CubeX.Left, CubeY.Center, CubeZ.Back),    GetIndex(CubeX.Center, CubeY.Down, CubeZ.Back)),
            (GetIndex(CubeX.Left, CubeY.Top, CubeZ.Back),       GetIndex(CubeX.Left, CubeY.Down, CubeZ.Back)),
            (GetIndex(CubeX.Center, CubeY.Top, CubeZ.Back),     GetIndex(CubeX.Left, CubeY.Center, CubeZ.Back)),
        ];

        Replace[CubeMove.Down] = [
            (GetIndex(CubeX.Right, CubeY.Down, CubeZ.Back),     GetIndex(CubeX.Left, CubeY.Down, CubeZ.Back)),
            (GetIndex(CubeX.Right, CubeY.Down, CubeZ.Center),   GetIndex(CubeX.Center, CubeY.Down, CubeZ.Back)),
            (GetIndex(CubeX.Right, CubeY.Down, CubeZ.Front),    GetIndex(CubeX.Right, CubeY.Down, CubeZ.Back)),
            (GetIndex(CubeX.Center, CubeY.Down, CubeZ.Front),   GetIndex(CubeX.Right, CubeY.Down, CubeZ.Center)),
            (GetIndex(CubeX.Left, CubeY.Down, CubeZ.Front),     GetIndex(CubeX.Right, CubeY.Down, CubeZ.Front)),
            (GetIndex(CubeX.Left, CubeY.Down, CubeZ.Center),    GetIndex(CubeX.Center, CubeY.Down, CubeZ.Front)),
            (GetIndex(CubeX.Left, CubeY.Down, CubeZ.Back),      GetIndex(CubeX.Left, CubeY.Down, CubeZ.Front)),
            (GetIndex(CubeX.Center, CubeY.Down, CubeZ.Back),    GetIndex(CubeX.Left, CubeY.Down, CubeZ.Center)),
        ];
    }

    public Rubiks()
    {
        CubieStore = new Cube[27];


        Transformation = Matrix4x4.Identity;

        Color black = new(0, 0, 0);

        // slightly smaller than full 1x1x1 cube to avoid sides clipping into each other.
        Matrix4x4 scale = Matrix4x4.CreateScale(0.99f);

        foreach(CubeX x in Enum.GetValues(typeof(CubeX)))
            foreach (CubeY y in Enum.GetValues(typeof(CubeY)))
                foreach (CubeZ z in Enum.GetValues(typeof(CubeZ)))
                {
                    Color cLeft  = x == CubeX.Left  ? CubieFaceColors[(int)CubieFace.Left]  : black;
                    Color cRight = x == CubeX.Right ? CubieFaceColors[(int)CubieFace.Right] : black;

                    Color cFront = z == CubeZ.Front ? CubieFaceColors[(int)CubieFace.Front] : black;
                    Color cBack  = z == CubeZ.Back  ? CubieFaceColors[(int)CubieFace.Back]  : black;

                    Color cTop   = y == CubeY.Top   ? CubieFaceColors[(int)CubieFace.Top]   : black;
                    Color cDown  = y == CubeY.Down  ? CubieFaceColors[(int)CubieFace.Down]  : black;

                    var translate = Matrix4x4.CreateTranslation(
                            (float)x - 1f, (float)y - 1f, (float)z - 1f
                        );

                    CubieStore[GetIndex(x, y, z)] = new Cube(
                        [cFront, cRight, cBack, cLeft, cTop, cDown],
                        scale * translate
                        );
                }
    }


    private static int GetIndex(CubeX x, CubeY y, CubeZ z)
    {
        //ternary number system
        return ((int)x * 9) + ((int)y * 3) + (int)z;
    }

    /// <summary>The indicies into the <see cref="CubieStore"/> using a predicate based on the axis.</summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    private static IEnumerable<int> Where(Func<CubeX, CubeY, CubeZ, bool> predicate)
    {
        foreach (CubeX x in Enum.GetValues(typeof(CubeX)))
            foreach (CubeY y in Enum.GetValues(typeof(CubeY)))
                foreach (CubeZ z in Enum.GetValues(typeof(CubeZ)))
                    if (predicate(x, y, z))
                        yield return GetIndex(x, y, z);
    }

    /// <summary></summary>
    /// <param name="move"></param>
    /// <returns></returns>
    private static IEnumerable<int> MoveIndizes(CubeMove move)
    {
        int inv = (int)CubeMove.LeftInverted - (int)CubeMove.Left;

        if ((int)move >= inv)
            move -= inv;

        return move switch
        {
            CubeMove.Right => Where((x, y, z) => x == CubeX.Right),
            CubeMove.Left => Where((x, y, z) => x == CubeX.Left),
            CubeMove.Top => Where((x, y, z) => y == CubeY.Top),
            CubeMove.Down => Where((x, y, z) => y == CubeY.Down),
            CubeMove.Front => Where((x, y, z) => z == CubeZ.Front),
            CubeMove.Back => Where((x, y, z) => z == CubeZ.Back),
            _ => Enumerable.Empty<int>()
        };
    }

    public void MoveStructure(CubeMove move)
    {
        Cube[] storeClone = (Cube[])CubieStore.Clone();

        bool inverted = false;
        int invertedThreshold = (CubeMove.RightInverted - CubeMove.Right);
        if ((int)move >= invertedThreshold)
        {
            inverted = true;
            move -= invertedThreshold;
        }

        foreach(var tuple in Replace[move])
        {
            int from, to;
            if (inverted)
                (to, from) = tuple;
            else
                (from, to) = tuple;

            CubieStore[to] = storeClone[from];
        }
    }

    public void MoveTransformation(CubeMove move, float angle)
    {
        Matrix4x4 m = move switch
        {
            CubeMove.Front => Matrix4x4.CreateRotationZ(-angle),
            CubeMove.Right => Matrix4x4.CreateRotationX(-angle),
            CubeMove.Back => Matrix4x4.CreateRotationZ(angle),
            CubeMove.Left => Matrix4x4.CreateRotationX(angle),
            CubeMove.Top => Matrix4x4.CreateRotationY(-angle),
            CubeMove.Down => Matrix4x4.CreateRotationY(angle),
            CubeMove.FrontInverted => Matrix4x4.CreateRotationZ(angle),
            CubeMove.RightInverted => Matrix4x4.CreateRotationX(angle),
            CubeMove.BackInverted => Matrix4x4.CreateRotationZ(-angle),
            CubeMove.LeftInverted => Matrix4x4.CreateRotationX(-angle),
            CubeMove.TopInverted => Matrix4x4.CreateRotationY(angle),
            CubeMove.DownInverted => Matrix4x4.CreateRotationY(-angle),
            _ => Matrix4x4.Identity
        };

        foreach (int index in MoveIndizes(move))
            CubieStore[index].Transformation *= m;
    }


    public Matrix4x4 Transformation { get; set; }

    public IEnumerable<Triangle> GetTriangles() =>
        CubieStore
        .SelectMany(t => t.GetTriangles())
        .Select(t => t * Transformation);
}
