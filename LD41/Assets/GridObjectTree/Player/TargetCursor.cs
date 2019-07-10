using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCursor : MonoBehaviour {
        private Vector2Int position;
        private bool[,] mask;
        private Board board;
        private Player player;
        private TargetedMetaAction action;
        private ActionInfo actionInfo;
        private KeyCode keyPressed;

        public void Setup(Player player, bool[,] mask, TargetedMetaAction action, ActionInfo actionInfo, KeyCode keyPressed) {
            this.position = player.position;
            this.mask = mask;
            this.board = player.board;
            this.player = player;
            this.action = action;
            this.actionInfo = actionInfo;
            this.keyPressed = keyPressed;
            this.GetComponent<Transform>().position = board.tiles[position.x, position.y].GetComponent<GridTile>().Center();
            this.GetComponent<Transform>().localScale *= board.world.scale;
            this.GetComponent<SpriteRenderer>().color = Colors.cursorColor;
            board.MarkTiles(mask, Colors.cursorSelectableColor);
        }
        
        void Update () {
            this.HandleCancelInput();
            this.HandleMovementInput();
        }

        void HandleCancelInput() {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(this.keyPressed)) {
                Destroy(this.gameObject);
                this.board.ResetColorings();
                this.player.CancelTargetPhase();
            }
        }

        void HandleMovementInput() {
            if (Input.GetKeyDown("space")) {
                if (mask[position.x, position.y]) {
                    board.ResetColorings();
					if (board.tiles[position.x, position.y].GetComponent<GridTile>().inhabitant != null)
                    	actionInfo.targetId = board.tiles[position.x, position.y].GetComponent<GridTile>().inhabitant.id;
                    actionInfo.targetPos = position;
                    player.EndTargetPhase(action, actionInfo);
                    Destroy(this.gameObject);
                }
            }

            if (player.OutOfTime()) {
                Destroy(this.gameObject);
                player.HandleTimeOut();
            }

            Vector2Int move;

            if (Input.GetKeyDown("down")) {
                move = new Vector2Int(0, -1);
            } else if (Input.GetKeyDown("up")) {
                move = new Vector2Int(0, 1);
            } else if (Input.GetKeyDown("right")) {
                move = new Vector2Int(1, 0);
            } else if (Input.GetKeyDown("left")) {
                move = new Vector2Int(-1, 0);
            } else {
                return;
            }

            Vector2Int newPosition = this.position + move;
            if (this.board.InBounds(newPosition)) {
                this.GetComponent<Transform>().position = board.tiles[newPosition.x, newPosition.y].GetComponent<GridTile>().Center();
                this.position = newPosition;
            }
        }
    }