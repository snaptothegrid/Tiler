﻿using UnityEngine;
using System.Collections;

public class Gorilla : Humanoid {

	public override void Init (Grid grid, int x, int y, float scale = 1, Sprite asset = null) {
		asset = Resources.Load<Sprite>("Tilesets/Monster/Humanoid/Monkey/Gorilla/gorilla-" + Random.Range(1, 5));
		base.Init(grid, x, y, scale, asset);

		stats.energyRate = 1.2f;
		stats.energy = Mathf.Max(1f, stats.energyRate);
	}
}
