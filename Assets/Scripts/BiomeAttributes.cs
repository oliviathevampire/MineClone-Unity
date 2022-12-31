using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject {

	[Header("Major Flora")]
	public string biomeName;
	public int offset;
	public float scale;
	public float minNoiseValue = 0f;
	public float maxNoiseValue = 1f;

	public int terrainHeight;
	public float terrainScale;

	public byte surfaceBlock;
	public byte subSurfaceBlock;
	public Blocks.Block newSurfaceBlock;
	public Blocks.Block newSubSurfaceBlock;

	[Header("Major Flora")]
	public int majorFloraIndex;
	public float majorFloraZoneScale = 1.3f;
	[Range(0.1f, 1f)]
	public float majorFloraZoneThreshold = 0.6f;
	public float majorFloraPlacementScale = 15f;
	[Range(0.1f, 1f)]
	public float majorFloraPlacementThreshold = 0.8f;
	public bool placeMajorFlora = true;

	public int maxHeight = 12;
	public int minHeight = 5;

	public Lode[] lodes;

}

[System.Serializable]
public class Lode {
	public string nodeName;
	public byte blockID;
	public Blocks.Block block;
	public int minHeight;
	public int maxHeight;
	public float scale;
	public float threshold;
	public float noiseOffset;
}