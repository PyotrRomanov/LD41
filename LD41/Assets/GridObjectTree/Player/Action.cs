using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDist {
	ActionWeightPair[] actions;

	public Action GetRandomAction() {
		int r = Random.Range(0, 4);
		return this.actions[r].action;
	}

	public Action GetAction(int i) {
		return this.actions[i].action;
	}

	public float GetWeight(int i) {
		return this.actions[i].weight;
	}

	public ActionDist(ActionWeightPair[] actions) {
		this.actions = actions;
		Debug.Assert(actions.GetLength(0) == 4);
		Debug.Assert((actions[0].weight + actions[1].weight +
					  actions[2].weight + actions[3].weight)
					  	== 1.0f);
	}

	public static ActionDist NonActions(float[] weights) {
		Debug.Assert(weights.GetLength(0) == 4);
		ActionWeightPair[] a = new ActionWeightPair[] 
		{
			new ActionWeightPair(new NonAction(), weights[0]),
			new ActionWeightPair(new NonAction(), weights[1]),
			new ActionWeightPair(new NonAction(), weights[2]),
			new ActionWeightPair(new NonAction(), weights[3])
		};
		return new ActionDist(a);
	}

	public static ActionDist NonActions() {
		return ActionDist.NonActions(new float[] {0.25f, 0.25f, 0.25f, 0.25f});
	}

	public static ActionDist DegeneratedNonActions() {
		return ActionDist.NonActions(new float[] {1.0f, 0, 0, 0});
	}

	public bool IsDegenerated() {
		foreach (ActionWeightPair a in this.actions) {
			if (a.weight == 1.0f) {
				return true;
			}
		}
		return false;		 
	}

}

public class ActionWeightPair {
	public float weight {get; private set;}
	public Action action {get; private set;}

	public static ActionWeightPair Zero() {
		return new ActionWeightPair(null, 0.0f);
	}

	public ActionWeightPair(Action action, float weight) {
		Debug.Assert(weight >= 0 && weight <= 1);
		Debug.Assert(action != null);
		this.action = action;
		this.weight = weight;
	}
}

// This class should contain all data needed to create an ActionDist.
public class ActionInfo {
	public Vector2Int originPos = Board.InvalidPos;
	public int originId = -1;

	public Vector2Int targetPos = Board.InvalidPos;
	public int targetId = -1;
}
public abstract class Action {
    public ActionInfo info {get; private set;}

    protected Action(ActionInfo info) {        
        this.info = info;
    }
	public abstract IEnumerator Execute(Board board, Lock locker = null);
}

public class NonAction : Action {
	public override IEnumerator Execute(Board board = null, Lock locker = null) {yield break;}
	public NonAction(ActionInfo info = null) : base(info) {}
}

public abstract class TargetedAction : Action {

    public TargetedAction(Board board, ActionInfo info) : base(info) {
        
    }

	public abstract bool[,] PotentialTargets(Board board, ActionInfo info);
}

public abstract class MetaAction
{
    public abstract ActionDist Execute(ActionInfo info);
}

public abstract class TargetedMetaAction : MetaAction
{

    public abstract bool[,] PotentialTargets(Board board, ActionInfo info);
}




