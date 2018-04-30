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
		private double lifeCounter;
		private int maxParticles;

		private Random randomGenerator;

		public struct Particle
		{
			public bool isActive;
			public int matrixIndex;
			public Vector3 direction;
			public float speed;
			public float timeLeft;
		}

		public ParticleEmitter(int particleAmount, float emitRate, Vector3 worldPosition)
		{
			maxParticles = particleAmount;
			// Load quad mesh
			AssetManager ass = AssetManager.GetAssetManagerSingleton();

			ParticleShader = ass.GetShaderProgram("particle");
			ParticleMesh = ass.GetMesh("particle", MeshDataGenerator.CreateQuadMesh(), null, ParticleShader
			, new Vector3(0, 0, 0), 0.1f);

			Matrices = new List<Matrix4>(particleAmount);
			Particles = new List<Particle>(particleAmount);
			ActiveParticles = 0;
			EmitRate = emitRate;
			emitCounter = 0.0f;
			LifeTime = 1.0f;
			ParticleSpeed = 1.0f;
			EmitDirection = new Vector3(0, 1, 0);
			ParticleSize = 1.0f;
			Transform = new TransformComponent();
			Transform.WorldPosition = worldPosition;

			randomGenerator = new Random();
		}

		public void update()
		{
			double dt = 1.0 / DemoSettings.GetDefaults().UpdatesPerSecond;
			emitCounter += dt;
			lifeCounter += dt;

			float emitTarget = (1.0f / EmitRate);

			while (emitCounter >= emitTarget)
			{
				//emit

				if (ActiveParticles < maxParticles - 1)
				{
					ActiveParticles += 1;
					if (Matrices.Count < ActiveParticles)
					{
						Matrix4 newMatrix = new Matrix4();
						newMatrix = Matrix4.CreateTranslation(Transform.WorldPosition);
						Matrices.Add(newMatrix);

						Particle p = new Particle();
						p.matrixIndex = Matrices.Count - 1;
						float rx = (float)(EmitDirection.X + (1.0 - 2.0 * randomGenerator.NextDouble()));
						float rz = (float)(EmitDirection.Z + (1.0 - 2.0 * randomGenerator.NextDouble()));
						p.direction = new Vector3(rx, EmitDirection.Y, rz);
						p.direction.Normalize();
						p.timeLeft = LifeTime;
						p.isActive = true;
						Particles.Add(p);
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
	}
}