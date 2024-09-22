using RubiksCubeSfml;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Numerics;


//Console.WriteLine(Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2, 16f/9f, 1f, 10f).ToPrettyString());
//Console.WriteLine(Matrix4x4.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY).ToPrettyString());

PolygonList<Cube> polygons = new(8);
Rubiks rubik = new();

Texture texture = new Texture("FaceTexture.png");

float radsToDo = 0;
CubeMove? move = null;

var camera = new Camera(
    //new Vector3(-3f, 3f, 4f),
    new Vector3(5f, 0f, 0f),
    new Vector3(0),
    new Vector3(0f, 1f, 0f),
    MathF.PI * 2f/4f            // 90°
    );

Triangle3f tri = new Triangle3f(
    new Vector3f(-1f, 0f, 0f),
    new Vector3f(1f, 0f, 0f),
    new Vector3f(0f, 1.5f, 0f),
    Color.White
    );

var window = new PolygonWindow(camera, rubik);
window.Window.KeyPressed += Window_KeyPressed;
window.ShowAxis = true;

void Window_KeyPressed(object? sender, KeyEventArgs e)
{
    if (move is not null)
        return;

    move = e.Code switch
    {
        Keyboard.Key.R => CubeMove.Right,
        Keyboard.Key.L => CubeMove.Left,
        Keyboard.Key.T => CubeMove.Top,
        Keyboard.Key.D => CubeMove.Down,
        Keyboard.Key.F => CubeMove.Front,
        Keyboard.Key.B => CubeMove.Back,
        _ => null
    };

    if (move is null)
        return;

    radsToDo = MathF.PI / 2f;
    if(e.Shift)
        move = (CubeMove)(move.Value + (CubeMove.RightInverted - CubeMove.Right));

}

//Color[] colors =
//[
//    new Color(0,0,255),
//    new Color(255, 106, 0),
//    new Color(0, 200, 0),
//    new Color(255, 0, 0),
//    new Color(255, 255, 255),
//    new Color(242, 242, 0)
//];


//Matrix4x4[] animation = new Matrix4x4[8];
//Random random = new(42);


//for (int i = 0; i <  8; i++)
//{
//    float move = 40f;
//    int x = (i & 1) == 0 ? 1 : -1;
//    int y = (i & 2) == 0 ? 1 : -1;
//    int z = (i & 4) == 0 ? 1 : -1;


//    polygons.Add(new Cube(colors,
//        Matrix4x4.CreateScale(25f) *
//        Matrix4x4.CreateTranslation(move * x, move * y, move * z)));

//    animation[i] =
//        Matrix4x4.CreateRotationX((float)(random.NextDouble() * 0.04 - 0.02)) *
//        Matrix4x4.CreateRotationY((float)(random.NextDouble() * 0.04 - 0.02)) *
//        Matrix4x4.CreateRotationZ((float)(random.NextDouble() * 0.04 - 0.02));
//}

const float speed = MathF.PI / (2f * 30f);

window.Run(tf => new RenderStates(BlendMode.Alpha, tf, texture, null), null, () =>
{
    if(radsToDo > 0 && move is not null)
    {
        float r = radsToDo;
        radsToDo = float.Max(radsToDo - speed, 0f);
        rubik.MoveTransformation(move.Value, r - radsToDo);

        if(float.Abs(radsToDo) < 0.0001f)
        {
            radsToDo = 0;
            rubik.MoveStructure(move.Value);
            move = null;
        }
    }
});