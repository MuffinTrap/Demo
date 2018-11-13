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
		List<PosAndDir> cameraFrames;
		public void setCameraFrames(List<PosAndDir> frames) 
		{
			cameraFrames = frames;
			if (cameraFrames.Count > 0)
			{
				camera.Position = cameraFrames[0].position;
				camera.Direction = cameraFrames[0].direction;
			}
		}

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

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("Scene.loadScene");
		}

		public void drawScene(float cameraFrame)
		{
			camera.setFrame(cameraFrame, cameraFrames);
			Renderer rend = Renderer.GetSingleton();
			rend.RenderWithShader(MainShader);
			rend.RenderCamera(camera);
			
			foreach(DrawableMesh m in allModels)
			{
				rend.RenderMesh(m);
			}

			Error.checkGLError("Scene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState)
		{
			camera.Update(keyState, mouseState);
		}
	}
}