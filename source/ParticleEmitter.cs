using System;
using System.Collections.Generic;
using OpenTK;

namespace OpenTkConsole
{
	public class ParticleEmitter
	{
		public TransformComponent Transform { get; set; }

		public float EmitRate { get; set; } // How many particles per second
		public float LifeTime { get; set; }
		public Vector3 EmitDirection { get; set; }
		public float ParticleSize { get; set; }
		public float ParticleSpeed { get; set; }
		public Vector4 ParticleColor { get; set; }
	

		public int ActiveParticles { get; private set; }
		public List<Matrix4> Matrices { get; private set; }
		public List<Particle> Particles { get; private set; }
		public DrawableMesh ParticleMesh { get; private set; }
		public ShaderProgram ParticleShader { get; private set; }


		private double emitCounter;
		private int maxParticles;

		private Random randomGenerator;
		
		public float Width { get; set; }
		public float Height { get; set; }
		public float Lenght { get; set; }

		public class Particle
		{
			public bool isActive;
			public int matrixIndex;
			public Vector3 direction;
			public Vector4 color;
			public float speed;
			public float timeLeft;
		}

		public enum EmitterShape
		{
			Point,
			Rectangle
		}

		public EmitterShape Shape { get; set; }

		public void SetColors(List<Vector4> possibleColors)
		{
			if (possibleColors.Count > 0)
			{
				colors = possibleColors;
			}
		}
		private List<Vector4> colors;
		

		public ParticleEmitter(int particleAmount, float emitRate, float lifeTime, Vector3 worldPosition, EmitterShape shape, Vector3 sizes)
		{
			maxParticles = particleAmount;
			// Load quad mesh
			AssetManager ass = AssetManager.GetAssetManagerSingleton();

			ParticleShader = ass.GetShaderProgram("particle");

			

			ParticleMesh = ass.GetMesh("particle", MeshDataGenerator.CreateQuadMesh(false, false), null, ParticleShader
			, new Vector3(0, 0, 0), 0.1f);

			Matrices = new List<Matrix4>(particleAmount);
			Particles = new List<Particle>(particleAmount);
			ActiveParticles = 0;
			EmitRate = emitRate;
			emitCounter = 0.0f;
			LifeTime = lifeTime;
			ParticleSpeed = 1.0f;
			EmitDirection = new Vector3(0, 1, 0);
			ParticleSize = 1.0f;
			Transform = new TransformComponent();
			Transform.Position = worldPosition;

			Shape = shape;
			Width = sizes.X;
			Height = sizes.Y;
			Lenght = sizes.Z;

			randomGenerator = new Random();

			colors = new List<Vector4>(1);
			colors.Add(new Vector4(1, 0, 0, 1));
		}

		public void update()
		{
			double dt = 1.0 / DemoSettings.GetDefaults().UpdatesPerSecond;
			emitCounter += dt;

			float emitTarget = (1.0f / EmitRate);

			while (emitCounter >= emitTarget)
			{
				//emit

				if (ActiveParticles < maxParticles - 1)
				{
					ActiveParticles += 1;
					if (Matrices.Count < ActiveParticles)
					{
						Emit(1);
					}
				}
				else
				{
					// find first inactive
					for (int i = 0; i < Particles.Count; i++)
					{
						if (!Particles[i].isActive)
						{
							Restart(Particles[i]);
							break;
						}
					}
				}

				emitCounter -= emitTarget;
			}

			for (int i = 0; i < Particles.Count; i++)
			{
				Particle p = Particles[i];
				p.timeLeft -= (float)dt;
				if (p.timeLeft > 0)
				{
					Vector3 t = Matrices[p.matrixIndex].ExtractTranslation();
					t += p.direction * (float)(ParticleSpeed * dt);
					Matrices[p.matrixIndex] = Matrix4.CreateTranslation(t);
				}
				else
				{
					p.isActive = false;
				}
			}
		}

		public void Emit(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				Particle p = new Particle();
				Matrices.Add(Matrix4.CreateTranslation(Transform.Position));
				p.matrixIndex = Matrices.Count - 1;
				Restart(p);
				Particles.Add(p);
			}
		}

		public void Restart(Particle p)
		{
			Vector3 pos = Transform.Position;
			if (Shape == EmitterShape.Rectangle)
			{
				pos.X += GetRandomFromRange(Width);
				pos.Y += GetRandomFromRange(Height);
				pos.Z += GetRandomFromRange(Lenght);
			}
			Matrices[p.matrixIndex] = Matrix4.CreateTranslation(pos);

			float rx = EmitDirection.X + GetRandomFromRange(1.0f);
			float rz = EmitDirection.Z + GetRandomFromRange(1.0f);
			p.direction = new Vector3(rx, EmitDirection.Y, rz);
			p.direction.Normalize();
			int colorIndex = randomGenerator.Next(colors.Count - 1);
			p.color = colors[colorIndex];
			p.timeLeft = LifeTime;
			p.isActive = true;
		}

		public float GetRandomFromRange(float Max)
		{
			float rangeStart = -(Max / 2.0f);
			return rangeStart + (float)randomGenerator.NextDouble() * Max;
		}

		

		public void Draw(CameraComponent camera)
		{
			// Draw particles, but how?

			ShaderUniformManager uniMan = ShaderUniformManager.GetSingleton();
			List<Matrix4> mat = Matrices;
			DrawableMesh particleMesh = ParticleMesh;
			
			for (int p = 0; p < Particles.Count; p++)
			{
				ParticleEmitter.Particle particle = Particles[p];
				if (particle.isActive)
				{
					particleMesh.Transform.Position = mat[particle.matrixIndex].ExtractTranslation();
					particleMesh.Transform.SetRotationMatrix(camera.GetRotationMatrix());

					particleMesh.draw();
				}
			}
		}
	}
}