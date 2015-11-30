﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Grid : MonoSingleton <Grid> {

	public Transform container;

	public GameObject tilePrefab;
	public GameObject entityPrefab;
	public GameObject creaturePrefab;

	public int width = 16;
	public int height = 16;

	public GridLayers layers;

	public Tile ladderUp;
	public Tile ladderDown;

	public Creature player;
	public List<Creature> monsters;


	public void InitializeGrid (int width, int height) {
		this.width = width;
		this.height = height;

		// reset grid
		ResetGrid();

		// initialize grid layers
		layers = new GridLayers () {
			{ typeof(Tile), new GridLayer<Tile> (height, width) },
			{ typeof(Entity), new GridLayer<Entity> (height, width) },
			{ typeof(Creature), new GridLayer<Creature> (height, width) },
		};

		// initialize astar
		InitializeAstar();
	}


	public void InitializeAstar () {
		Cell [,] arr = new Cell[width, height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Tile tile = GetTile(x, y);
				arr[x, y] = new Cell(x, y, tile != null && tile.IsWalkable());
			}
		}
		
		new Astar(arr);
	}


	private void ResetGrid () {
		// destroy all previously generated gameobjects

		Tile[] tilesToDestroy = container.GetComponentsInChildren<Tile>();
		foreach (Tile tile in tilesToDestroy) {
			Destroy(tile.gameObject);
		}

		Entity[] entitiesToDestroy = container.GetComponentsInChildren<Entity>();
		foreach (Entity entity in entitiesToDestroy) {
			Destroy(entity.gameObject);
		}
	}


	// Tile

	public T CreateTile <T> (int x, int y, Sprite asset, float scale = 1) where T: Tile {
		Transform parent = container.Find("Tiles");

		GameObject obj = (GameObject)Instantiate(tilePrefab);
		obj.transform.SetParent(parent, false);
		obj.name = typeof(T).ToString();

		T tile = obj.AddComponent<T>();
		tile.Init(this, x, y, asset, scale);

		SetTile(x, y, (Tile)tile);

		return tile;
	}


	public void SetTile (int x, int y, Tile tile) {
		// refresh astar walkability
		Astar.instance.walkability[x, y] = (tile != null && !tile.IsWalkable()) ? 1 : 0;

		layers.Set<Tile>(y, x, tile);
	}


	public Tile GetTile (int x, int y) {
		return layers.Get<Tile>(y, x);
	}


	// Entity

	public T CreateEntity<T> (int x, int y, Sprite asset, float scale = 1) where T: Entity {
		Transform parent = container.Find("Entities");

		GameObject obj = (GameObject)Instantiate(tilePrefab);
		obj.transform.SetParent(parent, false);
		obj.name = typeof(T).ToString();

		T entity = obj.AddComponent<T>();
		entity.Init(this, x, y, asset, scale);

		SetEntity(x, y, entity);

		return entity;
	}


	public void SetEntity (int x, int y, Entity entity) {
		// refresh astar walkability
		Astar.instance.walkability[x, y] = (entity != null && !entity.IsWalkable()) ? 1 : 0;

		layers.Set<Entity>(y, x, entity);
	}


	public Entity GetEntity (int x, int y) {
		return layers.Get<Entity>(y, x);
	}


	// Creature

	public T CreateCreature<T> (int x, int y, Sprite asset, float scale = 1) where T: Creature {
		Transform parent = container.Find("Creatures");

		GameObject obj = (GameObject)Instantiate(tilePrefab);
		obj.transform.SetParent(parent, false);
		obj.name = typeof(T).ToString();

		T creature = obj.AddComponent<T>();
		creature.Init(this, x, y, asset, scale);

		SetCreature(x, y, creature);

		return creature;
	}


	public void SetCreature (int x, int y, Creature creature) {
		// refresh astar walkability
		Astar.instance.walkability[x, y] = (creature != null && !creature.IsWalkable()) ? 1 : 0;

		layers.Set<Creature>(y, x, creature);
	}


	public Creature GetCreature (int x, int y) {
		return layers.Get<Creature>(y, x);
	}
}
