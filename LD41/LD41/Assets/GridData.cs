using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour {

	const int gridSizeX = 4;
	const int gridSizeY = 4;

    public Tile[,] tiles { get; private set; }

    // Use this for initialization
    public GridData () {
		tiles = new Tile[gridSizeX, gridSizeY];
		for (int i = 0; i < gridSizeX; i++) {
			for (int j = 0; j < gridSizeY; j++) {
				tiles[i, j] = new Tile();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

public class Tile {
	GridObject inhabitant;
	bool accessible = true;
	public void SetInhabitant(GridObject inhabitant) {
		if (this.inhabitant == null) {
			this.inhabitant = inhabitant;
		}
		else {
			Debug.LogError("Attempted to set an inhabitant on a tile that already had one!");
		}
	}
	public GridObject GetInhabitant() {
		return this.inhabitant;
	}
	public void RemoveInhabitant() {
		this.inhabitant = null;
	}

	public bool GetAccessible() {
		return this.accessible;
	}
	public void SetAccessible(bool accessible) {
		this.accessible = accessible;
		if (!accessible && this.inhabitant != null) {
			Debug.LogError("Made tile accessible while it still had an inhabitant!");
		}
	}
}
