﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


public class Controls : MonoBehaviour {

	private Grid grid;
	

	void Awake () {
		grid = Grid.instance;
	}
	
	void Update () {
		// escape if mouse is over hud
		if (EventSystem.current.IsPointerOverGameObject()) {
			return;
		}

		if (Input.GetMouseButtonDown(0)) {
			TapAtPos(Input.mousePosition);
		}

		if (Input.GetMouseButtonDown(1)) {
			InfoAtPos(Input.mousePosition);
		}
	}


	private void TapAtPos (Vector3 pos) {
		if (!CanTap()) {
			if (grid.player.state == CreatureStates.Moving) {
				grid.player.markedToStop = true;
			}
			return;
		}

		// get tap position in world/grid units
		pos = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, Camera.main.nearClipPlane));

		// get goal coordinates
		int x = Mathf.RoundToInt(pos.x);
		int y = Mathf.RoundToInt(pos.y);

		// escape if coordinates are out of grid bounds
		if (pos.x < 0 || pos.y < 0 || pos.x > grid.width - 1 || pos.y > grid.height - 1) {
			return;
		}

		// tell player to look for an astar path and follow it
		grid.player.markedToStop = false;
		grid.player.SetPath(x, y);
	}


	private bool CanTap () {
		// make sure player is in idle state
		if (grid.player.state != CreatureStates.Idle) {
			return false;
		}

		// make sure that all monsters are in idle state
		Monster[] monsters = FindObjectsOfType<Monster>();
		int c = 0;
		foreach (Monster monster in monsters) {
			if (monster.state == CreatureStates.Idle) {
				c++;
			}
		}

		//print ("Idle monsters " + c + "/" + monsters.Length);
		if (c == monsters.Length) {
			return true;
		}

		return false;
	}


	private void InfoAtPos (Vector3 pos) {
		// get tap position in world/grid units
		pos = Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, Camera.main.nearClipPlane));

		// get goal coordinates
		int x = Mathf.RoundToInt(pos.x);
		int y = Mathf.RoundToInt(pos.y);

		Creature creature = grid.GetCreature(x, y);
		if (creature != null) {
			Hud.instance.Log("You see " + 
				Descriptions.GetTileDescription(creature) + " " + 
				Descriptions.GetEquipmentDescription(creature)
			);
			return;
		}

		Entity entity = grid.GetEntity(x, y);
		if (entity != null) {
			Hud.instance.Log("You see " + Descriptions.GetTileDescription(entity));
			return;
		}

		Tile tile = grid.GetTile(x, y);
		if (tile != null) {
			Hud.instance.Log("You see " + Descriptions.GetTileDescription(tile));
			return;
		}
	}

}
