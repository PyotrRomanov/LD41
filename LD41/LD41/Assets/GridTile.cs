using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour {
	GridObject inhabitant {get; set;}
	private bool accessible = false;

	private Vector3 Center() {
		return GetComponent<Transform>().position;
	}

	public void Populate(GridObject obj) {
		inhabitant = obj;
		inhabitant.GetComponent<Transform>().position = this.Center();
	}

	public void ToggleAccessibility() {
		if (!this.accessible) {
			this.accessible = true;
			GetComponent<SpriteRenderer>().color = Color.red;
		}
	}
	
	void Start() {
	}
}