﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Creature : Entity {

	public int hp = 5;

	protected List<Vector2> path;
	protected float speed = 0.15f;
	protected bool moving = false;


	public override void Init (Grid grid, int x, int y, float scale = 1, Sprite asset = null) {
		base.Init(grid, x, y, scale, asset);
		walkable = false;

		SetImages(scale, new Vector3(0, 0.1f, 0), 0.04f);
		LocateAtCoords(x, y);
	}


	protected virtual void LocateAtCoords (int x, int y) {
		grid.SetCreature(this.x, this.y, null);
		this.x = x;
		this.y = y;
		grid.SetCreature(x, y, this);

		transform.localPosition = new Vector3(x, y, 0);

		SetSortingOrder(200);
	}


	// =====================================================
	// Path
	// =====================================================

	public virtual void SetPath (int x, int y) {
		// clear previous path
		if (path != null) {
			DrawPath(Color.white);
		}

		// if already moving, abort current move
		if (moving) {
			StopMoving();
			return;
		}

		// if goal is the creature's tile, wait one turn instead
		if (x == this.x && y == this.y) {
			Hud.instance.Log("You wait...");
			return;
		}

		// search for new path
		path = Astar.instance.SearchPath(grid.player.x, grid.player.y, x, y);
		path = SetPathAfterEncounter(path);

		// render new path
		DrawPath(Color.magenta);

		// follow new path
		StartCoroutine(FollowPath());
	}


	protected void DrawPath (Color color) {
		foreach (Vector2 p in path) {
			Tile tile = grid.GetTile((int)p.x, (int)p.y);
			if (tile != null) {
				tile.SetColor(color);
			}
		}
	}


	// =====================================================
	// Encounters
	// =====================================================

	private List<Vector2> SetPathAfterEncounter (List<Vector2> path) {
		int i;
		for (i = 0; i < path.Count; i ++) {
			Vector2 p = path[i];
			int x = (int)p.x;
			int y = (int)p.y;

			Entity entity = grid.GetEntity(x, y);

			// closed door
			if (entity is Door) {
				Door door = (Door)entity;
				if (door.state != EntityStates.Open) {
					break;
				}
			}
		}

		if (i < path.Count -1) {
			path.RemoveRange(i + 1, path.Count - 1 - i);
		}

		return path;
	}


	private IEnumerator ResolveEncounters (int x, int y) {
		Entity entity = grid.GetEntity(x, y);
		if (entity != null) {

			// resolve doors
			if (entity is Door) {
				Door door = (Door)entity;
				if (door.state != EntityStates.Open) {
					DrawPath(Color.white);

					// closed door
					if (door.state == EntityStates.Closed) { 
						// open door
						moving = false;
						StartCoroutine(door.Open()); 
						Hud.instance.Log("You open the door.");
			
					} else if (door.state == EntityStates.Locked) { 
						// unlock door
						moving = false;
						StartCoroutine(door.Unlock(success => {}));
						Hud.instance.Log("You unlock the door.");
					}
				}
			}
		}

		yield break;
	}


	private IEnumerator ResolveEncountersAtGoal (int x, int y) {
		Entity entity = grid.GetEntity(x, y);
		if (entity != null) {

			// resolve stairs
			if (entity is Stair) {
				StopMoving();

				yield return new WaitForSeconds(0.25f);

				Stair stair = (Stair)entity;
				Dungeon.instance.ExitLevel (stair.direction);

			}
		}

		yield break;
	}


	// =====================================================
	// Movement
	// =====================================================

	protected IEnumerator FollowPath () {
		moving = true;

		for (int i = 0; i < path.Count; i++) {
			yield return StartCoroutine (FollowPathStep(i));
		}

		// after moving, check for encounters on goal tile
		yield return StartCoroutine(ResolveEncountersAtGoal(this.x, this.y));

		StopMoving();
	}


	protected virtual IEnumerator FollowPathStep (int i) {
		Hud.instance.Log("");

		// get next tile coords
		Vector2 p = path[i];
		int x = (int)p.x;
		int y = (int)p.y;

		// before moving, check for encounters on next tile in path
		yield return StartCoroutine(ResolveEncounters(x, y));

		// escape if we stopped moving for any reason
		if (!moving) { 
			StopMoving();
			yield break;
		}

		// interpolate creature position
		float t = 0;
		Vector3 startPos = transform.localPosition;
		Vector3 endPos = new Vector3(x, y, 0);
		while (t <= 1) {
			t += Time.deltaTime / speed;
			transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

			Tile currentTile = grid.GetTile(this.x, this.y);
			CheckForMoveUpdate (currentTile);

			yield return null;
		}

		// update tile position in grid
		LocateAtCoords (x, y);
		sfx.Play("Audio/Sfx/Step/step", 0.8f, Random.Range(0.8f, 1.2f));

		// clear path color at tile
		grid.GetTile(x, y).SetColor(Color.white);

		// check if camera needs to track player
		CheckCamera();
	}


	protected Tile CheckForMoveUpdate (Tile lastTile) {
		Tile newTile = grid.GetTile(transform.localPosition);
		if (newTile != lastTile) {

			this.x = newTile.x;
			this.y = newTile.y;

			// pick collectables
			//PickupItemAtPos(transform.localPosition);

			// TODO: update vision...
			//UpdateVision();

			// emit event
			//if (OnMoveUpdate != null) { OnMoveUpdate.Invoke(); }

			// update last tile reference
			//lastTile = grid.GetTile(transform.localPosition);
		}

		return lastTile;
	}


	private void StopMoving () {
		moving = false;
		DrawPath(Color.white);
	}


	private void CheckCamera () {
		Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

		if (screenPos.x < Screen.width * 0.25f || screenPos.y < Screen.height * 0.25f || 
			screenPos.x > Screen.width * 0.75f || screenPos.y > Screen.height * 0.75f) {

			Camera2D.instance.StopAllCoroutines();
			Camera2D.instance.StartCoroutine(Camera2D.instance.MoveToPos(new Vector2(this.x, this.y)));
		}
	}



	/*private void UpdateVision() {
		// get lit array from shadowcaster class
		bool[,] lit = new bool[grid.width, grid.height];
		int radius = 5;

		print ("Updating vision: " + this.x + "," + this.y);

		ShadowCaster.ComputeFieldOfViewWithShadowCasting(
			this.x, this.y, radius,
			(x1, y1) => grid.IsInsideBounds(x1, y1) && !grid.GetTile(x1, y1).IsWalkable(),
			(x2, y2) => { lit[x2, y2] = true; });

		// iterate grid tiles and render them
		for (int y = 0; y < grid.height; y++) {
			for (int x = 0; x < grid.width; x++) {
				// render tiles
				Tile tile = grid.GetTile(x, y);
				if (tile != null) {
					tile.gameObject.SetActive(lit[x, y] || tile.visited);
					Material tileMat = tile.transform.Find("Sprite").GetComponent<SpriteRenderer>().material;
					tileMat.color = lit[x, y] ? Color.white : Color.gray;

					// render entities
					Entity entity = grid.GetEntity(x, y);
					if (entity != null) {
						entity.gameObject.SetActive(lit[x, y] || tile.visited);
						Material entMat = entity.transform.Find("Sprite").GetComponent<SpriteRenderer>().material;
						entMat = tileMat;
					}

					// mark lit tiles as visited
					if (lit[x, y]) { 
						tile.visited = true; 
					}
				}
			}
		}
	}*/
}








