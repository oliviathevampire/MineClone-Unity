using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour {
	private const int HOTBAR_LENGTH = 12;
	[SerializeField] private int[] highlightPositions = new int[HOTBAR_LENGTH];
	public Slot[] slots = new Slot[HOTBAR_LENGTH];
	public RawImage elementPrefab;
	public int[] elements = new int[HOTBAR_LENGTH];
	public RawImage[] elementGraphics = new RawImage[HOTBAR_LENGTH];
	public int currentHighlighted;
	public RectTransform currentSelectedGraphic;
	public bool requiresAnotherKey = false;
	public KeyCode additionalKeyCode = KeyCode.None;
	public Hotbar otherHotbar;

	public void Initialize() {
		for (var i = 0; i < HOTBAR_LENGTH; ++i) {
			var elementGraphic = Instantiate(elementPrefab, elementPrefab.transform.parent, true);
			elementGraphic.rectTransform.localScale = Vector3.one;
			var anchoredPosition = elementGraphic.rectTransform.anchoredPosition;
			anchoredPosition.x = -110 + 20 * i;
			anchoredPosition.y = elementPrefab.rectTransform.anchoredPosition.y;
			elementGraphic.rectTransform.anchoredPosition = anchoredPosition;
			elementGraphics[i] = elementGraphic;
		}

		elementPrefab.gameObject.SetActive(false);
		Rebuild();
	}

	public void UpdateHotbar() {
		var scroll = Input.mouseScrollDelta.y;
		switch (requiresAnotherKey) {
			case true: {
				if (scroll != 0 && Input.GetKeyDown(additionalKeyCode)) {
					var scrollDirection = (int)Mathf.Sign(scroll);
					currentHighlighted -= scrollDirection;
					
					if (otherHotbar != null && currentHighlighted > HOTBAR_LENGTH - 1) {
						switch (currentHighlighted) {
							case < 0:
								otherHotbar.currentHighlighted += HOTBAR_LENGTH;
								break;
							case > HOTBAR_LENGTH - 1:
								currentHighlighted -= HOTBAR_LENGTH;
								break;
						}
					}
					Rebuild();
				}

				break;
			}
			case false: {
				if (scroll != 0) {
					var scrollDirection = (int)Mathf.Sign(scroll);
					currentHighlighted -= scrollDirection;
					if (currentHighlighted < 0) currentHighlighted += HOTBAR_LENGTH;
					if (currentHighlighted > HOTBAR_LENGTH - 1) currentHighlighted -= HOTBAR_LENGTH;
					
					Rebuild();
				}

				break;
			}
		}
		

		const int alpha1 = (int)KeyCode.Alpha1;
		for (var i = 0; i < HOTBAR_LENGTH; ++i) {
			var keycode = (alpha1 + i) switch {
							  10 => KeyCode.Alpha0,
							  11 => KeyCode.Plus,
							  12 => KeyCode.Backslash,
							  _ => (KeyCode)(alpha1 + 1)
						  };

			switch (requiresAnotherKey) {
				case true when !(Input.GetKeyDown(keycode) && Input.GetKeyDown(additionalKeyCode)):
					continue;
				case true:
					currentHighlighted = i;
					Rebuild();
					break;
				case false when !Input.GetKeyDown(keycode):
					continue;
				case false:
					currentHighlighted = i;
					Rebuild();
					break;
			}
		}
	}

	private void Rebuild() {
		var graphicAnchoredPosition = currentSelectedGraphic.anchoredPosition;
		graphicAnchoredPosition.x = highlightPositions[currentHighlighted];
		currentSelectedGraphic.anchoredPosition = graphicAnchoredPosition;
		var textureMapper = GameManager.Instance.textureMapper;
		for (var i = 0; i < HOTBAR_LENGTH; i++) {
			var rawImage = elementGraphics[i];
			var textureId = slots[i].blockId;

			rawImage.enabled = true;
			var textureMap = textureMapper.map[BlockTypes.byteToBlock[textureId]];
			var face = textureMap.front;
			var uvRect = new Rect(
				1.0f / 256 * face.bl.x * 16,
				1 - 1.0f / 256 * face.bl.y * 16,
				1.0f / 256 * 16,
				1.0f / 256 * 16
			);
			rawImage.uvRect = uvRect;
		}
	}

	public Blocks.Block GetCurrentHighlighted() {
		return BlockTypes.byteToBlock[(byte)elements[currentHighlighted]];
	}
}