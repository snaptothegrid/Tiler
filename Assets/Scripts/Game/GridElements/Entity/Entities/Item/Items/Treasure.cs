﻿using UnityEngine;
using System.Collections;

using AssetLoader;


public class Treasure : Item {

	public override void Init (Grid grid, int x, int y, float scale = 1, Sprite asset = null, string id = null) {
		Sprite[] assets = Assets.GetCategory("Item/Treasure");
		asset = assets[Random.Range(0, assets.Length)];

		base.Init(grid, x, y, scale, asset);
		walkable = true;

		SetImages(scale, Vector3.zero, 0.04f);

		ammount = Random.Range(1, 10);
	}


	protected override string GetRandomAssetName () {
		string[] arr = new string[] { "gold" };

		return arr[Random.Range(0, arr.Length)];
	}

	public override CreatureInventoryItem Pickup(Creature creature) {
		creature.stats.gold += ammount;

		if (creature.visible) {
			Speak("+" + ammount, Color.yellow);
		}

		return base.Pickup(creature);
	}


	public override void PlaySoundPickup () {
		sfx.Play("Audio/Sfx/Item/treasure", 0.15f, Random.Range(0.5f, 1.0f));
	}

	public override void PlaySoundUse () {
		sfx.Play("Audio/Sfx/Item/treasure", 0.15f, Random.Range(0.5f, 1.0f));
	}
	
}
