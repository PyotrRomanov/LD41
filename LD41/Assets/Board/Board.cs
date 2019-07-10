using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board {
    public World world { get; private set; }
    public static Vector2Int InvalidPos = new Vector2Int(-1, -1);
	public int size { get; private set; }

	private float scale;

	public void SpawnGridObject(GridObject obj, int x, int y) {
		tiles[x,y].GetComponent<GridTile>().Populate(obj, true); 
	}

	public GameObject[,] tiles { get; private set; }

	public void DestroyAssets() {
		foreach (GameObject tile in this.tiles) {
			GameObject.Destroy(tile.gameObject);
		}
	}

    public GridTile TileByPos(Vector2Int pos) {
        return this.tiles[pos.x, pos.y].GetComponent<GridTile>();
    }

    public List<GridTile> Tiles() {
        List<GridTile> ret = new List<GridTile>();
        foreach(GameObject g in this.tiles) {
            ret.Add(g.GetComponent<GridTile>());
        }
        return ret;
    }

    // This copy constructor creates a board that is a copy of the original board w.r.t.
    // a new origin and scale. 
    // The following are not copied:
    //      - Inhabitants of tiles.
    //      - Custom colorings of the tiles (all tiles will reset their coloring).
    public Board (Board original, World world, Vector3 origin, float scale) {
        this.world = world;
        this.size = original.size;
        this.scale = scale;
        this.tiles = new GameObject[original.size, original.size];
        for (int i = 0; i < this.size; i++) { 
			for (int j = 0; j < this.size; j++) {
				this.tiles[i, j] = (GameObject) GridTile.Instantiate(Resources.Load("tile_pf"));
				this.tiles[i, j].GetComponent<Transform>().position = new Vector3(i * this.scale, j * this.scale, 0) + origin;
				this.tiles[i, j].GetComponent<Transform>().localScale *= this.scale;
                this.tiles[i, j].GetComponent<GridTile>().Setup(original.tiles[i, j].GetComponent<GridTile>(), world);
            }
		}
    }

    public Board (World world, int size, Vector3 origin, float scale) {
        this.world = world;
		this.size = size;
		this.scale = scale;
		tiles = new GameObject[this.size, this.size];

		for (int i = 0; i < this.size; i++) { 
			for (int j = 0; j < this.size; j++) {
				tiles[i, j] = (GameObject) GridTile.Instantiate(Resources.Load("tile_pf"));
				tiles[i, j].GetComponent<Transform>().position = new Vector3(i * this.scale, j * this.scale, 0) + origin;
				tiles[i, j].GetComponent<Transform>().localScale *= this.scale;
                Color color = world.worldColor.passableTile;;
                tiles[i, j].GetComponent<GridTile>().Setup(this, new Vector2Int(i, j), color, world.worldColor.unpassableTile);
			} 
		}
	}

    public GridObject GridObjectById (int id) {
        return this.world.GridObjectById(id);
    }

    public void MarkTiles (bool[,] mask, Color color) {
        for (int i = 0; i < this.size; i++) {
            for (int j = 0; j < this.size; j++) {
                if (mask[i,j])
                    tiles[i, j].GetComponent<SpriteRenderer>().color = color;
            }
        }
    }

    public bool InBounds(Vector2Int pos) {
        return (pos.x < this.size && pos.x >= 0 && pos.y < this.size && pos.y >= 0);
    }
    public bool InXBounds(Vector2Int pos) {
        return (pos.x < this.size && pos.x >= 0);
    }
    public bool InYBounds(Vector2Int pos) {
        return (pos.y < this.size && pos.y >= 0);
    }

    public bool MoveObject(GridObject obj, Vector2Int movement) {
        Vector2Int newPos = obj.position + movement;
        if (this.InBounds(newPos) && this.IsFreeTile(newPos)) {
            tiles[obj.position.x, obj.position.y].GetComponent<GridTile>().Depopulate();
            tiles[obj.position.x + movement.x, obj.position.y + movement.y].GetComponent<GridTile>().Populate(obj);
            return true;
        }
        else if (!this.InBounds(newPos)) {
            obj.MovedOutOfBounds(newPos);
            return false;
        }
        else if (this.TileByPos(newPos).accessible) {
            obj.BumpIntoObject(this.TileByPos(newPos).inhabitant);
            return true;
        }
        return false;
    }

    public void SwapObjects(GridObject o1, GridObject o2) {
        if (o1 != null && o2 != null) {
            Vector2Int pos1 = o1.position;
            Vector2Int pos2 = o2.position;

            this.TileByPos(pos1).Depopulate();
            this.TileByPos(pos2).Depopulate();            
            this.TileByPos(pos2).Populate(o1);
            this.TileByPos(pos1).Populate(o2);
        }
    }

    // Returns whether the tile at given position is free, i.e. accessible and unihabited.
    public bool IsFreeTile (Vector2Int pos) {
        if (this.InBounds(pos)) {
            GridTile tile = this.tiles[pos.x, pos.y].GetComponent<GridTile>();
            return tile.accessible && !tile.IsInhabited();
        }
        else {
            Debug.LogWarning("Asked for freeness of out of bounds tile coordinates (" + pos.x + ", " + pos.y + ")");
            return true;
        }
    }

    public void ResetColorings() {
        foreach (GameObject tile in this.tiles) {
            tile.GetComponent<GridTile>().ResetColoring();
        }
    }

    public bool[,] FindReachableTiles(int moveRange, Vector2Int pos) {
        bool[,] map = new bool[this.size, this.size];
        FloodFillBoard f = new FloodFillBoard(this, pos);
        int [,] distances = f.Distances();
        
        for (int i = 0; i < this.size; i++) {
            for (int j = 0; j < this.size; j++) {  
                if (this.IsFreeTile(new Vector2Int(i, j)) && distances[i, j] <= moveRange) {
                    map[i, j] = true;
                } else {
                    map[i, j] = false;
                }
            }
        }
        map[pos.x, pos.y] = true;

        return map;
    }
}

class FloodFillBoard {
    private Board board;
    private static List<Vector2Int> neighbours = 
        new List<Vector2Int> {
                                new Vector2Int(0, 1),
                                new Vector2Int(0, -1),
                                new Vector2Int(1, 0),
                                new Vector2Int(-1, 0),
                             };

    private DijkstraInfo [,] floodBoard;
    public FloodFillBoard (Board board, Vector2Int center) {
        this.board = board;
        this.floodBoard = new DijkstraInfo[this.board.size, this.board.size];
        for (int i = 0; i < this.board.size; i++) {
            for (int j = 0; j < this.board.size; j++) {
                this.floodBoard[i,j] = new DijkstraInfo(int.MaxValue, i, j);
            }
        }
        this.floodBoard[center.x, center.y].tDist = 0;
        int counter = 0;
        int maxLoops = 10000;
        while(Propagate()) {
            counter ++;
            if (counter > maxLoops) {
                Debug.LogError("Floodboard filling was stopped after it propagated " + maxLoops + " times!");
                break;
            }
        }
    }

    public int[,] Distances() {
        int[,] distances = new int[this.board.size, this.board.size];
        for (int i = 0; i < this.board.size; i++) {
            for (int j = 0; j < this.board.size; j++) {
                distances[i, j] = this.floodBoard[i, j].tDist;
            }
        }
        return distances;
    }

    private bool Propagate () {
        DijkstraInfo node = this.FindNext();
        if (node == null) {
            return false;
        }
        else {
            node.visited = true;
            foreach (Vector2Int offset in FloodFillBoard.neighbours) {
                if (this.board.InBounds(new Vector2Int (node.x, node.y) + offset)) 
                {
                    this.Update(node, this.floodBoard[node.x + offset.x, node.y + offset.y]);
                }
            }
            return true;
        }
    }

    private void Update(DijkstraInfo n1, DijkstraInfo n2) {
        if (n1.tDist < n2.tDist && this.board.IsFreeTile(new Vector2Int(n2.x, n2.y))) {
            n2.tDist = n1.tDist + 1;
        }
    }

    private DijkstraInfo FindNext() {
        DijkstraInfo bestInfo = null;
        foreach (DijkstraInfo info in this.floodBoard) {
            int bestDist = bestInfo != null ? bestInfo.tDist : int.MaxValue;
            if (info.tDist < bestDist && !info.visited) {
                bestInfo = info;
            }
        }
        return bestInfo;
    }

    private class DijkstraInfo {
    public int tDist;
    public bool visited;
    public int x;
    public int y;
    public DijkstraInfo (int tDist, int x, int y) {
        this.tDist = tDist;
        this.x = x; 
        this.y = y;
    }
}
}


