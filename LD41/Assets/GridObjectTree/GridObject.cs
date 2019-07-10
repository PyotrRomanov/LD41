using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridObject : MonoBehaviour {
    public Board board { get; private set; }
    public Vector2Int position { get; set; }
    public EventManager eventManager = new EventManager();

    public int id {get; private set;}
    protected Lock animationLock = new Lock();
    protected Lock movementLock = new Lock();
    protected bool dead = false;

    protected int bumpDmg = 25;
    protected int health = 50;


    protected virtual void Update() {
        if (this.dead && !this.IsAnimating()) {
            this.DestroyAndClean();
        }
    }

    public virtual void Setup(Board board, int x, int y, int id) {
        this.board = board;
        this.id = id;
        this.position = new Vector2Int(x, y);

        this.UpdateColor();
        this.GetComponent<SpriteRenderer>().sortingLayerName = "MainLayer";
        this.GetComponent<SpriteRenderer>().sortingOrder = 1;
    }

    public virtual void Setup(GridObject original, Board board) {
        this.board = board;
        this.position = original.position;
        this.id = original.id;

        this.UpdateColor();
        this.GetComponent<SpriteRenderer>().sortingLayerName = "MainLayer";
        this.GetComponent<SpriteRenderer>().sortingOrder = 1;       
    }

    public virtual void DestroyAndClean() {
        this.board.tiles[this.position.x, this.position.y].GetComponent<GridTile>().Depopulate();
        this.board.world.RemoveGridObject(this);
        Destroy(this.gameObject);
    }

    public bool IsAnimating() {
        return this.animationLock.Locked() || this.movementLock.Locked();
    }

    public bool IsMoving() {
        return this.movementLock.Locked();
    }

    public void Animate(SpriteColorAnimator A, Color c, bool locker = false) {
        if(!this.IsAnimating()) {
            if (locker)
                StartCoroutine(A.Animate(this.GetComponent<SpriteRenderer>(), c, this.animationLock));
            else
                StartCoroutine(A.Animate(this.GetComponent<SpriteRenderer>(), c));
        }
    }

    public void AnimateMove(MovementAnimator A, Vector3 newPos, bool locker = false) {
        if (locker)
            StartCoroutine(A.Animate(this.GetComponent<Transform>(), newPos, this.movementLock));
        else
            StartCoroutine(A.Animate(this.GetComponent<Transform>(), newPos));
    }

    public void MoveToPosition(Vector3 newPos, bool animate) {
        if (!animate)
            this.GetComponent<Transform>().position = newPos; 
        else
            this.AnimateMove(new MovementAnimator(new LogisticInterpolator()), newPos, true);
    }

    public void BumpIntoObject(GridObject o) {
        if (Options.Animation.animateBump)
            StartCoroutine(this.AnimateBump(o));
        o.DoDamage(this.bumpDmg);
    }

    private IEnumerator AnimateBump(GridObject o) {
        MovementAnimator A = new MovementAnimator(new LogisticInterpolator());
        A.SetSpeed(1.5f);
        Vector3 newPos = (this.GetComponent<Transform>().position + board.TileByPos(o.position).Center()) / 2f;
        this.AnimateMove(A, newPos, true);
        while(this.IsAnimating())
            yield return new WaitForEndOfFrame();
        this.AnimateMove(A, board.TileByPos(this.position).Center(), true);
    }

    public void OnMouseOver() {
        Canvas uiCanvas = FindObjectOfType<Canvas>();
        uiCanvas.GetComponentsInChildren<Text>()[12].text = this.health.ToString();
        uiCanvas.GetComponentsInChildren<Text>()[13].text = this.position.ToString();
    }

    public void OnMouseExit() {
        Canvas uiCanvas = FindObjectOfType<Canvas>();
        uiCanvas.GetComponentsInChildren<Text>()[12].text = "-";
        uiCanvas.GetComponentsInChildren<Text>()[13].text = "-";
    }

    protected virtual void Kill() {
        this.Animate(new Fader(), Colors.agentDeath, true);
        this.dead = true;
    }

    public virtual void DoDamage(int dmg) {
        this.health -= dmg;
        if (this.health <= 0)
            this.Kill();
    }

    public virtual void MovedOutOfBounds(Vector2Int pos) {
        this.Kill();
    }

    public virtual void UpdateColor() {
        
    }

    public virtual void Activate () { }
}
