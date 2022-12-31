using System;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class Structure {
	public enum Type {
		OakTree,
		BirchTree,
		Well,
		CaveEntrance
	}

	private static Dictionary<Type, List<Change>> _templates;
	//private static byte[,,] caveMap;

	public struct Change {
		public Change(int x, int y, int z, Blocks.Block b) {
			this.x = x;
			this.y = y;
			this.z = z;
			this.b = b;
		}

		public readonly int x, y, z;
		public readonly Blocks.Block b;
	}

	public static void Initialize() {
		_templates = new Dictionary<Type, List<Change>>();
		foreach (Type type in Enum.GetValues(typeof(Type))) {
			GenerateTemplate(type);
		}
	}

	public static bool OverwritesEverything(Type type) {
		return type switch {
				   Type.OakTree => false,
				   Type.BirchTree => false,
				   Type.Well => true,
				   Type.CaveEntrance => true,
				   _ => false
			   };
	}


	public static List<Change> Generate(Type type, int seed) {
		var rnd = new System.Random(seed);
		var template = _templates[type];
		var result = template;
		switch (type) {
			case Type.OakTree:
				result = new List<Change> { new(0, -1, 0, Blocks.Dirt) }; // new list because there are variants
				var cutOff = rnd.Next(100) == 0;
				if (cutOff) {
					result.Add(new Change(0, 0, 0, Blocks.OakLog));
					return result;
				}

				int height = (byte)rnd.Next(4, 7);
				var superHigh = rnd.Next(100) == 0;
				if (superHigh) height = 10;

				for (int i = 0; i < height; ++i) {
					result.Add(new Change(0, i, 0, Blocks.OakLog));
				}

				result.Add(new Change(0, height, 0, Blocks.OakLeaves));

				for (var i = 0; i < 4; ++i) {
					result.Add(new Change(1, height - i, 0, Blocks.OakLeaves));
					result.Add(new Change(0, height - i, 1, Blocks.OakLeaves));
					result.Add(new Change(-1, height - i, 0, Blocks.OakLeaves));
					result.Add(new Change(0, height - i, -1, Blocks.OakLeaves));
				}


				if (rnd.Next(0, 2) == 0) result.Add(new Change(1, height - 1, 1, Blocks.OakLeaves));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(-1, height - 1, 1, Blocks.OakLeaves));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(1, height - 1, -1, Blocks.OakLeaves));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(-1, height - 1, -1, Blocks.OakLeaves));


				for (var i = 2; i < 4; ++i) {
					result.Add(new Change(2, height - i, -1, Blocks.OakLeaves));
					result.Add(new Change(2, height - i, 0, Blocks.OakLeaves));
					result.Add(new Change(2, height - i, 1, Blocks.OakLeaves));

					result.Add(new Change(-2, height - i, -1, Blocks.OakLeaves));
					result.Add(new Change(-2, height - i, 0, Blocks.OakLeaves));
					result.Add(new Change(-2, height - i, 1, Blocks.OakLeaves));

					result.Add(new Change(-1, height - i, 2, Blocks.OakLeaves));
					result.Add(new Change(0, height - i, 2, Blocks.OakLeaves));
					result.Add(new Change(1, height - i, 2, Blocks.OakLeaves));

					result.Add(new Change(-1, height - i, -2, Blocks.OakLeaves));
					result.Add(new Change(0, height - i, -2, Blocks.OakLeaves));
					result.Add(new Change(1, height - i, -2, Blocks.OakLeaves));

					result.Add(new Change(1, height - i, 1, Blocks.OakLeaves));
					result.Add(new Change(-1, height - i, 1, Blocks.OakLeaves));
					result.Add(new Change(1, height - i, -1, Blocks.OakLeaves));
					result.Add(new Change(-1, height - i, -1, Blocks.OakLeaves));

					if (rnd.Next(0, 2) == 0) result.Add(new Change(2, height - i, 2, Blocks.OakLeaves));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(-2, height - i, 2, Blocks.OakLeaves));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(2, height - i, -2, Blocks.OakLeaves));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(-2, height - i, -2, Blocks.OakLeaves));
				}

				break;
			case Type.BirchTree:
				result = new List<Change> { new(0, -1, 0, Blocks.Dirt) }; // new list because there are variants
				var cutOff2 = rnd.Next(100) == 0;
				if (cutOff2) {
					result.Add(new Change(0, 0, 0, Blocks.BirchLog));
					return result;
				}

				int height2 = (byte)rnd.Next(4, 7);
				var superHigh2 = rnd.Next(100) == 0;
				if (superHigh2) height2 = 10;

				for (var i = 0; i < height2; ++i) {
					result.Add(new Change(0, i, 0, Blocks.BirchLog));
				}

				result.Add(new Change(0, height2, 0, Blocks.BirchLeaves));

				for (var i = 0; i < 4; ++i) {
					result.Add(new Change(1, height2 - i, 0, Blocks.BirchLeaves));
					result.Add(new Change(0, height2 - i, 1, Blocks.BirchLeaves));
					result.Add(new Change(-1, height2 - i, 0, Blocks.BirchLeaves));
					result.Add(new Change(0, height2 - i, -1, Blocks.BirchLeaves));
				}


				if (rnd.Next(0, 2) == 0) result.Add(new Change(1, height2 - 1, 1, Blocks.BirchLeaves));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(-1, height2 - 1, 1, Blocks.BirchLeaves));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(1, height2 - 1, -1, Blocks.BirchLeaves));
				if (rnd.Next(0, 2) == 0) result.Add(new Change(-1, height2 - 1, -1, Blocks.BirchLeaves));


				for (var i = 2; i < 4; ++i) {
					result.Add(new Change(2, height2 - i, -1, Blocks.BirchLeaves));
					result.Add(new Change(2, height2 - i, 0, Blocks.BirchLeaves));
					result.Add(new Change(2, height2 - i, 1, Blocks.BirchLeaves));

					result.Add(new Change(-2, height2 - i, -1, Blocks.BirchLeaves));
					result.Add(new Change(-2, height2 - i, 0, Blocks.BirchLeaves));
					result.Add(new Change(-2, height2 - i, 1, Blocks.BirchLeaves));

					result.Add(new Change(-1, height2 - i, 2, Blocks.BirchLeaves));
					result.Add(new Change(0, height2 - i, 2, Blocks.BirchLeaves));
					result.Add(new Change(1, height2 - i, 2, Blocks.BirchLeaves));

					result.Add(new Change(-1, height2 - i, -2, Blocks.BirchLeaves));
					result.Add(new Change(0, height2 - i, -2, Blocks.BirchLeaves));
					result.Add(new Change(1, height2 - i, -2, Blocks.BirchLeaves));

					result.Add(new Change(1, height2 - i, 1, Blocks.BirchLeaves));
					result.Add(new Change(-1, height2 - i, 1, Blocks.BirchLeaves));
					result.Add(new Change(1, height2 - i, -1, Blocks.BirchLeaves));
					result.Add(new Change(-1, height2 - i, -1, Blocks.BirchLeaves));

					if (rnd.Next(0, 2) == 0) result.Add(new Change(2, height2 - i, 2, Blocks.BirchLeaves));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(-2, height2 - i, 2, Blocks.BirchLeaves));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(2, height2 - i, -2, Blocks.BirchLeaves));
					if (rnd.Next(0, 2) == 0) result.Add(new Change(-2, height2 - i, -2, Blocks.BirchLeaves));
				}

				break;
			case Type.Well:
				//no variants
				break;
			case Type.CaveEntrance:
				//byte[,,] map = new byte[16, 48, 16];
				result = new List<Change>();
				var caveMap = new byte[16, 48, 16];
				if (caveMap == null) throw new ArgumentNullException(nameof(caveMap));
				//rnd = new System.Random(rnd.Next());
				var path = new Queue<Vector3Int>();
				var depth = rnd.Next(5, 11);
				for (var i = 0; i < depth; i++) {
					path.Enqueue(new Vector3Int(
									 rnd.Next(2, 13),
									 44 - (i * 4),
									 rnd.Next(2, 13)
								 ));
				}

				var nextPos = path.Dequeue();
				float d = 0;
				while (path.Count > 0) {
					var currentPos = nextPos;
					nextPos = path.Dequeue();
					var size = Mathf.Lerp(2, 0.75f, d / depth);

					for (var i = 0; i < 16; ++i) {
						var lerpPos = i / 15f;
						var lerped = Vector3.Lerp(currentPos, nextPos, lerpPos);
						var p = new Vector3Int((int)lerped.x, (int)lerped.y, (int)lerped.z);
						for (var z = -2; z < 3; ++z) {
							for (var y = -2; y < 3; ++y) {
								for (var x = -2; x < 3; ++x) {
									var b = new Vector3Int(p.x + x, p.y + y, p.z + z);
									if (Vector3Int.Distance(p, b) > size) continue;
									if (b.x is < 0 or > 15) continue;
									if (b.y is < 0 or > 47) continue;
									if (b.z is < 0 or > 15) continue;

									caveMap[b.x, b.y, b.z] = 1;
								}
							}
						}
					}

					d++;
				}

				for (var z = 0; z < 16; ++z) {
					for (var y = 0; y < 48; ++y) {
						for (var x = 0; x < 16; ++x) {
							if (caveMap[x, y, z] == 1) {
								result.Add(new Change(x, y - 48, z, Blocks.Air));
							}
						}
					}
				}

				//Debug.Log("Cave size: " + result.Count);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}

		return result;
	}

	private static void GenerateTemplate(Type type) {
		Debug.Log("Generating structure type: " + type);
		var result = new List<Change>();
		switch (type) {
			case Type.OakTree:
				//no template
				break;
			case Type.BirchTree:
				break;
			case Type.Well:
				for (var z = -2; z < 4; ++z) {
					for (var x = -2; x < 4; ++x) {
						result.Add(new Change(x, -1, z, Blocks.Cobblestone));
					}
				}

				for (int z = -1; z < 3; ++z) {
					for (int x = -1; x < 3; ++x) {
						result.Add(new Change(x, 0, z, Blocks.Cobblestone));
					}
				}

				for (int z = -1; z < 3; ++z) {
					for (int x = -1; x < 3; ++x) {
						result.Add(new Change(x, 3, z, Blocks.OakPlanks));
					}
				}

				result.Add(new Change(-1, 1, -1, Blocks.OakPlanks));
				result.Add(new Change(2, 1, -1, Blocks.OakPlanks));
				result.Add(new Change(-1, 1, 2, Blocks.OakPlanks));
				result.Add(new Change(2, 1, 2, Blocks.OakPlanks));
				result.Add(new Change(-1, 2, -1, Blocks.OakPlanks));
				result.Add(new Change(2, 2, -1, Blocks.OakPlanks));
				result.Add(new Change(-1, 2, 2, Blocks.OakPlanks));
				result.Add(new Change(2, 2, 2, Blocks.OakPlanks));

				for (int i = 0; i < 16; ++i) {
					result.Add(new Change(0, -i, 0, Blocks.Air));
					result.Add(new Change(1, -i, 0, Blocks.Air));
					result.Add(new Change(0, -i, 1, Blocks.Air));
					result.Add(new Change(1, -i, 1, Blocks.Air));
				}

				break;
			case Type.CaveEntrance:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}

		_templates.Add(type, result);
		//Debug.Log("added to template " + type);
	}

	/// <summary>
	/// Creates the tree canopy...a ball of leaves.
	/// </summary>
	/// <param name="result"></param>
	/// <param name="blockX"></param>
	/// <param name="blockY"></param>
	/// <param name="blockZ"></param>
	/// <param name="radius"></param>
	/// <param name="block"></param>
	private static void CreateSphereAt(List<Change> result, int blockX, int blockY, int blockZ, int radius, Blocks.Block block) {
		for (var x = blockX - radius; x <= blockX + radius; x++) {
			for (var y = blockY - radius; y <= blockY + radius; y++) {
				for (var z = blockZ - radius; z <= blockZ + radius; z++) {
					if (Vector3.Distance(new Vector3(blockX, blockY, blockZ), new Vector3(x, y, z)) <= radius) {
						result.Add(new Change(x, y, z, block));
					}
				}
			}
		}
	}

	private static void CreateColumnAt([NotNull] List<Change> result, int blockX, int blockY, int blockZ, int columnLength,
									   Blocks.Block blockType) {
		if (result == null) throw new ArgumentNullException(nameof(result));
		// Trunk
		for (var z = blockZ + 1; z <= blockZ + columnLength; z++) {
			CreateColumnAt(result, blockX, blockY, z, blockType);
		}
	}

	private static void CreateColumnAt(ICollection<Change> result, int blockX, int blockY, int z, Blocks.Block blockType) {
		result.Add(new Change(blockX, blockY, z, blockType));
	}

	private static void CreateDiskAt(List<Change> result, int blockX, int blockY, int blockZ, int radius, Blocks.Block blockType) {
		for (var x = blockX - radius; x <= blockX + radius; x++) {
			for (var y = blockY - radius; y <= blockY + radius; y++) {
				if (Vector3.Distance(new Vector3(blockX, blockY, blockZ), new Vector3(x, y, blockZ)) <= radius) {
					result.Add(new Change(x, y, blockZ, blockType));
				}
			}
		}
	}

	/// <summary>
	/// Creates the tree canopy...a ball of leaves.
	/// </summary>
	/// <param name="result"></param>
	/// <param name="blockX"></param>
	/// <param name="blockY"></param>
	/// <param name="blockZ"></param>
	/// <param name="radius"></param>
	/// <param name="blockType"></param>
	private static void CreateCrossAt(ICollection<Change> result, int blockX, int blockY, int blockZ, int radius, Blocks.Block blockType) {
		var raised = 0;
		var x = 0;
		var y = 0;
		var z = 0;
		if (radius > 2) {
			raised = 1;
		}

		result.Add(new Change(blockX + x, blockY + z, blockZ + y, blockType));
		for (var i = 0; i < radius; i++) {
			var j = i + 1;
			y = -i + raised;
			x = j;
			z = j;
			result.Add(new Change(blockX + x, blockY + z, blockZ + y, blockType));
			x = -j;
			z = -j;
			result.Add(new Change(blockX + x, blockY + z, blockZ + y, blockType));
			x = j;
			z = -j;
			result.Add(new Change(blockX + x, blockY + z, blockZ + y, blockType));
			x = -j;
			z = j;
			result.Add(new Change(blockX + x, blockY + z, blockZ + y, blockType));
		}
	}
}