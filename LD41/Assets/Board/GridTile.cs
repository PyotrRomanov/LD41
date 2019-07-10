using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridTile : MonoBehaviour {
    public GridObject inhabitant { get; private set; }
    public bool accessible  { get; private set; }
	public Vector2Int position {get; private set;}

	private Board board;

    private Color myColor;
    private Color inaccessibleColor;
	private Lock animationLock = new Lock();

	private bool isGoalTile = false;

	public Vector3 Center() {
		return GetComponent<Transform>().position;
	}

	void OnMouseOver() {
		if (Input.GetMouseButtonDown(0)) {
			if (this.board.world.isActive) {
				Player player = this.board.world.GetPlayer();
				if (player != null)
	    			this.board.world.GetPlayer().eventManager.AddEvent(new TileClickedEvent("TileClicked@" + this.position, this.position));
			}
		}
		Canvas uiCanvas = FindObjectOfType<Canvas>();
        uiCanvas.GetComponentsInChildren<Text>()[13].text = this.position.ToString();
	}

	void Update() {
		if (this.IsInhabited()) {
			SpriteRenderer R = this.inhabitant.GetComponent<SpriteRenderer>();
			if (this.IsAnimating() && this.IsInhabited()) {
				R.color = new Color(R.color.r, R.color.g, R.color.b, 0);
			} 
			else {
				if (R.color.a != 1) {
					R.color = new Color(R.color.r, R.color.g, R.color.b, 1);
				}
			}
		}
	}

	public void Populate(GridObject obj, bool animate = true) {
		if (this.inhabitant != null) {
			Debug.LogError("Attempted to populated a tile that was already populated!");
			return;
		}
		this.inhabitant = obj;
		this.inhabitant.MoveToPosition(this.Center(), Options.Animation.animateMovement);
        this.inhabitant.position = this.position;
	}

    public void Depopulate() {
        this.inhabitant = null;
    }

	public bool IsAnimating() {
		return this.animationLock.Locked();
	}

	public void Animate(SpriteColorAnimator A, Color c, bool locker = false) {
		if (!this.IsAnimating()) {
			if (locker)
				StartCoroutine(A.Animate(GetComponent<SpriteRenderer>(), c, this.animationLock));
			else 
				StartCoroutine(A.Animate(GetComponent<SpriteRenderer>(), c));
		}
	}

	public bool IsInhabited () {
		return this.inhabitant != null;
	}

    public void DestroyInhabitant() {
		if (inhabitant != null) {
        	Destroy(inhabitant.gameObject);
		}
	}

	public void ResetColoring() {
		if (this.accessible) {
			GetComponent<SpriteRenderer>().color = myColor;
		}
		else {
			GetComponent<SpriteRenderer>().color = inaccessibleColor;
		}
	}

	public void ToggleAccessibility() {
		if (!this.accessible) {
			this.accessible = true;
			GetComponent<SpriteRenderer>().color = myColor;
		}
		if (this.accessible) {
			this.accessible = false;
			GetComponent<SpriteRenderer>().color = inaccessibleColor;
		}
	}

	public void MakeInAccessible() {
		if (this.accessible) {
			this.accessible = false;
			GetComponent<SpriteRenderer>().color = inaccessibleColor;
		}
	}

	public void MakeGoalTile() {
		this.isGoalTile = true;
		this.myColor = Colors.goalTile;
		this.ResetColoring();
	}

	public bool IsGoalTile() {
		return this.isGoalTile;
	}
	
    // This copy setup copies the settings of an original tile.
    // The following are not copied:
    //      - Inhabitants.
	public void Setup (GridTile original, World world) {
		this.accessible = original.accessible;
		this.position = original.position;
		this.isGoalTile = original.isGoalTile;
		this.board = world.board;
        this.myColor = world.worldColor.ShiftHue(original.myColor);
        this.inaccessibleColor = world.worldColor.unpassableTile;
        this.ResetColoring();
	}

	public void Setup(Board board, Vector2Int position, Color color, Color inaccessibleColor) {
		this.board = board;
		this.accessible = true;
		this.GetComponent<SpriteRenderer>().sortingLayerName = "MainLayer";
        this.GetComponent<SpriteRenderer>().sortingOrder = 0;
		this.position = position;
        this.myColor = color;
        this.inaccessibleColor = inaccessibleColor;
		this.ResetColoring();
	}
}