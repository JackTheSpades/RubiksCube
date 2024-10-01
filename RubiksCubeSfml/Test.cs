using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubiksCubeSfml;

public static class Test
{
    public static void FloatSorts()
    {
        // Arrange random float array
        float[] floats = Enumerable.Range(0, 100).Select(i => (float)i / 100f).ToArray();
        Random.Shared.Shuffle(floats);

        // Print for verification
        for (int i = 0; i <  floats.Length; i++)
        {
            Console.WriteLine(floats[i].ToString("0.000"));
        }

        // Get fragment shader stream
        using var ms = new MemoryStream();
        ms.Write(Encoding.UTF8.GetBytes($"const float length = {floats.Length}f;\r\nuniform float[{floats.Length}] values;\r\n"));
        using (var fs = File.OpenRead("sort.frag"))
            fs.CopyTo(ms);
        ms.Position = 0;

        // initialize shader
        Shader sh = new Shader(null, null, ms);
        sh.SetUniformArray("values", floats);

        // Draw image
        RenderTexture rt = new RenderTexture((uint)floats.Length, (uint)floats.Length);
        var fill = new RectangleShape(new SFML.System.Vector2f((float)floats.Length, (float)floats.Length));
        var rs = new RenderStates(sh);
        rt.Draw(fill, rs);
        rt.Display();

        // Extrakt image and debug save
        var img = rt.Texture.CopyToImage();
        img.SaveToFile("img.png");


        Console.WriteLine("--------------------------------");

        // actual sorting

        // index-array to keep track of swapped entries
        // swapping entries would require also swapping rows/columns in the image, instead just swap index array
        int[] indezies = Enumerable.Range(0, floats.Length).ToArray();
        
        
        for (int x = 0; x < floats.Length; x++)
            for (int y = x + 1; y < floats.Length; y++)
            {
                if (img.GetPixel((uint)indezies[x], (uint)indezies[y]).R == 255)
                {
                    (floats[x], floats[y]) = (floats[y], floats[x]);
                    (indezies[x], indezies[y]) = (indezies[y], indezies[x]);
                }
            }

        // Print sorted array
        for (int i = 0; i <  floats.Length; i++)
        {
            Console.Write(floats[i].ToString("0.000"));
            if (i == 0)
                Console.WriteLine();
            else if (floats[i] >= floats[i - 1])
                Console.WriteLine(" Corrrect");
            else
                Console.WriteLine(" Wrong");
        }
    }
}
