
using RubiksCubeSfml;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Numerics;


var window = new RenderWindow(new VideoMode(800, 600), "Rubik's Cube");
window.Closed += (_, _) => window.Close();
window.SetFramerateLimit(60);

// feels backwards but results in the correct matrix
Transform transform = Transform.Identity;
transform.Translate(400, 300);
transform.Scale(10, -10);

//Matrix4x4 matrix = Matrix4x4.CreateRotationY((float)Math.Tau / 8f);

Texture texture = new Texture("FaceTexture.png");

Color[] colors = new Color[]
{
    new Color(0,0,255),
    new Color(255, 106, 0),
    new Color(0, 200, 0),
    new Color(255, 0, 0),
    new Color(255, 255, 255),
    new Color(242, 242, 0)
};


Cube[] cubies = new Cube[8];
Matrix4x4[] animation = new Matrix4x4[8];
Random random = new Random(42);

for(int i = 0; i <  cubies.Length; i++)
{
    float move = 8f;
    int x = (i & 1) == 0 ? 1 : -1;
    int y = (i & 2) == 0 ? 1 : -1;
    int z = (i & 4) == 0 ? -1 : 1;

    var translation = new Vector3f(move * x, move * y, move * z);
    var scale = new Vector3f(2.5f, 2.5f, 2.5f);

    cubies[i] = new Cube(translation, scale, colors);
    animation[i] = //Matrix4x4.Identity;
        Matrix4x4.CreateRotationX((float)(random.NextDouble() * 0.04 - 0.02)) *
        Matrix4x4.CreateRotationY((float)(random.NextDouble() * 0.04 - 0.02)) *
        Matrix4x4.CreateRotationZ((float)(random.NextDouble() * 0.04 - 0.02));

}



//c.Transform(matrix);

while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear(Color.Cyan);


    RenderStates rs = new RenderStates(BlendMode.Alpha, transform, texture, null);

    for (int i = 0; i < cubies.Length; i++)
    {
        window.Draw(cubies[i], rs);
        cubies[i].Transform(animation[i]);
    }

    window.Display();
}