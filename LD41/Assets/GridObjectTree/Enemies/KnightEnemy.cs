using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightEnemy : SimpleMoveListEnemy
{
    public override void Setup(Board board, int x, int y, int id) {
        base.Setup(board, x, y, id);
        this.turnPriority = 4;
    }

    override protected void SetMoveList()
    {
        this.moves = new List<Vector2Int> {
                                            new Vector2Int(-2, -1),
                                            new Vector2Int(2, -1)
                                           };
    }
    public override void UpdateColor()
    {
        GetComponent<SpriteRenderer>().color = board.world.worldColor.knightColor;
    }
}