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
	
	Matrix4Uniform projectionMatrix;
	Matrix4Uniform viewMatrix;
	
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
		
		

		shaderProgram.Use();
		origoTriangle.bufferData(shaderProgram.GetAttributeLocation("vPosition"));
		xTriangle.bufferData(shaderProgram.GetAttributeLocation("vPosition"));
		zTriangle.bufferData(shaderProgram.GetAttributeLocation("vPosition"));
		yTriangle.bufferData(shaderProgram.GetAttributeLocation("vPosition"));

		origoTriangle.worldMatrix.Matrix = Matrix4.CreateTranslation(new Vector3(0, 0, 0));
		xTriangle.worldMatrix.Matrix = Matrix4.CreateTranslation(new Vector3(1, 0, 0));
		zTriangle.worldMatrix.Matrix = Matrix4.CreateTranslation(new Vector3(0, 0, 1));
		yTriangle.worldMatrix.Matrix = Matrix4.CreateTranslation(new Vector3(0, 1, 0));

		projectionMatrix = new Matrix4Uniform("projectionMatrix");
		projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 16.0f / 9.0f, 0.1f, 100f);
		
		viewMatrix = new Matrix4Uniform("viewMatrix");
		viewMatrix.Matrix = Matrix4.CreateTranslation(0.0f, 0.0f, -2.0f);
		
		Error.checkGLError("Scene.loadScene");
	}

	public void drawMesh(Mesh mesh)
	{
		mesh.worldMatrix.Set(shaderProgram);
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

		Error.checkGLError("Scene.drawScene");

	}

}
}