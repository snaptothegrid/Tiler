﻿using UnityEngine;
using System.Collections;

public class Monkey : Humanoid {

	public override void Init (Grid grid, int x, int y, float scale = 1, Sprite asset = null) {
		asset = Resources.Load<Sprite>("Tilesets/Monster/Humanoid/Monkey/Monkey/monkey-" + Random.Range(1, 7));
		base.Init(grid, x, y, scale, asset);

		isAgressive = true;
		SetEnergy(1.1f);
	}
}