
using RubiksCubeSfml;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Numerics;


var settings = new ContextSettings();
settings.AntialiasingLevel = 16;

var window = new RenderWindow(new VideoMode(800, 600), "Rubik's Cube", Styles.Default, settings);
window.Closed += (_, _) => window.Close();
window.SetFramerateLimit(60);

Transform transform = Transform.Identity;
transform.Translate(400, 300);
transform.Scale(10, -10);


Texture texture = new Texture("FaceTexture.png");

Color[] colors =
[
    new Color(0,0,255),
    new Color(255, 106, 0),
    new Color(0, 200, 0),
    new Color(255, 0, 0),
    new Color(255, 255, 255),
    new Color(242, 242, 0)
];


PolygonList<Cube> polygons = new(8);
Matrix4x4[] animation = new Matrix4x4[8];
Random random = new(42);

RenderStates rs = new RenderStates(BlendMode.Alpha, transform, texture, null);

for (int i = 0; i <  8; i++)
{
    float move = 8f;
    int x = (i & 1) == 0 ? 1 : -1;
    int y = (i & 2) == 0 ? 1 : -1;
    int z = (i & 4) == 0 ? 1 : -1;

    var position = new Vector3f(move * x, move * y, move * z);
    var scale = new Vector3f(2.5f, 2.5f, 2.5f);

    polygons.Add(new Cube(position, scale, colors));
    animation[i] =
        Matrix4x4.CreateRotationX((float)(random.NextDouble() * 0.04 - 0.02)) *
        Matrix4x4.CreateRotationY((float)(random.NextDouble() * 0.04 - 0.02)) *
        Matrix4x4.CreateRotationZ((float)(random.NextDouble() * 0.04 - 0.02));

}



while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Cyan);



    window.Draw(polygons, rs);

    for (int i = 0; i < polygons.Count; i++)
    {
        polygons[i].Transform(animation[i]);
    }

    window.Display();
}