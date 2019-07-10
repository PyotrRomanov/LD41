using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Agent {
	public override void StartTurn() {
		base.StartTurn();
		this.Move();
		base.EndTurn();
    }

	public virtual void Move() {}
	public virtual void ExecuteAction() {}
}