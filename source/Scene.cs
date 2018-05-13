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

public interface IScene
{
	void loadScene(AssetManager materialManager);
	void drawScene(float cameraFrame);
	void updateScene(KeyboardState keyState, MouseState mouseState);
	void setCameraFrames(List<PosAndDir> frames);
}

class EmptyScene : IScene
{
	public void loadScene(AssetManager assetManager) {}

	public void drawScene(float cameraFrame) {}

	public void updateScene(KeyboardState keyState, MouseState mouseState) {}

	public void setCameraFrames(List<PosAndDir> frames) { }
}

class RotatingScene : IScene
{ 
	ShaderProgram shaderProgram;

	DrawableMesh voxelMesh;
	
	CameraComponent camera;

	public void setCameraFrames(List<PosAndDir> frames) { }

	public RotatingScene()
	{
		camera = new CameraComponent();
	}
	
	public void loadScene(AssetManager assetManager)
	{
		shaderProgram = assetManager.GetShaderProgram("objmesh");
		shaderProgram.Use();
		shaderProgram.setSamplerUniform("inputTexture", 0);

		voxelMesh = assetManager.GetMesh("Monu9", "monu9.obj", "palette"
		, shaderProgram
		, new Vector3(0.0f, 0.0f, 0.0f), 0.2f);

		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);

		Error.checkGLError("Scene.loadScene");
	}
	
	public void drawScene(float cameraFrame)
	{
		shaderProgram.Use();
		camera.setMatrices(shaderProgram);

		voxelMesh.draw();

		Error.checkGLError("Scene.drawScene");
	}

	public void updateScene(KeyboardState keyState, MouseState mouseState)
	{
		camera.Update(keyState, mouseState);
		// voxelMesh.Transform.rotateAroundY(0.01f);
	}
}
}