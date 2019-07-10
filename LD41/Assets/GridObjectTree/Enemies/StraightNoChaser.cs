using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightNoChaser : PathfinderEnemy {
	override protected void SetMoveList() {
		this.moves = new List<Vector2Int> {
											new Vector2Int(0, 1), 
											new Vector2Int(1, 0),
											new Vector2Int(0, -1),
											new Vector2Int(-1, 0),
											new Vector2Int(0, 0)
											};
	}

	override protected Vector2Int Target() {
		return this.board.world.GridObjectById(0).position;
	}

	public override void UpdateColor()
    {
        GetComponent<SpriteRenderer>().color = board.world.worldColor.chaserColor;
    }
}
