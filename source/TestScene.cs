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
			, MeshDataGenerator.CreateTriangleMesh(false)
			, "default"
			, gridShader
			, new Vector3(worldWidth, 0.0f, 0.0f));

			DrawableMesh zt = assetManager.CreateMesh("Triangle Z"
			, MeshDataGenerator.CreateTriangleMesh(false)
			, "default"
			, gridShader
			, new Vector3(0.0f, 0.0f, worldDepth));

			DrawableMesh ot = assetManager.CreateMesh("Triangle O"
			, MeshDataGenerator.CreateTriangleMesh(false)
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

	public class LoadingScene
	{
		ShaderProgram guiShader;
		DrawableMesh above;
		DrawableMesh below;

		float triangleDistance = 15.0f;
		float triangleScale = 2f;

		public void Load(AssetManager assetManager)
		{
			guiShader = assetManager.GetShaderProgram("gridmesh");

			above = assetManager.CreateMesh("Triangle Above"
			, MeshDataGenerator.CreateTriangleMesh(false)
			, "default"
			, guiShader
			, new Vector3(0, triangleDistance, 0.0f));
			above.Transform.Scale = triangleScale;

			below = assetManager.CreateMesh("Triangle Below"
			, MeshDataGenerator.CreateTriangleMesh(false)
			, "default"
			, guiShader
			, new Vector3(0.0f, -triangleDistance, 0.0f));

			below.Transform.Scale = triangleScale;
			below.Transform.SetRotationAxis(new Vector3(0, 0, 1));
			below.Transform.SetRotation(MathHelper.Pi);

			Renderer rend = Renderer.GetSingleton();
			rend.GetCamera().Position = new Vector3(0.0f, 0.0f, 20.0f);
			rend.GetCamera().CameraFront = new Vector3(0.0f, 0.0f, -1.0f);
			rend.GetCamera().CreateMatrices();

			// rend.SetClearColor(new Vector3(0.0f, 100.0f / 255f, 120 / 255f));  // Hospital green
			rend.SetClearColor(new Vector3(0, 0, 0));
			Error.checkGLError("TestScene.loadScene");

		}
		public void Draw()
		{
			Renderer rend = Renderer.GetSingleton();
			rend.RenderMesh(above);
			rend.RenderMesh(below);
		}

		public void Update(float timeDecreasing)
		{
			Renderer.GetSingleton().SetClearColor(new Vector3(0.0f, 100.0f / 255f, 120 / 255f) * timeDecreasing);  // Hospital green
			above.Transform.Translation = new Vector4(0.0f, triangleDistance * timeDecreasing, 0.0f, 1.0f);
			below.Transform.Translation = new Vector4(0.0f, -1f * triangleDistance * timeDecreasing, 0.0f, 1.0f);
		}
	}
}