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
	public class UnicornScene : IScene
	{
		ShaderProgram modelShader;
		ParticleEmitter particles;
		ParticleEmitter smoke;

		DrawableMesh unicornMesh;
		DrawableMesh terrainMesh;

		Vector3 cameraPos = new Vector3(0, 5, 10);
		Vector3 cameraTarget = new Vector3(0, 0, 0);
		float cameraOrbitSpeed = 0.003f;
		const float fullCircle = (float)(Math.PI * 2.0f);
		float angleSpeed = fullCircle / (float)DemoSettings.GetDefaults().UpdatesPerSecond;

		CameraComponent camera;
		public void setCameraFrames(List<PosAndDir> frames) { }

		public UnicornScene()
		{
			camera = new CameraComponent();
		}

		public void loadScene(AssetManager assetManager)
		{
			modelShader = assetManager.GetShaderProgram("objmesh");
			modelShader.Use();
			modelShader.setSamplerUniform("inputTexture", 0);

			float houseScale = 0.1f;
			unicornMesh = assetManager.GetMesh(
				name: "House"
				, modelFile: "house.obj"
				, material: "house"
				, shader: modelShader
				, position: new Vector3(0, 0, 0)
				, scale: houseScale);

			terrainMesh = assetManager.GetMesh("Terrain", MeshDataGenerator.CreateTerrain(10, 10, 1), "green", modelShader, new Vector3(0, 0, 0), 1.0f);

			smoke = new ParticleEmitter(
				100,
				3.0f,
				6.0f,
				new Vector3(0.8f, 2.5f, -0.2f),
				ParticleEmitter.EmitterShape.Point,
				new Vector3(0, 0, 0));

			List<Vector4> smokeColors = new List<Vector4>();
			smokeColors.Add(new Vector4(new Vector3(0.4f), 1.0f));
			smokeColors.Add(new Vector4(new Vector3(0.2f), 1.0f));
			smokeColors.Add(new Vector4(new Vector3(0.6f), 1.0f));
			smokeColors.Add(new Vector4(new Vector3(0.8f), 1.0f));
			smokeColors.Add(new Vector4(new Vector3(0.7f), 1.0f));
			smoke.SetColors(smokeColors);

			particles = new ParticleEmitter(
				particleAmount: 800, 
				emitRate: 30.0f, 
				lifeTime: 10.0f, 
				worldPosition: new Vector3(0, 0, 0), 
				shape: ParticleEmitter.EmitterShape.Rectangle, 
				sizes: new Vector3(10.0f, 0.2f, 10.0f));

			List<Vector4> particleColors = new List<Vector4>();
			particleColors.Add(new Vector4(1.0f, 0.4f, 0.4f, 1.0f));
			particleColors.Add(new Vector4(1.0f, 0.8f, 0.8f, 1.0f));
			particleColors.Add(new Vector4(0.2f, 1.0f, 0.5f, 1.0f));
			particleColors.Add(new Vector4(0.7f, 1.0f, 0.5f, 1.0f));
			particles.SetColors(particleColors);
			particles.ParticleSpeed = 0.1f;
			particles.ParticleSize = 0.5f;

			//particles.Emit(100);


			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			Error.checkGLError("Scene.loadScene");

			camera.Position = cameraPos;
			camera.SetTarget(cameraTarget);

		}

		public void drawScene(float cameraFrame)
		{
			modelShader.Use();

			camera.setMatrices(modelShader);

			terrainMesh.draw();
			unicornMesh.draw();

			particles.Draw(camera);
			smoke.Draw(camera);

			Error.checkGLError("UnicornScene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState)
		{
			particles.update();
			smoke.update();
			
			camera.Orbit(cameraOrbitSpeed, 2.0f, 4.5f, cameraTarget);
		}
	}
}