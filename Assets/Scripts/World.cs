using System;
using UnityEngine;
using UnityEngine.Serialization;

public class World : MonoBehaviour {
	public static World activeWorld;
	public static FastNoiseLite noise;
	[FormerlySerializedAs("_info")] public WorldInfo info;
	public Camera mainCamera;
	public ChunkManager chunkManager;
	private bool _initialized;

	[Header("World Generation Values")] public BiomeAttributes[] biomes;

	public void Initialize(WorldInfo info) {
		Debug.Log("Creating world " + info);

		this.info = info;
		activeWorld = this;
		noise = new FastNoiseLite(info.seed);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		noise.SetFractalOctaves(3);
		chunkManager.Initialize(this);
		SimplexNoise.Noise.Seed = info.seed;
		GC.Collect();
		_initialized = true;
	}

	public void UpdateWorld() {
		if (!_initialized) return;
		GameManager.Instance.AddDebugLine($"World: id[{info.id}] seed[{info.seed}] name[{info.name}]");
		//update chunks if no modifications have happened this frame
		//only rebuild 1 chunk per frame to avoid framedrops
		chunkManager.UpdateChunks(mainCamera);
	}

	public bool Modify(int x, int y, int z, Blocks.Block blockType) {
		if (!_initialized) return false;
		if (y is < 0 or > 255) {
			Debug.LogWarning("This is outside build limit");
			return false;
		}

		var chunkX = Mathf.FloorToInt(x / 16f);
		var chunkY = Mathf.FloorToInt(z / 16f);
		var relativeX = x - chunkX * 16;
		var relativeZ = z - chunkY * 16;

		return chunkManager.Modify(new Vector2Int(chunkX, chunkY), relativeX, y, relativeZ, blockType);
	}

	public Blocks.Block GetBlock(int x, int y, int z) {
		if (!_initialized) return Blocks.Air;
		if (y is < 0 or > 255) {
			Debug.LogWarning("This is outside build limit");
			return Blocks.Air;
		}

		var chunkX = Mathf.FloorToInt(x / 16f);
		var chunkY = Mathf.FloorToInt(z / 16f);
		var relativeX = x - chunkX * 16;
		var relativeZ = z - chunkY * 16;
		return chunkManager.GetBlock(new Vector2Int(chunkX, chunkY), relativeX, y, relativeZ);
	}

	private static int GenerateSeed() {
		var tickCount = Environment.TickCount;
		var processId = System.Diagnostics.Process.GetCurrentProcess().Id;
		return new System.Random(tickCount + processId).Next();
	}
}