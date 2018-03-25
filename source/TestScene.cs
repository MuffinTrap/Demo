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
	public class TestScene : IScene
	{
		ShaderProgram shaderProgram;
		List<DrawableMesh> cornerTriangles;
		DrawableMesh megaGrid;

		CameraComponent camera;

		List<PosAndDir> cameraFrames;

		float worldWidth;
		float worldDepth;
		

		public TestScene()
		{
			camera = new CameraComponent();
			cornerTriangles = new List<DrawableMesh>(4);

			cameraFrames = new List<PosAndDir>();
			cameraFrames.Add(new PosAndDir(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f,0.0f, 0.0f)));
			cameraFrames.Add(new PosAndDir(new Vector3(0.0f, 0.0f, 10.0f), new Vector3(1.0f, 0.0f, -1.0f)));
			cameraFrames.Add(new PosAndDir(new Vector3(10.0f, 3.0f, -3.0f), new Vector3(0.0f, 0.0f, -1.0f)));
			cameraFrames.Add(new PosAndDir(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f)));

			camera.Position = cameraFrames[0].position;
			camera.Direction = cameraFrames[0].direction;

			worldWidth = 30;
			worldDepth = 30;
		}

		public void loadScene(AssetManager assetManager)
		{
			// Load program from single file...
			shaderProgram = new ShaderProgram(assetManager.GetShader("testmesh.vs")
			, assetManager.GetShader("testmesh.fs"));

			Error.checkGLError("Scene.loadScene");

			shaderProgram.Use();

			List<ShaderAttributeName> attrs = new List<ShaderAttributeName> { ShaderAttributeName.Position};
			List<ShaderAttribute> attributes = ShaderManager.getAttributes(attrs, shaderProgram);


			for (float cx = -1; cx <= 1; cx++)
			{
				for (float cz = -1; cz <= 1; cz++)
				{
					if (cz == 0 || cx == 0)
					{
						continue;
					}
					DrawableMesh t = new DrawableMesh("Triangle(" + cx + ",0," + cz + ")", MeshDataGenerator.CreateTriangleMesh(assetManager), attributes, new TransformComponent(), null, shaderProgram);
					t.Transform.setLocationAndScale(new Vector3(cx * (worldWidth /2), 0, cz * (worldDepth/2)), 1);
					cornerTriangles.Add(t);
				}
				
			}

			megaGrid = new DrawableMesh("MegaGrid", MeshDataGenerator.CreateXZGrid(worldWidth, worldDepth, 1, 1), attributes, new TransformComponent(), null, shaderProgram);
			megaGrid.Transform.setLocationAndScale(new Vector3(0.0f, -1.0f, 0.0f), 1);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
		}

		public void drawScene(float cameraFrame)
		{
			int firstFrame = (int)Math.Floor(cameraFrame);
			int secondFrame = firstFrame + 1;
			float diff = cameraFrame - (float)firstFrame;

			if (firstFrame < cameraFrames.Count && secondFrame < cameraFrames.Count)
			{
				camera.Position = cameraFrames[firstFrame].position * (1.0f - diff) + cameraFrames[secondFrame].position * (diff);

				camera.Direction = cameraFrames[firstFrame].direction * (1.0f - diff) + cameraFrames[secondFrame].direction * (diff);
			}

			shaderProgram.Use();


			camera.setMatrices(shaderProgram);

			megaGrid.draw();

			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.draw();
			}
				
			Error.checkGLError("Scene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState)
		{

			camera.Update(keyState, mouseState);
			

			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.Transform.rotateAroundY(0.05f);
			}
		}

	}
}