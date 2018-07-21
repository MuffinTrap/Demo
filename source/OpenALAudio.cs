using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;

namespace OpenTkConsole
{
	class OpenALAudio : IAudioSystem
	{
		public bool Initialize()
		{
			IntPtr nullDevice = System.IntPtr.Zero;
			IList<string> allDevices = Alc.GetString(nullDevice, AlcGetStringList.DeviceSpecifier);
			foreach (string s in allDevices)
			{
				Logger.LogInfo("OpenAL device " + s);
			}

			// Open preferred device
			ContextHandle alContext;
			IntPtr ALDevicePtr = Alc.OpenDevice(null);
			if (ALDevicePtr != null)
			{
				int[] deviceAttributes = null;
				alContext = Alc.CreateContext(ALDevicePtr, deviceAttributes);
				Alc.MakeContextCurrent(alContext);
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Critical, "Could not get AL device");
				return false;
			}

			string alRenderer = AL.Get(ALGetString.Renderer);
			string alVendor = AL.Get(ALGetString.Vendor);
			string alVersion = AL.Get(ALGetString.Version);

			Logger.LogInfo(string.Format("OpenAL Renderer {0}  Vendor {1}  Version {2}", alRenderer, alVendor, alVersion));


			Error.checkALError("initAudio");
			return true;
		}

		public Audio LoadAudioFile(string filename)
		{
			// Init complete

			int alBuffer = AL.GenBuffer();
			Error.checkALError("initAudio genBuffer");


			int frequenzy = 44100;

			// Buffer data
			bool dataisVorbis = false;

			string vorbisEXTName = "AL_EXT_vorbis";
			if (AL.IsExtensionPresent(vorbisEXTName) && dataisVorbis)
			{
				Logger.LogInfo("AL can use vorbis");
				IntPtr vorbisBuffer = System.IntPtr.Zero;
				int vorbisSize = 0;
				AL.BufferData(alBuffer, ALFormat.VorbisExt, vorbisBuffer, vorbisSize, frequenzy);
			}
			else
			{
				// Load wav

				FileStream audioFile = File.Open(filename, FileMode.Open, FileAccess.Read);
				long wavSize = audioFile.Length;
				byte[] audioContents = new byte[wavSize];

				audioFile.Read(audioContents, 0, (int)wavSize);
				IntPtr wavBuffer = Marshal.AllocHGlobal(audioContents.Length);
				Marshal.Copy(audioContents, 0, wavBuffer, audioContents.Length);

				AL.BufferData(alBuffer, ALFormat.Stereo16, wavBuffer, (int)wavSize, frequenzy);
				Marshal.FreeHGlobal(wavBuffer);
				audioFile.Close();
			}

			Error.checkALError("initAudio bufferAudio");

			int alSource = AL.GenSource();
			Error.checkALError("initAudio genSource");

			// Attach buffer to source.
			AL.Source(alSource, ALSourcei.Buffer, alBuffer);

			Audio loadedAudio = new Audio();
			loadedAudio.id = alSource;
			loadedAudio.debugName = filename;
			return loadedAudio;
		}

		public void PlayAudioFile(Audio audioFile)
		{

			// Set listener and source to same place
			Vector3 listenerPos = new Vector3(0, 0, 0);
			Vector3 sourcePos = new Vector3(0, 0, 0);
			AL.Listener(ALListener3f.Position, ref listenerPos);
			AL.Source(audioFile.id, ALSource3f.Position, ref sourcePos);

			// Play buffer
			AL.SourcePlay(audioFile.id);
		}

		public void Shutdown()
		{
			ContextHandle alContext = Alc.GetCurrentContext();
			IntPtr alDevice = Alc.GetContextsDevice(alContext);
			ContextHandle emptyContext = ContextHandle.Zero;
			Alc.MakeContextCurrent(emptyContext);
			Alc.DestroyContext(alContext);
			Alc.CloseDevice(alDevice);
		}

	}
}