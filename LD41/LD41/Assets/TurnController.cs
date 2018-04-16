using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnController : MonoBehaviour {
    List<Agent> agents;
    Queue<Agent> roundQueue;
    int roundNumber;
    // Use this for initialization
    void Start () {
        agents = FindObjectsOfType<Agent>().ToList();
        agents.OrderBy(a => a.speed);
        NextRound();
    }
	
	// Update is called once per frame
	void Update () {

	}

    void NextRound() {
        roundQueue = new Queue<Agent>();
        foreach (Agent a in agents) {
            roundQueue.Enqueue(a);
        }
        roundNumber++;
        NextTurn();
    }

    public void NextTurn() {
        if (roundQueue.Count > 0) {
            Debug.Log(roundQueue.Count);
            roundQueue.Dequeue().TakeTurn();
        } else {
            NextRound();
        }
        
    }
}
