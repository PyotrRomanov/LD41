using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scattershot : TargetedMetaAction {

    public override ActionDist Execute(ActionInfo info)
    {
        ActionWeightPair[] weightedActions = new ActionWeightPair[4];
        if (info.targetId != World.InvalidId && info.originId != World.InvalidId) {
            Vector2Int[] moves = { new Vector2Int(0,1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0), };
            for (int i = 0; i < moves.Length; i++) {
                weightedActions[i] = new ActionWeightPair
                                        (
                                            new MoveAction(info, moves[i]), 
                                            0.25f
                                        );
            }
            return new ActionDist(weightedActions);
        }
        else {
            return ActionDist.DegeneratedNonActions();
        }        
    }

    override public bool[,] PotentialTargets(Board board, ActionInfo info) {
        bool[,] targets = new bool[board.size, board.size];
        for (int i = 0; i < board.size; i++) {
            for (int j = 0; j < board.size; j++) {
                GridObject origin = board.GridObjectById(info.originId);
                if (origin.position.x == i || origin.position.y == j) {
                    targets[i, j] = true;
                }
            }
        }
        return targets;
    }
}

public class Swaperoo : TargetedMetaAction {

    public override ActionDist Execute(ActionInfo info)
    {
        ActionWeightPair[] weightedActions = new ActionWeightPair[4];
        if (info.targetId != World.InvalidId && info.originId != World.InvalidId) {
            weightedActions[0] = new ActionWeightPair(new SwapAction(info), 1);
            weightedActions[1] = new ActionWeightPair(new NonAction(), 0);
            weightedActions[2] = new ActionWeightPair(new NonAction(), 0);
            weightedActions[3] = new ActionWeightPair(new NonAction(), 0);
            return new ActionDist(weightedActions);
        }
        else {
            return ActionDist.DegeneratedNonActions();
        }        
    }

    override public bool[,] PotentialTargets(Board board, ActionInfo info) {
        bool[,] targets = new bool[board.size, board.size];
        for (int i = 0; i < board.size; i++) {
            for (int j = 0; j < board.size; j++) {
                Vector2Int pos = new Vector2Int(i ,j);
                if (board.GridObjectById(info.originId).position != pos && board.TileByPos(pos).IsInhabited()) {
                    targets[i, j] = true;
                }
            }
        }
        return targets;
    }
}

public class CrossFire : MetaAction {
    public override ActionDist Execute(ActionInfo info)
    {
        ActionWeightPair[] weightedActions = new ActionWeightPair[4];
        Vector2Int[] dirs = { new Vector2Int(0,1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0), };
        for (int i = 0; i < dirs.Length; i++) {
                weightedActions[i] = new ActionWeightPair
                                        (
                                            new DirectionalShot(info, dirs[i]), 
                                            0.25f
                                        );
            }
        return new ActionDist(weightedActions);
    }    
}

public class SwapAction : Action {
    public SwapAction(ActionInfo info) : base(info) { }

    public override IEnumerator Execute(Board board, Lock locker = null) {
        if (locker != null)
            locker.SignalStart();
        if (info.targetId != World.InvalidId && info.targetId !=  World.InvalidId) {
            GridObject origin = board.GridObjectById(info.originId);
            GridObject target = board.GridObjectById(info.targetId);            

            // Make origin blink and wait for it to finish.
            origin.Animate(new Blinker(), Colors.movementActionColor, true);
            while(origin.IsAnimating())
                yield return null;

            // Make target blink and wait for it to finish.
            target.Animate(new Blinker(), Colors.movementActionColor, true);
            while(target.IsAnimating())
                yield return null;

            board.SwapObjects(origin, target);
            board.world.DamageIntegrity(0.5f);
            board.world.manager.audioPlayer.PlaySound("ActionSound", 0.5f);
        }
        else {
            Debug.LogError("Invalid info provided to Swapaction");
        }


        if (locker != null)
            locker.SignalStop();
        yield return null;
    }
}

public class DOOM : MetaAction {
    public override ActionDist Execute(ActionInfo info)
    {
        ActionWeightPair[] weightedActions = new ActionWeightPair[4];
        int[] quadrants = {1, 2, 3, 4};
        for (int i = 0; i < quadrants.Length; i++) {
                weightedActions[i] = new ActionWeightPair
                                        (
                                            new LocalDOOM(info, quadrants[i]), 
                                            0.25f
                                        );
            }
        return new ActionDist(weightedActions);
    }    
}

public class LocalDOOM : Action {
    int quadrant;

    public LocalDOOM(ActionInfo info, int quadrant) : base(info) {
        this.quadrant = quadrant;
    }

    private bool InQuadrant (Vector2Int pos, int gridSize) {
        switch (this.quadrant) {
            case 1:
                return (2 * pos.x < gridSize - 1 && 2 * pos.y + 1 > gridSize);
            case 2:
                return (2 * pos.x + 1 > gridSize && 2 * pos.y + 1 > gridSize);            
            case 3:
                return (2 * pos.x < gridSize - 1 && 2 * pos.y < gridSize - 1);
            case 4:
                return (2 * pos.x + 1 > gridSize && 2 * pos.y < gridSize - 1);
            default:
                return false;
        }
    }

    public override IEnumerator Execute(Board board, Lock locker = null) {
        if (locker != null)
            locker.SignalStart();
        foreach (GameObject g_ in board.tiles) {
            GridTile g = g_.GetComponent<GridTile>();
            if (this.InQuadrant(g.position, board.size)) {
                g.Animate(new HueSine(), Colors.DOOM, true);
                g.MakeInAccessible();
                if (g.IsInhabited() && g.inhabitant is Agent)
                    ((Agent) g.inhabitant).DoDamage(1);
                // TODO: add accessibility toggle?                
            }
        }
        if (locker != null) 
            locker.SignalStop();

        yield return null;
    }
}

public class MoveAction : Action {
    Vector2Int movement;
    public MoveAction(ActionInfo info, Vector2Int movement) : base(info) {
        this.movement = movement;
    }

    public override IEnumerator Execute(Board board, Lock locker = null) {
        if (locker != null)
            locker.SignalStart();

        if (info.targetId != World.InvalidId && info.originId != World.InvalidId) {
            GridObject target = board.GridObjectById(info.targetId);       
            // Make target blink and wait for it to finish.
            target.Animate(new Blinker(), Colors.movementActionColor, true);
            while(target.IsAnimating())
                yield return null;
            board.MoveObject(target, movement);
        } 
        else {
            Debug.LogError("Invalid info provided to MoveAction!");
        }

        if (locker != null)
            locker.SignalStop();
        yield return null;
    }
}

public class DirectionalShot : Action {
    Vector2Int direction;
    public DirectionalShot(ActionInfo info, Vector2Int direction) : base(info) {
        this.direction = direction;
    }

    public override IEnumerator Execute(Board board, Lock locker = null) {
        if (locker != null)
            locker.SignalStart();
        if (info.originId != World.InvalidId) {
            Vector2Int targetPos = board.GridObjectById(info.originId).position + this.direction;
            while (board.InBounds(targetPos)) {
                board.TileByPos(targetPos).Animate(new Blinker(), Colors.directionalShotColor, true);

                // Make targeted tile blink and wait for 0.2 seconds.
                if(board.TileByPos(targetPos).IsAnimating())
                    yield return new WaitForSeconds(0.02f);

                if (board.TileByPos(targetPos).IsInhabited()) {
                    board.TileByPos(targetPos).inhabitant.DoDamage(100);
                }
                targetPos += this.direction;
            }
        }
        if (locker != null) {
            locker.SignalStop();
        }
        yield return null;
    }
}