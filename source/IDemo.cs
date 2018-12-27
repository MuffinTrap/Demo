
namespace MuffinSpace
{
	// Demo interface
	interface IDemo
	{
		void Load(IAudioSystem audioSystem, AssetManager assetManager);
		void Start();
		void Sync();
		void Draw();
		void CleanAndExit();
	}

}