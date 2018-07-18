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
	public class LightScene : IScene
	{
		ShaderProgram shaderProgram;
		ShaderProgram lampObjectShader;

		DrawableMesh quadMesh;
		DrawableMesh lampMesh;

		CameraComponent camera;

		int viewPositionLocation;

		public LightScene(CameraComponent mainCamera)
		{
			camera = mainCamera;
		}

		public void setCameraFrames(List<PosAndDir> frames) { }

		public void loadScene(AssetManager assetManager)
		{
			// Load program from single file...
			shaderProgram = assetManager.GetShaderProgram("litobjmesh");
			lampObjectShader = assetManager.GetShaderProgram("lampmesh");

			Vector3 lightPosition = new Vector3(0.0f, 0.0f, 2.0f);
			
			quadMesh = assetManager.GetMesh("Lit_Cube"
			, MeshDataGenerator.CreateQuadMesh(true, false)
			, null
			, shaderProgram
			, new Vector3(0.0f, 0.0f, 1.0f)
			, 1.0f);

			lampMesh = assetManager.GetMesh("Lamp"
			, MeshDataGenerator.CreateTriangleMesh()
			, null
			, lampObjectShader
			, lightPosition
			, 0.2f);

			Vector4 lightColor = new Vector4(1, 1, 1, 1);
			Vector4 objectColor = new Vector4(1.0f, 0.5f, 0.31f, 1.0f);
			float ambientStrength = 0.2f;
			float diffuseStrength = 0.2f;
			float specularStrength = 0.7f;
			int specularPower = 4;

			shaderProgram.Use();

			// How to send light information to shader
			/*
			shaderProgram.SetColorUniform(shaderProgram.GetUniformLocation("lightColor"), lightColor);
			shaderProgram.SetColorUniform(shaderProgram.GetUniformLocation("objectColor"), objectColor);
			shaderProgram.SetFloatUniform(shaderProgram.GetUniformLocation("ambientStrength"), ambientStrength);
			shaderProgram.SetFloatUniform(shaderProgram.GetUniformLocation("diffuseStrength"), diffuseStrength);
			shaderProgram.SetFloatUniform(shaderProgram.GetUniformLocation("specularStrength"), specularStrength);
			shaderProgram.SetIntUniform(shaderProgram.GetUniformLocation("specularPower"), specularPower);
			shaderProgram.SetVec3Uniform(shaderProgram.GetUniformLocation("lightPosition"), lightPosition);
			*/
			//viewPositionLocation = shaderProgram.GetUniformLocation("viewPosition");
			lampObjectShader.Use();
			lampObjectShader.SetColorUniform(lampObjectShader.GetUniformLocation("lampColor"), lightColor);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);

			camera.ActivateForDrawing();
			Error.checkGLError("LightScene.loadScene");
		}


		public void drawScene(float cameraFrame) 
		{
			ShaderUniformManager uniformManager = ShaderUniformManager.GetSingleton();
			uniformManager.ActivateShader(shaderProgram);
			quadMesh.ActivateForDrawing();
			//shaderProgram.SetVec3Uniform(viewPositionLocation, camera.Position);
			quadMesh.draw();

			lampMesh.ActivateForDrawing();
			uniformManager.ActivateShader(lampObjectShader);
			lampMesh.draw();

			Error.checkGLError("LightScene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState) 
		{
			quadMesh.Transform.rotateAroundY(0.01f);
			camera.Update(keyState, mouseState);
			shaderProgram.Use();
			//shaderProgram.SetVec3Uniform(shaderProgram.GetUniformLocation("lightPosition"), camera.Position);
		}
	}
}