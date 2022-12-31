using UnityEngine.Serialization;

[System.Serializable]
public class GameSettings
{
	[FormerlySerializedAs("RenderDistance")] public int renderDistance=1;
	public int maximumLoadQueueSize=8;
	public float minimumLightLevel = 0.1f;
}
