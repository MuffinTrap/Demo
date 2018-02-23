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
	void drawScene();
	void updateScene(KeyboardState keyState, MouseState mouseState);
}

class EmptyScene : IScene
{
	public void loadScene(AssetManager assetManager) {}

	public void drawScene() {}

	public void updateScene(KeyboardState keyState, MouseState mouseState) {}
}

class RotatingScene : IScene
{ 
	ShaderProgram shaderProgram;

	DrawableMesh voxelMesh;
	
	CameraComponent camera;

	public RotatingScene()
	{
		camera = new CameraComponent();
	}
	
	public void loadScene(AssetManager assetManager)
	{
			// Load program from single file...
		shaderProgram = new ShaderProgram(assetManager.GetShader("objmesh.vs"), assetManager.GetShader("objmesh.fs"));
		
		Error.checkGLError("Scene.loadScene");

		shaderProgram.Use();
		
		shaderProgram.setSamplerUniform("inputTexture", 0);
		voxelMesh = new DrawableMesh(
			name: "Monu9"
			, data: assetManager.getMeshData("monu9.obj")
			, attributes: ShaderManager.getAttributes(new List<ShaderAttributeName> { ShaderAttributeName.Position, ShaderAttributeName.TexCoord, ShaderAttributeName.Normal }, shaderProgram)
			, transform: new TransformComponent()
			, material: assetManager.GetMaterial("palette")
			, shader: shaderProgram);
	
		voxelMesh.Transform.setLocationAndScale(new Vector3(0.0f, 0.0f, 0.0f), 0.2f);


		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);

		Error.checkGLError("Scene.loadScene");
	}
	
	public void drawScene()
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