using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRenderer : MonoBehaviour {

	public GameObject tilePf;
	private GridData gridData;
	private GameObject[,] tiles;
	void Start () {
        // GridData gridData = FindObjectOfType<GridData>();
		this.gridData = new GridData();

		tiles = new GameObject[GridData.gridSizeX, GridData.gridSizeY];

        for (int i = 0; i < GridData.gridSizeX; i ++) {
			for (int j = 0; j < GridData.gridSizeX; j ++) {
				Debug.Log("Created a render tile!");
				this.tiles[i,j] = Instantiate(tilePf, new Vector3(i, j, 0), Quaternion.identity);
			}
        }
	}
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < GridData.gridSizeX; i ++) {
			for (int j = 0; j < GridData.gridSizeX; j ++) {
				if (!this.gridData.tiles[i, j].Accessible()) {
					this.tiles[i,j].GetComponent<SpriteRenderer>().color = Color.red;
				}
			}
        }
	}
}
