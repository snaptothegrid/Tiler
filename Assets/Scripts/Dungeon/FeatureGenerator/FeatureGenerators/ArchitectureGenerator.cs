﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ArchitectureGenerator : DungeonFeatureGenerator {

	Color floorColor = new Color(0.8f, 0.8f, 0.6f);
	Color wallColor = new Color(0.8f, 0.8f, 0.6f);

	Sprite floorAsset;
	Sprite wallAsset;

	// =====================================================
	// Architecture generation
	// =====================================================

	// Generate dungeon architecture (floor, wall and door tiles) 
	// for each dungeonGenerator's tree quad recursively

	// TODO:
	// Each room or treeQuad should have a distincting aspect by theme
	
	public void GenerateArchitecture (QuadTree mainQuadTree) {
		SetRandomTheme();
		GenerateQuadTree(mainQuadTree);
	}


	public void GenerateQuadTree (QuadTree quadtree) {
		if (quadtree.HasChildren() == false) {

			

			for (int y = quadtree.boundary.BottomTile(); y <= quadtree.boundary.TopTile() - 1; y++) {
				for (int x = quadtree.boundary.LeftTile(); x <= quadtree.boundary.RightTile() - 1; x++) {
					// get dungeon tile on the quadtree zone
					DungeonTile dtile = dungeonGenerator.tiles[y, x];

					// In case we want to use different color in each quadtree zone...
					//Color color = quadtree.color;
					
					// create floors
					if (dtile.id == DungeonTileType.ROOM || dtile.id == DungeonTileType.CORRIDOR || 
						dtile.id == DungeonTileType.DOORH || dtile.id == DungeonTileType.DOORV) {

						Sprite asset = floorAsset;;
						if (floorAsset == null) {
							asset = Resources.Load<Sprite>("Tilesets/Dungeon/Floor/floor-" + Random.Range(1, 5));
						}
						
						Floor floor = (Floor)grid.CreateTile(typeof(Floor), x, y, 1, asset) as Floor;
						floor.SetColor(floorColor, true);
						//floor.SetColor(quadtree.color, true);

						// set room info in floor tile
						if (dtile.room != null) {
							floor.roomId = dtile.room.id;
						}
					}

					// create walls
					if (dtile.id == DungeonTileType.WALL || dtile.id == DungeonTileType.WALLCORNER) {
						grid.CreateTile(typeof(Tile), x, y, 1, null);

						Sprite asset = wallAsset;;
						if (wallAsset == null) {
							asset = Resources.Load<Sprite>("Tilesets/Dungeon/Wall/stone-" + Random.Range(1, 5));
						}

						Wall wall = (Wall)grid.CreateEntity(typeof(Wall), x, y, 1, asset) as Wall;
						wall.SetColor(wallColor, true);
					}
					
					// create doors
					if (dtile.id == DungeonTileType.DOORH || dtile.id == DungeonTileType.DOORV) {
						Door door = (Door)grid.CreateEntity(typeof(Door), x, y, 1, null) as Door;
						EntityStates[] states = new EntityStates[] { 
							EntityStates.Open, EntityStates.Closed, EntityStates.Locked 
						};
						door.SetState(states[Random.Range(0, states.Length)]);
					}
				}
			}
		} else {
			// Keep iterating on the quadtree
			GenerateQuadTree(quadtree.northWest);
			GenerateQuadTree(quadtree.northEast);
			GenerateQuadTree(quadtree.southWest);
			GenerateQuadTree(quadtree.southEast);
		}
	}


	private void SetRandomTheme () {
		Color baseColor = new Color(Random.Range(0.3f, 1f), Random.Range(0.3f, 1f), Random.Range(0.3f, 1f));
		floorColor = baseColor * 1f;
		wallColor = baseColor * 1f;

		floorAsset = Resources.Load<Sprite>("Tilesets/Dungeon/Floor/floor-gray-" + Random.Range(1, 6));
		wallAsset = Resources.Load<Sprite>("Tilesets/Dungeon/Wall/wall-gray-" + Random.Range(1, 8));
	}

}
