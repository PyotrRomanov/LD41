using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World {
	public WorldManager manager {get; private set;}
	public bool isActive {get; private set;}	

	public Board board {get; private set;}

	public Vector3 origin {get; private set;}
	public float scale {get; private set;}
	public float integrity {get; private set;}
	public int id {get; private set;}

    public TurnController turnController;
	public List<GridObject> gridObjects = new List<GridObject>();
	public const int InvalidId = -1;
	private int gridObjectId;	// Used to assign new ids when spawning GridObjects.
	
	private Queue<Action> actionQueue = new Queue<Action>();
	private Lock actionLock = new Lock();

    public WorldColor worldColor;

	public World(WorldManager manager, int boardSize, Vector3 origin, float scale, float integrity, int id, int hue) {
		this.manager = manager;
        worldColor = new WorldColor(Random.Range(0, 360));
        this.id = id;
		this.isActive = false;
		this.gridObjectId = 0;

		this.origin = origin;
		this.scale = scale;
		this.integrity = integrity;
		this.board = new Board(this, boardSize, origin, scale);
    }

	// This copy constructor creates a world that is a copy of the original world w.r.t.
    // a new origin, scale, integrity and id. 
    // The following are not copied:
    //      - Inhabitants of tiles (This should be done via SpawnGridObject in the world manager).     
	//		- The turncontroller (this should be done in the world manager)
	// 		- Custom colorings of the tiles (all tiles will reset their coloring).
	public World(World original, Vector3 origin, float scale, float integrity, int id, int hue) {
		this.manager = original.manager;
        worldColor = new WorldColor(hue);
        this.id = id;
		this.isActive = false;
		this.gridObjectId = original.gridObjectId;
		this.origin = origin;
		this.scale = scale;
		this.integrity = integrity;
		this.board = new Board(original.board, this, this.origin, this.scale);

		this.gridObjects = new List<GridObject>();

		foreach(GridObject g in original.gridObjects) {
			this.CopyGridObject(g);
		}

        this.turnController = new TurnController(this, original.turnController);
	}

	// TODO: make safer.
	public Player GetPlayer() {
		return (Player) this.GridObjectById(0);
	}

	public void DamageIntegrity(float dmgPercent) {
		this.integrity *= (1-dmgPercent);
        manager.PokeIntegrityChange(this, dmgPercent);
		if (this.integrity == 0f) {
			this.Collapse();
		}
	}

	public Vector2 Center() {
		return new Vector2(this.board.size * this.scale / 2f + this.origin.x, 
						   this.board.size * this.scale / 2f + this.origin.y);
	}

	// Returns whether the given game coord. position is within this world.
	public bool PosInWorld(Vector2 pos) {
		return (pos.x < this.Center().x + this.board.size * this.scale / 2f - 0.5f * this.scale && pos.x > this.origin.x - 0.5f * this.scale) &&
			   (pos.y < this.Center().y + this.board.size * this.scale / 2f - 0.5f * this.scale && pos.y > this.origin.y - 0.5f * this.scale);
	}
	
	public void DestroyAssets() {
		foreach (GridObject g in this.gridObjects) 
			GameObject.Destroy(g.gameObject);
		this.board.DestroyAssets();
	}

	// Returns whether anything in this world is currently animating.
	public bool IsAnimating () {
		bool ret = false;
		foreach (GridObject g in this.gridObjects) {
			if (g.IsAnimating())
				ret = true;
		}
		foreach (GameObject g in this.board.tiles) {
			if (g.GetComponent<GridTile>().IsAnimating())
				ret = true;
		}
		return ret;
	}

	public void Activate() {
		if (this.isActive) {
			Debug.LogWarning("Activated an active world!");
		}
		this.isActive = true;
		foreach (GridObject o in this.gridObjects) {
			o.Activate();
		}
	}

	public void Deactivate() {
		if (!this.isActive) {
			Debug.LogWarning("Deactivated an inactive world");
		}
		this.isActive = false;
	}

	public void RemoveGridObject(GridObject o) {
		this.gridObjects.Remove(o);
	}

	public void PokeEndOfRound(int roundNumber) {
		if (roundNumber >= this.manager.maxRounds)
			this.Collapse();
	}

	// Returns whether a new turn may be started in this world.
	public bool Locked() {
		return this.IsAnimating() || this.actionLock.Locked();
	}

	public void PokePlayerEndedTurn() {
		this.manager.PokePlayerEndedTurn(this);
	}

	private GridObject SpawnGridObject(string prefab, Vector2Int pos) {
	    GameObject gridObj_ = (GameObject)GameObject.Instantiate(Resources.Load(prefab));
        gridObj_.GetComponent<GridObject>().Setup(board, pos.x, pos.y, this.gridObjectId);
        gridObj_.GetComponent<Transform>().localScale *= scale;
	    GridObject gridObj = gridObj_.GetComponent<GridObject>();

		this.board.SpawnGridObject(gridObj, pos.x, pos.y);
		this.gridObjects.Add(gridObj);

		this.gridObjectId ++;
		return gridObj;
	}

	private GridObject CopyGridObject(GridObject original) {
		if (original == null) {
			Debug.LogError("Attempted to copy null GridObject");
			return null;
		}
		GameObject gridObj_ = Object.Instantiate(original.gameObject);
        gridObj_.GetComponent<GridObject>().Setup(original, this.board);
        gridObj_.GetComponent<Transform>().localScale = this.scale * new Vector3(1, 1, 1);
	    GridObject gridObj = gridObj_.GetComponent<GridObject>();
        gridObj.UpdateColor();

		this.board.SpawnGridObject(gridObj, original.position.x, original.position.y);
		
		this.gridObjects.Add(gridObj);
		return gridObj;
	}

	// Not very efficient but probably ok.
	public GridObject GridObjectById(int id) {
		foreach (GridObject g in this.gridObjects) {
			if (g.id == id)
				return g;
		}
		Debug.LogError("Could not find GridObject with id " + id);
		return null;
	}

	// Return a list of all indices of GridObject that are Agents.
    public List<Agent> Agents() {
		List<Agent> ret = new List<Agent>();
		foreach (GridObject g in this.gridObjects) {
			if (g is Agent)
				ret.Add((Agent) g);
		}
        return ret;
    }

	public void ExecuteActionDist(ActionDist distro) {
		this.manager.ExecuteActionDist(this, distro);
	}

	public bool ActionsQueued() {
		return this.actionQueue.Count > 0;
	}

	public void EnqueueAction(Action action) {
		this.actionQueue.Enqueue(action);
	}

	public void ExecuteNextAction() {
		Action action = this.actionQueue.Dequeue();
		this.manager.StartCoroutine(action.Execute(this.board, this.actionLock));
	}

	public void PlayerReachedGoal() {
		this.Collapse();
	}

	private void SetTurnController() {
		this.turnController = new TurnController(this);
	}

	private void Collapse() {
		manager.PokeWorldCollapse(this);
	}

	public static World TripleThreat(WorldManager manager, Vector3 origin, float scale, float integrity, int id) {
		World TTT = new World(manager, 5, origin, scale, integrity, id, 200);
		
		TTT.manager.SetMaxRounds(6);

		TTT.SpawnGridObject("player_pf", new Vector2Int(2, 0));
		TTT.SpawnGridObject("pawnenemy_pf", new Vector2Int(4, 4));
		TTT.SpawnGridObject("bishopenemy_pf", new Vector2Int(2, 4));
		TTT.SpawnGridObject("pawnenemy_pf", new Vector2Int(0, 4));
		
        TTT.SetTurnController();
		
		return TTT;
	}

    public static World KnightlyEscort(WorldManager manager, Vector3 origin, float scale, float integrity, int id) {
        World TTT = new World(manager, 5, origin, scale, integrity, id, 200);

		TTT.manager.SetMaxRounds(7);

        TTT.SpawnGridObject("player_pf", new Vector2Int(2, 0));
        TTT.SpawnGridObject("knightenemy_pf", new Vector2Int(3, 3));
        TTT.SpawnGridObject("knightenemy_pf", new Vector2Int(1, 3));
        TTT.SpawnGridObject("bishopenemy_pf", new Vector2Int(2, 4));

        TTT.SetTurnController();

        return TTT;
    }

    public static World Tutorial(WorldManager manager, Vector3 origin, float scale, float integrity, int id)
    {
        World TTT = new World(manager, 5, origin, scale, integrity, id, 200);
		
		TTT.manager.SetMaxRounds(6);

		TTT.board.TileByPos(new Vector2Int(3, 2)).MakeGoalTile();

        TTT.SpawnGridObject("player_pf", new Vector2Int(2, 0));
        TTT.SpawnGridObject("pawnenemy_pf", new Vector2Int(0, 4));
        TTT.SpawnGridObject("pawnenemy_pf", new Vector2Int(4, 4));


        TTT.SetTurnController();


        return TTT;
    }

    public static World Diagonalley(WorldManager manager, Vector3 origin, float scale, float integrity, int id) {
        World TTT = new World(manager, 5, origin, scale, integrity, id, 200);

		TTT.manager.SetMaxRounds(5);

		TTT.board.TileByPos(new Vector2Int(4, 2)).MakeGoalTile();

        TTT.SpawnGridObject("player_pf", new Vector2Int(2, 0));
        TTT.SpawnGridObject("bishopenemy_pf", new Vector2Int(3, 2));
        TTT.SpawnGridObject("bishopenemy_pf", new Vector2Int(2, 3));
        TTT.SpawnGridObject("bishopenemy_pf", new Vector2Int(1, 4));

		TTT.board.TileByPos(new Vector2Int(4, 0)).MakeInAccessible();
		TTT.board.TileByPos(new Vector2Int(3, 1)).MakeInAccessible();
		TTT.board.TileByPos(new Vector2Int(2, 2)).MakeInAccessible();
		TTT.board.TileByPos(new Vector2Int(1, 3)).MakeInAccessible();
		TTT.board.TileByPos(new Vector2Int(0, 4)).MakeInAccessible();		

        TTT.SetTurnController();
        return TTT;
	}

	public static World FourByFour(WorldManager manager, Vector3 origin, float scale, float integrity, int id) {
		World TTT = new World(manager, 4, origin, scale, integrity, id, 200);

		TTT.manager.SetMaxRounds(40);

		TTT.board.TileByPos(new Vector2Int(3, 2)).MakeGoalTile();

        TTT.SpawnGridObject("player_pf", new Vector2Int(2, 0));
        TTT.SpawnGridObject("chaser_pf", new Vector2Int(3, 2));

		TTT.board.TileByPos(new Vector2Int(3, 1)).MakeInAccessible();
		TTT.board.TileByPos(new Vector2Int(2, 2)).MakeInAccessible();

        TTT.SetTurnController();
        return TTT;
	}

	public static World LargeThing(WorldManager manager, Vector3 origin, float scale, float integrity, int id) {
		World TTT = new World(manager, 15, origin, scale, integrity, id, 200);

		TTT.manager.SetMaxRounds(1984);

		TTT.board.TileByPos(new Vector2Int(10, 6)).MakeGoalTile();

        TTT.SpawnGridObject("player_pf", new Vector2Int(2, 0));
        TTT.SpawnGridObject("chaser_pf", new Vector2Int(9, 2));
		TTT.SpawnGridObject("chaser_pf", new Vector2Int(10, 10));

		for (int dummy = 0; dummy < 10; dummy ++) {
			int i = Random.Range(0, 12);
			int j = Random.Range(0, 12);
			TTT.board.TileByPos(new Vector2Int(i, j)).MakeInAccessible();
		}

        TTT.SetTurnController();
        return TTT;
	}
}