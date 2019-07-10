using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BishopEnemy : SimpleMoveListEnemy {
	
	public override void Setup(Board board, int x, int y, int id) {
        base.Setup(board, x, y, id);
        this.turnPriority = 6;
    }

	protected override void SetMoveList() {
		this.moves = new List<Vector2Int> {
											new Vector2Int(-1, -1), 
											new Vector2Int(1, -1)
									 	  };
	}

    public override void UpdateColor()
    {
        GetComponent<SpriteRenderer>().color = board.world.worldColor.bishopColor;
    }
}