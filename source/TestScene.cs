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
		ParticleEmitter emitter;

		List<PosAndDir> cameraFrames = new List<PosAndDir>();

		float worldWidth;
		float worldDepth;
		public void setCameraFrames(List<PosAndDir> frames) { }

		Vector3 cameraTarget = new Vector3(0, 1, 0);
		//float angleSpeed = 0.01f;

		public TestScene()
		{
			camera = new CameraComponent();
			cornerTriangles = new List<DrawableMesh>(4);

			/*
			cameraFrames = new List<PosAndDir>();
			cameraFrames.Add(new PosAndDir(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f,0.0f, 0.0f)));
			cameraFrames.Add(new PosAndDir(new Vector3(0.0f, 0.0f, 10.0f), new Vector3(1.0f, 0.0f, -1.0f)));
			cameraFrames.Add(new PosAndDir(new Vector3(10.0f, 3.0f, -3.0f), new Vector3(0.0f, 0.0f, -1.0f)));
			cameraFrames.Add(new PosAndDir(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f)));

			camera.Position = cameraFrames[0].position;
			camera.Direction = cameraFrames[0].direction;
			*/

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

			for (float cx = -1; cx <= 1; cx++)
			{
				for (float cz = -1; cz <= 1; cz++)
				{
					if (cz == 0 || cx == 0)
					{
						continue;
					}
					DrawableMesh t = assetManager.GetMesh("Triangle(" + cx + ",0," + cz + ")"
					, MeshDataGenerator.CreateTriangleMesh()
					, null
					, shaderProgram
					, new Vector3(cx * (worldWidth / 2), 0, cz * (worldDepth / 2)), 1);

					cornerTriangles.Add(t);
				}
				
			}

			megaGrid = assetManager.GetMesh("Megagrid"
			, MeshDataGenerator.CreateXZGrid(worldWidth, worldDepth, 1, 1)
			, null
			, shaderProgram
			, new Vector3(0.0f, -1.0f, 0.0f), 1);

			emitter = new ParticleEmitter(200, 30.0f, 0.5f, new Vector3(0, 0, 0), ParticleEmitter.EmitterShape.Rectangle, new Vector3(4.0f ,0.3f, 4.0f));

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
		}

		public void drawScene(float cameraFrame)
		{
			// Interpolate between frames
			// If there is no second frame stay still
			if (DemoSettings.GetDefaults().CameraSetting == DemoSettings.CameraMode.Frames)
			{
				camera.setFrame(cameraFrame, cameraFrames);
			}
			
			shaderProgram.Use();

			camera.setMatrices(shaderProgram);

			megaGrid.draw();

			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.draw();
			}

			//emitter.Draw(camera);
			
			
				
			Error.checkGLError("Scene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState)
		{

			camera.Update(keyState, mouseState);
			//camera.Orbit(angleSpeed, 3.0f, cameraTarget);

			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.Transform.rotateAroundY(0.05f);
			}
			//emitter.update();
		}

	}
}