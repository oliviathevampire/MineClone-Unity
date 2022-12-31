using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
	public static GameManager Instance { get; private set; }
	public bool showLoadingScreen = true;
	public Player player;
	public World world;
	public GameSettings gameSettings;
	public UI ui;
	private SaveDataManager _saveDataManager;
	public TextureMapper textureMapper;
	public AudioManager audioManager;
	public bool isInStartup = false;
	public WorldInfo testWorld;
	public Texture2D textures, uvTexture;
	public Camera screenshotCamera;
	public Texture2D latestScreenshot;

	public TextMeshProUGUI debugText;
	private static readonly int SkyColorTop = Shader.PropertyToID("_SkyColorTop");
	private static readonly int SkyColorHorizon = Shader.PropertyToID("_SkyColorHorizon");
	private static readonly int SkyColorBottom = Shader.PropertyToID("_SkyColorBottom");
	private static readonly int MinLightLevel = Shader.PropertyToID("_MinLightLevel");
	private static readonly int RenderDistance = Shader.PropertyToID("_RenderDistance");
	private static readonly int BlockTextures = Shader.PropertyToID("_BlockTextures");
	private static readonly int UVTexture = Shader.PropertyToID("_UVTexture");

	private void Start() {
		Instance = this;
		Initialize();
		BlockTypes.Initialize();
		textureMapper = new TextureMapper();

		if (AudioManager.instance == null) {
			audioManager.Initialize();
		}

		audioManager = AudioManager.instance;


		CreateTextures();
		Structure.Initialize();

		var worldInfo = MainMenu.worldToLoad != null ? MainMenu.worldToLoad : testWorld;
		InitializeWorld(worldInfo);

		ui.Initialize();

		//_ColorHorizon, _ColorTop, _ColorBottom;
		Shader.SetGlobalColor(SkyColorTop, new Color(0.7692239f, 0.7906416f, 0.8113208f, 1f));
		Shader.SetGlobalColor(SkyColorHorizon, new Color(0.3632075f, 0.6424405f, 1f, 1f));
		Shader.SetGlobalColor(SkyColorBottom, new Color(0.1632253f, 0.2146282f, 0.2641509f, 1f));
		Shader.SetGlobalFloat(MinLightLevel, gameSettings.minimumLightLevel);
		Shader.SetGlobalInt(RenderDistance, gameSettings.renderDistance);

		#if !UNITY_EDITOR
		showLoadingScreen = true;
		#endif
		if (!showLoadingScreen) return;
		isInStartup = true;
		world.chunkManager.isInStartup = isInStartup;
		ui.loadingScreen.gameObject.SetActive(true);
	}

	private void Update() {
		debugText.text = "";
		if (!audioManager.IsPlayingMusic()) {
			audioManager.PlayNewPlaylist(isInStartup ? audioManager.music.menu.clips : audioManager.music.game.clips);
		}
		else {
			if (!isInStartup) {
				if (audioManager.musicPlaylist != audioManager.music.game.clips) {
					audioManager.PlayNewPlaylist(audioManager.music.game.clips);
				}
			}
		}

		if (isInStartup) {
			if (world.chunkManager.StartupFinished()) {
				world.chunkManager.isInStartup = false;
				isInStartup = false;
				ui.loadingScreen.gameObject.SetActive(false);
				audioManager.PlayNewPlaylist(audioManager.music.game.clips);
				GC.Collect();
			}
		}

		player.disableInput = ui.console.gameObject.activeSelf;
		player.UpdatePlayer();
		world.UpdateWorld();
		ui.UpdateUI();
		DebugStuff();
	}

	private void Initialize() {
		_saveDataManager = new SaveDataManager();
	}

	private void InitializeWorld(WorldInfo worldInfo) {
		worldInfo = _saveDataManager.Initialize(worldInfo);
		world.Initialize(worldInfo);
	}

	private void CreateTextures() {
		var temp = new Texture2D(textures.width, textures.height, TextureFormat.ARGB32, 5, false);
		temp.SetPixels(textures.GetPixels());
		temp.filterMode = FilterMode.Point;
		temp.Apply();
		textures = temp;
		Shader.SetGlobalTexture(BlockTextures, textures);
		Shader.SetGlobalTexture(UVTexture, uvTexture);
	}

	public void AddDebugLine(string line) {
		debugText.text += line + "\n";
	}

	private void DebugStuff() {
		if (Input.GetKeyDown(KeyCode.F3)) {
			debugText.gameObject.SetActive(!debugText.gameObject.activeSelf);
		}

		//360 screenshot
		if (Input.GetKeyDown(KeyCode.F4)) {
			var cubemap = new RenderTexture(4096, 4096, 0, RenderTextureFormat.ARGB32) {
				dimension = UnityEngine.Rendering.TextureDimension.Cube
			};
			cubemap.Create();
			screenshotCamera.transform.position = world.mainCamera.transform.position;
			screenshotCamera.RenderToCubemap(cubemap);

			var equirect = new RenderTexture(4096, 2048, 0, RenderTextureFormat.ARGB32);
			var texture = new Texture2D(4096, 2048, TextureFormat.ARGB32, false);
			cubemap.ConvertToEquirect(equirect);
			var temp = RenderTexture.active;
			RenderTexture.active = equirect;
			texture.ReadPixels(new Rect(0, 0, equirect.width, equirect.height), 0, 0);
			RenderTexture.active = temp;
			texture.Apply();
			latestScreenshot = texture;
			var file =
				new System.IO.FileInfo(Application.persistentDataPath + "/" + TimeStamp() + ".png");
			System.IO.File.WriteAllBytes(file.FullName, texture.EncodeToPNG());
		}

		if (Input.GetKeyDown(KeyCode.F8)) {
			UnloadAll(); //refresh test
		}
	}

	private void UnloadAll() {
		CreateScreenshot();
		world.chunkManager.UnloadAll();
	}

	private void OnApplicationQuit() {
		if (!enabled) return;
		Debug.Log("OnApplicationQuit called in GameManager");
		UnloadAll();
		enabled = false;
	}

	private void OnDestroy() {
		if (!enabled) return;
		Debug.Log("OnDestroy called in GameManager");
		UnloadAll();
		enabled = false;
	}

	private void CreateScreenshot() {
		RenderTexture temporary = RenderTexture.GetTemporary(256, 144, 0, RenderTextureFormat.ARGB32);
		if (world.mainCamera != null) {
			screenshotCamera.transform.position = world.mainCamera.transform.position;
			screenshotCamera.transform.rotation = world.mainCamera.transform.rotation;
			screenshotCamera.fieldOfView = world.mainCamera.fieldOfView;
		}
		screenshotCamera.targetTexture = temporary;
		screenshotCamera.Render();
		Texture2D texture = new Texture2D(256, 144, TextureFormat.ARGB32, false);

		RenderTexture temp = RenderTexture.active;
		RenderTexture.active = temporary;
		texture.ReadPixels(new Rect(0, 0, temporary.width, temporary.height), 0, 0);
		RenderTexture.active = temp;
		texture.Apply();
		latestScreenshot = texture;
		RenderTexture.ReleaseTemporary(temporary);
		WorldInfo info = world.info;
		var thumb =
			new System.IO.FileInfo(Application.persistentDataPath + "/Worlds/" + info.id + "/Thumbnail.png");
		if (thumb == null) throw new ArgumentNullException(nameof(thumb));
		System.IO.File.WriteAllBytes(thumb.FullName, texture.EncodeToPNG());
	}

	private long TimeStamp() {
		return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
	}
}