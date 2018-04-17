using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {

	private QuadTree worldTree;
	int world_identifier = 0;
	void Start () {
		World world1 = CreateWorld(new Vector3(-2,-2,0), 1);
	 	worldTree = new QuadTree(world1, this);
		worldTree.SplitNode(world1);
		// World world2 = createWorld(new Vector3(2,2,0), 1);
		// World world3 = createWorld(new Vector3(2,-2,0), 0.5f);
		// World world4 = createWorld(new Vector3(-2,2,0), 1);
	}

	public World CreateWorld(Vector3 origin, float scale) {
		this.world_identifier ++;
		return new World(origin, scale, this.world_identifier);
	}

	void Update() {
	}
}

public class World {
	private GridWorld grid;
	public Vector3 origin {get; private set;}
	public float scale {get; private set;}
	public int id {get; private set;}

	private List<GridObject> gridObjects;

	public void DestroySelf() {
		this.grid.DestroySelf();
	}
	public World(Vector3 origin, float scale, int id) {
		this.id = id;

		this.origin = origin;
		this.scale = scale;
		this.grid = new GridWorld(4, 4, origin, scale);

		GameObject steen = (GameObject) GridTile.Instantiate(Resources.Load("gridobject_pf"));
		this.grid.SpawnGridObject(steen.GetComponent<GridObject>(), 2, 1);
	}
}

public class QuadTree {
	public class Node {
		private World world;
		private Node parent;
		private Node[] children;


		int depth;

		// Parent == null implies this is the root
		// World  == null implies this is a leaf.
		public Node(Node parent, World world) {
			if (parent == null && world == null) {
				Debug.LogError("Added node that is both leaf and root to quad-tree");
			}
			this.world = world;
			this.parent = parent;
			if (parent == null) {
				this.depth = 0;
			}
			else {				
				this.depth = parent.depth + 1;
			}			
		}

		// All 4 children must be provided immediately by design, and they should not be null.
		// The tree must remain a FULL quad-tree (either no children or 4 children).
		public void Split(World c0, World c1, World c2, World c3) {
			if (c0 == null || c1 == null || c2 == null || c3 == null) {
				Debug.LogError("Invalid split, provided null child");
			}
			if (this.world == null) {
				Debug.LogError("Attempted to split when world is null");
			}
			this.children = new Node[4];
			this.children[0] = new Node(this, c0);
			this.children[1] = new Node(this, c1);
			this.children[2] = new Node(this, c2);
			this.children[3] = new Node(this, c3);
			this.world = null;
		}
	}
	private WorldManager manager;
	private Node root;
	private List<List<Node>> nodesByDepth;
	private Hashtable nodesByWorldId;
	public QuadTree(World rootWorld, WorldManager manager) {
		this.manager = manager;
		this.nodesByDepth = new List<List<Node>>();
		this.nodesByDepth.Add(new List<Node>());
		this.nodesByWorldId = new Hashtable();

		this.root = new Node(null, rootWorld);

		this.nodesByDepth[0].Add(this.root);
		this.nodesByWorldId[rootWorld.id] = this.root;
	}

	// For now this just creates four new worlds.
	public void SplitNode(World world) {
		Vector3 origin = world.origin;
		float scale = world.scale;

		float new_scale = scale / 2.0f;
		Vector3 origin1 = origin;
		Vector3 origin2 = origin + new Vector3(new_scale, 0, 0) * 4;
		Vector3 origin3 = origin + new Vector3(0, new_scale, 0) * 4;
		Vector3 origin4 = origin + new Vector3(new_scale, new_scale, 0) * 4;

		World World1 = this.manager.CreateWorld(origin1, new_scale);
		World World2 = this.manager.CreateWorld(origin2, new_scale);
		World World3 = this.manager.CreateWorld(origin3, new_scale);
		World World4 = this.manager.CreateWorld(origin4, new_scale);

		Node node = (Node) this.nodesByWorldId[world.id];
		node.Split(World1, World2, World3, World4);
		world.DestroySelf();
	}
}