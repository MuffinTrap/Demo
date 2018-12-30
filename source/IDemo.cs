
namespace MuffinSpace
{
	// Demo interface
	interface IDemo
	{
		void Load(IAudioSystem audioSystem, AssetManager assetManager);
		void Sync(SyncSystem syncer);
		void Draw(SyncSystem syncer, Renderer renderer);
	}

}