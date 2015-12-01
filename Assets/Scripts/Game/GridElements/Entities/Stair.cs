﻿using UnityEngine;
using System.Collections;


public class Stair : Entity {

	public EntityStates state { get; set; }
	public int direction = 1;

	public override void Init (Grid grid, int x, int y, Sprite asset, float scale = 1) {
		base.Init(grid, x, y, asset, scale);
		walkable = true;

		SetImages(scale, Vector3.zero, 0.04f);

		state = EntityStates.Open;
	}


	public void SetDirection (int direction) {
		this.direction = direction;

		if (direction == 1) {
			SetAsset(Game.assets.dungeon["stairs-down"]);
		} else if (direction == -1) {
			SetAsset(Game.assets.dungeon["stairs-up"]);
		}
	}

}