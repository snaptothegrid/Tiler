﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CreatureCombat : CreatureModule {


	public CreatureCombat (Creature creature) {
		Init(creature);
	}


	// =====================================================
	// Combat Outcome
	// =====================================================

	private bool ResolveCombatOutcome (Creature attacker, Creature defender) { // return isDead
		int atk, def;

		atk = Dice.Roll("1d100");
		def = Dice.Roll("1d100");

		// attacker missed
		if (atk > attacker.stats.attack + 25) { // 25% extra accuracy
			Miss(attacker, defender);
			return false;
		}

		// defender missed
		if (def > defender.stats.defense) {
			return Damage(attacker, defender);
		}

		// both succeed, so get difference
		int attack = (attacker.stats.attack - atk);
		int defense = (defender.stats.defense - def);
		int diff = attack - defense;
		//Debug.Log (attack + "/" + defense + " -> " + diff);

		if (diff > 0) {
			// attacker wins
			return Damage(attacker, defender);
		} else {
			// defender wins
			Block(attacker, defender);
			return false;
		}
	}


	// =====================================================
	// Outcomes
	// =====================================================

	private void Miss (Creature attacker, Creature defender) {
		sfx.Play("Audio/Sfx/Combat/swishA", 0.1f, Random.Range(0.5f, 1.2f));
		defender.Speak("Miss", Color.white);
	}


	private void Block (Creature attacker, Creature defender) {
		string[] arr = new string[] { "swordB", "swordC" };
		sfx.Play("Audio/Sfx/Combat/" + arr[Random.Range(0, arr.Length)], 0.15f, Random.Range(0.6f, 1.8f));
		defender.Speak("Block", Color.white);
	}


	private void Dodge (Creature attacker, Creature defender) {
		sfx.Play("Audio/Sfx/Combat/swishA", 0.1f, Random.Range(0.5f, 1.2f));
		defender.Speak("Dodge", Color.white);
	}


	private bool Damage (Creature attacker, Creature defender) {
		// damage
		int damage = GetTotalDamage (attacker, defender);
		
		if (damage > 0) {
			// apply damage
			defender.UpdateHp(-damage);

			// display damage info
			string[] arr = new string[] { "painA", "painB", "painC", "painD" };
			sfx.Play("Audio/Sfx/Combat/" + arr[Random.Range(0, arr.Length)], 0.1f, Random.Range(0.6f, 1.8f));
			sfx.Play("Audio/Sfx/Combat/hitB", 0.5f, Random.Range(0.8f, 1.2f));
			defender.Speak("-" + damage, Color.red);

			// create blood
			grid.CreateBlood(defender.transform.position, damage, Color.red);
			
			// set isDead to true
			if (defender.stats.hp == 0) {
				return true;
			}
		}

		return false;
	}


	// =====================================================
	// Damage Calculations
	// =====================================================

	private int GetTotalDamage (Creature attacker, Creature defender, bool debug = false) {
		int damage = attacker.stats.str;

		// get weapon adittional damage
		if (attacker.stats.weapon != null) {
			damage += Dice.Roll(attacker.stats.weapon.damage);
		} else {
			damage += Dice.Roll("1d4-1");
		}

		// get total gdr (garanteed damage reduction %)
		List<Armour> armours = GetArmourParts(defender);
		int gdr = GetArmourGdr(armours);

		// get damage protection (how many points will be discounted from damage)
		int protection = Mathf.RoundToInt(gdr * damage / 100f);

		if (debug) {
			Debug.Log(
				attacker.name + " Damage " + 
				damage + " - " + 
				protection + " (" + gdr + "%) = " + 
				(damage - protection)
			);
		}
		
		// apply damage protection
		damage -= protection;
		return damage;
	}


	private List<Armour> GetArmourParts (Creature defender) {
		Dictionary<string, CreatureInventoryItem> equipment = defender.inventory.equipment;

		List<Armour> armours = new List<Armour>();

		if (equipment["Armour"] != null) { armours.Add((Armour)equipment["Armour"].item); }
		if (equipment["Hat"] != null) { armours.Add((Armour)equipment["Hat"].item); }
		if (equipment["Gloves"] != null) { armours.Add((Armour)equipment["Gloves"].item); }
		if (equipment["Boots"] != null) { armours.Add((Armour)equipment["Boots"].item); }
		if (equipment["Cloak"] != null) { armours.Add((Armour)equipment["Cloak"].item); }

		return armours;
	}


	private int GetArmourGdr (List<Armour> armours) {
		int gdr = 0;
		foreach (Armour armour in armours) { gdr += armour.gdr; }
		return gdr;
	}

	
	// =====================================================
	// Shoot
	// =====================================================

	public void Shoot (Creature target, float delay = 0) {
		me.StopMoving();

		Attack(target, delay);

		// create bullet
		grid.CreateBullet(me.transform.localPosition, target.transform.localPosition, me.speed, 8, Color.yellow);
	}


	public void ShootToBreak (Entity target) {
		me.StopMoving();

		// create bullet
		grid.CreateBullet(me.transform.localPosition, target.transform.localPosition, me.speed, 8, Color.yellow);

		// break container
		Container container = (Container)target;
		container.StartCoroutine(container.Open(me));
	}


	// =====================================================
	// Attack
	// =====================================================

	public void AttackToBreak (Entity target) {
		float delay = me.state == CreatureStates.Moving ? me.speed : 0;
		//me.StartCoroutine(AttackAnimation(target, delay, 4));
		me.StartCoroutine(CombatAnimation(target, delay));
	}


	public void Attack (Creature target, float delay = 0) {
		if (me.state == CreatureStates.Using) { return; }
		if (target.state == CreatureStates.Dying) { return; }

		me.StopMoving();
		target.StopMoving();

		me.StartCoroutine(CombatAnimation(target, delay));
		
	}


	private IEnumerator CombatAnimation (Tile target, float delay = 0) {
		// play both attack and defend animations
		me.StartCoroutine(AttackAnimation(target, delay, 3));

		if (target is Creature) {
			// play both attack and defend animations
			me.StartCoroutine(AttackAnimation((Creature)target, delay, 3));
			yield return target.StartCoroutine(((Creature)target).combat.DefendAnimation(me, delay, 8));
		} else {
			yield return me.StartCoroutine(AttackAnimation(target, delay, 3));
		}
		

		// recharge all energy
		me.stats.energy = Mathf.Max(me.stats.energyRate, 1f);

		// once combat turn has fully finished, emit game event
		if (me is Player) {
			me.UpdateGameTurn();
			yield return null;
		}
	}


	private IEnumerator AttackAnimation (Tile target, float delay = 0, float advanceDiv = 2) {
		me.state = CreatureStates.Attacking;

		// wait for attack to start
		yield return new WaitForSeconds(delay);
		if (target == null) { yield break; }
		
		float duration = me.speed * 0.5f;

		sfx.Play("Audio/Sfx/Combat/woosh", 0.4f, Random.Range(0.5f, 1.5f));

		// move towards target
		float t = 0;
		Vector3 startPos = me.transform.localPosition;
		Vector3 endPos = startPos + (target.transform.position - me.transform.position).normalized / advanceDiv;
		while (t <= 1) {
			t += Time.deltaTime / duration;
			me.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
			yield return null;
		}

		// move back to position
		t = 0;
		while (t <= 1) {
			t += Time.deltaTime / duration;
			me.transform.localPosition = Vector3.Lerp(endPos, startPos, Mathf.SmoothStep(0f, 1f, t));
			yield return null;
		}

		me.state = CreatureStates.Idle;
	}


	// =====================================================
	// Defend
	// =====================================================

	private IEnumerator DefendAnimation (Creature attacker, float delay = 0, float advanceDiv = 8) {
		float duration = me.speed * 0.5f;
		me.state = CreatureStates.Defending;

		// wait for impact
		yield return new WaitForSeconds(delay + duration);

		// get combat positions
		Vector3 startPos = new Vector3(me.x, me.y, 0);
		Vector3 vec = (new Vector3(attacker.x, attacker.y, 0) - startPos).normalized / advanceDiv;
		Vector3 endPos = startPos - vec;

		// resolve combat outcome and apply combat sounds and effects

		bool isDead = ResolveCombatOutcome(attacker, me);
		if (isDead) {
			Die(me);
			yield break;
		}

		// move towards attacker
		float t = 0;
		while (t <= 1) {
			t += Time.deltaTime / duration * 0.5f;
			me.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

			yield return null;
		}

		// move back to position
		t = 0;
		while (t <= 1) {
			t += Time.deltaTime / (duration * 0.5f);
			me.transform.localPosition = Vector3.Lerp(endPos, startPos, Mathf.SmoothStep(0f, 1f, t));

			yield return null;
		}

		me.state = CreatureStates.Idle;
	}

	// =====================================================
	// Death
	// =====================================================

	protected void Die (Creature attacker, float delay = 0) {
		me.StopMoving();

		me.state = CreatureStates.Dying;
		me.StartCoroutine(DeathAnimation(attacker, delay));

		// get a list of all items carried by the creature
		List<Item> allItems = new List<Item>();
		foreach (CreatureInventoryItem invItem in me.inventory.items) {
			allItems.Add(invItem.item);
		}

		// spawn all the items carried by the creature
		me.SpawnItemsFromInventory(allItems);
	}


	protected IEnumerator DeathAnimation (Creature attacker, float delay = 0) {
		string[] arr = new string[] { "painA", "painB", "painC", "painD" };
		sfx.Play("Audio/Sfx/Combat/" + arr[Random.Range(0, arr.Length)], 0.3f, Random.Range(0.6f, 1.8f));
		sfx.Play("Audio/Sfx/Combat/hitB", 0.6f, Random.Range(0.5f, 2.0f));

		grid.CreateBlood(me.transform.localPosition, 16, Color.red);

		grid.SetCreature(me.x, me.y, null);
		me.Destroy();

		// update attacker xp
		attacker.UpdateXp(me.stats.xpValue);

		// if player died, emit gameover event
		if (me is Player) {
			me.GameOver();
		}
		
		yield break;
	}
}