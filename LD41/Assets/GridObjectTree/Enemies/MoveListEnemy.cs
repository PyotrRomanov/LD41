using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveListEnemy : Enemy {
	
	public List<Vector2Int> moves;
	
	override public void Setup(GridObject original, Board board) {
		this.SetMoveList();
		base.Setup(original, board);
	}

	override public void Setup(Board board, int x, int y, int id) {
		this.SetMoveList();
		base.Setup(board, x, y, id);
	}

	override public void Move() {
		for (int dummy = 0; dummy < this.moves.Count; dummy++) {
			Vector2Int move = this.FetchMove();
			if (this.board.InBounds(this.position + move)) {
				this.board.MoveObject(this, move);
				return;
			}
		}
	}

	abstract public List<Vector2Int> FindNextMoves();
	abstract protected void SetMoveList();
    abstract protected Vector2Int FetchNthMove(int n);
	abstract protected Vector2Int FetchMove();
}

public abstract class SimpleMoveListEnemy : MoveListEnemy {
   	private int movePointer = 0;
	
	override protected Vector2Int FetchNthMove(int n) {
        if (this.moves.Count == 0) {
            Debug.LogError("Called FetchMove for empty movelist!");
            return new Vector2Int(0, 0);
        }
        Vector2Int ret = moves[n];
        return ret;
    }

	override protected Vector2Int FetchMove() {
		if (this.moves.Count == 0) {
			Debug.LogError("Called FetchMove for empty movelist!");
			return new Vector2Int(0, 0);
		}
		Vector2Int ret = moves[movePointer];
		this.movePointer = (this.movePointer + 1) % this.moves.Count;
		return ret;
	}

    override public List<Vector2Int> FindNextMoves() {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int dummy = 0; dummy < this.moves.Count; dummy++) {
            Vector2Int move = this.FetchNthMove(dummy);
            if (this.board.InBounds(this.position + move) && this.board.TileByPos(this.position + move).accessible) {
                list.Add(move);
            }
        }
        return list;
    }
}

public abstract class PathfinderEnemy : MoveListEnemy {
	protected abstract Vector2Int Target();

	override protected Vector2Int FetchNthMove(int n) {
        Debug.LogError("Called unimplemented function!");
        return new Vector2Int(0, 0);
    }

	override protected Vector2Int FetchMove() {
		DijkstraPathfinder pf = new DijkstraPathfinder(this.board, this.position, this.Target(), this.moves);
		return pf.BestMoveList()[0];
	}

    override public List<Vector2Int> FindNextMoves() {
        Debug.LogError("Called unimplemented function!");
		return null;
	}
}