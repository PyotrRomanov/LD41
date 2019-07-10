using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    GameObject currentGame;
    string[] levelNames = {"Triple Threat", "Knightly Escort", "Diagonally", "Tutorial"};
    // Use this for initialization
    void Start()
    {
        currentGame = Instantiate((GameObject)Resources.Load("WorldManagerPf"));
        currentGame.GetComponent<WorldManager>().Setup(Memory.levelName, this);
    }

    public IEnumerator NewWorld() {
        while (currentGame.GetComponent<WorldManager>().collapseLock.Locked()) {
            yield return null;
        }
        Destroy(currentGame);
        currentGame = Instantiate((GameObject)Resources.Load("WorldManagerPf"));
        currentGame.GetComponent<WorldManager>().Setup(levelNames[Random.Range(0, levelNames.Length)], this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape")) {
        }
    }
}