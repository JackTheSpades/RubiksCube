using RubiksCubeSfml;
using SFML.Graphics;
using SFML.Graphics.Glsl;
using SFML.Window;
using System.Numerics;
using System.Text;



//var obj = new ObjModel("CubeRounded226.obj");


//string modelFilename;
//modelFilename = "x-wing";
//modelFilename = "vader";

//var modelTexture = new Texture(Path.ChangeExtension(modelFilename, "png"));
//var modelObject = new ObjModel(Path.ChangeExtension(modelFilename, "obj"), (x,y) => new Vector2(x * modelTexture.Size.X, (1 - y) * modelTexture.Size.Y));


Rubiks rubik = new();


var cubeModel = new CubeModel(Rubiks.CubieFaceColors);

//Texture texture = new Texture("FaceTexture.png");


var camera = new Camera(
    new Vector3(-3f, 3f, 4f),   // Position
    Vector3.Zero,               // Look-At
    Vector3.UnitY,              // Up
    MathF.PI * 2f/4f            // FOV: 90°
    );


float radsToDo = 0;
CubeMove? move = null;


var window = new PolygonWindow(camera, "Rubik's Cube");
window.Models.Add(cubeModel);
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


const float speed = MathF.PI / (2f * 30f);


window.Run(tf => new RenderStates(BlendMode.Alpha, tf, null, null), null, () =>
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