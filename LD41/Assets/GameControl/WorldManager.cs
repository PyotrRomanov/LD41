using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class WorldManager : MonoBehaviour {
    private QuadTree worldTree;
	private List<World> worlds;

	public AudioPlayer audioPlayer;
	public int maxRounds {get; private set;}

	private Lock splitLock = new Lock();
    public Lock collapseLock { get; private set; }

    GameController gameController;

	int worldIdentifier = 0;
	static float scaleMod = 0.95f;
	float savedIntegrity = 0.0f;
    Text[] uiTexts;
    UIManager uiManager;
    Canvas uiCanvas;

	public int gridSize {get; private set;}
	private CameraController cameraControl;
    
	private World activeWorld;

	public void SetMaxRounds(int maxRounds) {
		if (this.maxRounds == 0)
			this.maxRounds = maxRounds;
	}

    public void Setup(string worldName, GameController gameController) {
        uiCanvas = FindObjectOfType<Canvas>();
        uiTexts = uiCanvas.GetComponentsInChildren<Text>();
        this.uiManager = FindObjectOfType<UIManager>();
        this.gameController = gameController;

        collapseLock = new Lock();

        GameObject audioPlayer_ = (GameObject) GameObject.Instantiate(Resources.Load("audioplayer_pf"));
		this.audioPlayer = (AudioPlayer) audioPlayer_.GetComponent<AudioPlayer>();

		this.cameraControl = new CameraController(this);

        this.worlds = new List<World>();
        World world1 = CreateWorld(new Vector3(0, 0, 0), 1, worldName);
		this.gridSize = world1.board.size;
        worldTree = new QuadTree(world1, this);

		this.cameraControl.SnapToFullView();
        this.WakeUpWorld(world1);
    }

	private void updateUI() {
		if (this.ActiveWorld()) {
			uiTexts[1].text = (this.activeWorld.integrity * 100) + "%";
			uiTexts[4].text = this.activeWorld.turnController.roundNumber + "";
			uiTexts[6].text = WorldName(this.activeWorld);
			uiManager.UpdateTurnOrder(this.activeWorld);
		} 
		else {
			uiTexts[1].text = "-";
			uiTexts[4].text = "-";
			uiTexts[6].text = "-";
		}

        uiTexts[2].text = this.savedIntegrity * 100 + "%";
        uiTexts[3].text = TotalIntegrity() * 100 + "%";
        uiTexts[5].text = this.maxRounds + "";
        uiTexts[7].text = this.worlds.Count + "";
	}

	public void ExecuteActionDist(World world, ActionDist distro) {
		if (!distro.IsDegenerated()) {
			this.splitLock.SignalStart();
			StartCoroutine(this.SplitWorld(world, distro));
		}
		else {
			// TODO this should probably look for the action which has weight 1!
			world.EnqueueAction(distro.GetAction(0));
			world.ExecuteNextAction();
		}
	}

	private void WakeUpWorld(World world) {
		if (world == null) {
			Debug.LogError("Attempted to wake up null");
			return;
		}

		if (this.ActiveWorld())
			this.activeWorld.Deactivate();
		if (!world.Locked()) {
			this.activeWorld = world;
			this.activeWorld.Activate();
			// this.cameraControl.SnapToWorld(world);
		}
	}

	private bool Locked() {
		return this.AnyWorldLocked() || this.splitLock.Locked();
	}

	private bool ActiveWorld(){
		return this.activeWorld != null;
	}

	private void HandleCameraInput() {
		// TODO
	}

	private World MouseOverWorld() {
		foreach(World w in this.worlds) {
			if(w.PosInWorld(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
				return w;
		}
		return null;
	}

	private void HandleMouseInput() {
		if (Input.GetMouseButtonDown(0) ) {
			if (this.activeWorld != this.MouseOverWorld())
				WakeUpWorld(this.MouseOverWorld());
		}
	}

	void Update() {
		this.updateUI();
		this.HandleCameraInput();
		this.HandleMouseInput();

		// if (this.worlds.Count > 0 && !this.ActiveWorld() && !this.Locked())
		// 	WakeUpRandomWorld();

		if (this.ActiveWorld() && !this.Locked())
			activeWorld.turnController.TryNextTurn();
	}

	public void LogWorldIds() {
		Debug.Log("Listing all existing worlds...");
		foreach (World w in this.worlds) {
			Debug.Log("World " + w.id);
		}
		Debug.Log("Total integrity is " + this.TotalIntegrity());
	}

	public World CreateWorld(Vector3 origin, float scale, string worldName, float integrity = 1.0f) {
        World newWorld;
        switch (worldName) {
            case "Triple Threat":
                this.worldIdentifier++;
                newWorld = World.LargeThing(this, origin, scale, integrity, this.worldIdentifier);
                this.worlds.Add(newWorld);
                Debug.Log("Returning new world " + worldName);
                return newWorld;
            case "Knightly Escort":
                this.worldIdentifier++;
                newWorld = World.KnightlyEscort(this, origin, scale, integrity, this.worldIdentifier);
                this.worlds.Add(newWorld);
                Debug.Log("Returning new world " + worldName);
                return newWorld;
            case "Diagonally":
                this.worldIdentifier++;
                newWorld = World.Diagonalley(this, origin, scale, integrity, this.worldIdentifier);
                this.worlds.Add(newWorld);
                Debug.Log("Returning new world " + worldName);
                return newWorld;
            case "Tutorial":
                this.worldIdentifier++;
                newWorld = World.Tutorial(this, origin, scale, integrity, this.worldIdentifier);
                this.worlds.Add(newWorld);
                Debug.Log("Returning new world " + worldName);
                return newWorld;
            default:
                Debug.LogWarning("Attempted to create nonexistent world with name: " + worldName);
                return null;
        }		
	}

	public World CopyWorld(World original, Vector3 origin, float scale, float integrity, int hue) {
		this.worldIdentifier ++;
		World newWorld = new World(original, origin, scale, integrity, this.worldIdentifier, hue);
        
		this.worlds.Add(newWorld);
		return newWorld;
	}

	public void PokePlayerEndedTurn(World world) {
		// this.SuspendWorld(world);
	}

	private void WakeUpRandomWorld() {
		World selectedWorld = this.worlds[UnityEngine.Random.Range(0, this.worlds.Count)];
		this.WakeUpWorld(selectedWorld);
	}

	private bool AnyWorldLocked() {
		bool ret = false;
		foreach (World w in this.worlds) {
			if (w.Locked()) {
				ret = true;
			}
		}
		return ret;
	}

	private void SuspendWorld(World world) {
		if (world == this.activeWorld) {
			this.activeWorld = null;
		}
		if (world.isActive)
			world.Deactivate();
	}
	
	public void PokeWorldCollapse(World world) {
		Debug.Log("WorldManager was notified of collapse of world " + world.id);
		Debug.Log("WorldManager permanently added " + world.integrity + " to saved integrity!");
		this.savedIntegrity += world.integrity;
        
        uiTexts[2].text = this.savedIntegrity * 100 + "%";
        uiTexts[3].text = TotalIntegrity() * 100 + "%";
        uiTexts[4].text = world.turnController.roundNumber + "";
        uiTexts[5].text = this.maxRounds + "";
        uiManager.UpdateTurnOrder(world);

        // The world should be made unplayable immediately
        this.SuspendWorld(world);
		this.worlds.Remove(world);
        uiTexts[7].text = this.worlds.Count + "";

        // Animate a nice collapse.
        StartCoroutine(this.AnimateWorldCollapse(world));
	}

	private IEnumerator AnimateWorldCollapse(World world) {

		int n = 1;

        collapseLock.SignalStart();

		while (world.IsAnimating())
			yield return null;

		foreach(GridObject o in world.gridObjects) {
			n ++;
			o.Animate(new Fader(), Colors.collapseFadeColor, true);
			yield return new WaitForSeconds(3.0f / (n*n));
		}

		List<GridTile> tiles = new List<GridTile>();

		foreach(GameObject tile_ in world.board.tiles) {
			GridTile tile = tile_.GetComponent<GridTile>();
			tiles.Add(tile);
		}
			
		foreach(GridTile tile in tiles.OrderBy(i => UnityEngine.Random.Range(0, 100))) {
			n ++;
			tile.Animate(new Fader(), Colors.collapseFadeColor, true);
			yield return new WaitForSeconds(3.0f / (n*n));
		}

		while (world.IsAnimating())
			yield return null;
        Vector3 worldPos = world.board.tiles[2, 2].GetComponent<GridTile>().Center();
        Vector2 cameraPos = Camera.main.WorldToScreenPoint(worldPos);
        GameObject savedText = (GameObject)Instantiate(Resources.Load("SavedIntegText_pf"), uiCanvas.transform);
        savedText.transform.position = cameraPos;
        float worldSize = world.scale * 800f;
        savedText.GetComponent<RectTransform>().sizeDelta = new Vector2(worldSize, worldSize);
        savedText.GetComponent<Text>().text = "Saved " + Mathf.Ceil((world.integrity * 100)) + "%" ;

        yield return new WaitForSeconds(3);
        Destroy(savedText);

        collapseLock.SignalStop();
        
        this.DestroyWorld(world);
        
        if (worlds.Count == 0) {
            StartCoroutine(gameController.NewWorld());
        }
        
	}

    public void PokeIntegrityChange(World world, float dmgPercent) {
        Debug.Log("WorldManager was notified of damaged integrity of world " + world.id);
        Debug.Log("WorldManager permanently added " + dmgPercent + " to lost integrity!");
    }

	private float TotalIntegrity() {
		float totalIntegrity = 0;
		foreach (World w in this.worlds) {
			totalIntegrity += w.integrity;
		}
		return totalIntegrity;
	}

	private static float ScaleAfterSplit(World world) {
		return scaleMod * (world.scale / 2.0f);
	}

	private Vector3[] OriginsAfterSplit(World world) {
		Vector3 origin = world.origin;
		float newScale = WorldManager.ScaleAfterSplit(world);
		Vector3 aidsShift = new Vector3(newScale / 2, newScale / 2, 0);

		Vector3 origin3 = world.origin - aidsShift;
		Vector3 origin4 = world.origin + new Vector3(newScale, 0, 0) * this.gridSize - aidsShift
									    + new Vector3((1 - scaleMod) * newScale, 0 ,0) * (this.gridSize) * 2;
		Vector3 origin1 = world.origin + new Vector3(0, newScale, 0) * this.gridSize - aidsShift
									    + new Vector3(0, (1 - scaleMod) * newScale ,0) * (this.gridSize) * 2;
		Vector3 origin2 = world.origin + new Vector3(newScale, newScale, 0) * this.gridSize - aidsShift
									    + new Vector3((1 - scaleMod) * newScale, (1 - scaleMod) * newScale, 0) * (this.gridSize) * 2;
		return new Vector3[] {origin1, origin2, origin3, origin4};
	}

	public void DestroyWorld(World world) {
		if (this.activeWorld == world)
			this.activeWorld = null;
		this.worlds.Remove(world);
		world.DestroyAssets();
	}

	public IEnumerator SplitWorld(World world, ActionDist distro) {
		QuadTree.Node worldNode = (QuadTree.Node) this.worldTree.nodesByWorldId[world.id];
		Vector3[] newOrigins = this.OriginsAfterSplit(world);
		float newScale = WorldManager.ScaleAfterSplit(world);
        List<int> hues = new List<int>();

        while (hues.Count < 4) {
            int hue = UnityEngine.Random.Range(0, 360);
            if (hues.All(x => Math.Abs(x - hue) > 10)) {
                hues.Add(hue);
            }
        }
        


		
		World[] newWorlds = new World[4];
		for (int i = 0; i < 4; i++) {
			newWorlds[i] = this.CopyWorld(world, 
										 newOrigins[i],
										 newScale,
										 world.integrity * distro.GetWeight(i), hues[i]);
			newWorlds[i].EnqueueAction(distro.GetAction(i));
		}

		// Add new worlds to the tree
		this.worldTree.SplitNode(worldNode, newWorlds);
		this.DestroyWorld(world);

		while (this.AnyWorldLocked())
			yield return new WaitForEndOfFrame();

		for (int i = 0; i < 4; i++) {
			newWorlds[i].ExecuteNextAction();
			while (newWorlds[i].Locked())
				yield return null;
		}
		this.splitLock.SignalStop();
		yield break;
	}

	public string WorldName(World world) {
		return Util.Obfuscate(((QuadTree.Node) this.worldTree.nodesByWorldId[world.id]).quadrants);
	}
}

public class QuadTree {
	public class Node {
		public int quadrant {get; private set;}
		public List<int> quadrants {get; private set;}
		public World world {get; private set;}
		private Node parent;
		public List<Node> children {get; private set;}
		public int depth {get; private set;}

		public void RemoveWorld() {
			if (this.world == null ) {
				Debug.LogWarning("Attempted to remove world from node that had no world!");
			}
			this.world = null;
		}

		public List<Node> Siblings() {
			return this.parent.children;
		}

		public Node NextSibling(Node node) {
			if (node.quadrant == -1) {
				Debug.LogError("Asked for next sibling of the root!");
				return node;
			}
			return this.parent.children.Find(a => a.quadrant == (node.quadrant + 1) % 4);
		}

		public Node FirstChild(Node node) {
			if (node.children.Count > 0) {
				return node.children.Find(a => a.quadrant == 0);
			}
			else {
				Debug.LogError("Asked for first child of a leaf node!");
				return null;
			}
		}

		public void AddChild(Node child) {
			if (this.world != null) {
				Debug.LogError("Attempted to add a child to a node which has a world!");
			}
			this.children.Add(child);
		}

		// Parent == null implies this is the root
		// World  == null implies this is a leaf.
		public Node(Node parent, World world, int quadrant) {
			if (parent == null && world == null) {
				Debug.LogError("Added node that is both leaf and root to quad-tree");
			}
			this.world = world;
			this.parent = parent;
			this.quadrant = quadrant;
			this.children = new List<Node>();
			if (parent == null) {
				this.depth = 0;
				this.quadrants = new List<int>((int)UnityEngine.Random.Range(0, 800));
			}
			else {				
				this.quadrants = new List<int>(parent.quadrants);
				this.quadrants.Add((byte) quadrant);
				this.depth = parent.depth + 1;
			}			
		}
	}
	public Node root {get; private set;}
	public List<List<Node>> nodesByDepth {get; private set;}
	public Hashtable nodesByWorldId {get; private set;}

	public void SplitNode(Node node, World[] worlds) {
		node.RemoveWorld();
		for (int i = 0; i < worlds.GetLength(0); i++) {
			Node newNode = new Node(node, worlds[i], i);
			node.AddChild(newNode);
			this.SetLookups(newNode);
		}
	}

	private void SetLookups(Node node) {
		if (this.nodesByDepth.Count < node.depth + 1) {
			this.nodesByDepth.Add(new List<Node>());
		}
		this.nodesByDepth[node.depth].Add(node);
		if (node.world != null) {
			if (!this.nodesByWorldId.ContainsKey(node.world.id)) {
				this.nodesByWorldId.Add(node.world.id, node);
			}
			else {
				Debug.LogWarning("Set lookups for node that was already present (in world hash)!");
			}
		}
	}

	public void PrintTree() {
		Queue<Node> openNodes = new Queue<Node>();
		openNodes.Enqueue(this.root);
		while (openNodes.Count > 0) {
			Node curNode = openNodes.Dequeue();
			foreach (Node child in curNode.children) {
				openNodes.Enqueue(child);
			}
			if (curNode.world != null) {
				Debug.Log("leaf node, world: " + curNode.world.id + " quadrant: " + curNode.quadrant);
			}
			else {
				Debug.Log("internal node, quadrant: " + curNode.quadrant);
			}
		}
	}
	public QuadTree(World rootWorld, WorldManager manager) {
		this.nodesByDepth = new List<List<Node>>();
		this.nodesByDepth.Add(new List<Node>());
		this.nodesByWorldId = new Hashtable();

		this.root = new Node(null, rootWorld, -1);

		this.nodesByDepth[0].Add(this.root);
		this.nodesByWorldId.Add(root.world.id, this.root);
	}
}
