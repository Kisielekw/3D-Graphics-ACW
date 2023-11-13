using OpenTK;
using OpenTK.Graphics;
using System;
using System.Drawing.Text;
using OpenTK.Graphics.OpenGL;
using Labs.Utility;
using OpenTK.Input;
using System.Drawing;
using System.Drawing.Imaging;

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

        private ModelUtility mCylinderModel, mArmadiloModel, mCubeModel, mSphereModel;
        
        private BitmapData mBrickTexture, mWoodTexture;
        private int[] mTextureID = new int[2];

        private bool[] mKeyPressed = new bool[256];

        private int[] mVAO_IDs = new int[11];
        private int[] mVBO_IDs = new int[17];

        private Matrix4 mRoom, mCylinderM, mCylinderL, mCylinderR, mArmadilo, mCube, mShpere;

        private Matrix4[] mViews = new Matrix4[2];
        private int cameraID = 0;

        private double mTime = 0;

        protected override void OnLoad(EventArgs e)
        {
 	        base.OnLoad(e);

            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            //load models
            mCylinderModel = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");
            mArmadiloModel = ModelUtility.LoadModel(@"Utility/Models/model.bin");
            mCubeModel = ModelUtility.LoadModel(@"Utility/Models/lab22model.sjg");
            mSphereModel = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");

            //Load Shader
            mShader = new ShaderUtility(@"ACW/Shaders/vertexShader.vert", @"ACW/Shaders/fragmentShader.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            int vTexCoordLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vTexCoords");

            #region Texture Loading
            GL.GenTextures(mTextureID.Length, mTextureID);

            string File = @"ACW/Textures/Brick.jpg";
            if (System.IO.File.Exists(File))
            {
                Bitmap TextureBitmap = new Bitmap(File);
                mBrickTexture = TextureBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, mTextureID[0]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, mBrickTexture.Width, mBrickTexture.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, mBrickTexture.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                TextureBitmap.UnlockBits(mBrickTexture);
            }
            else
            {
                throw new Exception("Could not find texture file " + File);
            }

            File = @"ACW/Textures/Wood.jpg";
            if (System.IO.File.Exists(File))
            {
                Bitmap TextureBitmap = new Bitmap(File);
                mWoodTexture = TextureBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, mTextureID[1]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, mWoodTexture.Width, mWoodTexture.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, mWoodTexture.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                TextureBitmap.UnlockBits(mWoodTexture);
            }
            else
            {
                throw new Exception("Could not find texture file " + File);
            }
            #endregion

            //Generate VAO and VBO
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            #region Floor
            float[] floorVertexs = new float[] 
            {
                -10, 0, -10, 0, 1, 0, 0, 1,
                -10, 0, 10, 0, 1, 0, 0, 0,
                10, 0, 10, 0, 1, 0, 1, 0,
                10, 0, -10, 0, 1, 0, 1, 1
            };

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(floorVertexs.Length * sizeof(float)), floorVertexs, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(vTexCoordLocation);
            GL.VertexAttribPointer(vTexCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            #endregion

            #region Walls
            float[] wallOneVertexs = new float[]
            {
                -10, 0, -10, 0, 0, 1f, 0, 0,
                10, 0, -10, 0, 0, 1f, 1, 0,
                10, 10, -10, 0, 0, 1f, 1, 1,
                -10, 10, -10, 0, 0, 1f, 0, 1
            };

            float[] wallTwoVertexs = new float[]
            {
                -10, 0, 10, 0, 0, -1, 1, 0,
                -10, 10, 10, 0, 0, -1, 1, 1,
                10, 10, 10, 0, 0, -1, 0, 1,
                10, 0, 10, 0, 0, -1, 0, 0
            };

            float[] wallThreeVertexs = new float[]
            {
                -10, 0, -10, 1, 0, 0, 1, 0,
                -10, 10, -10, 1, 0, 0, 1, 1,
                -10, 10, 10, 1, 0, 0, 0, 1,
                -10, 0, 10, 1, 0, 0, 0, 0
            };

            float[] wallFourVertexs = new float[]
            {
                10, 10, 10, -1, 0, 0, 1, 1,
                10, 10, -10, -1, 0, 0, 0, 1,
                10, 0, -10, -1, 0, 0, 0, 0,
                10, 0, 10, -1, 0, 0, 1, 0
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
                GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                GL.EnableVertexAttribArray(vNormalLocation);
                GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(vTexCoordLocation);
                GL.VertexAttribPointer(vTexCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
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

            #region Cube
            GL.BindVertexArray(mVAO_IDs[9]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[13]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCubeModel.Vertices.Length * sizeof(float)), mCubeModel.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[14]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCubeModel.Indices.Length * sizeof(float)), mCubeModel.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            #region Sphere
            GL.BindVertexArray(mVAO_IDs[10]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[15]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereModel.Vertices.Length * sizeof(float)), mSphereModel.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[16]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereModel.Indices.Length * sizeof(float)), mSphereModel.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            mViews[0] = Matrix4.CreateTranslation(0, 2.75f, -4f);
            mViews[1] = Matrix4.CreateTranslation(0, 2.75f, -4f);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uViewMatrix");
            GL.UniformMatrix4(uView, true, ref mViews[cameraID]);

            #region Lighting
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[0].position");
            Vector4 light1Position = Vector4.Transform(new Vector4(-6, 0, 0, 1), mViews[cameraID]);
            GL.Uniform4(uLightPositionLocation, light1Position);

            int uLightColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[0].colour");
            Vector3 light1Colour = new Vector3(1, 0, 0);
            GL.Uniform3(uLightColourLocation, light1Colour);

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[1].position");
            Vector4 light2Position = Vector4.Transform(new Vector4(0, 0, 0, 1), mViews[cameraID]);
            GL.Uniform4(uLightPositionLocation, light2Position);

            uLightColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[1].colour");
            Vector3 light2Colour = new Vector3(0, 1, 0);
            GL.Uniform3(uLightColourLocation, light2Colour);

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[2].position");
            Vector4 light3Position = Vector4.Transform(new Vector4(6, 0, 0, 1), mViews[cameraID]);
            GL.Uniform4(uLightPositionLocation, light3Position);

            uLightColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[2].colour");
            Vector3 light3Colour = new Vector3(0, 0, 1);
            GL.Uniform3(uLightColourLocation, light3Colour);
            #endregion

            #region Material
            int uMaterialAmbientLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.ambient");
            int uMaterialDiffuseLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.diffuse");
            int uMaterialSpecularLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.specular");
            int uMaterialShininessLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.shininess");

            Vector3 materialAmbient = new Vector3(0.1f, 0.1f, 0.1f);
            Vector3 materialDiffuse = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 materialSpecular = new Vector3(0.5f, 0.5f, 0.5f);

            GL.Uniform3(uMaterialAmbientLocation, materialAmbient);
            GL.Uniform3(uMaterialDiffuseLocation, materialDiffuse);
            GL.Uniform3(uMaterialSpecularLocation, materialSpecular);
            GL.Uniform1(uMaterialShininessLocation, 30f);
            #endregion

            int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = new Vector4(0, 0, 0, 1);
            GL.Uniform4(uEyePositionLocation, eyePosition);

            mRoom = Matrix4.CreateTranslation(0, -5.0f, 0);

            mCylinderM = Matrix4.CreateTranslation(0, -4f, -2);
            mCylinderL = Matrix4.CreateTranslation(-4, -4f, -2);
            mCylinderR = Matrix4.CreateTranslation(4, -4f, -2);

            mArmadilo = Matrix4.CreateRotationY(-(float)Math.PI / 2f) * Matrix4.CreateTranslation(0, -2f, -2);

            mCube = Matrix4.CreateScale(3) * Matrix4.CreateTranslation(-4, -2f, -2);

            mShpere = Matrix4.CreateTranslation(4, -2f, -2);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            mKeyPressed[e.KeyChar] = true;

            if (e.KeyChar == ' ')
            {
                cameraID = (cameraID + 1) % 2;
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.Key)
            {
                case Key.A:
                    mKeyPressed['a'] = false;
                    break;
                case Key.D:
                    mKeyPressed['d'] = false;
                    break;
                case Key.W:
                    mKeyPressed['w'] = false;
                    break;
                case Key.S:
                    mKeyPressed['s'] = false;
                    break;
                case Key.Q:
                    mKeyPressed['q'] = false;
                    break;
                case Key.E:
                    mKeyPressed['e'] = false;
                    break;
            }
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

            mTime += this.RenderTime;

            #region Camera Movement
            if (cameraID == 1)
            {
                if (mKeyPressed['a'])
                {
                    mViews[1] *= Matrix4.CreateRotationY(-0.025f);
                }
                if (mKeyPressed['d'])
                {
                    mViews[1] *= Matrix4.CreateRotationY(0.025f);
                }
                if (mKeyPressed['w'])
                {
                    mViews[1] *= Matrix4.CreateTranslation(0, 0, 0.1f);
                }
                if (mKeyPressed['s'])
                {
                    mViews[1] *= Matrix4.CreateTranslation(0, 0, -0.1f);
                }
                if (mKeyPressed['q'])
                {
                    mViews[1] *= Matrix4.CreateTranslation(0.1f, 0, 0);
                }
                if (mKeyPressed['e'])
                {
                    mViews[1] *= Matrix4.CreateTranslation(-0.1f, 0, 0);
                }
            }

            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uViewMatrix");
            GL.UniformMatrix4(uView, true, ref mViews[cameraID]);

            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[0].position");
            Vector4 light1Position = Vector4.Transform(new Vector4(-6, 0, 0, 1), mViews[cameraID]);
            GL.Uniform4(uLightPositionLocation, light1Position);

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[1].position");
            Vector4 light2Position = Vector4.Transform(new Vector4(0, 0, 0, 1), mViews[cameraID]);
            GL.Uniform4(uLightPositionLocation, light2Position);

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLights[2].position");
            Vector4 light3Position = Vector4.Transform(new Vector4(6, 0, 0, 1), mViews[cameraID]);
            GL.Uniform4(uLightPositionLocation, light3Position);
            #endregion

            #region Sphere Movement
            double y = Math.Sin(mTime) + 1;
            mShpere = Matrix4.CreateTranslation(4, (float)y - 2, -2);
            #endregion

            #region Armadilo Movement
            double rotation = (Math.Sin(mTime) * (Math.PI));
            mArmadilo = Matrix4.CreateRotationY((float)rotation) * Matrix4.CreateRotationY(-(float)Math.PI / 2f) * Matrix4.CreateTranslation(0, -2f, -2);
            #endregion

            #region Cube Movement
            float x = (float)Math.Sin(mTime * 2) * 0.3f;
            float z = (float)Math.Cos(mTime * 2) * 0.3f;

            mCube = Matrix4.CreateScale(3) * Matrix4.CreateTranslation(-4 + x, -2f, -2 + z);
            #endregion
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uTextureLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTexture");
            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModelMatrix");
            
            GL.UniformMatrix4(uModelLocation, true, ref mRoom);
            int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTexture");
            GL.Uniform1(uTextureSamplerLocation, 1);

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            GL.Uniform1(uTextureLocation, mTextureID[0]);

            for (int i = 1; i < 5; i++)
            {
                GL.BindVertexArray(mVAO_IDs[i]);
                GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            }

            GL.UniformMatrix4(uModelLocation, true, ref mCylinderM);

            GL.BindVertexArray(mVAO_IDs[5]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.UniformMatrix4(uModelLocation, true, ref mCylinderL);

            GL.BindVertexArray(mVAO_IDs[6]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.UniformMatrix4(uModelLocation, true, ref mCylinderR);

            GL.BindVertexArray(mVAO_IDs[7]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.UniformMatrix4(uModelLocation, true, ref mArmadilo);

            GL.BindVertexArray(mVAO_IDs[8]);
            GL.DrawElements(PrimitiveType.Triangles, mArmadiloModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.UniformMatrix4(uModelLocation, true, ref mCube);

            GL.BindVertexArray(mVAO_IDs[9]);
            GL.DrawElements(PrimitiveType.Triangles, mCubeModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.UniformMatrix4(uModelLocation, true, ref mShpere);

            GL.BindVertexArray(mVAO_IDs[10]);
            GL.DrawElements(PrimitiveType.Triangles, mSphereModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

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
