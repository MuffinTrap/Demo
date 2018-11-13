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
		// ShaderProgram guiShader;

		// DrawableMesh quadMesh;

		CameraComponent cameraOrthogonal;

		public Scene2D()
		{
			cameraOrthogonal = new CameraComponent();
		}

		public void setCameraFrames(List<PosAndDir> frames) { }

		public void loadScene(AssetManager assetManager)
		{
			// This is the gui scene

			Error.checkGLError("Scene.loadScene");
		}


		public void drawScene(float cameraFrame) 
		{
			Error.checkGLError("Scene2D.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState) 
		{
		}
	}
}