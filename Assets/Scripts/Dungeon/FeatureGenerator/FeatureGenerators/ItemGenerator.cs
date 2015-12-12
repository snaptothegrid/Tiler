﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ItemGenerator : DungeonFeatureGenerator {

	// =====================================================
	// Item generation
	// =====================================================

	public override void Generate () {
		for (int n = 0; n < dungeonGenerator.rooms.Count; n++) {

			DungeonRoom room = dungeonGenerator.rooms[n];
			int maxItems = Random.Range(0, 100) <= 80 ? Random.Range(1, Mathf.RoundToInt((room.tiles.Count * 0.2f))) : 0;

			for (int i = 1; i <= maxItems; i ++) {
				Tile tile = GetFreeTileOnRoom(room, 0);
				if (tile == null) { continue; }

				// Pick a weighted random item type
				System.Type itemType = Dice.GetRandomTypeFromDict(new Dictionary<System.Type, double>() {
					{ typeof(Armour), 	5 },
					{ typeof(Weapon), 	5 },
					{ typeof(Treasure), 80 },
					{ typeof(Book), 	25 },
					{ typeof(Food), 	10 },
					{ typeof(Potion), 	25 },
				});
				
				grid.CreateEntity(itemType, tile.x, tile.y, 0.8f);

			}
		}
	}

}
