using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkSaveData {
	public Vector2Int position;
	public List<C> changes;

	public ChunkSaveData(Vector2Int position) {
		this.position = position;
		changes = new List<C>();
	}

	[System.Serializable]
	public struct C //Change
	{
		public C(int x, int y, int z, Blocks.Block b) {
			this.x = (byte)x;
			this.y = (byte)y;
			this.z = (byte)z;
			this.b = Registry.GetByteValue(b);
		}

		public C(int x, int y, int z, byte b) {
			this.x = (byte)x;
			this.y = (byte)y;
			this.z = (byte)z;
			this.b = b;
		}

		public byte x, y, z, b;
	}
}