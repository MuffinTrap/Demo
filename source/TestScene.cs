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
			cornerTriangles = new List<DrawableMesh>(3);

			worldWidth = 10;
			worldDepth = 10;
		}

		public void Load(AssetManager assetManager)
		{
			gridShader = assetManager.GetShaderProgram("gridmesh");

			DrawableMesh xt = assetManager.GetMesh("Triangle X"
			, MeshDataGenerator.CreateTriangleMesh()
			, null
			, gridShader
			, new Vector3(worldWidth, 0.0f, 0.0f), 1.0f);

			DrawableMesh zt = assetManager.GetMesh("Triangle Z"
			, MeshDataGenerator.CreateTriangleMesh()
			, null
			, gridShader
			, new Vector3(0.0f, 0.0f, worldDepth), 1.0f);

			DrawableMesh ot = assetManager.GetMesh("Triangle O"
			, MeshDataGenerator.CreateTriangleMesh()
			, null
			, gridShader
			, new Vector3(0.0f, 0.0f, 0.0f), 1.0f);

			cornerTriangles.Add(xt);
			cornerTriangles.Add(zt);
			cornerTriangles.Add(ot);
				
			megaGrid = assetManager.GetMesh("Megagrid"
			, MeshDataGenerator.CreateXZGrid(worldWidth * 2.0f, worldDepth * 2.0f, 1, 1)
			, null
			, gridShader
			, new Vector3(0.0f, 0.0f, 0.0f), 1);

			Error.checkGLError("TestScene.loadScene");
		}

		public void Draw()
		{
			Renderer rend = Renderer.GetSingleton();
			rend.ClearScreen(Color4.AliceBlue);
			rend.RenderMesh(megaGrid);

			foreach (DrawableMesh ct in cornerTriangles)
			{
				rend.RenderMesh(ct);
			}

			Error.checkGLError("Scene.drawScene");
		}

		public void Update()
		{
			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.Transform.rotateAroundY(0.05f);
			}
		}
	}
}