﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Vase : Container {

	public override void Init (Grid grid, int x, int y, float scale = 1, Sprite asset = null) {
		this.assetType = GetRandomAssetName();
		asset = Resources.Load<Sprite>("Tilesets/Container/" + assetType + "-closed");

		base.Init(grid, x, y, scale, asset);
		
		breakable = true;
		maxItems = 1;
	}

	protected override string GetRandomAssetName () {
		string[] arr = new string[] { "vase" };
		return arr[Random.Range(0, arr.Length)];
	}


	protected override System.Type GetRandomItemType () {
		// Pick a weighted random item type
		return Dice.GetRandomTypeFromDict(new Dictionary<System.Type, double>() {
			{ typeof(Armour), 	10 },
			{ typeof(Weapon), 	10 },
			{ typeof(Treasure), 5 },
			{ typeof(Book), 	10 },
			{ typeof(Food), 	5 },
			{ typeof(Potion), 	10 },
		});
	}

}