using UnityEngine;

[CreateAssetMenu(fileName = "New BlockResources", menuName = "Minecraft/BlockResources", order = 0)]
public class BlockResources : ScriptableObject {
	public Texture[] textures;
	public Mesh[] meshes;
	public Shader shader; // Multiple shaders on one object is hard ):
}