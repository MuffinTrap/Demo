using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace MuffinSpace
{
	public class TestScene
	{
		ShaderProgram gridShader;
		List<DrawableMesh> cornerTriangles;
		DrawableMesh megaGrid;

		float worldWidth;
		float worldDepth;

		public TestScene()
		{
			cornerTriangles = new List<DrawableMesh>(4);

			worldWidth = 30;
			worldDepth = 30;
		}

		public void loadScene(AssetManager assetManager)
		{
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

		}

		public void drawScene(float cameraFrame)
		{
			Renderer rend = Renderer.GetSingleton();
			rend.RenderCamera();
			rend.RenderWithShader(gridShader);
			rend.RenderMesh(megaGrid);

			foreach (DrawableMesh ct in cornerTriangles)
			{
				rend.RenderMesh(ct);
			}

			Error.checkGLError("Scene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState)
		{
			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.Transform.rotateAroundY(0.05f);
			}
		}
	}
}