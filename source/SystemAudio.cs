using System.Media;
using System.Collections.Generic;

namespace MuffinSpace
{
	public class SystemAudio : IAudioSystem
	{
		private List<SoundPlayer> loadedAudios;

		public bool Initialize()
		{
			loadedAudios = new List<SoundPlayer>();
			return true;
		}

		public Audio LoadAudioFile(string filenameAndPath)
		{
			SoundPlayer player = new SoundPlayer();
			player.SoundLocation = filenameAndPath;
			player.Load();

			Audio newAudio = new Audio();
			newAudio.debugName = filenameAndPath;
			newAudio.id = loadedAudios.Count;
			loadedAudios.Add(player);
			Logger.LogInfo("Loaded audio from " + filenameAndPath);
			return newAudio;
		}

		public void PlayAudio(Audio audioFile)
		{
			loadedAudios[audioFile.id].Play();
		}
		public void StopAudio(Audio audioFile)
		{
			loadedAudios[audioFile.id].Stop();
		}
		public void RestartAudio(Audio audioFile)
		{
			StopAudio(audioFile);
			PlayAudio(audioFile);
		}

		public void CleanAndExit()
		{
			for(int i = 0; i < loadedAudios.Count; i++)
			{
				loadedAudios[i].Stop();
				loadedAudios[i].Dispose();
			}
		}
	}
}