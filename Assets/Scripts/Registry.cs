using System.Collections.Generic;

public static class Registry {
	public static readonly Dictionary<Blocks.Block, byte> Blocks = new();
	
	public static void RegisterBlock(Blocks.Block blockTypes, byte @byte) {
		Blocks.Add(blockTypes, @byte);
	}

	public static byte GetByteValue(Blocks.Block block) {
		return Blocks[block];
	}
}