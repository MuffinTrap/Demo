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

		float triangleRot = 0.0f;

		public TestScene()
		{
			cornerTriangles = new List<DrawableMesh>(3);

			worldWidth = 10;
			worldDepth = 20;
		}

		public void Load(AssetManager assetManager)
		{
			gridShader = assetManager.GetShaderProgram("gridmesh");

			DrawableMesh xt = assetManager.CreateMesh("Triangle X"
			, MeshDataGenerator.CreateTriangleMesh()
			, "default"
			, gridShader
			, new Vector3(worldWidth, 0.0f, 0.0f));

			DrawableMesh zt = assetManager.CreateMesh("Triangle Z"
			, MeshDataGenerator.CreateTriangleMesh()
			, "default"
			, gridShader
			, new Vector3(0.0f, 0.0f, worldDepth));

			DrawableMesh ot = assetManager.CreateMesh("Triangle O"
			, MeshDataGenerator.CreateTriangleMesh()
			, "default"
			, gridShader
			, new Vector3(0.0f, 0.0f, 0.0f));

			cornerTriangles.Add(xt);
			cornerTriangles.Add(zt);
			cornerTriangles.Add(ot);
				
			megaGrid = assetManager.CreateMesh("Megagrid"
			, MeshDataGenerator.CreateXZGrid(worldWidth * 2.0f, worldDepth * 2.0f, 1, 1)
			, "default"
			, gridShader
			, new Vector3(0.0f, 0.0f, 0.0f));

			Renderer rend = Renderer.GetSingleton();

			// rend.SetClearColor(new Vector3(0.5f, 0.5f, 1.0f));
			rend.SetClearColor(new Vector3(0.001f, 0.001f, 0.01f));
			Error.checkGLError("TestScene.loadScene");
		}

		public void Draw()
		{
			Renderer rend = Renderer.GetSingleton();
			rend.RenderMesh(megaGrid);

			foreach (DrawableMesh ct in cornerTriangles)
			{
				rend.RenderMesh(ct);
			}

			Error.checkGLError("Scene.drawScene");
		}

		public void Update()
		{

			triangleRot += 0.1f;
			foreach (DrawableMesh ct in cornerTriangles)
			{
				ct.Transform.SetRotation(triangleRot);
			}
		}
	}
}