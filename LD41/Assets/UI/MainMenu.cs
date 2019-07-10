using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void LoadTripleThreat() {
        Memory.levelName = "Triple Threat";
        SceneManager.LoadScene("main_scene");
    }

    public void LoadKnightlyEscort()
    {
        Memory.levelName = "Knightly Escort";
        SceneManager.LoadScene("main_scene");
    }

    public void LoadDiagonally()
    {
        Memory.levelName = "Diagonally";
        SceneManager.LoadScene("main_scene");
    }

    public void LoadTutorial() {
        Memory.levelName = "Tutorial";
        SceneManager.LoadScene("main_scene");
    }


}

