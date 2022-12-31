using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MenuSelectWorld : MonoBehaviour {
	public MenuWorldElement worldElementPrefab;
	private List<MenuWorldElement> _elements;

	void OnEnable() {
		if (_elements != null) {
			for (int i = _elements.Count - 1; i > -1; --i) {
				Destroy(_elements[i].gameObject);
			}

			_elements.Clear();
		}
		else {
			_elements = new List<MenuWorldElement>();
		}

		DirectoryInfo worldFolder = new DirectoryInfo(Application.persistentDataPath + "/Worlds");
		if (worldFolder.Exists) {
			foreach (DirectoryInfo d in worldFolder.GetDirectories()) {
				FileInfo worldInfoFile = new FileInfo(d.FullName + "/Info.json");
				if (worldInfoFile.Exists) {
					WorldInfo worldInfo = JsonUtility.FromJson<WorldInfo>(File.ReadAllText(worldInfoFile.FullName));
					MenuWorldElement element = Instantiate(worldElementPrefab);
					element.worldInfo = worldInfo;
					element.transform.SetParent(worldElementPrefab.transform.parent);
					element.transform.localScale = worldElementPrefab.transform.localScale;
					element.worldName.text = worldInfo.name;
					FileInfo thumbnail = new FileInfo(d.FullName + "/Thumbnail.png");
					if (thumbnail.Exists) {
						byte[] thumbnailBytes = File.ReadAllBytes(thumbnail.FullName);
						Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
						texture.LoadImage(thumbnailBytes);
						texture.Apply();
						element.thumbnail.texture = texture;
					}

					_elements.Add(element);
					element.gameObject.SetActive(true);
				}
			}
		}

		worldElementPrefab.gameObject.SetActive(false);
	}
}