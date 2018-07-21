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
		ShaderProgram gridShader;
		List<DrawableMesh> cornerTriangles;
		DrawableMesh megaGrid;

		CameraComponent camera;

		float worldWidth;
		float worldDepth;

		public TestScene(CameraComponent mainCamera)
		{
			camera = mainCamera;
			cornerTriangles = new List<DrawableMesh>(4);

			worldWidth = 30;
			worldDepth = 30;
		}

		public void loadScene(AssetManager assetManager)
		{
			// Load program from single file...
			gridShader = assetManager.GetShaderProgram("gridmesh");

			Error.checkGLError("Scene.loadScene");

			gridShader.Use();

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
					, gridShader
					, new Vector3(cx * (worldWidth / 2), 0, cz * (worldDepth / 2)), 1);

					cornerTriangles.Add(t);
				}
				
			}

			megaGrid = assetManager.GetMesh("Megagrid"
			, MeshDataGenerator.CreateXZGrid(worldWidth, worldDepth, 1, 1)
			, null
			, gridShader
			, new Vector3(0.0f, -1.0f, 0.0f), 1);



			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
		}

		public void drawScene(float cameraFrame)
		{
			ShaderUniformManager uniMan = ShaderUniformManager.GetSingleton();
			megaGrid.ActivateForDrawing();
			uniMan.ActivateShader(gridShader);

			megaGrid.draw();

			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.ActivateForDrawing();
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