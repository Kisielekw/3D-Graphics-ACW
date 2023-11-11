using OpenTK;
using OpenTK.Graphics;
using System;
using System.Drawing.Text;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;
using OpenTK.Input;

namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        public ACWWindow()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private ShaderUtility mShader;

        private ModelUtility mCylinderModel, mArmadiloModel;

        private bool[] mKeyPressed = new bool[255];

        private int[] mVAO_IDs = new int[9];
        private int[] mVBO_IDs = new int[13];

        private Matrix4 mView, mRoom, mCylinderM, mCylinderL, mCylinderR, mArmadilo;

        protected override void OnLoad(EventArgs e)
        {
 	        base.OnLoad(e);

            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.CullFace);

            //load models
            mCylinderModel = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");
            mArmadiloModel = ModelUtility.LoadModel(@"Utility/Models/model.bin");

            //Load Shader
            mShader = new ShaderUtility(@"ACW/Shaders/vertexShader.vert", @"ACW/Shaders/fragmentShader.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");

            //Generate VAO and VBO
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            #region Floor
            float[] floorVertexs = new float[] 
            {
                -10, 0, -10, 0, 1, 0,
                -10, 0, 10, 0, 1, 0,
                10, 0, 10, 0, 1, 0,
                10, 0, -10, 0, 1, 0
            };

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(floorVertexs.Length * sizeof(float)), floorVertexs, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            #region Walls
            float[] wallOneVertexs = new float[]
            {
                -10, 0, -10, 0, 0, 1f,
                -10, 10, -10, 0, 0, 1f,
                10, 10, -10, 0, 0, 1f,
                10, 0, -10, 0, 0, 1f
            };

            float[] wallTwoVertexs = new float[]
            {
                10, 0, 10, 0, 0, 1,
                -10, 0, 10, 0, 0, 1,
                -10, 10, 10, 0, 0, 1,
                10, 10, 10, 0, 0, 1
            };

            float[] wallThreeVertexs = new float[]
            {
                -10, 0, -10, 0, 0, 1,
                -10, 10, -10, 0, 0, 1,
                -10, 10, 10, 0, 0, 1,
                -10, 0, 10, 0, 0, 1
            };

            float[] wallFourVertexs = new float[]
            {
                10, 0, -10, 0, 0, 1,
                10, 10, -10, 0,01, 1,
                10, 10, 10, 0, 0, 1,
                10, 0, 10, 0, 0, 1
            };

            float[][] wallVertexs = new float[][]
            {
                wallOneVertexs,
                wallTwoVertexs,
                wallThreeVertexs,
                wallFourVertexs
            };

            for(int wallID = 1; wallID < 5; wallID++)
            {
                GL.BindVertexArray(mVAO_IDs[wallID]);
                GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[wallID]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(wallVertexs[wallID - 1].Length * sizeof(float)), wallVertexs[wallID - 1], BufferUsageHint.StaticDraw);

                GL.EnableVertexAttribArray(vPositionLocation);
                GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(vNormalLocation);
                GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            }
            #endregion

            #region Cylinder
            int count = 0;

            for(int CylinderID = 5; CylinderID < 8; CylinderID++)
            {
                GL.BindVertexArray(mVAO_IDs[CylinderID]);
                GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[CylinderID + count]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModel.Vertices.Length * sizeof(float)), mCylinderModel.Vertices, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[CylinderID + 1 + count]);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderModel.Indices.Length * sizeof(float)), mCylinderModel.Indices, BufferUsageHint.StaticDraw);

                GL.EnableVertexAttribArray(vPositionLocation);
                GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(vNormalLocation);
                GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

                count++;
            }
            #endregion

            #region Armadilo
            GL.BindVertexArray(mVAO_IDs[8]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[11]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mArmadiloModel.Vertices.Length * sizeof(float)), mArmadiloModel.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[12]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mArmadiloModel.Indices.Length * sizeof(float)), mArmadiloModel.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightDirection");
            Vector3 normalisedLightDirection, lightDirection = new Vector3(-1, -1, -1);
            Vector3.Normalize(ref lightDirection, out normalisedLightDirection);
            GL.Uniform3(uLightPositionLocation, normalisedLightDirection);

            mView = Matrix4.CreateTranslation(0, 2.75f, -4f);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uViewMatrix");
            GL.UniformMatrix4(uView, true, ref mView);

            mRoom = Matrix4.CreateTranslation(0, -5.0f, 0);

            mCylinderM = Matrix4.CreateTranslation(0, -4f, -2);
            mCylinderL = Matrix4.CreateTranslation(-4, -4f, -2);
            mCylinderR = Matrix4.CreateTranslation(4, -4f, -2);

            mArmadilo = Matrix4.CreateRotationY(-(float)Math.PI / 2f) * Matrix4.CreateTranslation(0, -2f, -2);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            mKeyPressed[e.KeyChar] = true;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            mKeyPressed[(int)e.Key] = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjectionMatrix");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            //TODO: Add movement to camera

            if (mKeyPressed['a'])
            {
                mView *= Matrix4.CreateRotationY(-2.5f);
            }
            if (mKeyPressed['d'])
            {
                mView *= Matrix4.CreateRotationY(2.5f);
            }
            if (mKeyPressed['w'])
            {
                mView *= Matrix4.CreateTranslation(0, 0, 0.1f);
            }
            if (mKeyPressed['s'])
            {
                mView *= Matrix4.CreateTranslation(0, 0, -0.1f);
            }

            int mViewLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "uViewMatrix");
            GL.UniformMatrix4(mViewLocation, true, ref mView);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //TODO: Work out issue with rendering

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModelMatrix");
            GL.UniformMatrix4(uModelLocation, true, ref mRoom);

            for(int i = 0; i < 5; i++)
            {
                GL.BindVertexArray(mVAO_IDs[i]);
                GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            }

            uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModelMatrix");
            GL.UniformMatrix4(uModelLocation, true, ref mCylinderM);

            GL.BindVertexArray(mVAO_IDs[5]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModelMatrix");
            GL.UniformMatrix4(uModelLocation, true, ref mCylinderL);

            GL.BindVertexArray(mVAO_IDs[6]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModelMatrix");
            GL.UniformMatrix4(uModelLocation, true, ref mCylinderR);

            GL.BindVertexArray(mVAO_IDs[7]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModelMatrix");
            GL.UniformMatrix4(uModelLocation, true, ref mArmadilo);

            GL.BindVertexArray(mVAO_IDs[8]);
            GL.DrawElements(PrimitiveType.Triangles, mArmadiloModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
