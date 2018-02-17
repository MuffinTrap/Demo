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
	void updateScene(KeyboardState keyState);
}

class EmptyScene : IScene
{
	public void loadScene(AssetManager assetManager) {}

	public void drawScene() {}

	public void updateScene(KeyboardState keyState) {}
}

class RotatingScene : IScene
{ 
	ShaderProgram shaderProgram;

	DrawableMesh voxelMesh;
	
	Matrix4Uniform projectionMatrix;
	Matrix4Uniform viewMatrix;

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
	
		voxelMesh.Transform.setLocationAndScale(new Vector3(0.0f, 0.0f, 0.0f), 0.1f);

		projectionMatrix = new Matrix4Uniform(ShaderAttribute.getUniformName(ShaderUniformName.ProjectionMatrix));
		projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 16.0f / 9.0f, 0.1f, 100f);

		viewMatrix = new Matrix4Uniform(ShaderAttribute.getUniformName(ShaderUniformName.ViewMatrix));
		viewMatrix.Matrix = camera.GetViewMatrix();

		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);

		Error.checkGLError("Scene.loadScene");
	}
	
	public void drawScene()
	{
		shaderProgram.Use();
		
		projectionMatrix.Set(shaderProgram);
		viewMatrix.Set(shaderProgram);

		voxelMesh.draw();

		Error.checkGLError("Scene.drawScene");
	}

	public void updateScene(KeyboardState keyState)
	{
		camera.Update(keyState);
		viewMatrix.Matrix = camera.GetViewMatrix();

		// voxelMesh.Transform.rotateAroundY(0.01f);
	}
}
}