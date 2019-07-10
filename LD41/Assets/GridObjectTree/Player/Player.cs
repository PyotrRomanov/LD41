using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : Agent {
    private static string cursorPf = "targetcursor_pf";

    public int movementRange { get; private set; }
    static float timeLimit = -1;
    private float turnTimer = 0f;

    private bool[,] reachableTiles;
    ui_ability_script[] buttonManager;
    UIManager uiManager;
    Text timerField;
    Vector2Int startPosition;

    private bool movePhase = false;
    private bool selectActionPhase = false;
    
    private int movesPerTurn = 1;
    private int movesTaken = 0;

    KeyCode keyPressed;
 
    public override void StartTurn() {
        base.StartTurn();

        this.turnTimer = 0f;
        
        this.startPosition = position;
        
        this.movePhase = true;
        this.movesTaken = 0;
        this.selectActionPhase = true;
    }

    void TileClicked(Vector2Int pos) {
        Debug.Log(this + " logged " + pos);
    }

    private void TargetPhase(MetaAction metaAction, KeyCode keyPressed) {
        ActionInfo actionInfo = new ActionInfo();
        if (metaAction is TargetedMetaAction) {
            TargetedMetaAction targetAction = metaAction as TargetedMetaAction;
            GameObject cursor = Instantiate((GameObject) Resources.Load(Player.cursorPf));
            actionInfo.originId = this.id;
            actionInfo.originPos = this.position;
            cursor.GetComponent<TargetCursor>().Setup(this, targetAction.PotentialTargets(board, actionInfo), targetAction, actionInfo, keyPressed);
            selectActionPhase = false;
        } else {
            actionInfo.originId = this.id;
            actionInfo.originPos = this.position;
            actionInfo.targetId = World.InvalidId;
            actionInfo.targetPos = Board.InvalidPos;
            EndTargetPhase(metaAction, actionInfo);
        }
    }

    private bool CanMove() {
        return this.movesTaken < this.movesPerTurn;
    }

    private void EndMovementPhase() {
        movePhase = false;
        this.board.ResetColorings();
    }

    private void MarkReachableTiles(int movementRange, Vector2Int pos, Color color) {
        board.MarkTiles(this.reachableTiles, color);
    }
    
    private void EndSelectActionPhase() {
        selectActionPhase = false;
    }

    public void CancelTargetPhase() {
        foreach (ui_ability_script b in buttonManager) {
            b.TurnOffHighlight();
        }
        if (this.CanMove())
            this.movePhase = true;
        selectActionPhase = true;
    }

    public void EndTargetPhase(MetaAction metaAction, ActionInfo actionInfo) {
        foreach (ui_ability_script b in buttonManager) {
            b.TurnOffHighlight();
        }

        ActionDist actions = metaAction.Execute(actionInfo);
        this.board.world.ExecuteActionDist(actions);
        this.EndTurn();
    }

    override public void Setup(Board board, int x, int y, int id) {
        base.Setup(board, x, y, id);
        this.health = 100;
        buttonManager = FindObjectOfType<Canvas>().gameObject.GetComponentsInChildren<ui_ability_script>();
        Canvas uiCanvas = FindObjectOfType<Canvas>();
        timerField = uiCanvas.GetComponentsInChildren<Text>()[10];
    }

    override public void Activate() {
        base.Activate();

        this.turnTimer = 0f;
        this.Animate(new Blinker(), Color.white, true);
        
        // TODO: this should not be here.
        Canvas uiCanvas = FindObjectOfType<Canvas>();
        timerField = uiCanvas.GetComponentsInChildren<Text>()[10];
    }

    override public void Setup(GridObject original, Board board) {
        base.Setup(original, board);
        this.movementRange = 2;
        this.buttonManager = FindObjectOfType<Canvas>().gameObject.GetComponentsInChildren<ui_ability_script>();
    }
    
	override protected void Update () {
        base.Update();

        this.HandleEvents();


        if(this.takingTurn && this.board.world.isActive) {
            this.IncrementTurnTime();
        }

        if (this.board.world.isActive) {
            HandleInput();
            timerField.text = (int)(timeLimit - turnTimer) + "";
        }
    }

    protected void HandleEvents() {
        if (this.eventManager.HasNext()) {
            Event next = this.eventManager.GetNext();
            Debug.Log("Handling event " + next.name);
            if(this.board.world.isActive && this.movePhase) {
                if (next is TileClickedEvent) {
                    Vector2Int move = ((TileClickedEvent) next).pos - this.position;
                    HandleMove(move);
                }
            }
        }
    }

    override protected void Kill() {
        this.board.world.DamageIntegrity(1f);
        base.Kill();
    }

    private void HandleInput() {
        if (this.movePhase)
            HandleMovementInput();
        if (this.selectActionPhase)
            HandleSelectActionInput();
    }

    private void HandleMovementInput() {
        Vector2Int move;
        if (Input.GetKeyDown(Options.KeyBinds.down)) {
            move = new Vector2Int(0, -1);
        }
        else if (Input.GetKeyDown(Options.KeyBinds.up)) {
            move = new Vector2Int(0, 1);
        }
        else if (Input.GetKeyDown(Options.KeyBinds.right)) {
            move = new Vector2Int(1, 0);
        }
        else if (Input.GetKeyDown(Options.KeyBinds.left)) {
            move = new Vector2Int(-1, 0);
        }
        else {
            return;
        }
        this.HandleMove(move);
    }

    bool LegalMove(Vector2Int move) {
        return Mathf.Abs(move.x) + Mathf.Abs(move.y) <= 1;
    }

    void HandleMove(Vector2Int move) {
        if (this.LegalMove(move)) {
            Vector2Int newPosition = this.position + move;
            if (this.board.InBounds(newPosition)) {
                if (this.board.MoveObject(this, move))
                    this.movesTaken += 1;
                if (!this.CanMove())
                    this.EndTurn();
            }
        }
    }

    void HandleSelectActionInput() {
        if (Input.GetKeyDown(Options.KeyBinds.action1)) {
            this.EndMovementPhase();
            this.EndSelectActionPhase();

            TargetPhase(new Scattershot(),Options.KeyBinds.action1);
            buttonManager[0].TurnOnHighlight();
            return;
        }

        else if (Input.GetKeyDown(Options.KeyBinds.action2)) {
            this.EndMovementPhase();
            this.EndSelectActionPhase();

            TargetPhase(new CrossFire(), Options.KeyBinds.action2);
            return;
        }

        else if (Input.GetKeyDown(Options.KeyBinds.action3)) {
            this.EndSelectActionPhase();
            this.EndMovementPhase();

            TargetPhase(new Swaperoo(), Options.KeyBinds.action3);
            buttonManager[3].TurnOnHighlight();
            return;
        }

        else if (Input.GetKeyDown(Options.KeyBinds.action4)) {
            this.EndSelectActionPhase();
            this.EndMovementPhase();
            
            TargetPhase(new DOOM(), Options.KeyBinds.action4);
            return;
        }

        else if (Input.GetKeyDown(Options.KeyBinds.action0)) {
            this.EndTurn();
            return;
        }
        
        else if (Input.GetKeyDown(Options.KeyBinds.showMovements)) {
            if(!this.board.world.IsAnimating()) {
                StartCoroutine(AnimateEnemyMoves());
            }
            return;
        }

        else if (this.OutOfTime()) {
            this.HandleTimeOut();
            return;
        }
    }

    override protected void EndTurn() {
        if (board.TileByPos(this.position).IsGoalTile()) {
            this.board.world.PlayerReachedGoal();
        }
        this.EndMovementPhase();
        this.EndSelectActionPhase();
        base.EndTurn();
    }

    public void IncrementTurnTime() {
        this.turnTimer += Time.deltaTime;
    }

    public bool OutOfTime() {
        if (Player.timeLimit == -1) {
            return false;
        }
        return this.turnTimer > Player.timeLimit;
    }

    public void HandleTimeOut() {
        foreach (ui_ability_script b in buttonManager) {
            b.TurnOffHighlight();
        }
        this.EndTurn();
    }

    private IEnumerator AnimateEnemyMoves() {
        List<Agent> agents = board.world.Agents().FindAll(x => x is Enemy);
        agents = agents.OrderBy(x => x.turnPriority).ToList();
        List<MoveListEnemy> enemies = new List<MoveListEnemy>();
        foreach (Agent a in agents) {
            enemies.Add(a as MoveListEnemy);
        }
        foreach (MoveListEnemy e in enemies) {
            List<Vector2Int> moves = e.FindNextMoves();
            Blinker blink = new Blinker();
            Color color = new Color(e.GetComponent<SpriteRenderer>().color.r, e.GetComponent<SpriteRenderer>().color.g, e.GetComponent<SpriteRenderer>().color.b);
            foreach (Vector2Int move in moves) {
                board.tiles[e.position.x + move.x, e.position.y + move.y].GetComponent<GridTile>().Animate(blink, color, true);
                while (board.tiles[e.position.x + move.x, e.position.y + move.y].GetComponent<GridTile>().IsAnimating()) {
                    yield return null;
                }
            }
        }
        yield break;
    }

    public override void UpdateColor()
    {
        GetComponent<SpriteRenderer>().color = board.world.worldColor.playerColor;
    }
}

