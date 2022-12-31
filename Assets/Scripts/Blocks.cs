using VampireStudios;

public abstract class Blocks {
	public static readonly Block Bedrock = new();
	public static readonly Block Grass = new(Materials.GRASS);
	public static readonly Block Dirt = new(Materials.ORGANIC);
	public static readonly Block Stone = new();
	public static readonly Block Cobblestone = new();
	public static readonly Block CoalOre = new();
	public static readonly Block IronOre = new();
	public static readonly Block GoldOre = new();
	public static readonly Block DiamondOre = new();
	public static readonly Block OakLog = new(Materials.WOOD);
	public static readonly Block OakPlanks = new(Materials.WOOD);
	public static readonly Block OakLeaves = new TranslucentBlock(Materials.LEAVES);
	public static readonly Block BirchLog = new(Materials.WOOD);
	public static readonly Block BirchPlanks = new(Materials.WOOD);
	public static readonly Block BirchLeaves = new TranslucentBlock(Materials.LEAVES);
	public static readonly Block DarkOakLog = new(Materials.WOOD);
	public static readonly Block DarkOakPlanks = new(Materials.WOOD);
	public static readonly Block DarkOakLeaves = new TranslucentBlock(Materials.LEAVES);
	public static readonly Block SpruceLog = new(Materials.WOOD);
	public static readonly Block SprucePlanks = new(Materials.WOOD);
	public static readonly Block SpruceLeaves = new TranslucentBlock(Materials.LEAVES);
	public static readonly Block AcaciaLog = new(Materials.WOOD);
	public static readonly Block AcaciaPlanks = new(Materials.WOOD);
	public static readonly Block AcaciaLeaves = new TranslucentBlock(Materials.LEAVES);
	public static readonly Block Glowstone = new(19);
	public static readonly Block Diorite = new();
	public static readonly Block Granite = new();
	public static readonly Block Andesite = new();
	public static readonly Block Sand = new(Materials.ORGANIC);
	public static readonly Block Sandstone = new();
	public static readonly Block Podzol = new(Materials.GRASS);
	public static readonly Block Deepslate = new();
	public static readonly Block Air = new TranslucentBlock(Materials.AIR);
	public static readonly Block Terracotta = new();
	public static readonly Block Calcite = new();
	public static readonly Block Tuff = new();
	public static readonly Block Snow = new(Materials.GRASS);

	public class Block {
		private readonly int _lightLevel;
		private protected Material material;
		private protected int hardness, strength;

		public Block() {
			material = Materials.STONE;
			_lightLevel = 0;
		}
	
		public Block(Material material) {
			this.material = material;
			_lightLevel = 0;
		}

		public Block(int lightLevel) {
			material = Materials.STONE;
			_lightLevel = lightLevel;
		}

		public Material GetMaterial() {
			return material;
		}

		public int GetLightLevel() {
			return _lightLevel;
		}
	}

	public class TranslucentBlock : Block {
		public TranslucentBlock(Material material) {
			this.material = material;
		}
	}

}