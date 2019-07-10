using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Pathfinder {
	protected Board board;
	protected Vector2Int origin;
	protected Vector2Int target;
	protected List<Vector2Int> moves;

	public Pathfinder(Board board, Vector2Int origin, Vector2Int target, List<Vector2Int> moves) {
		this.board = board;
		this.origin = origin;
		this.target = target;
		this.moves = moves;
	}

	abstract public List<Vector2Int> BestMoveList();
}

public class DijkstraPathfinder : Pathfinder {
	DijkstraNode[,] nodes;

	public DijkstraPathfinder(Board board, Vector2Int origin, Vector2Int target, List<Vector2Int> moves) : base(board, origin, target, moves) {
		this.InitNodes();
		this.Fill();
	}
	
	override public List<Vector2Int> BestMoveList() {
		List<Vector2Int> ret = new List<Vector2Int>(this.moves);
		ret.Sort((m1, m2) => this.CompareMoves(m1, m2));
		return ret;
	}
 
	public int CompareMoves(Vector2Int m1, Vector2Int m2) {
		Vector2Int p1 = m1 + origin;
		Vector2Int p2 = m2 + origin;

		if (!this.board.InBounds(p1))
			return 1;
		if (!this.board.InBounds(p2))
			return -1;

		return this.nodes[p1.x, p1.y].tDist.CompareTo(this.nodes[p2.x, p2.y].tDist);
	}

	private void InitNodes() {
		this.nodes = new DijkstraNode[this.board.size, this.board.size];
		for (int i = 0; i < this.board.size; i++) {
			for (int j = 0; j < this.board.size; j++) {
				this.nodes[i,j] = new DijkstraNode(new Vector2Int(i, j));
			}
		}
		this.nodes[this.target.x, this.target.y].tDist = 0;
	}

	private void Fill() {
		DijkstraNode curNode = this.nodes[target.x, target.y];
		while (curNode.tDist < int.MaxValue) {
			this.Propagate(curNode);
			curNode = this.NextNode();
		}
	}

	// TODO: This can be implemented more efficiently.
	private DijkstraNode NextNode() {
		DijkstraNode bestNode = DijkstraNode.WorstNode();
		for (int i = 0; i < this.board.size; i++) {
			for (int j = 0; j < this.board.size; j++) {
				if (!this.nodes[i, j].visited && this.nodes[i, j].tDist < bestNode.tDist) {
					bestNode = this.nodes[i, j];
				}
			}
		}
		return bestNode;
	}

	private void Propagate(DijkstraNode node) {
		node.visited = true;
		foreach(Vector2Int move in this.moves) {
			Vector2Int newPos = node.pos - move;
			if (this.board.InBounds(newPos) && (this.board.IsFreeTile(newPos) || newPos == origin)) {
				DijkstraNode newNode = this.nodes[newPos.x, newPos.y];
				if (node.tDist + 1 < newNode.tDist) {
					newNode.tDist = node.tDist + 1;
				}
			}
		}
	}

	private class DijkstraNode {
		public Vector2Int pos;
		public int tDist = int.MaxValue;
		public bool visited = false;
		
		public DijkstraNode(Vector2Int pos) {
			this.pos = pos;
		}

		public static DijkstraNode WorstNode () {
			return new DijkstraNode(Board.InvalidPos);
		}
	}
}
