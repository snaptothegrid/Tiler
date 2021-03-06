using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]

public class DungeonRoom {

	// to avoid unity 4.5 "exceded depth" warnings
	[System.NonSerialized] 

	public AABB boundary;
	public QuadTree quadtree;

	public int id;
	public Color color = Color.white;
	public List<DungeonTile> tiles;


	public DungeonRoom (int id) {
		this.id = id;
		this.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		this.tiles = new List<DungeonTile>();
	}
	

	public DungeonRoom (int id, AABB b) {
		boundary = b;

		this.id = id;
		this.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		this.tiles = new List<DungeonTile>();
	}

	
	public DungeonRoom (int id, AABB b, QuadTree q) {
		boundary = b;
		quadtree = q;
		quadtree.room = this;

		this.id = id;
		this.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		this.tiles = new List<DungeonTile>();
	}
}
