using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renderer : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GridData grid = FindObjectOfType<GridData>();
        foreach (Tile t in grid.tiles) {

        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
