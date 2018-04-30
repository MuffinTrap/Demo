using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTkConsole
{
	public class Scene2D : IScene 
	{
		ShaderProgram shaderProgram;

		DrawableMesh quadMesh;

		CameraComponent camera;

		public Scene2D()
		{
			camera = new CameraComponent();
		}

		public void loadScene(AssetManager assetManager)
		{
			// Load program from single file...
			shaderProgram = assetManager.GetShaderProgram("texturequad");

			shaderProgram.Use();
			
			shaderProgram.setSamplerUniform("inputTexture", 0);

			quadMesh = assetManager.GetMesh("Konata"
			, MeshDataGenerator.CreateTexturedQuadMesh()
			, "konata"
			, shaderProgram
			, new Vector3(0.0f, 11.0f, 0.0f)
			, 1.0f);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("Scene.loadScene");
		}


		public void drawScene(float cameraFrame) 
		{
			shaderProgram.Use();

			camera.setMatrices(shaderProgram);

			quadMesh.draw();

			Error.checkGLError("Scene2D.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState) 
		{
			camera.Update(keyState, mouseState);
			quadMesh.Transform.rotateAroundY(0.04f);
		}
	}
}