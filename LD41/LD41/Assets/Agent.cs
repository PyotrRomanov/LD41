using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : GridObject {
    public int speed { get; private set; }
    TurnController turnController;
    bool myTurn = false;

	// Use this for initialization
	void Start () {
        turnController = FindObjectOfType<TurnController>();
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    public void TakeTurn() {
        myTurn = true;
    }

    void EndTurn() {
        turnController.NextTurn();
    }
}
