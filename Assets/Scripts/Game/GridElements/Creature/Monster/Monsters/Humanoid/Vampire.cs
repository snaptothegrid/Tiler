﻿using UnityEngine;
using System.Collections;

public class Vampire : Humanoid {

	public override void Init (Grid grid, int x, int y, float scale = 1, Sprite asset = null) {
		asset = Resources.Load<Sprite>("Tilesets/Monster/Humanoid/Vampire/vampire-" + Random.Range(1, 4));
		base.Init(grid, x, y, scale, asset);

		stats.energyRate = 1.5f;
		stats.energy = Mathf.Max(1f, stats.energyRate);
	}
}
