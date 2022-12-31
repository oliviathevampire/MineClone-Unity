using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Item {
	// item properties

	public string name;
	public bool valid;
	public int amount;

	public Item(ItemTemplate template, int _amount = 1) {
		name = template.name;
		amount = _amount;
		valid = true;
	}

	public bool TemplateExist() {
		return name != null && ItemTemplate.Dictionary.ContainsKey(name);
	}

	public ItemTemplate template => ItemTemplate.Dictionary[name];

	public string category => template.category;

	public int usageHp => template.usageHp;

	public bool usageDestroy => template.usageDestroy;

	public Sprite image => template.image;

	public int equipDamageBonus => template.equipDamageBonus;

	public int equipDefenseBonus => template.equipDefenseBonus;

	public int equipHpBonus => template.equipHpBonus;

	public bool destroyable => template.destroyable;

	public GameObject _3dmodel => template._3dmodel;

	public GameObject spawnOnUse => template.spawnOnUse;

	public string ToolTip() {
		var tip = template.tooltip;

		tip = tip.Replace("{NAME}", name);
		tip = tip.Replace("{CATEGORY}", category);
		tip = tip.Replace("{EQUIPDAMAGEBONUS}", equipDamageBonus.ToString());
		tip = tip.Replace("{EQUIPDEFENSEBONUS}", equipDefenseBonus.ToString());
		tip = tip.Replace("{USAGEHP}", usageHp.ToString());
		tip = tip.Replace("{EQUIPHPBONUS}", equipHpBonus.ToString());
		tip = tip.Replace("{DESTROYABLE}", destroyable ? "Yes" : "No");
		tip = tip.Replace("{AMOUNT}", amount.ToString());

		return tip;
	}
}