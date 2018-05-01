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

			emitter = new ParticleEmitter(20, 3.0f, new Vector3(0, 0, 0));

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
		}

		public void drawScene(float cameraFrame)
		{
			// Interpolate between frames
			// If there is no second frame stay still
			if (DemoSettings.GetDefaults().CameraSetting == DemoSettings.CameraMode.Frames)
			{
				int firstFrame = (int)Math.Floor(cameraFrame);
				int secondFrame = firstFrame + 1;

				bool firstInFrames = firstFrame < cameraFrames.Count;
				bool secondInFrames = secondFrame < cameraFrames.Count;
				if (firstInFrames && secondInFrames)
				{
					Vector3 startPos = cameraFrames[firstFrame].position;
					Vector3 startDir = cameraFrames[firstFrame].direction;
					Vector3 targetPos = cameraFrames[secondFrame].position;
					Vector3 targetDir = cameraFrames[secondFrame].direction;

					float diff = cameraFrame - (float)firstFrame;
					camera.Position = startPos * (1.0f - diff) + targetPos * (diff);
					camera.Direction = startDir * (1.0f - diff) + targetDir * (diff);
				}
				else if (firstInFrames && !secondInFrames)
				{
					camera.Position = cameraFrames[firstFrame].position;
					camera.Direction = cameraFrames[firstFrame].direction;
				}
				else
				{
					// nop
				}
			}
			
			shaderProgram.Use();

			camera.setMatrices(shaderProgram);

			megaGrid.draw();

			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.draw();
			}

			// Draw particles, but how?

			ShaderProgram pShader = emitter.ParticleShader;
			pShader.Use();
			camera.setMatrices(pShader);
			List<Matrix4> mat = emitter.Matrices;
			DrawableMesh part = emitter.ParticleMesh;
			int colorLoc = pShader.GetUniformLocation("uParticleColor");
			if (colorLoc != -1)
			{
				pShader.SetColorUniform(colorLoc, new Vector4(1, 0, 0, 1));
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Limited, "Invalid uniform location");
			}
			

			for (int p = 0; p < emitter.Particles.Count; p++)
			{
				ParticleEmitter.Particle par = emitter.Particles[p];
				if (par.isActive)
				{
					part.Transform.WorldPosition = mat[par.matrixIndex].ExtractTranslation();
					part.Transform.SetRotationMatrix(camera.GetRotationMatrix());
					part.draw();
				}
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
			emitter.update();
		}

	}
}