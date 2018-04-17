using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour {
	GridObject inhabitant {get; set;}
	private bool accessible = false;

	public void ToggleAccessibility() {
		if (!this.accessible) {
			this.accessible = true;
			GetComponent<SpriteRenderer>().color = Color.red;
		}
	}
	
	void Start() {
		if (this.accessible) {
			GetComponent<SpriteRenderer>().color = Color.red;
		}
	}
}