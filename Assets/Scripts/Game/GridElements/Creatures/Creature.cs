﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum CreatureStates  {
	Idle = 0,
	Moving = 1,
	Attacking = 2,
	Defending = 4
}


public class Creature : Tile {

	public CreatureStates state { get; set; }

	public int hp = 5;

	protected List<Vector2> path;
	protected float speed = 0.15f;
	//public bool moving = false;
	//public bool stopAtNextTile = false;


	public override void Init (Grid grid, int x, int y, float scale = 1, Sprite asset = null) {
		base.Init(grid, x, y, scale, asset);
		walkable = false;

		SetImages(scale, new Vector3(0, 0.1f, 0), 0.04f);
		LocateAtCoords(x, y);

		state = CreatureStates.Idle;
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
		// escape if creature is not in idle state
		if (state != CreatureStates.Idle) {
			return;
		}

		// clear previous path
		if (path != null) {
			DrawPath(Color.white);
		}

		// escape if goal is not visible
		Tile tile = grid.GetTile(x, y);
		if (tile == null) { return; }
		if (!tile.gameObject.activeSelf) { return; }

		// if goal is the creature's tile, wait one turn instead
		if (x == this.x && y == this.y) {
			path = new List<Vector2>() { new Vector2(this.x, this.y) };
			Hud.instance.CreateLabel(transform.position, "...", Color.yellow);
		} else {
			// search for new path
			path = Astar.instance.SearchPath(grid.player.x, grid.player.y, x, y);
			path = SetPathAfterEncounter(path);
		}
		

		// escape if no path was found
		if (path.Count == 0) {
			StopMoving();
			return;
		}

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
	// Movement
	// =====================================================

	protected IEnumerator FollowPath () {
		state = CreatureStates.Moving;

		for (int i = 0; i < path.Count; i++) {
			Hud.instance.Log("");

			// get next tile coords
			Vector2 p = path[i];
			int x = (int)p.x;
			int y = (int)p.y;

			// resolve encounters with next tile
			ResolveEntityEncounters(x, y);
			ResolveCreatureEncounters(x, y);

			yield return StartCoroutine (FollowPathStep(x, y));
		}

		// stop moving once we reach the goal
		StopMoving();

		// resolve encounters with current tile after moving
		ResolveEncountersAtGoal(this.x, this.y);
	}


	protected virtual void UpdatePosInGrid (int x, int y) {
		grid.SetCreature(this.x, this.y, null);
		this.x = x;
		this.y = y;
		grid.SetCreature(x, y, this);
	}


	protected virtual IEnumerator FollowPathStep (int x, int y) {
		if (state != CreatureStates.Moving) { yield break; }

		// interpolate creature position
		float t = 0;
		Vector3 startPos = transform.localPosition;
		Vector3 endPos = new Vector3(x, y, 0);
		while (t <= 1) {
			t += Time.deltaTime / speed;
			transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

			MoveToNextTile(x, y);

			yield return null;
		}

		// clear path color at tile
		grid.GetTile(x, y).SetColor(Color.white);

		sfx.Play("Audio/Sfx/Step/step", 0.8f, Random.Range(0.8f, 1.2f));

		// stop moving if we manually aborted
		if (state != CreatureStates.Moving) {
			StopMoving();
		}

		// wait a bit until next move step
		//yield return new WaitForSeconds(0.1f);
	}


	protected void MoveToNextTile (int x, int y) {
		UpdateVision();
		UpdatePosInGrid(x, y);
	}


	public void StopMoving () {
		state = CreatureStates.Idle;
		DrawPath(Color.white);
		//UpdateVision();

		StopAllCoroutines();
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


	private bool ResolveEntityEncounters (int x, int y) {
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
						//moving = false;
						StopMoving();
						StartCoroutine(door.Open()); 
						Hud.instance.Log("You open the door.");
						CenterCamera();
						return true;
			
					} else if (door.state == EntityStates.Locked) { 
						// unlock door
						//moving = false;
						StopMoving();
						StartCoroutine(door.Unlock(success => {}));
						Hud.instance.Log("You unlock the door.");
						CenterCamera();
						return true;
					}
				}
			}
		}

		return false;
	}


	private bool ResolveCreatureEncounters (int x, int y) {
		Creature creature = grid.GetCreature(x, y);
		if (creature != null && creature != this) {
			Attack(creature);
			return true;
		}

		return false;
	}


	private bool ResolveEncountersAtGoal (int x, int y) {
		Entity entity = grid.GetEntity(x, y);
		if (entity != null) {

			// resolve stairs
			if ((this is Player) && (entity is Stair)) {
				StopMoving();
				//yield return new WaitForSeconds(0.25f);

				Stair stair = (Stair)entity;
				if (stair.state == EntityStates.Open) { 
					Dungeon.instance.ExitLevel (stair.direction);
				} else {
					Hud.instance.Log("The stair doors are locked.");
				}
			}
		}

		return false;
	}


	// =====================================================
	// Functions overriden by Player class
	// =====================================================

	protected virtual void MoveCameraTo (int x, int y) {}
	protected virtual void CenterCamera () {}
	public virtual void UpdateVision () {}


	// =====================================================
	// Attack
	// =====================================================

	protected void Attack (Creature target) {
		StopMoving();

		state = CreatureStates.Attacking;
		StartCoroutine(AttackAnimation(target));

		target.Defend(this);
	}

	protected IEnumerator AttackAnimation (Creature target) {
		float duration = speed * 0.75f;

		// move towards target
		float t = 0;
		Vector3 startPos = transform.localPosition;
		Vector3 endPos = startPos + (target.transform.position - transform.position).normalized / 2;
		while (t <= 1) {
			t += Time.deltaTime / duration;
			transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

			yield return null;
		}

		// move back to position
		t = 0;
		while (t <= 1) {
			t += Time.deltaTime / duration;
			transform.localPosition = Vector3.Lerp(endPos, startPos, Mathf.SmoothStep(0f, 1f, t));

			yield return null;
		}

		state = CreatureStates.Idle;
	}


	// =====================================================
	// Defend
	// =====================================================

	protected void Defend (Creature attacker) {
		StopMoving();

		state = CreatureStates.Defending;
		StartCoroutine(DefendAnimation(attacker));
	}

	protected IEnumerator DefendAnimation (Creature attacker) {
		// wait for impact
		float duration = speed * 0.75f;
		yield return new WaitForSeconds(duration);

		// apply damage
		int damage = Random.Range(1, 7);
		Hud.instance.CreateLabel(transform.position, "-" + damage, Color.red);
		sfx.Play("Audio/Sfx/Combat/hitB", 1f, Random.Range(0.8f, 1.2f));

		// move towards attacker
		float t = 0;
		Vector3 startPos = transform.localPosition;
		Vector3 endPos = startPos - (attacker.transform.position - transform.position).normalized / 4;
		while (t <= 1) {
			t += Time.deltaTime / duration;
			transform.localPosition = Vector3.Lerp(transform.localPosition, endPos, Mathf.SmoothStep(0f, 1f, t));

			yield return null;
		}

		// move back to position
		t = 0;
		while (t <= 1) {
			t += Time.deltaTime / duration;
			transform.localPosition = Vector3.Lerp(endPos, startPos, Mathf.SmoothStep(0f, 1f, t));

			yield return null;
		}

		state = CreatureStates.Idle;
	}
	
}

