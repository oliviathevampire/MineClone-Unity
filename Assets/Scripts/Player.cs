using UnityEngine;

public class Player : MonoBehaviour {
	const int playerLayer = 8;
	private Vector3 euler;
	private Quaternion cameraRotation;
	private Vector3 cameraPosition;
	private long lastPressedSpace = 0;
	public bool disableInput = false;
	public State state = State.CreativeWalking;
	private bool running = false;
	private float wobble = 0;
	private float wobbleIntensity = 0;
	public Setup setup;
	private string debugInfo;

	public enum State {
		Survival = 0,
		CreativeWalking = 1,
		CreativeFlying = 2,
		Spectator = 3
	}

	[System.Serializable]
	public class Setup {
		public World world;
		public Camera mainCamera;
		public float walkSpeed, walkForce, fallSpeed, fallForce, jumpVelocity, runSpeed, runForce;
		public float fieldOfView = 60;
		public Rigidbody myRigidbody;
		public GameObject highlightPrefab;
	}

	private void Start() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		cameraRotation = Quaternion.Euler(euler);
		cameraPosition = transform.position + new Vector3(0, .5f, 0);
	}

	public void UpdatePlayer() {
		var pos = transform.position;
		var playerIntPos = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
		var playerChunk =
			new Vector2Int(Mathf.FloorToInt(playerIntPos.x / 16f), Mathf.FloorToInt(playerIntPos.z / 16f));
		debugInfo = $"Player: Position[{playerIntPos}] Chunk[{playerChunk}] State[{state}]";

		var canMove = true;
		canMove &= !GameManager.Instance.isInStartup;
		if (World.activeWorld.chunkManager != null) canMove &= World.activeWorld.chunkManager.playerCanMove;
		setup.myRigidbody.isKinematic = !canMove || state == State.Spectator;

		Vector2 movement = new Vector2();
		if (!disableInput) {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				// Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
				// Cursor.visible = Cursor.lockState == CursorLockMode.None;
			}

			movement.x += Input.GetKey(KeyCode.D) ? 1 : 0;
			movement.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
			movement.y += Input.GetKey(KeyCode.W) ? 1 : 0;
			movement.y -= Input.GetKey(KeyCode.S) ? 1 : 0;
			if (Input.GetKeyDown(KeyCode.LeftControl)) {
				if (movement != Vector2.zero) running = true;
			}
		}

		if (movement == Vector2.zero) running = false;
		var wobbleTargetIntensity = movement == Vector2.zero ? 0 : running ? 2f : 1f;
		if (state > (State)1) wobbleTargetIntensity = 0;
		wobbleIntensity = Mathf.Lerp(wobbleIntensity, wobbleTargetIntensity, Time.deltaTime * 16f);

		if (state < State.Spectator) {
			Movement(movement, running);
		}
		else {
			SpectatorMovement(movement, running);
		}

		CameraUpdate();
		BlockPlacement();
		GameManager.Instance.AddDebugLine(debugInfo);
	}

	private long TimeStamp() {
		return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}

	private void CameraUpdate() {
		var cam = setup.mainCamera;
		if (!disableInput) {
			euler.x -= Input.GetAxis("Mouse Y") * 2f;
			euler.y += Input.GetAxis("Mouse X") * 2f;
		}

		euler.x = Mathf.Clamp(euler.x, -89.99f, 89.99f);
		cameraRotation = Quaternion.Euler(euler);
		Vector3 camTargetPosition = transform.position + new Vector3(0, .5f, 0);
		cameraPosition = Vector3.Lerp(
			cameraPosition,
			camTargetPosition,
			Time.deltaTime * 20f
		);

		wobble += Time.deltaTime * 8f * (running ? 1.3f : 1f);
		cam.transform.rotation = cameraRotation;
		cam.transform.position = cameraPosition;

		cam.transform.Rotate(Vector3.forward, Mathf.Sin(wobble) * 0.2f * wobbleIntensity);
		cam.transform.Rotate(Vector3.right, Mathf.Sin(wobble * 2f) * 0.3f * wobbleIntensity);
		cam.transform.Rotate(Vector3.up, -Mathf.Sin(wobble) * 0.2f * wobbleIntensity);
		cam.transform.position += cam.transform.up * Mathf.Sin(wobble * 2f) * 0.05f * wobbleIntensity;
		cam.transform.position += cam.transform.right * Mathf.Sin(wobble) * 0.05f * wobbleIntensity;

		float fov = setup.fieldOfView + (running ? 10 : 0);
		//if (movement == Vector2.zero) fov = Input.GetKey(KeyCode.Tab) ? 10 : fov;
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, Time.deltaTime * 8f);
		if (!disableInput) {
			if (Input.GetKey(KeyCode.Tab)) cam.fieldOfView = 20;
		}

		if (Input.GetKeyUp(KeyCode.Tab)) cam.fieldOfView = fov;
	}

	private void Movement(Vector2 movement, bool running) {
		Vector3 forward = setup.mainCamera.transform.forward;
		forward.y = 0;
		forward.Normalize();

		Vector3 right = setup.mainCamera.transform.right;
		right.y = 0;
		right.Normalize();

		float moveForce = running ? setup.runForce : setup.walkForce;
		float moveSpeed = running ? setup.runSpeed : setup.walkSpeed;

		Vector3 stillVelocity = setup.myRigidbody.velocity;
		stillVelocity.x = 0;
		stillVelocity.z = 0;
		setup.myRigidbody.velocity = Vector3.Lerp(setup.myRigidbody.velocity, stillVelocity, Time.deltaTime * 8f);

		setup.myRigidbody.AddForce(forward * movement.y * (moveForce * Time.deltaTime));
		setup.myRigidbody.AddForce(right * movement.x * (moveForce * Time.deltaTime));
		if (state < (State)2) {
			setup.myRigidbody.AddForce(Vector3.down * setup.fallForce * Time.deltaTime);
		}

		Vector3 velocityWalk = setup.myRigidbody.velocity;
		velocityWalk.y = 0;
		Vector3 velocityFall = setup.myRigidbody.velocity;
		velocityFall.x = 0;
		velocityFall.z = 0;
		if (velocityWalk.magnitude > moveSpeed) {
			velocityWalk = velocityWalk.normalized * moveSpeed;
		}

		if (velocityFall.magnitude > setup.fallSpeed) {
			velocityFall = velocityFall.normalized * setup.fallSpeed;
		}

		Vector3 targetVelocity = velocityWalk + velocityFall;

		if (!disableInput) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				var timestamp = TimeStamp();
				if (timestamp < lastPressedSpace + 500) {
					state = state switch {
								State.CreativeWalking => State.CreativeFlying,
								State.CreativeFlying => State.CreativeWalking,
								_ => state
							};
					lastPressedSpace = 0;
				}
				else {
					lastPressedSpace = TimeStamp();
					targetVelocity.y = setup.jumpVelocity;
				}
			}
		}

		if (state > (State)1) {
			targetVelocity.y = 0;
			if (!disableInput) {
				if (Input.GetKey(KeyCode.Space)) {
					targetVelocity.y += 8;
				}

				if (Input.GetKey(KeyCode.LeftShift)) {
					targetVelocity.y -= 8;
				}
			}
		}

		//myRigidbody.velocity = Vector3.Lerp(myRigidbody.velocity, targetVelocity, Time.deltaTime * 8f);
		setup.myRigidbody.velocity = targetVelocity;
	}

	private void BlockPlacement() {
		int layerMask = ~(1 << playerLayer);
		RaycastHit hitInfo;
		if (Physics.Raycast(setup.mainCamera.transform.position, setup.mainCamera.transform.forward, out hitInfo, 1024,
						    layerMask)) {
			Vector3 inCube = hitInfo.point - hitInfo.normal * 0.5f;
			Vector3Int removeBlock = new Vector3Int(
				Mathf.FloorToInt(inCube.x),
				Mathf.FloorToInt(inCube.y),
				Mathf.FloorToInt(inCube.z)
			);
			Vector3 fromCube = hitInfo.point + hitInfo.normal * 0.5f;
			Vector3Int placeBlock = new Vector3Int(
				Mathf.FloorToInt(fromCube.x),
				Mathf.FloorToInt(fromCube.y),
				Mathf.FloorToInt(fromCube.z)
			);
			debugInfo += $" Target[Pos: {removeBlock}]";
			if (hitInfo.distance < 7) {
				//Debug.Log(hitInfo.collider.gameObject.name);


				bool remove = false;
				bool place = false;
				if (!disableInput) {
					remove |= Input.GetKeyDown(KeyCode.Mouse0);
					remove |= (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Mouse0));

					place |= Input.GetKeyDown(KeyCode.Mouse1);
					place |= (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Mouse1));
				}

				if (remove) {
					Blocks.Block blockToReplace = setup.world.GetBlock(removeBlock.x, removeBlock.y, removeBlock.z);
					if (setup.world.Modify(removeBlock.x, removeBlock.y, removeBlock.z, Blocks.Air)) {
						AudioManager.instance.dig.Play(blockToReplace.GetMaterial().GetDigSound(), removeBlock);
						place = false;
					}
				}

				if (place) {
					var block = UI.Instance.hotbar.GetCurrentHighlighted();
					if (setup.world.Modify(placeBlock.x, placeBlock.y, placeBlock.z, block)) {
						AudioManager.instance.dig.Play(block.GetMaterial().GetDigSound(), removeBlock);
					}
				}

				setup.highlightPrefab.transform.position = removeBlock + new Vector3(.5f, .5f, .5f);
				setup.highlightPrefab.SetActive(true);
				return;
			}
		}

		setup.highlightPrefab.SetActive(false);
	}

	private void SpectatorMovement(Vector2 movement, bool running) {
		float moveSpeed = running ? 64 : 32;

		Vector3 forward = setup.mainCamera.transform.forward;
		forward.y = 0;
		forward.Normalize();

		Vector3 right = setup.mainCamera.transform.right;
		right.y = 0;
		right.Normalize();

		float altitude = 0;

		if (Input.GetKey(KeyCode.Space)) {
			altitude += 20;
		}

		if (Input.GetKey(KeyCode.LeftShift)) {
			altitude -= 20;
		}

		transform.position += movement.y * forward * Time.deltaTime * moveSpeed;
		transform.position += movement.x * right * Time.deltaTime * moveSpeed;
		transform.position += Vector3.up * altitude * Time.deltaTime;
	}
}