using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct3D9;
using Color = Microsoft.Xna.Framework.Color;
using Effect = SharpDX.Direct3D9.Effect;
using Font = SharpDX.Direct3D9.Font;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Texture = SharpDX.Direct3D9.Texture;
using VertexBuffer = Microsoft.Xna.Framework.Graphics.VertexBuffer;
using VertexDeclaration = Microsoft.Xna.Framework.Graphics.VertexDeclaration;
using VertexElement = Microsoft.Xna.Framework.Graphics.VertexElement;

namespace _3DBoxPackingMonoGame;

public struct BoxData
{
    public Vector3 Position;
    public float Width;
    public float Height;
    public float Depth;
    public Color Color;

    public BoxData(Vector3 position, float width, float height, float depth, Color color)
    {
        this.Position = position;
        this.Width = width;
        this.Height = height;
        this.Depth = depth;
        this.Color = color;
    }
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;

    private SpriteBatch _spriteBatch;
    private Font boxFont;

    private BasicEffect basicEffect;

// We only need a single
    private bool mblnCtrlPressed = false;
    private Model mobjBox;
    private string mstrGFX = Guid.NewGuid().ToString();
    private List<BoxData> mobjBoxes = new List<BoxData>();
    private BoxData mobjContainer = new BoxData(Vector3.Zero, 25, 25, 25, Color.White);
    private int mintBoxesDrawn = 1;
    private Point mintLastMousePos;
    private Vector3 mintRotation;
    private float ONEDEGREE = (float)(Math.PI / 180f);
    private Matrix WorldMatrix;

    private bool mblnShowHelp = false;

//private mobjPP As Direct3D.PresentParameters
//private WithEvents mobjDevice As Direct3D.Device
    private VertexBuffer VB;
    private VertexBuffer AxisVB;
    private float msngZoom = 1;
    private List<Texture2D> mobjTextures = new List<Texture2D>();
    private int mintTiling = 1;
    private KeyboardState previousKB;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var t in this.mobjTextures)
        {
            t.Dispose();
        }

        mobjTextures.Clear();
        mobjTextures = null;

        mobjBoxes.Clear();
        mobjBoxes = null;
        //mobjBox.Dispose();
        mobjBox = null;

        VB.Dispose();
        VB = null;
        //mobjDevice.Dispose()
        //mobjDevice = Nothing
        //mobjPP = Nothing

        base.Dispose(disposing);
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    public struct PositionOnly : IVertexType
    {
        public Vector3 Position;

        public PositionOnly(Vector3 position)
        {
            Position = position;
        }

        public PositionOnly(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }
    }

    // new vertex struct containing position and color
    public struct PositionColored : IVertexType
    {
        public Vector3 Position;
        public uint Color;

        public PositionColored(Vector3 position, uint color)
        {
            Position = position;
            Color = color;
        }

        public PositionColored(float x, float y, float z, uint color)
        {
            Position = new Vector3(x, y, z);
            Color = color;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }
    }

    private bool LoadBoxData()
    {
        mobjBoxes.Clear();
        msngZoom = 1;


        var dataFilePath = Path.Combine("Content", "BoxData.txt");
        if (!File.Exists(dataFilePath)) return false;

        var Lines = File.ReadAllLines(dataFilePath);
        //Dim Lines As String()
        //Lines = Data.Split(vbCr)


        //  with mobjContainer
        var Parts = Lines[0].Trim().Split(",");
        mobjContainer.Width = Convert.ToSingle(Parts[0].Trim()); // CSng(Parts(0).Trim)
        mobjContainer.Height = Convert.ToSingle(Parts[1].Trim());
        mobjContainer.Depth = Convert.ToSingle(Parts[2].Trim());
        //mobjContainer .Position = new Vector3 '(.Width / -2, .Height / -2, .Depth / -2)
        //  End With

        var random = new Random((int)DateTime.Now.Ticks);
        for (var idx = 1; idx < Lines.Length - 1; idx++)
        {
            var L = Lines[idx].Trim();
            if (!string.IsNullOrWhiteSpace(L))
            {
                //  Dim Parts()
                //  As String
                Parts = L.Trim().Split(",");
                BoxData B;
                B.Position.X = Convert.ToSingle(Parts[0].Trim());
                B.Position.Y = Convert.ToSingle(Parts[1].Trim());
                B.Position.Z = Convert.ToSingle(Parts[2].Trim());
                B.Width = Convert.ToSingle(Parts[3].Trim());
                B.Height = Convert.ToSingle(Parts[4].Trim());
                B.Depth = Convert.ToSingle(Parts[5].Trim());

                //C As Color
                //Randomize(Now.Ticks)
                //var C = 
                B.Color = new Color((int)(191 * random.NextSingle() + 64),
                                    (int)(191 * random.NextSingle() + 64),
                                    (int)(191 * random.NextSingle() + 64));
                mobjBoxes.Add(B);
            }
        }

        return true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        //      Me.Show()
        //  mobjPP = New Direct3D.PresentParameters
        // With mobjPP
        //     .AutoDepthStencilFormat = Direct3D.DepthFormat.D16
        //     .EnableAutoDepthStencil = True
        //     .Windowed = True
        //     .SwapEffect = Direct3D.SwapEffect.Discard
        // End With
        // mobjDevice = New Direct3D.Device(0, Direct3D.DeviceType.Hardware, Me, Direct3D.CreateFlags.SoftwareVertexProcessing, mobjPP)
        //
        // mobjDevice.VertexFormat = PositionTextured.Format
//        mobjBox =  Content.Load<Mesh>() Mesh .FromStream(LoadResource("Box.X"), Direct3D.MeshFlags.Managed, mobjDevice)
        mobjBox = Content.Load<Model>("Box");
        //mobjBox = Direct3D.Mesh.Box(mobjDevice, 1, 1, 1)
        //mobjBox = mobjBox.Clone(Direct3D.MeshFlags.Managed, Positiontextured.Format, mobjDevice)
        //
        if (!LoadBoxData())
        {
            MessageBox.Show("Error", "Could not load box data!", new[] { "Close" });
            this.Exit();
            return;
        }


        //mobjDevice.RenderState.Lighting = True
        //mobjDevice.RenderState.Ambient = Color.White

        var Verts = new[]
        {
            new PositionOnly(0, 0, 0), new PositionOnly(1, 0, 0),
            new PositionOnly(1, 0, 0), new PositionOnly(1, 1, 0),
            new PositionOnly(1, 1, 0), new PositionOnly(0, 1, 0),
            new PositionOnly(0, 1, 0), new PositionOnly(0, 0, 0),

            new PositionOnly(0, 0, 1), new PositionOnly(1, 0, 1),
            new PositionOnly(1, 0, 1), new PositionOnly(1, 1, 1),
            new PositionOnly(1, 1, 1), new PositionOnly(0, 1, 1),
            new PositionOnly(0, 1, 1), new PositionOnly(0, 0, 1),

            new PositionOnly(0, 0, 0), new PositionOnly(0, 0, 1),
            new PositionOnly(1, 0, 0), new PositionOnly(1, 0, 1),
            new PositionOnly(1, 1, 0), new PositionOnly(1, 1, 1),
            new PositionOnly(0, 1, 0), new PositionOnly(0, 1, 1)
        };

        this.VB = new VertexBuffer(this.GraphicsDevice, typeof(PositionOnly), Verts.Length, BufferUsage.WriteOnly);
        this.VB.SetData(Verts);

        // Me.VB = CreateVertexBuffer(Verts(0).GetType, Verts, Direct3D.Usage.None, Direct3D.Pool.Managed, Direct3D.PrimitiveType.LineList)

        var Axis = new[]
        {
            new PositionColored(0, 0, 0, Color.Red.PackedValue), new PositionColored(25, 0, 0, Color.Red.PackedValue),
            new PositionColored(25, 0, 0, Color.Red.PackedValue), new PositionColored(20, -5, 0, Color.Red.PackedValue),
            new PositionColored(25, 0, 0, Color.Red.PackedValue), new PositionColored(20, 5, 0, Color.Red.PackedValue),
            new PositionColored(25, 0, 0, Color.Red.PackedValue), new PositionColored(20, 0, -5, Color.Red.PackedValue),
            new PositionColored(25, 0, 0, Color.Red.PackedValue), new PositionColored(20, 0, 5, Color.Red.PackedValue),

            new PositionColored(0, 0, 0, Color.Green.PackedValue), new PositionColored(0, -25, 0, Color.Green.PackedValue),
            new PositionColored(0, -25, 0, Color.Green.PackedValue), new PositionColored(-5, -20, 0, Color.Green.PackedValue),
            new PositionColored(0, -25, 0, Color.Green.PackedValue), new PositionColored(5, -20, 0, Color.Green.PackedValue),
            new PositionColored(0, -25, 0, Color.Green.PackedValue), new PositionColored(0, -20, -5, Color.Green.PackedValue),
            new PositionColored(0, -25, 0, Color.Green.PackedValue), new PositionColored(0, -20, 5, Color.Green.PackedValue),

            new PositionColored(0, 0, 0, Color.Blue.PackedValue), new PositionColored(0, 0, 25, Color.Blue.PackedValue),
            new PositionColored(0, 0, 25, Color.Blue.PackedValue), new PositionColored(-5, 0, 20, Color.Blue.PackedValue),
            new PositionColored(0, 0, 25, Color.Blue.PackedValue), new PositionColored(5, 0, 20, Color.Blue.PackedValue),
            new PositionColored(0, 0, 25, Color.Blue.PackedValue), new PositionColored(0, -5, 20, Color.Blue.PackedValue),
            new PositionColored(0, 0, 25, Color.Blue.PackedValue), new PositionColored(0, 5, 20, Color.Blue.PackedValue)
        };

        this.AxisVB = new VertexBuffer(this.GraphicsDevice, typeof(PositionColored), Axis.Length, BufferUsage.WriteOnly);
        this.AxisVB.SetData(Verts);
        //  Me.AxisVB = CreateVertexBuffer(Axis(0).GetType, Axis, Direct3D.Usage.None, Direct3D.Pool.Managed, Direct3D.PrimitiveType.LineList)


        CreateTextures();

        this.basicEffect = new BasicEffect(GraphicsDevice);


        // '  CreateFont(Me.Font.Name, Me.Font, True)
        // Timer1.Start()
    }

    private void CreateTextures()
    {
        //  Bitmap Bmp;

        // var B = new SolidBrush(System.Drawing.Color.Black);
// create a Arial Font  with a size of 36 and bold

//Font Fnt  =new SharpDX.Direct3D9.Font (GraphicsDevice, new FontDescription( "Arial", 36, FontStyle.Bold )
        //      SizeF S;
        //    float X, Y;

        var fnt = new System.Drawing.Font("Arial", 36, FontStyle.Bold);

        for (var idx = 0; idx < mobjBoxes.Count; idx++)
        {
            // Bmp = new Bitmap(64, 64, PixelFormat.Format24bppRgb)
            // var G = Graphics.FromImage(Bmp)
            // G.Clear(System.Drawing.Color.White)
            //
            // S = G.MeasureString(idx.ToString(), Fnt)
            // X = 32 - (S.Width / 2)
            //
            // Y = 32 - (S.Height / 2)
            // G.DrawString(idx.ToString(), Fnt, B, X, Y)

            var tex = this.CreateFontTexture(this.GraphicsDevice, idx.ToString(), fnt, System.Drawing.Color.Black, System.Drawing.Color.White);

            //var Tex = Direct3D.Texture.FromBitmap(mobjDevice, Bmp, Direct3D.Usage.None, Direct3D.Pool.Managed)
            mobjTextures.Add(tex);

//            G.Dispose()
        }

        fnt.Dispose();
    }

    private Texture2D CreateFontTexture(GraphicsDevice graphicsDevice, string text, System.Drawing.Font font, System.Drawing.Color fontColor,
                                        System.Drawing.Color backgroundColor)
    {
        // Create a new bitmap to draw the text
        Bitmap bmp = new Bitmap(1, 1);
        Graphics g = Graphics.FromImage(bmp);
        // Measure the text size
        SizeF textSize = g.MeasureString(text, font);
        // Resize the bitmap
        bmp.Dispose();
        bmp = new Bitmap((int)textSize.Width, (int)textSize.Height);
        g.Dispose();
        g = Graphics.FromImage(bmp);
        g.Clear(backgroundColor);

        // Draw the string to the bitmap
        g.DrawString(text, font, new SolidBrush(fontColor), 0, 0);
        g.Flush();

        // Lock the bitmap data and get the byte array 
        BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
        int numBytes = bmpData.Stride * bmp.Height;
        byte[] byteData = new byte[numBytes];
        Marshal.Copy(bmpData.Scan0, byteData, 0, numBytes);
        bmp.UnlockBits(bmpData);

        // Create the texture 
        Texture2D fontTexture2D = new Texture2D(graphicsDevice, bmp.Width, bmp.Height, false, SurfaceFormat.Color);
        fontTexture2D.SetData(byteData);

        return fontTexture2D;
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();


        this.mblnCtrlPressed = keyboard.IsKeyDown(Keys.LeftControl) | keyboard.IsKeyDown(Keys.RightControl);

        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();


        if (mouse.LeftButton == ButtonState.Pressed)
        {
            mintRotation.Y += mouse.X - mintLastMousePos.X;
            if (mintRotation.Y > 360) mintRotation.Y -= 360;
            if (mintRotation.Y < 0) mintRotation.Y += 360;
            mintLastMousePos = new Point(mouse.X, mouse.Y);
        }

        if (mouse.MiddleButton == ButtonState.Pressed)
        {
            mintRotation.Z += mouse.X - mintLastMousePos.X;
            if (mintRotation.Z > 360) mintRotation.Z -= 360;
            if (mintRotation.Z < 0) mintRotation.Z += 360;
            mintLastMousePos = new Point(mouse.X, mouse.Y);
        }

        if (mouse.RightButton == ButtonState.Pressed)
        {
            if (mblnCtrlPressed)
            {
                if (mouse.X < 0.5)
                {
                    msngZoom = 0.5f;
                }
                else if (mouse.X > this.Window.ClientBounds.Width - 1)
                {
                    msngZoom = 3;
                }

                else
                {
                    msngZoom = (mouse.X / this.Window.ClientBounds.Width) * 3;
                }

                if (msngZoom < 0.5) msngZoom = 0.5f;
            }
            else
            {
                mintRotation.X += mouse.X - mintLastMousePos.X;
                if (mintRotation.X > 360) mintRotation.X -= 360;
                if (mintRotation.X < 0) mintRotation.X += 360;
                mintLastMousePos = new Point(mouse.X, mouse.Y);
            }
        }


        //mblnCtrlPressed = e.Control
        if (this.KeyWasReleased(keyboard, Keys.Left))
        {
            mintBoxesDrawn -= 1;
            if (mintBoxesDrawn < 0) mintBoxesDrawn = 0;
        }

        if (this.KeyWasReleased(keyboard, Keys.Right))
        {
            mintBoxesDrawn += 1;
            if (mintBoxesDrawn > mobjBoxes.Count - 1) mintBoxesDrawn = mobjBoxes.Count - 1;
        }

        if (this.KeyWasReleased(keyboard, Keys.Space))
        {
            this.mblnShowHelp = !this.mblnShowHelp;
        }

        if (this.KeyWasReleased(keyboard, Keys.Up))
        {
            mintTiling += 1;
            if (mintTiling > 100) mintTiling = 100;
        }

        if (this.KeyWasReleased(keyboard, Keys.Down))
        {
            mintTiling -= 1;
            if (mintTiling < 1) mintTiling = 1;
        }


        // Select Case e.KeyCode
        //     Case Keys.Left
        // mintBoxesDrawn -= 1
        // If mintBoxesDrawn < 0 Then mintBoxesDrawn = 0
        //
        // Case Keys.Right
        // mintBoxesDrawn += 1
        // If mintBoxesDrawn > mobjBoxes.Count - 1 Then mintBoxesDrawn = mobjBoxes.Count - 1

        // Case Keys.Escape
        // Me.Close()
        //
        // Case Keys.Space
        //     mblnShowHelp = Not mblnShowHelp

        //     Case Keys.Up
        // mintTiling += 1
        // If mintTiling > 100 Then mintTiling = 100
        //
        // Case Keys.Down
        // mintTiling -= 1
        // If mintTiling < 1 Then mintTiling = 1
        // End Select


        // TODO: Add your update logic here
        // select Case e.Button
        //     Case Windows.Forms.MouseButtons.Left
        // mintRotation.Y += e.X - mintLastMousePos.X
        // If mintRotation.Y > 360 Then mintRotation.Y -= 360
        // If mintRotation.Y < 0 Then mintRotation.Y += 360
        // mintLastMousePos = New Point(e.X, e.Y)

        // Case MouseButtons.Middle
        // mintRotation.Z += e.X - mintLastMousePos.X
        // If mintRotation.Z > 360 Then mintRotation.Z -= 360
        // If mintRotation.Z < 0 Then mintRotation.Z += 360
        // mintLastMousePos = New Point(e.X, e.Y)

        // Case MouseButtons.Right
        //     If mblnCtrlPressed Then
        // If e.X < 0.5 Then
        //     msngZoom = 0.5
        // ElseIf e.X > Me.ClientSize.Width - 1 Then
        //     msngZoom = 3
        // Else
        //     msngZoom = (e.X / Me.ClientSize.Width) * 3
        // End If
        //
        // If msngZoom < 0.5 Then msngZoom = 0.5
        //
        // Else
        // mintRotation.X += e.X - mintLastMousePos.X
        // If mintRotation.X > 359 Then mintRotation.X -= 360
        // If mintRotation.X < 0 Then mintRotation.X += 360
        // mintLastMousePos = New Point(e.X, e.Y)
        //
        //
        // End If
        // End Select


        this.previousKB = keyboard;
        base.Update(gameTime);
    }

    private bool KeyWasReleased(KeyboardState keyboard, Keys key)
    {
        return keyboard.IsKeyUp(key) && this.previousKB.IsKeyDown(key);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        // we begin rendering our scene
        //mobjDevice.Clear(Direct3D.ClearFlags.Target Or Direct3D.ClearFlags.ZBuffer, Color.Black, 1, 0)
        //mobjDevice.BeginScene()
        //' we rotate our world according to what the user has rotated it to


        // set world matrix variable to identity

        WorldMatrix = Matrix.Identity;

        this.WorldMatrix = Matrix.CreateTranslation(-mobjContainer.Width / 2, -mobjContainer.Height / 2, -mobjContainer.Depth / 2) *
                           Matrix.CreateFromYawPitchRoll(ONEDEGREE * mintRotation.X, ONEDEGREE * mintRotation.Y, ONEDEGREE * mintRotation.Z);

        //'     WorldMatrix.Multiply(Matrix.Scaling(msngZoom, msngZoom, msngZoom))
        //WorldMatrix.Multiply(Matrix.CreateTranslation(-mobjContainer.Width / 2, -mobjContainer.Height / 2, -mobjContainer.Depth / 2));
        //WorldMatrix.Multiply(Matrix.CreateFromYawPitchRoll(ONEDEGREE * mintRotation.X, ONEDEGREE * mintRotation.Y, ONEDEGREE * mintRotation.Z));

        float Avg = (mobjContainer.Width + mobjContainer.Height + mobjContainer.Depth) / 3f;
        //' Avg = Avg * msngZoom
        var Pos = new Vector3(Avg, 0, -Avg);


        this.basicEffect.View = Matrix.CreateLookAt(Vector3.Lerp(Vector3.Zero, Pos, msngZoom), Vector3.Zero, new Vector3(0, 0, 1));

        //mobjDevice.Transform.View = Matrix.LookAtLH(Vector3.Lerp(New Vector3, Pos, msngZoom), New Vector3(0, 0, 0), New Vector3(0, 0, 1))
        float Aspect = this.GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;
        this.basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2f), Aspect, 1, 10000);

        //mobjDevice.Transform.Projection = Matrix.PerspectiveFovLH(Math.PI / 2, Aspect, 1, 10000)

        //' loop thru all the visible boxes
        for (var idx = 0; idx < mintBoxesDrawn; idx++)
        {
            if (idx > mobjBoxes.Count - 1) break;

            var B = this.mobjBoxes[idx];

            //' draw boxes
            //Dim M As Matrix
            //' setup a scale and translate the box into position then multiply it by the world matrix
            var M = Matrix.Identity;

            M = Matrix.CreateScale(B.Width, B.Height, B.Depth) *
                Matrix.CreateTranslation(B.Position) *
                WorldMatrix;

            // M.Multiply( Microsoft.Xna.Framework.Matrix.CreateScale(B.Width, B.Height, B.Depth));
            // M.Multiply(Matrix.CreateTranslation(B.Position));
            // M.Multiply(WorldMatrix);
            //

            //' set the world matrix
            this.basicEffect.World = M;

            //' create and apply a material using the box color as a reference
            //ApplyMaterial(B.Color)
            this.basicEffect.AmbientLightColor = B.Color.ToVector3();
            this.basicEffect.DiffuseColor = B.Color.ToVector3();

            this.basicEffect.TextureEnabled = true;
            this.basicEffect.Texture = this.mobjTextures[idx];
            // foreach (var pass in this.basicEffect.CurrentTechnique.Passes)
            // {
            //     pass.Apply();

            foreach (var mesh in this.mobjBox.Meshes)
            {
                var effect = mesh.Effects.Cast<BasicEffect>().First();
                effect.TextureEnabled = true;
                effect.Texture = this.mobjTextures[idx];
                effect.DiffuseColor = B.Color.ToVector3();
            }


            this.mobjBox.Draw(M, this.basicEffect.View, this.basicEffect.Projection);

            //graphicsDevice.DrawUserPrimitives(...);
            // or
            // graphicsDevice.DrawIndexedPrimitives(...);
            // }

            //' set texture to use
            //  mobjDevice.SetTexture(0, mobjTextures(idx))
            //  M = Matrix.Identity
            //  M.Multiply(Matrix.Scaling(mintTiling, mintTiling, 1))
            //  mobjDevice.Transform.Texture0 = M
            //  mobjDevice.TextureState(0).TextureTransform = Direct3D.TextureTransform.Count2

            //' finally draw the box on csreen
            //   mobjBox.DrawSubset(0)
            //' draw lines around box to show it's boundary better
            DrawLineBox(B, Color.White);
        }

        /*
        ' draw the container box
        DrawLineBox(mobjContainer)
        DrawAxis()
        mobjDevice.EndScene()
        mobjDevice.Present()

        ' show info
        Dim Msg As String
            Msg = "Box Count: " & mobjBoxes.Count.ToString & vbCrLf
        Msg &= "Boxes Shown: " & (mintBoxesDrawn + 1).ToString & vbCrLf
        Msg &= "Rotation: " & mintRotation.ToString & vbCrLf
        Msg &= "Zoom: " & msngZoom.ToString & vbCrLf
        If mblnShowHelp Then
        Msg &= "Left/Right Arrow Keys: Decrease/Increase number of boxes drawn" & vbCrLf
        Msg &= "Up/Down Arrow Keys: Decreate/Increase how many numbers are stammped on the box" & vbCrLf
        Msg &= "Drag Mouse Buttons L/R/M: To rotate the scene" & vbCrLf
        Msg &= "Hold control and drag Right mouse button: To Zoom in/out on the scene" & vbCrLf
        Msg &= "To Hide Help Press spacebar"
        Else
        Msg &= "To Show Help Press spacebar"
        End If

        '  DrawText(Msg, 10, 10, Color.Red)

        With Me.CreateGraphics
               .DrawString(Msg, Me.Font, New SolidBrush(Color.Red), 10, 10)
               .DrawString("X Axis", Me.Font, New SolidBrush(Color.Red), 10, Me.ClientSize.Height - 20)
               .DrawString("Y Axis", Me.Font, New SolidBrush(Color.Green), 10, Me.ClientSize.Height - 45)
               .DrawString("Z Axis", Me.Font, New SolidBrush(Color.Blue), 10, Me.ClientSize.Height - 70)
        End With
          */

        base.Draw(gameTime);
    }

    private void DrawLineBox(BoxData B, Color Color)
    {
        //'BeginLineDrawing()

        /*
        var MX = Matrix.Identity
        MX.Multiply(Matrix.Scaling(B.Width, B.Height, B.Depth))
        MX.Multiply(Matrix.Translation(B.Position))
        'MX.Multiply(Matrix.Translation(B.Position.X / 2, B.Position.Y / 2, B.Position.Z / 2))
        MX.Multiply(WorldMatrix)

        mobjDevice.Transform.World = MX
        ApplyMaterial(Color)
        ' DrawVertexBuffer(Direct3D.PrimitiveType.LineList)
        'ReturnD3DLine.DrawTransform(Lines, MX, B.Color)
        'EndLineDrawing()
        Dim Count As Integer
            Count = GetPrimitiveCount(Direct3D.PrimitiveType.LineList, VB.Description.Size \ PositionOnly.StrideSize)
        With mobjDevice
            .VertexFormat = VB.Description.VertexFormat
                              .SetStreamSource(0, VB, 0)
                              .DrawPrimitives(Direct3D.PrimitiveType.LineList, 0, Count)
        End With
            */
    }

    private void DrawLineBox(BoxData B)
    {
        DrawLineBox(B, B.Color);
    }
}