using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour {

	const int gridSizeX = 4;
	const int gridSizeY = 4;

	// Use this for initialization
	void Start () {
		Tile[,] tileData = new Tile[gridSizeX, gridSizeY];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

public class Tile {
	GridObject inhabitant;
	public void SetInhabitant(GridObject inhabitant) {
		this.inhabitant = inhabitant;
	}

	public GridObject GetInhabitant() {
		return inhabitant;
	}
	
	public Tile() {
	
	}
}
