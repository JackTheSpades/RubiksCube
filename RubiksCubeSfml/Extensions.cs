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

    public static bool IsNan(this Vector3 v) => float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);

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

}
