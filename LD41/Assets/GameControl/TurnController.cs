using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnController {
    
    private World world;
    private List<int> agents;
    private Queue<int> roundQueue;
    private int agentToAct;
    
    public int roundNumber { get; private set; }

    public TurnController(World world) {
        this.world = world;
        this.agents = new List<int>();
        foreach (GridObject o in world.gridObjects) {
            if (o is Agent) {
                ((Agent) o).turnController = this;
                this.agents.Add(o.id);
            }
        }
        this.agents = this.agents.OrderBy(i => this.AgentById(i).turnPriority).ToList();
        ResetQueue();
    }
    
    public TurnController(World world, TurnController original) {
        this.world = world;
        this.roundNumber = original.roundNumber;
        this.agents = original.agents.OrderBy(i => this.AgentById(i).turnPriority).ToList();                
        
        foreach (int id in this.agents) {
            this.AgentById(id).turnController = this;
        }
        this.roundQueue = new Queue<int>(original.roundQueue.ToList());
    }

    // Attempt to start the next turn.
    public void TryNextTurn() {
        if (!this.AgentTakingTurn() && !this.world.Locked() && this.world.isActive)
            this.NextTurn();
    }

    // Agents should call this when they end their turn.
    public void PokeEndedTurn(Agent agent) {
        if (agent is Player) {
            this.world.PokePlayerEndedTurn();
        }
    }

    public void RemoveAgent(int id) {
        this.agents.Remove(id);
        this.roundQueue = new Queue<int>(this.roundQueue.Where(i => i != id));
    }    

    // Actually start a new turn by calling StartTurn on an agent.
    private void NextTurn() {
        if (roundQueue.Count == 0) {
            this.world.PokeEndOfRound(this.roundNumber);
            this.ResetQueue();
        }

        this.agentToAct = roundQueue.Dequeue();
        this.AgentById(this.agentToAct).StartTurn();
    }

    private bool AgentTakingTurn() {
        bool ret = false;
        foreach(int id in this.agents) {
            if (this.AgentById(id).TakingTurn())
                ret = true;
        }
        return ret;
    }

    private Agent AgentById(int id) {
        GridObject o = this.world.GridObjectById(id);
        if(o is Agent) {
            return (Agent) o;
        }
        else {
            Debug.LogError("GridObject with ID " + id + " is not an Agent!");
            return null;
        } 
    }

    private void ResetQueue() {
        roundNumber ++;
        roundQueue = new Queue<int>();
        foreach (int id in agents) {
            roundQueue.Enqueue(id);
        }
    }
}
