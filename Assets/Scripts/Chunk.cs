﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class Chunk : MonoBehaviour {
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public MeshCollider meshCollider;
	public Vector2Int position;
	private Mesh mesh;

	//normals 0 front; 1 back; 2 top; 3 bottom; 4 left; 5 right
	private const byte NORMAL_FRONT = 0;
	private const byte NORMAL_BACK = 1;
	private const byte NORMAL_TOP = 2;
	private const byte NORMAL_BOTTOM = 3;
	private const byte NORMAL_LEFT = 4;
	private const byte NORMAL_RIGHT = 5;


	//reused on main thread
	//private static List<Vector3> vertices=new List<Vector3>();
	//private static List<Vector3> normals=new List<Vector3>();
	//private static List<Vector2> uvs=new List<Vector2>();
	//private static List<Color32> colors=new List<Color32>();
	//786,432 max possible vertices (i think)
	private static List<int> triangles = new List<int>();
	private static VertexData[] vBuffer = new VertexData[786432];
	private static int vBufferLength;


	//for generating
	private ChunkData[,] chunkMap;
	private readonly Vector2Int nFront = new Vector2Int(0, 1);
	private readonly Vector2Int nBack = new Vector2Int(0, -1);
	private readonly Vector2Int nLeft = new Vector2Int(-1, 0);
	private readonly Vector2Int nRight = new Vector2Int(1, 0);

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	private struct VertexData {
		public VertexData(float x, float y, float z, byte r, byte g, byte b, byte a) {
			this.x = x;
			this.y = y;
			this.z = z;
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public float x, y, z;
		public byte r, g, b, a; //rg = uv, b = normal, a = light
	}

	public void Awake() {
		mesh = new Mesh();
		meshFilter.sharedMesh = mesh;
		mesh.name = "ChunkMesh";
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		mesh.MarkDynamic();
		mesh.bounds = new Bounds(new Vector3(8, 128, 8), new Vector3(16, 256, 16));
		//meshCollider.sharedMesh = mesh;

		//vertices = new List<Vector3>();
		//normals = new List<Vector3>();
		//uvs = new List<Vector2>();
		triangles = new List<int>();
		//colors = new List<Color32>();
		chunkMap = new ChunkData[3, 3]; //start at backleft
	}

	public void Initialize(Vector2Int position) {
		this.position = position;
	}

	public void Tick(World world, System.Random randomTick) {
		for (int y = 0; y < 256; y += 16) {
			//4096 = 16*16*16
			RandomTick(y, world, randomTick.Next(4096));
		}
	}

	private void RandomTick(int minY, World world, int randomTickIndex) {
		//Debug.Log($"{position} random tick at height {minY}-{maxY}");
		Vector3Int randomPos = RandomTickPositionFromIndex(randomTickIndex) + new Vector3Int(0, minY, 0);
		Vector3Int blockWorldPos = randomPos + new Vector3Int(position.x * 16, 0, position.y * 16);
		int x = blockWorldPos.x;
		int y = blockWorldPos.y;
		int z = blockWorldPos.z;

		Blocks.Block block = world.GetBlock(x, y, z);
		if (block == Blocks.Dirt) {
			var becomeGrass = false;
			if (world.GetBlock(x, y + 1, z) != Blocks.Air) return;

			becomeGrass |= world.GetBlock(x + 1, y, z) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x - 1, y, z) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x, y, z + 1) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x, y, z - 1) == Blocks.Grass;

			becomeGrass |= world.GetBlock(x + 1, y + 1, z) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x - 1, y + 1, z) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x, y + 1, z + 1) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x, y + 1, z - 1) == Blocks.Grass;

			becomeGrass |= world.GetBlock(x + 1, y - 1, z) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x - 1, y - 1, z) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x, y - 1, z + 1) == Blocks.Grass;
			becomeGrass |= world.GetBlock(x, y - 1, z - 1) == Blocks.Grass;

			if (becomeGrass) {
				Debug.Log($"Grass spreading at {blockWorldPos}");
				world.Modify(x, y, z, Blocks.Grass);
			}
			return;
		}

		if (block != Blocks.Grass) return;
		if (world.GetBlock(x, y + 1, z) != Blocks.Air) {
			world.Modify(x, y, z, Blocks.Dirt);
		}
	}

	public void Build(ChunkDataManager chunkDataManager) {
//#if UNITY_EDITOR
		Profiler.BeginSample("BUILDING CHUNK");
//#endif
		Vector2Int renderPosition = 16 * position;
		transform.position = new Vector3(renderPosition.x, 0, renderPosition.y);
		mesh.Clear();

		Profiler.BeginSample("GRABBING BLOCK DATA");

		ChunkData chunkData = chunkDataManager.data[position];
		ChunkData front = chunkDataManager.data[position + nFront];
		ChunkData back = chunkDataManager.data[position + nBack];
		ChunkData left = chunkDataManager.data[position + nLeft];
		ChunkData right = chunkDataManager.data[position + nRight];
		ChunkData frontLeft = chunkDataManager.data[position + nFront + nLeft];
		ChunkData frontRight = chunkDataManager.data[position + nFront + nRight];
		ChunkData backLeft = chunkDataManager.data[position + nBack + nLeft];
		ChunkData backRight = chunkDataManager.data[position + nBack + nRight];

		int[,,] lightMap = new int[48, 256, 48];

		chunkMap[0, 0] = backLeft;
		chunkMap[1, 0] = back;
		chunkMap[2, 0] = backRight;
		chunkMap[0, 1] = left;
		chunkMap[1, 1] = chunkData;
		chunkMap[2, 1] = right;
		chunkMap[0, 2] = frontLeft;
		chunkMap[1, 2] = front;
		chunkMap[2, 2] = frontRight;

		Profiler.EndSample();

		Profiler.BeginSample("SIMULATING LIGHT");

		Profiler.BeginSample("PREPARING LIGHT SIMULATION");


		Queue<Vector3Int> simulateQueue = new Queue<Vector3Int>();
		//sunray tracing needs to start above the highest non-air block to increase performance
		//all blocks above that block need to be set to 15
		for (int z = 0; z < 48; ++z) {
			for (int x = 0; x < 48; ++x) {
				if ((x % 47) * (z % 47) == 0) //filters outer edges
				{
					//Debug.Log($"these should at least 0 or 47  ->  {x} {z}"); 
					for (int yy = 0; yy < 256; ++yy) //dont do outer edges
					{
						lightMap[x, yy, z] = 15; //set all edges to 15 to stop tracing at edges
					}

					continue;
				}

				int y = GetHighestNonAir(chunkMap, x, z);
				for (int sunlight = y; sunlight < 256; ++sunlight) {
					lightMap[x, sunlight, z] = 15;
				}


				if (x < 46) y = Mathf.Max(y, GetHighestNonAir(chunkMap, x + 1, z));
				if (x > 1) y = Mathf.Max(y, GetHighestNonAir(chunkMap, x - 1, z));
				if (z < 46) y = Mathf.Max(y, GetHighestNonAir(chunkMap, x, z + 1));
				if (z > 1) y = Mathf.Max(y, GetHighestNonAir(chunkMap, x, z - 1));
				y = Mathf.Min(y + 1, 255);
				if (y < 2) continue;

				simulateQueue.Enqueue(new Vector3Int(x, y, z));
			}
		}

		for (var y = 0; y < 3; ++y) {
			for (var x = 0; x < 3; ++x) {
				foreach (var (vector3Int, value) in chunkMap[x, y].lightSources) {
					var lX = 16 * x + vector3Int.x;
					var lY = vector3Int.y;
					var lZ = 16 * y + vector3Int.z;
					lightMap[lX, lY, lZ] = value;
					simulateQueue.Enqueue(new Vector3Int(lX, lY, lZ));
				}
			}
		}

		Profiler.EndSample();

		Profiler.BeginSample("RUNNING LIGHT SIMULATION");


		int simulateCount = 0;
		while (simulateQueue.Count > 0) {
			Vector3Int position = simulateQueue.Dequeue();
			int y = position.y;
			int x = position.x;
			int z = position.z;


			int light = lightMap[x, y, z];

			if (x < 47) {
				int lightR = lightMap[x + 1, y, z];
				if (lightR < light - 1) {
					Blocks.Block bR = GetBlockFromMap(chunkMap, x + 1, y, z);
					if (bR == Blocks.Air) {
						lightMap[x + 1, y, z] = (byte)(light - 1);
						simulateQueue.Enqueue(new Vector3Int(x + 1, y, z));
					}
				}
			}

			if (x > 0) {
				int lightL = lightMap[x - 1, y, z];
				if (lightL < light - 1) {
					Blocks.Block bL = GetBlockFromMap(chunkMap, x - 1, y, z);
					if (bL == Blocks.Air) {
						lightMap[x - 1, y, z] = (byte)(light - 1);
						simulateQueue.Enqueue(new Vector3Int(x - 1, y, z));
					}
				}
			}

			if (y > 0) {
				Blocks.Block bD = GetBlockFromMap(chunkMap, x, y - 1, z);
				if (bD == Blocks.Air) {
					if (light == 15) {
						lightMap[x, y - 1, z] = light;
						simulateQueue.Enqueue(new Vector3Int(x, y - 1, z));
					}
					else {
						int lightD = lightMap[x, y - 1, z];
						if (lightD < light - 1) {
							lightMap[x, y - 1, z] = (byte)(light - 1);
							simulateQueue.Enqueue(new Vector3Int(x, y - 1, z));
						}
					}
				}
			}

			if (y < 255) {
				int lightU = lightMap[x, y + 1, z];
				if (lightU < light - 1) {
					Blocks.Block bU = GetBlockFromMap(chunkMap, x, y + 1, z);
					if (bU == Blocks.Air) {
						lightMap[x, y + 1, z] = (byte)(light - 1);
						simulateQueue.Enqueue(new Vector3Int(x, y + 1, z));
					}
				}
			}

			if (z < 47) {
				int lightF = lightMap[x, y, z + 1];
				if (lightF < light - 1) {
					Blocks.Block bF = GetBlockFromMap(chunkMap, x, y, z + 1);
					if (bF == Blocks.Air) {
						lightMap[x, y, z + 1] = (byte)(light - 1);
						simulateQueue.Enqueue(new Vector3Int(x, y, z + 1));
					}
				}
			}

			if (z > 0) {
				int lightB = lightMap[x, y, z - 1];
				if (lightB < light - 1) {
					Blocks.Block bB = GetBlockFromMap(chunkMap, x, y, z - 1);
					if (bB == Blocks.Air) {
						lightMap[x, y, z - 1] = (byte)(light - 1);
						simulateQueue.Enqueue(new Vector3Int(x, y, z - 1));
					}
				}
			}

			simulateCount++;
		}

		Profiler.EndSample();

		Profiler.EndSample();


		Profiler.BeginSample("CREATING FACES");


		//low precision meshes possible with this? : https://docs.unity3d.com/ScriptReference/Rendering.VertexAttributeDescriptor.html
		vBufferLength = 0;
		TextureMapper textureMapper = GameManager.Instance.textureMapper;
		for (int z = 0; z < 16; ++z) {
			for (int y = 0; y < 256; ++y) {
				for (int x = 0; x < 16; ++x) {
					var c = chunkData.GetBlocks()[x, y, z];
					if (c == Blocks.Air) continue;
					var lx = x + 16;
					var lz = z + 16;

					var bR = x == 15
								 ? right.GetBlocks()[0, y, z]
								 : chunkData.GetBlocks()[x + 1, y, z];
					var bL = x == 0
								 ? left.GetBlocks()[15, y, z]
								 : chunkData.GetBlocks()[x - 1, y, z];
					var bF = z == 15
								 ? front.GetBlocks()[x, y, 0]
								 : chunkData.GetBlocks()[x, y, z + 1];
					var bB = z == 0
								 ? back.GetBlocks()[x, y, 15]
								 : chunkData.GetBlocks()[x, y, z - 1];
					var bU = y == 255 ? Blocks.Air : chunkData.GetBlocks()[x, y + 1, z];
					var bD = y == 0 ? Blocks.Air : chunkData.GetBlocks()[x, y - 1, z];

					var textureMap = textureMapper.map[c];

					if (bR is Blocks.TranslucentBlock) {
						//AddFace(
						//	new Vector3(x + 1, y, z),
						//	new Vector3(x + 1, y + 1, z),
						//	new Vector3(x + 1, y + 1, z + 1),
						//	new Vector3(x + 1, y, z + 1),
						//	Vector3.right
						//);
						//AddTextureFace(textureMap.right);
						int b = y == 0 ? 0 : 1;
						int t = y == 255 ? 0 : 1;
						byte bl = (byte)((lightMap[lx + 1, y, lz] + lightMap[lx + 1, y, lz - 1] +
										  lightMap[lx + 1, y - b, lz] + lightMap[lx + 1, y - b, lz - 1]) / 4);
						byte tl = (byte)((lightMap[lx + 1, y, lz] + lightMap[lx + 1, y, lz - 1] +
										  lightMap[lx + 1, y + t, lz] + lightMap[lx + 1, y + t, lz - 1]) / 4);
						byte tr = (byte)((lightMap[lx + 1, y, lz] + lightMap[lx + 1, y, lz + 1] +
										  lightMap[lx + 1, y + t, lz] + lightMap[lx + 1, y + t, lz + 1]) / 4);
						byte br = (byte)((lightMap[lx + 1, y, lz] + lightMap[lx + 1, y, lz + 1] +
										  lightMap[lx + 1, y - b, lz] + lightMap[lx + 1, y - b, lz + 1]) / 4);
						//AddColors(textureMap,bl, tl, tr, br);
						AddVertexData(
							new Vector3(x + 1, y, z),
							new Vector3(x + 1, y + 1, z),
							new Vector3(x + 1, y + 1, z + 1),
							new Vector3(x + 1, y, z + 1),
							NORMAL_RIGHT,
							textureMap.right,
							bl, tl, tr, br
						);
					}

					if (bL is Blocks.TranslucentBlock) {
						//AddFace(
						//	new Vector3(x, y, z + 1),
						//	new Vector3(x, y + 1, z + 1),
						//	new Vector3(x, y + 1, z),
						//	new Vector3(x, y, z),
						//	-Vector3.right
						//);
						//AddTextureFace(textureMap.left);
						int b = (y == 0 ? 0 : 1);
						int t = (y == 255 ? 0 : 1);
						byte br = (byte)((lightMap[lx - 1, y, lz] + lightMap[lx - 1, y, lz - 1] +
										  lightMap[lx - 1, y - b, lz] + lightMap[lx - 1, y - b, lz - 1]) / 4);
						byte tr = (byte)((lightMap[lx - 1, y, lz] + lightMap[lx - 1, y, lz - 1] +
										  lightMap[lx - 1, y + t, lz] + lightMap[lx - 1, y + t, lz - 1]) / 4);
						byte tl = (byte)((lightMap[lx - 1, y, lz] + lightMap[lx - 1, y, lz + 1] +
										  lightMap[lx - 1, y + t, lz] + lightMap[lx - 1, y + t, lz + 1]) / 4);
						byte bl = (byte)((lightMap[lx - 1, y, lz] + lightMap[lx - 1, y, lz + 1] +
										  lightMap[lx - 1, y - b, lz] + lightMap[lx - 1, y - b, lz + 1]) / 4);
						//AddColors(textureMap, bl, tl, tr, br);
						AddVertexData(
							new Vector3(x, y, z + 1),
							new Vector3(x, y + 1, z + 1),
							new Vector3(x, y + 1, z),
							new Vector3(x, y, z),
							NORMAL_LEFT,
							textureMap.left,
							bl, tl, tr, br);
					}

					if (bU is Blocks.TranslucentBlock) {
						//AddFace(
						//	new Vector3(x, y + 1, z),
						//	new Vector3(x, y + 1, z + 1),
						//	new Vector3(x + 1, y + 1, z + 1),
						//	new Vector3(x + 1, y + 1, z),
						//	Vector3.up
						//);
						//AddTextureFace(textureMap.top);
						int b = (y == 0 ? 0 : 1);
						int t = (y == 255 ? 0 : 1);
						byte bl = (byte)((lightMap[lx, y + t, lz] + lightMap[lx - 1, y + t, lz] +
										  lightMap[lx, y + t, lz - 1] + lightMap[lx - 1, y + t, lz - 1]) / 4);
						byte tl = (byte)((lightMap[lx, y + t, lz] + lightMap[lx - 1, y + t, lz] +
										  lightMap[lx, y + t, lz + 1] + lightMap[lx - 1, y + t, lz + 1]) / 4);
						byte tr = (byte)((lightMap[lx, y + t, lz] + lightMap[lx + 1, y + t, lz] +
										  lightMap[lx, y + t, lz + 1] + lightMap[lx + 1, y + t, lz + 1]) / 4);
						byte br = (byte)((lightMap[lx, y + t, lz] + lightMap[lx + 1, y + t, lz] +
										  lightMap[lx, y + t, lz - 1] + lightMap[lx + 1, y + t, lz - 1]) / 4);
						//AddColors(textureMap, bl, tl, tr, br);
						AddVertexData(
							new Vector3(x, y + 1, z),
							new Vector3(x, y + 1, z + 1),
							new Vector3(x + 1, y + 1, z + 1),
							new Vector3(x + 1, y + 1, z),
							NORMAL_TOP,
							textureMap.top,
							bl, tl, tr, br);
					}

					if (bD is Blocks.TranslucentBlock) {
						//AddFace(
						//	new Vector3(x, y, z + 1),
						//	new Vector3(x, y, z),
						//	new Vector3(x+1, y, z),
						//	new Vector3(x+1, y, z+1),

						//	-Vector3.up
						//);
						//AddTextureFace(textureMap.bottom);
						int b = (y == 0 ? 0 : 1);
						int t = (y == 255 ? 0 : 1);
						byte tl = (byte)((lightMap[lx, y - b, lz] + lightMap[lx - 1, y - b, lz] +
										  lightMap[lx, y - b, lz - 1] + lightMap[lx - 1, y - b, lz - 1]) / 4);
						byte bl = (byte)((lightMap[lx, y - b, lz] + lightMap[lx - 1, y - b, lz] +
										  lightMap[lx, y - b, lz + 1] + lightMap[lx - 1, y - b, lz + 1]) / 4);
						byte br = (byte)((lightMap[lx, y - b, lz] + lightMap[lx + 1, y - b, lz] +
										  lightMap[lx, y - b, lz + 1] + lightMap[lx + 1, y - b, lz + 1]) / 4);
						byte tr = (byte)((lightMap[lx, y - b, lz] + lightMap[lx + 1, y - b, lz] +
										  lightMap[lx, y - b, lz - 1] + lightMap[lx + 1, y - b, lz - 1]) / 4);
						//AddColors(textureMap, bl, tl, tr, br);
						AddVertexData(
							new Vector3(x, y, z + 1),
							new Vector3(x, y, z),
							new Vector3(x + 1, y, z),
							new Vector3(x + 1, y, z + 1),
							NORMAL_BOTTOM,
							textureMap.bottom,
							bl, tl, tr, br);
					}

					if (bF is Blocks.TranslucentBlock) {
						//AddFace(
						//	new Vector3(x + 1, y, z + 1),
						//	new Vector3(x + 1, y + 1, z + 1),
						//	new Vector3(x, y + 1, z + 1),
						//	new Vector3(x, y, z + 1),
						//	Vector3.forward
						//);
						//AddTextureFace(textureMap.front);
						int b = (y == 0 ? 0 : 1);
						int t = (y == 255 ? 0 : 1);
						byte br = (byte)((lightMap[lx, y, lz + 1] + lightMap[lx - 1, y, lz + 1] +
										  lightMap[lx, y - b, lz + 1] + lightMap[lx - 1, y - b, lz + 1]) / 4);
						byte tr = (byte)((lightMap[lx, y, lz + 1] + lightMap[lx - 1, y, lz + 1] +
										  lightMap[lx, y + t, lz + 1] + lightMap[lx - 1, y + t, lz + 1]) / 4);
						byte tl = (byte)((lightMap[lx, y, lz + 1] + lightMap[lx + 1, y, lz + 1] +
										  lightMap[lx, y + t, lz + 1] + lightMap[lx + 1, y + t, lz + 1]) / 4);
						byte bl = (byte)((lightMap[lx, y, lz + 1] + lightMap[lx + 1, y, lz + 1] +
										  lightMap[lx, y - b, lz + 1] + lightMap[lx + 1, y - b, lz + 1]) / 4);
						//AddColors(textureMap,bl, tl, tr, br);
						AddVertexData(
							new Vector3(x + 1, y, z + 1),
							new Vector3(x + 1, y + 1, z + 1),
							new Vector3(x, y + 1, z + 1),
							new Vector3(x, y, z + 1),
							NORMAL_FRONT,
							textureMap.front,
							bl, tl, tr, br);
					}

					if (bB is not Blocks.TranslucentBlock) continue;
					{
						//AddFace(
						//	new Vector3(x, y, z),
						//	new Vector3(x, y + 1, z),
						//	new Vector3(x + 1, y + 1, z),
						//	new Vector3(x + 1, y, z),
						//	-Vector3.forward
						//);
						//AddTextureFace(textureMap.back);
						int b = (y == 0 ? 0 : 1);
						int t = (y == 255 ? 0 : 1);
						byte bl = (byte)((lightMap[lx, y, lz - 1] + lightMap[lx - 1, y, lz - 1] +
										  lightMap[lx, y - b, lz - 1] + lightMap[lx - 1, y - b, lz - 1]) / 4);
						byte tl = (byte)((lightMap[lx, y, lz - 1] + lightMap[lx - 1, y, lz - 1] +
										  lightMap[lx, y + t, lz - 1] + lightMap[lx - 1, y + t, lz - 1]) / 4);
						byte tr = (byte)((lightMap[lx, y, lz - 1] + lightMap[lx + 1, y, lz - 1] +
										  lightMap[lx, y + t, lz - 1] + lightMap[lx + 1, y + t, lz - 1]) / 4);
						byte br = (byte)((lightMap[lx, y, lz - 1] + lightMap[lx + 1, y, lz - 1] +
										  lightMap[lx, y - b, lz - 1] + lightMap[lx + 1, y - b, lz - 1]) / 4);
						//AddColors(textureMap,bl, tl, tr, br);
						AddVertexData(
							new Vector3(x, y, z),
							new Vector3(x, y + 1, z),
							new Vector3(x + 1, y + 1, z),
							new Vector3(x + 1, y, z),
							NORMAL_BACK,
							textureMap.back,
							bl, tl, tr, br);
					}
				}
			}
		}

		Profiler.EndSample();

		Profiler.BeginSample("APPLYING MESH DATA");
		//mesh.SetVertices(vertices);
		//mesh.SetUVs(0, uvs);
		//mesh.SetNormals(normals);
		//mesh.SetColors(colors);

		VertexAttributeDescriptor[] layout = {
												 new VertexAttributeDescriptor(
													 VertexAttribute.Position),
												 new VertexAttributeDescriptor(
													 VertexAttribute.Color,
													 VertexAttributeFormat.UInt8, 4),
												 //new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
											 };
		mesh.SetVertexBufferParams(vBufferLength, layout);
		//VertexData[] verts = new VertexData[vertices.Count];
		////normals 0 front; 1 back; 2 top; 3 bottom; 4 left; 5 right
		//for (int i = 0; i < verts.Length; ++i)
		//{
		//	VertexData v = new VertexData();
		//	v.x = vertices[i].x;
		//	v.y = vertices[i].y;
		//	v.z = vertices[i].z;
		//	v.a = colors[i].a;
		//	Vector3 n = normals[i];
		//	if (n == Vector3.forward) v.b = 0;
		//	if (n == Vector3.back) v.b = 1;
		//	if (n == Vector3.up) v.b = 2;
		//	if (n == Vector3.down) v.b = 3;
		//	if (n == Vector3.left) v.b = 4;
		//	if (n == Vector3.right) v.b = 5;
		//	v.r = (byte)(int)uvs[i].x;
		//	v.g = (byte)(int)uvs[i].y;

		//	//v.uv = uvs[i];
		//	//v.color = colors[i];
		//	//v.normal = normals[i];
		//	verts[i] = v;
		//}
		mesh.SetVertexBufferData(vBuffer, 0, 0, vBufferLength);
		mesh.SetTriangles(triangles, 0);
		mesh.UploadMeshData(false);


		meshRenderer.enabled = (triangles.Count != 0);
		gameObject.SetActive(true);
		//vertices.Clear();
		triangles.Clear();
		//colors.Clear();
		//uvs.Clear();
		//normals.Clear();


		meshCollider.sharedMesh = mesh;
		Profiler.EndSample();

		//#if UNITY_EDITOR
		Profiler.EndSample();
//#endif
	}

	private int GetHighestNonAir(ChunkData[,] chunkMap, int x, int z) {
		var cX = x < 16 ? 0 : x < 32 ? 1 : 2;
		var cZ = z < 16 ? 0 : z < 32 ? 1 : 2;
		return chunkMap[cX, cZ].highestNonAirBlock[x - cX * 16, z - cZ * 16];
	}

	private Blocks.Block GetBlockFromMap(ChunkData[,] chunkMap, int x, int y, int z) {
		try {
			var cX = x < 16 ? 0 : x < 32 ? 1 : 2;
			var cZ = z < 16 ? 0 : z < 32 ? 1 : 2;
			return chunkMap[cX, cZ].GetBlocks()[x - cX * 16, y, z - cZ * 16];
		}
		catch (System.Exception) {
			Debug.LogWarning($"{x} {y} {z}");
			throw;
		}
	}

	private void AddVertexData(
		Vector3 a,
		Vector3 b,
		Vector3 c,
		Vector3 d,
		byte n,
		TextureMapper.TextureMap.Face face,
		byte lBL,
		byte lTL,
		byte lTR,
		byte lBR
	) {
		vBuffer[vBufferLength] = new VertexData(a.x, a.y, a.z, (byte)face.bl.x, (byte)face.bl.y, n, lBL);
		vBuffer[vBufferLength + 1] = new VertexData(b.x, b.y, b.z, (byte)face.tl.x, (byte)face.tl.y, n, lTL);
		vBuffer[vBufferLength + 2] = new VertexData(c.x, c.y, c.z, (byte)face.tr.x, (byte)face.tr.y, n, lTR);
		vBuffer[vBufferLength + 3] = new VertexData(d.x, d.y, d.z, (byte)face.br.x, (byte)face.br.y, n, lBR);

		triangles.Add(vBufferLength + 0);
		triangles.Add(vBufferLength + 1);
		triangles.Add(vBufferLength + 2);
		triangles.Add(vBufferLength + 2);
		triangles.Add(vBufferLength + 3);
		triangles.Add(vBufferLength + 0);

		vBufferLength += 4;
	}


	public void Unload() {
		mesh.Clear();
		gameObject.SetActive(false);
	}

	private Vector3Int RandomTickPositionFromIndex(int index) {
		int x = index % 16;
		int y = (index - x) / 256;
		int z = (index - (y * 256) - x) / 16;
		return new Vector3Int(x, y, z);
	}
}