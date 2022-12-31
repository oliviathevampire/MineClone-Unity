using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using Block = Blocks.Block;

public class ChunkData {
	public Vector2Int position;
	private Block[,,] _blocks;
	public bool TerrainReady { get; private set; }
	public bool StartedLoadingDetails { get; private set; }
	public bool ChunkReady { get; private set; }

	public bool isDirty;

	private const int SURFACE_HEIGHT = 64;

	//divides by chance ( 1 in X )
	private const int STRUCTURE_CHANCE_TREE = int.MaxValue / 100;
	private const int STRUCTURE_CHANCE_BIRCH_TREE = int.MaxValue / 150;
	private const int STRUCTURE_CHANCE_WELL = int.MaxValue / 255;
	private const int STRUCTURE_CHANCE_CAVE_ENTRANCE = int.MaxValue / 20;


	private Thread _loadTerrainThread;
	private Thread _loadDetailsThread;

	private readonly WorldInfo _worldInfo;
	private World world;

	public readonly HashSet<Vector2Int> references;

	private List<StructureInfo> _structures;

	public readonly Dictionary<Vector3Int, int> lightSources;

	public readonly int[,] highestNonAirBlock;

	private ChunkSaveData _saveData;

	private ChunkData front, left, back, right; //neighbours (only exist while loading structures)

	private volatile int _worldX, _worldZ;
	private static FastNoiseLite noise;

	private struct StructureInfo {
		public StructureInfo(Vector3Int position, Structure.Type type, int seed) {
			this.position = position;
			this.type = type;
			this.seed = seed;
		}

		public readonly Vector3Int position;
		public readonly Structure.Type type;
		public readonly int seed;
	}


	public ChunkData(Vector2Int position, World world) {
		this.position = position;
		_worldInfo = world.info;
		this.world = world;
		noise = World.noise;
		TerrainReady = false;
		StartedLoadingDetails = false;
		ChunkReady = false;
		isDirty = false;
		references = new HashSet<Vector2Int>();
		lightSources = new Dictionary<Vector3Int, int>();
		highestNonAirBlock = new int[VoxelData.ChunkWidth, VoxelData.ChunkWidth];
	}

	public Block[,,] GetBlocks() {
		//if (!chunkReady) throw new System.Exception($"Chunk {position} has not finished loading");
		return _blocks;
	}

	public void StartTerrainLoading() {
		//Debug.Log($"Chunk {position} start terrain loading");
		_loadTerrainThread = new Thread(LoadTerrain) {
														 IsBackground = true
													 };
		_loadTerrainThread.Start();
	}

	public void StartDetailsLoading(ChunkData front, ChunkData left, ChunkData back, ChunkData right) {
		//Debug.Log($"Chunk {position} start structure loading");
		//need to temporarily cache chunkdata of neighbors since generation is on another thread
		this.front = front;
		this.left = left;
		this.right = right;
		this.back = back;

		_loadDetailsThread = new Thread(LoadDetails) {
														 IsBackground = true
													 };
		_loadDetailsThread.Start();
		StartedLoadingDetails = true;
	}

	private void LoadTerrain() {
		//also loads structures INFO
		_blocks = new Block[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

		for (var z = 0; z < VoxelData.ChunkWidth; ++z) {
			for (var x = 0; x < VoxelData.ChunkWidth; ++x) {
				/*if (_worldInfo.type == WorldInfo.Type.Flat) {
					for (var y = 6; y < VoxelData.ChunkHeight; ++y) {
						_blocks[x, y, z] = BlockTypes.Air;
					}

					_blocks[x, 5, z] = BlockTypes.Grass;
					_blocks[x, 4, z] = BlockTypes.Dirt;
					_blocks[x, 3, z] = BlockTypes.Dirt;
					_blocks[x, 2, z] = BlockTypes.Stone;
					_blocks[x, 1, z] = BlockTypes.Stone;
					_blocks[x, 0, z] = BlockTypes.Bedrock;
					continue;
				}*/

				_worldX = position.x * VoxelData.ChunkWidth + x;
				_worldZ = position.y * VoxelData.ChunkWidth + z;
				var bottomHeight = 0;

				/* BIOME SELECTION PASS*/
				if (_worldInfo.type == WorldInfo.Type.FloatingIslands) {
					var distanceToSpawn = Vector2.Distance(new Vector2(_worldX, _worldZ), Vector2.zero);
					var bigIsland = Mathf.Clamp01((250f - distanceToSpawn) / 250f);
					var i1 = noise.GetNoise(_worldX * .5f, _worldZ * .5f);
					var i2 = noise.GetNoise(_worldX * 1f, _worldZ * 1f);
					var i3 = noise.GetNoise(_worldX * 5f, _worldZ * 5f);
					var height = Mathf.Min(i1, i2) + bigIsland + i3 * 0.02f;
					height = Mathf.Clamp01(height - 0.1f) / 0.9f;
					height = Mathf.Pow(height, 1f / 2);
					if (height == 0) {
						for (var y = 0; y < VoxelData.ChunkHeight; ++y) {
							_blocks[x, y, z] = Blocks.Air;
						}

						continue;
					}

					// hills *= height; //smooth edge
					bottomHeight = (int)(SURFACE_HEIGHT - height * 80);
				}

				for (var y = 0; y < VoxelData.ChunkHeight; ++y) {
					const int solidGroundHeight = 42;
					var sumOfHeights = 0f;
					var count = 0;
					var strongestWeight = 0f;
					var strongestBiomeIndex = 0;

					for (var i = 0; i < world.biomes.Length; i++) {
						var weight = Noise.Get2DPerlin(_worldInfo, new Vector2(_worldX, _worldZ),
													   world.biomes[i].offset,
													   world.biomes[i].scale);

						// Keep track of which weight is strongest.
						if (weight > strongestWeight) {
							strongestWeight = weight;
							strongestBiomeIndex = i;
						}

						// Get the height of the terrain (for the current biome) and multiply it by its weight.
						var height = world.biomes[i].terrainHeight *
									 Noise.Get2DPerlin(_worldInfo, new Vector2(_worldX, _worldZ), 0,
													   world.biomes[i].terrainScale) * weight;

						// If the height value is greater 0 add it to the sum of heights.
						if (!(height > 0)) continue;
						sumOfHeights += height;
						count++;
					}

					// Set biome to the one with the strongest weight.
					var biome = world.biomes[strongestBiomeIndex];

					// Get the average of the heights.
					sumOfHeights /= count;

					var terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);

					var hills = noise.GetNoise(_worldX * 4f + 500, _worldZ * 4f) * biome.terrainScale + 3f;

					var hillHeight = (int)(SURFACE_HEIGHT + hills * 32);
					var bedrock = noise.GetNoise(_worldX * 64f, _worldZ * 64f) * 0.5f + 0.5f;
					var bedrockHeight = (int)(bottomHeight + bedrock * 4);
					
					if (y > hillHeight || y < bottomHeight + 1) {
						_blocks[x, y, z] = Blocks.Air;
						continue;
					}

					// if (y <= bedrockHeight) {
					// 	_blocks[x, y, z] = BlockTypes.Bedrock;
					// 	continue;
					// }

					if (y > hillHeight - 4) {
						if (GenerateCaves(x, y, z, 0.2f)) continue;
						if (y == hillHeight) {
							var noiseValue = noise.GetNoise(x, y, z);

							if (noiseValue > biome.minNoiseValue && noiseValue < biome.maxNoiseValue) {
								_blocks[x, y, z] = biome.newSurfaceBlock;
							}
							
							/*if (noiseValue == 0) {
								_blocks[x, y, z] = BlockTypes.Andesite;
							} else if (noiseValue > 0 && noiseValue < 0.2) {
								_blocks[x, y, z] = BlockTypes.Diorite;
							} else if (noiseValue > 0.2 && noiseValue < 0.4) {
								_blocks[x, y, z] = BlockTypes.Granite;
							} else if (noiseValue > 0.4 && noiseValue < 0.6) {
								_blocks[x, y, z] = BlockTypes.OakPlanks;
							} else if (noiseValue > 0.6 && noiseValue < 0.8) {
								_blocks[x, y, z] = BlockTypes.BirchPlanks;
							} else if (noiseValue > 0.8 && noiseValue < 1) {
								_blocks[x, y, z] = BlockTypes.DarkOakPlanks;
							}*/
							continue;
						}

						_blocks[x, y - 1, z] = biome.newSubSurfaceBlock;
						_blocks[x, y - 2, z] = biome.newSubSurfaceBlock;
					}
					else {
						if (GenerateCaves(x, y, z, 0f)) continue;
						if (GenerateOres(x, y, z)) continue;

						_blocks[x, y, z] = y < 9 ? Blocks.Deepslate : Blocks.Stone;

						if (_blocks[x, y, z] != Blocks.Stone) continue;
						foreach (var lode in biome.lodes) {
							if (y <= lode.minHeight || y >= lode.maxHeight) continue;
							if (Noise.Get3DPerlin(_worldInfo, new Vector3(_worldX, y, _worldZ), lode.noiseOffset,
											      lode.scale, lode.threshold)) 
								_blocks[x, y, z] = lode.block;
						}
					}
				}
			}
		}

		var hash = World.activeWorld.info.seed.ToString() + position.x + position.y;
		var structuresSeed = hash.GetHashCode();
		var rnd = new System.Random(structuresSeed);
		_structures = new List<StructureInfo>();
		var spotsTaken = new bool[VoxelData.ChunkWidth, VoxelData.ChunkWidth];

		if (_worldInfo.type != WorldInfo.Type.Flat) {
			//cave entrances
			if (rnd.Next() < STRUCTURE_CHANCE_CAVE_ENTRANCE) {
				var h = 255;
				while (h > 0) {
					if (_blocks[8, h, 8] != Blocks.Air) {
						_structures.Add(new StructureInfo(new Vector3Int(0, h + 6, 0), Structure.Type.CaveEntrance,
														  rnd.Next()));
						break;
					}

					h--;
				}
			}

			//trees
			for (var y = 2; y < 14; ++y) {
				for (var x = 2; x < 14; ++x) {
					if (rnd.Next() >= STRUCTURE_CHANCE_TREE) continue;
					if (!IsSpotFree(spotsTaken, new Vector2Int(x, y), 2)) continue;
					spotsTaken[x, y] = true;
					var height = 255;
					while (height > 0) {
						if (_blocks[x, height, y] == Blocks.Grass) {
							_structures.Add(new StructureInfo(new Vector3Int(x, height + 1, y),
															  Structure.Type.OakTree, rnd.Next()));
							break;
						}

						height--;
					}
				}
			}

			//trees
			for (var y = 2; y < 14; ++y) {
				for (var x = 2; x < 14; ++x) {
					if (rnd.Next() >= STRUCTURE_CHANCE_BIRCH_TREE) continue;
					if (!IsSpotFree(spotsTaken, new Vector2Int(x, y), 2)) continue;
					spotsTaken[x, y] = true;
					var height = 255;
					while (height > 0) {
						if (_blocks[x, height, y] == Blocks.Grass) {
							_structures.Add(new StructureInfo(new Vector3Int(x, height + 1, y),
															  Structure.Type.BirchTree, rnd.Next()));
							break;
						}

						height--;
					}
				}
			}
		}

		if (rnd.Next() < STRUCTURE_CHANCE_WELL) {
			if (IsSpotFree(spotsTaken, new Vector2Int(7, 7), 3)) {
				//Debug.Log("Spot is free");

				var minH = 255;
				var maxH = 0;
				var canPlace = true;
				for (var y = 5; y < 11; ++y) {
					for (var x = 5; x < 11; ++x) {
						for (var h = 255; h > -1; h--) {
							var b = _blocks[x, h, y];
							if (b == Blocks.Air) continue;
							//Debug.Log(b);
							canPlace &= b == Blocks.Grass;
							minH = Mathf.Min(minH, h);
							maxH = Mathf.Max(maxH, h);
							break;
						}
					}
				}

				canPlace &= Mathf.Abs(minH - maxH) < 2;
				if (canPlace) {
					Debug.Log("spawning well structure");
					for (var y = 5; y < 11; ++y) {
						for (var x = 5; x < 11; ++x) {
							spotsTaken[x, y] = true;
						}
					}

					var h = 255;
					while (h > 0) {
						if (_blocks[7, h, 7] != Blocks.Air) {
							_structures.Add(
								new StructureInfo(new Vector3Int(7, h + 1, 7), Structure.Type.Well, rnd.Next()));
							break;
						}

						h--;
					}
				}
			}
		}

		//already load changes from disk here (apply later)
		_saveData = SaveDataManager.Instance.Load(position);

		TerrainReady = true;
		//Debug.Log($"Chunk {position} terrain ready");
	}

	private bool GenerateCaves(int x, int y, int z, float threshold) {
		var cave1 = noise.GetNoise(_worldX * 10f - 400, y * 10f, _worldZ * 10f);
		var cave2 = noise.GetNoise(_worldX * 20f - 600, y * 20f, _worldZ * 20f);
		var cave3 = noise.GetNoise(_worldX * 7f - 200, y * 7f, _worldZ * 7f);
		var cave4 = noise.GetNoise(_worldX * 4f - 300, y * 4f, _worldZ * 4f);
		var cave = Mathf.Min(Mathf.Min(cave1, cave4), Mathf.Min(cave2, cave3));

		if (!(cave > threshold)) return false;
		_blocks[x, y, z] = Blocks.Air;
		return true;
	}

	private bool GenerateOres(int x, int y, int z) {
		var ore1 = noise.GetNoise(_worldX * 15f, y * 15f, _worldZ * 15f + 300);
		var ore2 = noise.GetNoise(_worldX * 15f, y * 15f, _worldZ * 15f + 400);


		if (ore1 > 0.3 && ore2 > 0.4) {
			_blocks[x, y, z] = Blocks.Diorite;
			return true;
		}

		if (ore1 < -0.3 && ore2 < -0.4) {
			_blocks[x, y, z] = Blocks.Granite;
			return true;
		}

		if (ore1 > 0.3 && ore2 < -0.4) {
			_blocks[x, y, z] = Blocks.Andesite;
			return true;
		}


		var ore3 = noise.GetNoise(_worldX * 20f, y * 20f, _worldZ * 20f + 500);

		if (ore1 < -0.3 && ore3 > 0.4) {
			_blocks[x, y, z] = Blocks.CoalOre;
			return true;
		}

		var ore4 = noise.GetNoise(_worldX * 21f, y * 21f, _worldZ * 21f - 300);

		if (ore4 > 0.6) {
			_blocks[x, y, z] = Blocks.IronOre;
			return true;
		}

		if (y >= 32) return false;
		var ore5 = noise.GetNoise(_worldX * 22f, y * 22f, _worldZ * 22f - 400);

		if (ore5 > 0.7) {
			_blocks[x, y, z] = Blocks.GoldOre;
			return true;
		}

		if (y >= 16) return false;
		if (!(ore5 < -0.7)) return false;
		_blocks[x, y, z] = Blocks.DiamondOre;
		return true;
	}

	private static bool
		IsSpotFree(bool[,] spotsTaken, Vector2Int position, int size) //x area is for example size + 1 + size
	{
		var spotTaken = false;
		for (var y = Mathf.Max(0, position.y - size); y < Mathf.Min(15, position.y + size + 1); ++y) {
			for (var x = Mathf.Max(0, position.x - size); x < Mathf.Min(15, position.x + size + 1); ++x) {
				spotTaken |= spotsTaken[x, y];
			}
		}

		return !spotTaken;
	}

	private void LoadDetails() {
		//load structures

		foreach (var structure in _structures) {
			var overwritesEverything = Structure.OverwritesEverything(structure.type);
			var p = structure.position;
			var x = p.x;
			var y = p.y;
			var z = p.z;
			var changeList = Structure.Generate(structure.type, structure.seed);
			//Debug.Log($"placing {structure.type} which has {changeList.Count} blocks");
			foreach (var c in changeList) {
				var placeX = x + c.x;
				var placeY = y + c.y;
				var placeZ = z + c.z;
				if (placeX is < 0 or > 15) continue;
				if (placeZ is < 0 or > 15) continue;
				if (placeY is < 0 or > 255) continue;
				if (_blocks[placeX, placeY, placeZ] == Blocks.Bedrock) continue;

				if (!overwritesEverything) {
					//only place new blocks if density is higher or the same (leaves can't replace dirt for example)
					if (_blocks[placeX, placeY, placeZ].GetMaterial().IsReplaceable()) continue;
				}

				_blocks[x, y, z] = c.b;
			}
		}

		//remove all references to neighbors to avoid them staying in memory when unloading chunks
		front = null;
		left = null;
		right = null;
		back = null;

		//load changes
		var changes = _saveData.changes;
		foreach (var c in changes) {
			_blocks[c.x, c.y, c.z] = BlockTypes.byteToBlock[c.b];
			var lightLevel = BlockTypes.byteToBlock[c.b].GetLightLevel();
			if (lightLevel > 0) {
				lightSources[new Vector3Int(c.x, c.y, c.z)] = lightLevel;
			}
		}

		//get highest non-air blocks to speed up light simulation
		for (var z = 0; z < 16; ++z) {
			for (var x = 0; x < 16; ++x) {
				highestNonAirBlock[x, z] = 0;
				for (var y = 255; y > -1; --y) {
					if (_blocks[x, y, z] == Blocks.Air) continue;
					highestNonAirBlock[x, z] = y;
					break;
				}
			}
		}

		ChunkReady = true;
	}

	public void Modify(int x, int y, int z, Block blockType) {
		if (!ChunkReady) throw new System.Exception("Chunk has not finished loading");
		Debug.Log($"Current highest block at {x}x{z} is {highestNonAirBlock[x, z]}");

		_saveData.changes.Add(new ChunkSaveData.C((byte)x, (byte)y, (byte)z, blockType));
		_blocks[x, y, z] = blockType;
		if (blockType == Blocks.Air) {
			if (highestNonAirBlock[x, z] == y) {
				highestNonAirBlock[x, z] = 0;
				for (var yy = y; yy > -1; yy--) {
					if (_blocks[x, yy, z] == Blocks.Air) continue;
					highestNonAirBlock[x, z] = (byte)yy;
					break;
				}
			}
		}
		else {
			highestNonAirBlock[x, z] = (byte)Mathf.Max(highestNonAirBlock[x, z], y);
		}

		Debug.Log($"New highest block at {x}x{z} is {highestNonAirBlock[x, z]}");
	}

	public void Unload() {
		if (isDirty) {
			SaveDataManager.Instance.Save(_saveData);
		}
	}
}