using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnEnemy : SimpleMoveListEnemy {
	public override void Setup(Board board, int x, int y, int id) {
        base.Setup(board, x, y, id);
        this.turnPriority = 3;
    }

	override protected void SetMoveList() {
		this.moves = new List<Vector2Int> { new Vector2Int(0, -1) };
	}

    public override void UpdateColor()
    {
        GetComponent<SpriteRenderer>().color = board.world.worldColor.pawnColor;
    }
}