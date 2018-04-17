using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWorld {


	public int sizeX { get; private set; }
	public int sizeY { get; private set; }

	private GameObject[,] tiles;
    public GridWorld (int sizeX, int sizeY) {
		this.sizeX = sizeX;
		this.sizeY = sizeY;
		tiles = new GameObject[sizeX, sizeY];

		for (int i = 0; i < sizeX; i++) { 
			for (int j = 0; j < sizeY; j++) {
				tiles[i, j] = (GameObject) GridTile.Instantiate(Resources.Load("tile_pf"));
				tiles[i, j].GetComponent<Transform>().position = new Vector3(i, j, 0);
				if (i % 2 != j % 2) {
					tiles[i, j].GetComponent<GridTile>().ToggleAccessibility();
				}
			} 
		}
	}	
}