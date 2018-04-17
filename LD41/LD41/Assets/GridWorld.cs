using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWorld : MonoBehaviour{
	public int sizeX { get; private set; }
	public int sizeY { get; private set; }

	private Vector3 origin;
	private float scale;

	public void SpawnGridObject(GridObject obj, int x, int y) {
		tiles[x,y].GetComponent<GridTile>().Populate(obj); 
	}

	private GameObject[,] tiles;

	public void DestroySelf() {
		foreach (GameObject tile in this.tiles) {
			Destroy(tile);
		}
	}
    public GridWorld (int sizeX, int sizeY, Vector3 origin, float scale) {
		this.sizeX = sizeX;
		this.sizeY = sizeY;
		this.origin = origin;
		this.scale = scale;
		tiles = new GameObject[sizeX, sizeY];

		for (int i = 0; i < sizeX; i++) { 
			for (int j = 0; j < sizeY; j++) {
				tiles[i, j] = (GameObject) GridTile.Instantiate(Resources.Load("tile_pf"));
				tiles[i, j].GetComponent<Transform>().position = new Vector3(i * this.scale, j * this.scale, 0) + origin;
				tiles[i, j].GetComponent<Transform>().localScale *= this.scale;
				if (i % 2 != j % 2) {
					tiles[i, j].GetComponent<GridTile>().ToggleAccessibility();
				}
			} 
		}
	}	
}