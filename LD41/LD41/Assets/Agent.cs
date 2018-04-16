using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : GridObject {
    public int turnPriority;
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
public class WeightedAction {
	float weight;
	Action action;
	public WeightedAction(Action action, float weight) {
		this.action = action;
		this.weight = weight;
	}
}

public class MetaAction {
	List<WeightedAction> actions;
}
public class Action {
	
}
public class Movement : Action {

}

public class TileInteraction : Action {
	
}

public class GridObjectInteraction : Action {
	
}

public class ProjectileAction : Action {

}
