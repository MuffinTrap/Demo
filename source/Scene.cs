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
	public interface IScene
	{
		void loadScene(AssetManager materialManager);
		void drawScene(float cameraFrame);
		void updateScene(KeyboardState keyState, MouseState mouseState);
	}

	class EmptyScene : IScene
	{
		public void loadScene(AssetManager assetManager) {}

		public void drawScene(float cameraFrame) {}

		public void updateScene(KeyboardState keyState, MouseState mouseState) {}
	}
}