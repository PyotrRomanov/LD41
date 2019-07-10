using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : GridObject {
    public int turnPriority { get; protected set; }
    public TurnController turnController;

    protected bool takingTurn;
    
    public virtual void StartTurn() {
        this.takingTurn = true;
    }

    protected virtual void EndTurn() {
        this.takingTurn = false;
        this.turnController.PokeEndedTurn(this);
    }

    public bool TakingTurn() { return this.takingTurn; }

    override protected void Kill () {
        this.turnController.RemoveAgent(this.id);
        base.Kill();
    }

    override public void Setup(GridObject original, Board board) {
        this.turnPriority = ((Agent) original).turnPriority;
        base.Setup(original, board); 
    }
}
