﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class ItemGenerator : DungeonFeatureGenerator {

	// =====================================================
	// Item generation
	// =====================================================

	// generate random items in dungeon level

	public override void Generate (int chancePerRoom, float ratioPerFreeTile) {
		Random.seed = Dungeon.seed;

		// generate equipment rarity table dictionary
		int minRarity = GameData.GetDefaultEquipmentMinRarity();
		Dictionary<string, double> rarities = GameData.GenerateEquipmentRarityTable(minRarity);

		for (int n = 0; n < dungeonGenerator.rooms.Count; n++) {

			DungeonRoom room = dungeonGenerator.rooms[n];
			int maxItems = Random.Range(0, 100) <= chancePerRoom ? Random.Range(1, Mathf.RoundToInt((room.tiles.Count * ratioPerFreeTile))) : 0;

			//Debug.Log (">>> " + maxItems + " " + Mathf.RoundToInt((room.tiles.Count * ratioPerFreeTile));

			for (int i = 1; i <= maxItems; i ++) {
				Tile tile = GetFreeTileOnRoom(room, 0);
				if (tile == null) { continue; }

				// Pick a weighted random item type
				System.Type itemType = Dice.GetRandomTypeFromDict(new Dictionary<System.Type, double>() {
					{ typeof(Equipment), 	100 },
					{ typeof(Treasure), 	60 },
					{ typeof(Food), 		20 },
					{ typeof(Potion), 		20 },
					{ typeof(Book), 		20 },
				});

				// pick a random id
				string id = Dice.GetRandomStringFromDict(rarities);
				
				// create item
				grid.CreateEntity(itemType, tile.x, tile.y, 0.8f, null, id);
			}
		}
	}


	// generate maxItems inside given container or creature

	public override void Generate (Tile tile, int maxItems, int minRarity = 100) {
		// generate equipment rarity table dictionary
		Dictionary<string, double> rarities = GameData.GenerateEquipmentRarityTable(minRarity);

		for (int i = 0; i < maxItems; i++) {
			System.Type itemType = tile.GetRandomItemType();

			// pick a random id
			string id = Dice.GetRandomStringFromDict(rarities);
				
			// create item
			GenerateSingle(tile, itemType, id);
		}
	}


	public Item GenerateSingle (Tile tile, System.Type type, string id) {
		// create item
		Item item = (Item)grid.CreateEntity(type, 0, 0, 0.8f, null, id, false) as Item;
		
		// put the item inside the container
		item.transform.SetParent(tile.transform, false);
		item.transform.localPosition = Vector3.zero;
		item.gameObject.SetActive(false);

		// add to container's item list
		if (tile is Container) {
			((Container)tile).items.Add(item);
			return item;
		}

		// add to creature's inventory
		if (tile is Creature) {
			((Creature)tile).inventoryModule.AddItem(item);
			return item;
		}

		return item;
	}
}
