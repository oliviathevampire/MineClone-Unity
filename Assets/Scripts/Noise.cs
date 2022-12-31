using System;
using UnityEngine;
using Random = System.Random;

public static class Noise {
	public static float Get2DPerlin(WorldInfo worldInfo, Vector2 position, float offset, float scale) {
		FastNoiseLite fastNoise = new FastNoiseLite(worldInfo.seed);
		fastNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		fastNoise.SetFrequency(0.005f);
		fastNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		fastNoise.SetFractalOctaves(7);
		fastNoise.SetFractalLacunarity(2f);
		fastNoise.SetFractalGain(0.5f);
		fastNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
		fastNoise.SetDomainWarpAmp(10f);
		
		position.x += offset + worldInfo.seed + 0.1f;
		position.y += offset + worldInfo.seed + 0.1f;

		return fastNoise.GetNoise(position.x / 16 * scale, position.y / 16 * scale);
	}

	public static bool Get3DPerlin(WorldInfo worldInfo, Vector3 position, float offset, float scale, float threshold) {
		// https://www.youtube.com/watch?v=Aga0TBJkchM Carpilot on YouTube
		FastNoiseLite fastNoise = new FastNoiseLite(worldInfo.seed);
		fastNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		fastNoise.SetRotationType3D(FastNoiseLite.RotationType3D.ImproveXZPlanes);
		fastNoise.SetFrequency(0.010f);
		fastNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		fastNoise.SetFractalOctaves(5);
		fastNoise.SetFractalLacunarity(2f);
		fastNoise.SetFractalGain(0.5f);

		var x = (position.x + offset + new Random().Next() + 0.1f) * scale;
		var y = (position.y + offset + new Random().Next() + 0.1f) * scale;
		var z = (position.z + offset + new Random().Next() + 0.1f) * scale;

		var ab = fastNoise.GetNoise(x, y);
		var bc = fastNoise.GetNoise(y, z);
		var ac = fastNoise.GetNoise(x, z);
		var ba = fastNoise.GetNoise(y, x);
		var cb = fastNoise.GetNoise(z, y);
		var ca = fastNoise.GetNoise(z, x);

		return (ab + bc + ac + ba + cb + ca) / 6f > threshold;
	}
}