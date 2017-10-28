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
class Scene
{
	Shader vertexShader;
	Shader fragmentShader;
	
	ShaderProgram shaderProgram;

	Mesh origoTriangle;
	Mesh xTriangle;
	Mesh zTriangle;
	Mesh yTriangle;

	Mesh voxelMesh;
	
	Matrix4Uniform projectionMatrix;
	Matrix4Uniform viewMatrix;

	// Camera
	Vector3 cameraPosition;
	Vector3 cameraDirection;
	Vector3 sceneUp;
	
	public void loadScene()
	{
		vertexShader = Shader.CreateFromFile(ShaderType.VertexShader, "../data/shaders/vertex.vs");
		
		fragmentShader = Shader.CreateFromFile(ShaderType.FragmentShader, "../data/shaders/fragment.fs");
		
		shaderProgram = new ShaderProgram(vertexShader, fragmentShader);
		
		Error.checkGLError("Scene.loadScene");
		
		origoTriangle = Mesh.CreateTriangleMesh();
		xTriangle = Mesh.CreateTriangleMesh();
		zTriangle = Mesh.CreateTriangleMesh();
		yTriangle = Mesh.CreateTriangleMesh();

		voxelMesh = Mesh.CreateFromFile("../data/models/voxelBox.obj");

		shaderProgram.Use();
		Mesh.PositionDataIndex = shaderProgram.GetAttributeLocation("vPosition");
		Mesh.ColorDataIndex = shaderProgram.GetUniformLocation("uDiffuseColor");
		Mesh.ScaleDataIndex = shaderProgram.GetUniformLocation("uScale");


			origoTriangle.bufferData();
		xTriangle.bufferData();
		zTriangle.bufferData();
		yTriangle.bufferData();

		voxelMesh.bufferData();

		origoTriangle.WorldPosition = new Vector3(0, 0, 0);
		xTriangle.WorldPosition = new Vector3(1, 0, 0);
		zTriangle.WorldPosition = new Vector3(0, 0, 1);
		yTriangle.WorldPosition = new Vector3(0, 1, 0);

		voxelMesh.WorldPosition = new Vector3(3, 0, -3.0f);

		origoTriangle.DiffuseColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
		xTriangle.DiffuseColor = new Color4(1, 0.2f, 0.2f, 1);
		yTriangle.DiffuseColor = new Color4(0.2f, 1, 0.2f, 1);
		zTriangle.DiffuseColor = new Color4(0.2f, 0.2f, 1, 1);

		voxelMesh.DiffuseColor = new Color4(0.7f, 0.4f, 0.1f, 1.0f);
		voxelMesh.Scale = 0.3f;

		projectionMatrix = new Matrix4Uniform("projectionMatrix");
		projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 16.0f / 9.0f, 0.1f, 100f);

		cameraPosition = new Vector3(0, 0, -2);
		cameraDirection = new Vector3(0, 0, 1.0f);
		sceneUp = new Vector3(0, 1, 0);

		viewMatrix = new Matrix4Uniform("viewMatrix");
		viewMatrix.Matrix = Matrix4.CreateTranslation(cameraPosition);
		
		Error.checkGLError("Scene.loadScene");
	}

	public void drawMesh(Mesh mesh)
	{
		mesh.updateUniforms(shaderProgram);
		GL.BindVertexArray(mesh.VAOHandle);
		GL.DrawArrays(PrimitiveType.Triangles, 0, mesh.VertexAmount);
	}
	
	public void drawScene()
	{
		shaderProgram.Use();
		
		projectionMatrix.Set(shaderProgram);
		viewMatrix.Set(shaderProgram);

		drawMesh(origoTriangle);
		drawMesh(xTriangle);
		drawMesh(zTriangle);
		drawMesh(yTriangle);

		drawMesh(voxelMesh);

		Error.checkGLError("Scene.drawScene");

	}

	public void updateScene(KeyboardState keyState)
	{
		float cameraSpeed = 0.01f;
		if (keyState.IsKeyDown(key: Key.Up) || keyState.IsKeyDown(Key.W))
		{
			cameraPosition += cameraDirection * cameraSpeed;
		}
		else if (keyState.IsKeyDown(Key.Down) || keyState.IsKeyDown(Key.S))
		{
	        cameraPosition -= cameraDirection * cameraSpeed;
		}

		if (keyState.IsKeyDown(key: Key.Left) || keyState.IsKeyDown(Key.A))
		{
			cameraPosition -= Vector3.Cross(cameraDirection, sceneUp) * cameraSpeed;
		}
		else if (keyState.IsKeyDown(Key.Right) || keyState.IsKeyDown(Key.D))
		{
			cameraPosition += Vector3.Cross(cameraDirection, sceneUp) * cameraSpeed;
		}

		if (keyState.IsKeyDown(key: Key.R) )
		{
			cameraPosition +=  sceneUp * cameraSpeed;
		}
		else if (keyState.IsKeyDown(Key.F) )
		{
			cameraPosition -= sceneUp * cameraSpeed;
		}

			viewMatrix.Matrix = Matrix4.CreateTranslation(cameraPosition);

	}
}
}