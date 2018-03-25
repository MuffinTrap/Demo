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
	public class SceneFromFile : IScene
	{
		public List<DrawableMesh> allModels;

		CameraComponent camera;
		public string ConfigFile { get; }

		public SceneFromFile(string sceneFile)
		{
			camera = new CameraComponent();
			ConfigFile = sceneFile;
		}

		public ShaderProgram MainShader { get; set;}


		public void loadScene(AssetManager assetManager)
		{
			Error.checkGLError("Scene.loadScene");

			MainShader.Use();

			MainShader.setSamplerUniform("inputTexture", 0);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("Scene.loadScene");
		}

		public void drawScene(float cameraFrame)
		{
			MainShader.Use();
			camera.setMatrices(MainShader);

			foreach(DrawableMesh m in allModels)
			{
				m.draw();
			}

			Error.checkGLError("Scene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState)
		{
			camera.Update(keyState, mouseState);
		}
	}
}