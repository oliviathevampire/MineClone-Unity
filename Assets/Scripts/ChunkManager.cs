﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

public class ChunkManager : MonoBehaviour {
	public Chunk chunkPrefab;
	public ChunkDataManager chunkDataManager;

	public bool playerCanMove = false;

	private World world;
	private int renderDistance;
	private int maximumLoadQueueSize;
	private System.Random randomTick;

	public bool isInStartup; //start of the game loads faster and all around you

	private Queue<Chunk> chunkPool;
	private Queue<Vector2Int> modifiedRebuildQueue;
	private List<Vector2Int> modifyNeighborOrder;
	public Dictionary<Vector2Int, Chunk> chunkMap;

	//Camera info (multiple threads)
	private Vector3 cameraPosition;
	private Vector3 cameraForward;
	private Vector2Int cameraChunkPos;

	//ShouldRender thread
	private Thread shouldRenderThread;
	private System.Object shouldRenderLock = new System.Object();
	private bool shouldRenderWaitForUpdate = false;
	private volatile List<Vector2Int> loadQueue;
	private volatile Queue<Vector2Int> unloadQueue;
	private volatile List<Vector2Int> activeChunks;

	//Tick thread
	private Thread tickThread;
	private System.Object tickLock = new System.Object();
	private volatile bool tick;
	public int tickCounter;

	private readonly Vector2Int nFront = new Vector2Int(0, 1);
	private readonly Vector2Int nBack = new Vector2Int(0, -1);
	private readonly Vector2Int nLeft = new Vector2Int(-1, 0);
	private readonly Vector2Int nRight = new Vector2Int(1, 0);

	public void Initialize(World world) {
		this.world = world;
		renderDistance = GameManager.Instance.gameSettings.renderDistance;
		maximumLoadQueueSize = GameManager.Instance.gameSettings.maximumLoadQueueSize;
		playerCanMove = false;
		chunkDataManager = new ChunkDataManager(world);
		chunkMap = new Dictionary<Vector2Int, Chunk>();
		chunkPool = new Queue<Chunk>();
		activeChunks = new List<Vector2Int>();
		loadQueue = new List<Vector2Int>();
		unloadQueue = new Queue<Vector2Int>();
		modifiedRebuildQueue = new Queue<Vector2Int>();
		modifyNeighborOrder = new List<Vector2Int>();
		randomTick = new System.Random();
		int chunkPoolSize = renderDistance * renderDistance * 4;
		Debug.Log("Chunk pool size: " + chunkPoolSize);
		for (var i = 0; i < chunkPoolSize; ++i) {
			Chunk c = Instantiate(chunkPrefab, chunkPrefab.transform.parent, true);
			GameObject o;
			(o = c.gameObject).SetActive(false);
			o.name = "Chunk " + i;
			chunkPool.Enqueue(c);
		}

		shouldRenderThread = new Thread(ShouldRenderThread) {
															    IsBackground = true
														    };
		shouldRenderThread.Start();

		tickThread = new Thread(TickThread) {
											    IsBackground = true
										    };
		tickThread.Start(20);
	}

	public void UpdateChunks(Camera mainCamera) {
		UnityEngine.Profiling.Profiler.BeginSample("UPDATING CHUNKS");
		//Debug.Log("Active chunks: " + activeChunks.Count);
		chunkDataManager.Update();

		cameraPosition = mainCamera.transform.position;
		cameraForward = mainCamera.transform.forward;
		cameraChunkPos = new Vector2Int((int)cameraPosition.x / 16, (int)cameraPosition.z / 16);

		int loadQueueCount = 0;

		UnityEngine.Profiling.Profiler.BeginSample("LOCK SHOULD RENDER");


		for (int loop = 0; loop < (isInStartup ? 16 : 1); ++loop) {
			//startup loads a laggy 8 chunks per frame
			lock (shouldRenderLock) {
				loadQueueCount = loadQueue.Count;
				if (modifiedRebuildQueue.Count > 0) {
					UnityEngine.Profiling.Profiler.BeginSample("MODIFY CHUNK");
					Vector2Int position = modifiedRebuildQueue.Dequeue();
					if (activeChunks.Contains(position)) {
						chunkMap[position].Build(chunkDataManager);
					}

					UnityEngine.Profiling.Profiler.EndSample();
				}
				else {
					UnityEngine.Profiling.Profiler.BeginSample("UNLOADING");
					while (true) //loop until everything is unloaded
					{
						if (unloadQueue.Count == 0) break;

						Vector2Int position = unloadQueue.Dequeue();

						if (!activeChunks.Contains(position)) continue;

						Chunk chunk = chunkMap[position];
						chunk.Unload();
						chunkPool.Enqueue(chunk);

						activeChunks.Remove(position);
						chunkMap.Remove(position);
						chunkDataManager.UnloadChunk(position);
					}

					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("BUILD CHUNK CHECK");

					var buildChunk = false;
					var chunkToBuild = Vector2Int.zero;
					foreach (var position in loadQueue.Where(position => chunkDataManager.Load(position))
													  .Where(_ => !buildChunk)) {
						buildChunk = true;
						chunkToBuild = position;
					}

					UnityEngine.Profiling.Profiler.EndSample();

					UnityEngine.Profiling.Profiler.BeginSample("BUILD CHUNK");

					if (buildChunk) {
						loadQueue.Remove(chunkToBuild);
						Chunk chunk = null;
						if (!chunkMap.ContainsKey(chunkToBuild)) {
							chunk = chunkPool.Dequeue();
							chunk.Initialize(chunkToBuild);
							chunkMap.Add(chunkToBuild, chunk);
							chunk.Build(chunkDataManager);
							activeChunks.Add(chunkToBuild);
						}
					}

					UnityEngine.Profiling.Profiler.EndSample();
				}
			}

			playerCanMove = activeChunks.Count > 32;
		}

		shouldRenderWaitForUpdate = false;
		UnityEngine.Profiling.Profiler.EndSample();

		UnityEngine.Profiling.Profiler.BeginSample("TICK");
		bool newTick;
		lock (tickLock) {
			newTick = tick;
			tick = false;
		}

		if (newTick) {
			tickCounter++;
			for (int y = -7; y < 8; ++y) {
				for (int x = -7; x < 8; ++x) {
					Vector2Int chunkPos = cameraChunkPos + new Vector2Int(x, y);
					if (chunkMap.ContainsKey(chunkPos)) {
						chunkMap[chunkPos].Tick(world, randomTick);
					}
				}
			}
		}

		UnityEngine.Profiling.Profiler.EndSample();


		var activeChunksCount = activeChunks.Count;
		var rebuildQueueCount = modifiedRebuildQueue.Count;
		var chunksInMemoryCount = chunkDataManager.GetChunksInMemoryCount();
		GameManager.Instance.AddDebugLine(
			$"Chunks: (Q:{loadQueueCount} R:{rebuildQueueCount} A:{activeChunksCount} M:{chunksInMemoryCount})");
		UnityEngine.Profiling.Profiler.EndSample();
	}

	public bool StartupFinished() {
		var ready = false;
		lock (shouldRenderLock) {
			ready = activeChunks.Count >= renderDistance * renderDistance;
		}

		return ready;
	}

	public Vector2Int[] GetActiveChunkPositions() {
		Vector2Int[] positions = null;
		lock (shouldRenderLock) {
			positions = activeChunks.ToArray();
		}

		return positions;
	}

	private void ShouldRenderThread() {
		List<Vector2Int> visiblePoints = new List<Vector2Int>();
		List<Vector2Int> inRangePoints = new List<Vector2Int>();
		List<Vector2Int> copyOfActiveChunks = new List<Vector2Int>();
		Queue<Vector2Int> copyOfUnload = new Queue<Vector2Int>();
		Queue<Vector2Int> copyOfLoad = new Queue<Vector2Int>();
		while (true) {
			visiblePoints.Clear();
			inRangePoints.Clear();
			copyOfActiveChunks.Clear();
			//Vector2Int cameraChunkPos;
			//cameraChunkPos = new Vector2Int((int)cameraPosition.x / 16, (int)cameraPosition.z / 16);
			Vector3 cameraPositionFloor = new Vector3(cameraPosition.x, 0, cameraPosition.z);
			Vector3 cameraForwardFloor = cameraForward;
			cameraForwardFloor.y = 0;
			cameraForwardFloor.Normalize();
			for (int y = 0; y < renderDistance * 2; ++y) {
				for (int x = 0; x < renderDistance * 2; ++x) {
					var c = cameraChunkPos - new Vector2Int(renderDistance, renderDistance) +
							new Vector2Int(x, y);
					var renderPosition = new Vector3(c.x * 16, 0, c.y * 16);

					var toChunk = renderPosition - cameraPositionFloor;

					var inRange = toChunk.magnitude < renderDistance * 16;
					var inAngle = Vector3.Angle(toChunk, cameraForwardFloor) < 70;
					if (isInStartup) {
						var startupMin = renderDistance / 2;
						var startupMax = startupMin + renderDistance;
						inAngle = true;
						inRange = x > startupMin && x <= startupMax && y > startupMin && y <= startupMax;
					}

					var isClose = toChunk.magnitude < 16 * 3;
					if (inRange) inRangePoints.Add(c);
					if ((inAngle && inRange) || isClose) visiblePoints.Add(c);
				}
			}

			//List<Vector2Int> ordered = visiblePoints.OrderBy(vp => Vector2Int.Distance(cameraChunkPos, vp)).ToList<Vector2Int>();
			List<Vector2Int> ordered;
			if (isInStartup) {
				var rnd = new System.Random();
				ordered = visiblePoints.OrderBy(_ => rnd.Next()).ToList();
			}
			else {
				ordered = visiblePoints.OrderBy(vp => ChunkPriority(vp, cameraChunkPos, cameraForwardFloor))
									   .ToList();
			}

			while (shouldRenderWaitForUpdate) Thread.Sleep(8);
			shouldRenderWaitForUpdate = true;

			TimeStamp();
			lock (shouldRenderLock) {
				copyOfActiveChunks.AddRange(activeChunks);
			}
			//Debug.Log($"Locked main thread for {TimeStamp() - startTime} MS to copy active chunk list");

			for (var i = copyOfActiveChunks.Count - 1; i > -1; --i) {
				var position = copyOfActiveChunks[i];
				if (!inRangePoints.Contains(position)) {
					copyOfUnload.Enqueue(position);
				}
			}

			foreach (var position in ordered.TakeWhile(_ => copyOfLoad.Count != maximumLoadQueueSize)
										    .Where(position => !copyOfActiveChunks.Contains(position))) {
				copyOfLoad.Enqueue(position);
			}

			TimeStamp();
			lock (shouldRenderLock) {
				while (copyOfUnload.Count > 0) {
					unloadQueue.Enqueue(copyOfUnload.Dequeue());
				}

				while (copyOfLoad.Count > 0) {
					if (loadQueue.Count == maximumLoadQueueSize) break;
					Vector2Int position = copyOfLoad.Dequeue();
					if (!loadQueue.Contains(position)) {
						loadQueue.Add(position);
					}
				}
			}
			//Debug.Log($"Locked main thread for {TimeStamp() - startTime} MS to schedule loading");
		}
	}

	private float ChunkPriority(Vector2Int position, Vector2Int cameraChunkPos, Vector3 cameraForwardFloor) {
		var distanceValue = Vector2Int.Distance(cameraChunkPos, position) / renderDistance;
		var toChunk = position - cameraChunkPos;
		var cameraForwardVector2 = new Vector2(cameraForwardFloor.x, cameraForwardFloor.z);
		var angleValue = Vector2.Angle(toChunk, cameraForwardVector2) / 140;
		return angleValue + distanceValue;
	}

	public Blocks.Block GetBlock(Vector2Int chunk, int x, int y, int z) {
		if (!chunkMap.ContainsKey(chunk)) throw new System.Exception("Chunk is not available");
		return chunkDataManager.GetBlock(chunk, x, y, z);
	}

	public bool Modify(Vector2Int chunk, int x, int y, int z, Blocks.Block blockType) {
		if (modifiedRebuildQueue.Count > 0) return false;
		if (!chunkMap.ContainsKey(chunk)) throw new System.Exception($"Chunk {chunk} is not available");
		Debug.Log($"Chunk {chunk} Modifying {x} {y} {z} {blockType}");
		chunkDataManager.Modify(chunk, x, y, z, blockType);
		var f = z == 15;
		var b = z == 0;
		var l = x == 0;
		var r = x == 15;
		if (blockType != Blocks.Air) f = b = l = r = false;
		modifyNeighborOrder.Clear();
		modifyNeighborOrder.Add(chunk);
		if (f) {
			modifyNeighborOrder.Insert(0, chunk + nFront);
		}
		else {
			modifyNeighborOrder.Add(chunk + nFront);
		}

		if (b) {
			modifyNeighborOrder.Insert(0, chunk + nBack);
		}
		else {
			modifyNeighborOrder.Add(chunk + nBack);
		}

		if (l) {
			modifyNeighborOrder.Insert(0, chunk + nLeft);
		}
		else {
			modifyNeighborOrder.Add(chunk + nLeft);
		}

		if (r) {
			modifyNeighborOrder.Insert(0, chunk + nRight);
		}
		else {
			modifyNeighborOrder.Add(chunk + nRight);
		}

		foreach (var t in modifyNeighborOrder) {
			modifiedRebuildQueue.Enqueue(t);
		}

		modifiedRebuildQueue.Enqueue(chunk + nFront + nLeft);
		modifiedRebuildQueue.Enqueue(chunk + nFront + nRight);
		modifiedRebuildQueue.Enqueue(chunk + nBack + nLeft);
		modifiedRebuildQueue.Enqueue(chunk + nBack + nRight);
		return true;
	}

	public void UnloadAll() {
		playerCanMove = false;
		//saves everything and unloads
		lock (shouldRenderLock) {
			foreach (var t in activeChunks.Where(t => !unloadQueue.Contains(t))) {
				unloadQueue.Enqueue(t);
			}

			while (true) //loop until everything is unloaded
			{
				if (unloadQueue.Count == 0) break;

				Vector2Int position = unloadQueue.Dequeue();

				if (!activeChunks.Contains(position)) continue;

				Chunk chunk = chunkMap[position];
				chunk.Unload();
				chunkPool.Enqueue(chunk);

				activeChunks.Remove(position);
				chunkMap.Remove(position);
				chunkDataManager.UnloadChunk(position);
			}

			modifiedRebuildQueue.Clear();
			modifyNeighborOrder.Clear();
			chunkMap.Clear();
			loadQueue.Clear();
			unloadQueue.Clear();
			activeChunks.Clear();
			chunkDataManager.UnloadAll();
		}

		Debug.LogWarning("Unloaded all chunks");
	}

	private void TickThread(object ticksPerSecond) {
		var tps = (int)ticksPerSecond;
		var tickDeltaMS = (int)(1000f / tps);
		var previousTick = TimeStamp();
		var nextTick = previousTick + tickDeltaMS;
		while (true) {
			Thread.Sleep(Mathf.Max((int)(nextTick - TimeStamp()), 1));
			lock (tickLock) {
				tick = true;
			}

			previousTick = nextTick;
			nextTick += tickDeltaMS;
		}
	}


	private void OnDestroy() {
		Debug.Log("OnDestroy called in ChunkManager");
		shouldRenderThread?.Abort();
		tickThread.Abort();
		Thread.Sleep(30);
	}

	private static long TimeStamp() {
		return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}
}