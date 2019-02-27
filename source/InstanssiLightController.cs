
using System.Net;
using System.Net.Sockets;
using OpenTK;
using System.Threading;

using System.Collections.Generic;
using System;
using System.Text;

namespace MuffinSpace
{
	public class InstanssiLightController
	{
		private Thread udpThread;
		private Socket sendSocket;


		private IPEndPoint endPoint;

		private class LightStatus
		{
			public int lightNumber;
			public Vector3 color;
			public bool needsUpdate;
			public LightStatus(int number)
			{
				lightNumber = number;
				color = new Vector3(0, 0, 0);
				needsUpdate = false;
			}
			public void SetColor(Vector3 colorN)
			{
				color = colorN;
				needsUpdate = true;
			}
			public void MarkAsDone()
			{
				needsUpdate = false;
			}
		}

		List<LightStatus> status;
		int intervalMs = 16;
		bool syncThreadActive = false;
		public bool syncing = false;

		private Mutex lightMutex;

		public InstanssiLightController(string address, string hostName, int port, int lightsToSync, int physicalLights, int syncIntervalMs)
		{
			sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
			
			ProtocolType.Udp);

			IPAddress serverAddress = IPAddress.Parse(address);
			if (hostName != "" && hostName.Length > 1)
			{
				IPHostEntry hostEntry;
				hostEntry = Dns.GetHostEntry(hostName);
				//you might get more than one ip for a hostname since 
				//DNS supports more than one record
				if (hostEntry.AddressList.Length > 0)
				{
					serverAddress = hostEntry.AddressList[0];
				}
			}

			endPoint = new IPEndPoint(serverAddress, port);

			status = new List<LightStatus>();
			// Map light 0 to 0, last light to last id
			float step = (float)(physicalLights - 1) / (lightsToSync - 1);
			for (int lm = 0; lm < lightsToSync; lm++)
			{
				int physicalId = (int)(step * lm);
				Logger.LogInfo("Light to Physical light :" + lm + " to " + physicalId);
				status.Add(new LightStatus(physicalId));
			}

			lightMutex = new Mutex();
			intervalMs = syncIntervalMs;
		}

		public void StartSync()
		{
			syncThreadActive = true;
			ThreadStart LightSyncLoop = new ThreadStart(this.LightSyncLoop);
			udpThread = new Thread(LightSyncLoop);
			udpThread.Start();
			Thread.Sleep(10); // For thread logging
		}

		public void EndSync()
		{
			syncThreadActive = false;
		}

		public void LightSyncLoop()
		{
			Logger.LogPhase("Light syncing thread started");
			List<byte> packet = new List<byte>();
			bool sendPacket = false;

			while (syncThreadActive)
			{
				if (syncing)
				{
					lightMutex.WaitOne();
					sendPacket = CreateLightPacket(ref packet);
					lightMutex.ReleaseMutex();

					if (sendPacket)
					{
						sendSocket.SendTo(packet.ToArray(), endPoint);
					}
				}

				Thread.Sleep(intervalMs);
			}
		}

		public void SyncLightList(List<LightMesh> lights)
		{
			lightMutex.WaitOne();
			for (int lm = 0; lm < lights.Count; lm++)
			{
				Vector3 effectiveColor = lights[lm].getEffectiveColor();
				if (!IsSameColor(effectiveColor, status[lm].color))
				{
					status[lm].SetColor(effectiveColor);
				}
			}
			lightMutex.ReleaseMutex();
		}

		private bool IsSameColor(Vector3 a, Vector3 be)
		{
			return a == be;
		}

		private bool CreateLightPacket(ref List<byte> packet)
		{
			bool needSync = false;
			packet.Clear();
			packet.Add(1);	// Spec version

			// Not really needed
			packet.Add(0);  // Nickname
			packet.AddRange(Encoding.ASCII.GetBytes("muffintrap"));
			packet.Add(0); // END Nickname

			for (int i = 0; i < status.Count; i++)
			{
				if (status[i].needsUpdate)
				{
					packet.Add(1);	// Light effect
					packet.Add((byte)status[i].lightNumber);	// Light index
					packet.Add(0);  // This is an RGB light
					byte r = (byte)Math.Floor(status[i].color.X * 255);
					byte g = (byte)Math.Floor(status[i].color.Y * 255);
					byte b = (byte)Math.Floor(status[i].color.Z * 255);

					packet.Add(r);
					packet.Add(g);
					packet.Add(b);
					status[i].MarkAsDone();

					needSync = true;
				}
			}

			return needSync;
		}
	}
}