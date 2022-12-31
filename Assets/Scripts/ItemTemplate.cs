using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Minecraft New Item", menuName = "Minecraft/Item", order = 999)]
public class ItemTemplate : ScriptableObject {
	public string category;

	public int usageHp;

	public bool usageDestroy;

	[TextArea(1, 40)] public string tooltip;

	public Sprite image;

	public int maxStack;

	public bool destroyable;

	public int equipHpBonus;
	public int equipDefenseBonus;
	public int equipDamageBonus;

	public GameObject _3dmodel;


	[Header("Spawning")] public GameObject spawnOnUse;


	static Dictionary<string, ItemTemplate> _cache;

	public static Dictionary<string, ItemTemplate> Dictionary {
		get {
			return _cache ??= Resources.LoadAll<ItemTemplate>("").ToDictionary(
					   item => item.name, item => item);
		}
	}
}