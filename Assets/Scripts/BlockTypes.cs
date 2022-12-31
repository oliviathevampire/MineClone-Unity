using System.Collections.Generic;
using System.Linq;

public static class BlockTypes {
	//SOLID
	private const byte BEDROCK = 0;
	private const byte GRASS = 1;
	private const byte DIRT = 2;
	private const byte STONE = 3;
	private const byte COBBLESTONE = 4;
	private const byte COAL_ORE = 5;
	private const byte IRON_ORE = 6;
	private const byte GOLD_ORE = 7;
	private const byte DIAMOND_ORE = 8;
	private const byte OAK_LOG = 9;
	private const byte OAK_PLANKS = 10;
	private const byte GLOWSTONE = 11;
	private const byte DIORITE = 12;
	private const byte GRANITE = 13;
	private const byte ANDESITE = 14;
	private const byte BIRCH_LOG = 15;
	private const byte BIRCH_PLANKS = 16;
	private const byte DEEPSLATE = 17;
	private const byte SAND = 18;
	private const byte SANDSTONE = 19;
	private const byte PODZOL = 20;
	private const byte DARK_OAK_LOG = 21;
	private const byte DARK_OAK_PLANKS = 22;
	private const byte SPRUCE_LOG = 23;
	private const byte SPRUCE_PLANKS = 24;
	private const byte ACACIA_LOG = 25;
	private const byte ACACIA_PLANKS = 26;
	private const byte CALCITE = 27;
	private const byte TUFF = 28;
	private const byte TERRACOTTA = 29;
	private const byte SNOW = 30;

	//TRANSPARENT
	private const byte OAK_LEAVES = 128;
	private const byte BIRCH_LEAVES = 129;
	private const byte DARK_OAK_LEAVES = 130;
	private const byte SPRUCE_LEAVES = 131;
	private const byte ACACIA_LEAVES = 132;
	private const byte AIR = 255;

	public static Dictionary<byte, Blocks.Block> byteToBlock;
	
	public static void Initialize() {
		Registry.RegisterBlock(Blocks.Bedrock, BEDROCK);
		Registry.RegisterBlock(Blocks.Grass, GRASS);
		Registry.RegisterBlock(Blocks.Dirt, DIRT);
		Registry.RegisterBlock(Blocks.Stone, STONE);
		Registry.RegisterBlock(Blocks.Cobblestone, COBBLESTONE);
		Registry.RegisterBlock(Blocks.CoalOre, COAL_ORE);
		Registry.RegisterBlock(Blocks.IronOre, IRON_ORE);
		Registry.RegisterBlock(Blocks.GoldOre, GOLD_ORE);
		Registry.RegisterBlock(Blocks.DiamondOre, DIAMOND_ORE);
		Registry.RegisterBlock(Blocks.OakLog, OAK_LOG);
		Registry.RegisterBlock(Blocks.OakPlanks, OAK_PLANKS);
		Registry.RegisterBlock(Blocks.Glowstone, GLOWSTONE);
		Registry.RegisterBlock(Blocks.Diorite, DIORITE);
		Registry.RegisterBlock(Blocks.Granite, GRANITE);
		Registry.RegisterBlock(Blocks.Andesite, ANDESITE);
		Registry.RegisterBlock(Blocks.BirchLog, BIRCH_LOG);
		Registry.RegisterBlock(Blocks.BirchPlanks, BIRCH_PLANKS);
		Registry.RegisterBlock(Blocks.Deepslate, DEEPSLATE);
		Registry.RegisterBlock(Blocks.Sand, SAND);
		Registry.RegisterBlock(Blocks.Sandstone, SANDSTONE);
		Registry.RegisterBlock(Blocks.Podzol, PODZOL);
		Registry.RegisterBlock(Blocks.DarkOakLog, DARK_OAK_LOG);
		Registry.RegisterBlock(Blocks.DarkOakPlanks, DARK_OAK_PLANKS);
		Registry.RegisterBlock(Blocks.SpruceLog, SPRUCE_LOG);
		Registry.RegisterBlock(Blocks.SprucePlanks, SPRUCE_PLANKS);
		Registry.RegisterBlock(Blocks.AcaciaLog, ACACIA_LOG);
		Registry.RegisterBlock(Blocks.AcaciaPlanks, ACACIA_PLANKS);
		Registry.RegisterBlock(Blocks.Calcite, CALCITE);
		Registry.RegisterBlock(Blocks.Tuff, TUFF);
		Registry.RegisterBlock(Blocks.Terracotta, TERRACOTTA);
		Registry.RegisterBlock(Blocks.Snow, SNOW);
		Registry.RegisterBlock(Blocks.OakLeaves, OAK_LEAVES);
		Registry.RegisterBlock(Blocks.BirchLeaves, BIRCH_LEAVES);
		Registry.RegisterBlock(Blocks.DarkOakLeaves, DARK_OAK_LEAVES);
		Registry.RegisterBlock(Blocks.SpruceLeaves, SPRUCE_LEAVES);
		Registry.RegisterBlock(Blocks.AcaciaLeaves, ACACIA_LEAVES);
		Registry.RegisterBlock(Blocks.Air, AIR);
		
		byteToBlock = new Dictionary<byte, Blocks.Block>();
		foreach (var blocksKey in Registry.Blocks.Keys) {
			var blockByte = Registry.GetByteValue(blocksKey);
			byteToBlock.Add(blockByte, blocksKey);
		}
	}

}