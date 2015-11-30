﻿using UnityEngine;
using System.Collections;


public class Entity: Tile {

	public override void Init (Grid grid, int x, int y, Sprite asset, float scale = 1) {
		base.Init(grid, x, y, asset, scale);
		walkable = false;

		SetSortingOrder(100);
		SetImage(scale, new Vector3(0, 0.1f, 0), 0.04f);
	}
}