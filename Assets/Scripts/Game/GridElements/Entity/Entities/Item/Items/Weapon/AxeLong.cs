﻿using UnityEngine;
using System.Collections;

public class AxeLong : Weapon {

	
	public override void Init (Grid grid, int x, int y, float scale = 1, Sprite asset = null) {
		string assetName = GetRandomAssetName();
		string path = "Tilesets/Item/Weapon/Melee/" + assetName;
		asset = Resources.Load<Sprite>(path);
		if (asset == null) { Debug.LogError(path); }

		base.Init(grid, x, y, scale, asset);

		// stats
		this.damage = "1d10";
		this.speed = 1;
		this.range = 1;
	}


	protected override string GetRandomAssetName () {
		string[] arr = new string[] { "axe-long" };
		return arr[Random.Range(0, arr.Length)];
	}

}
