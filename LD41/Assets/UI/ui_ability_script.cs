using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ui_ability_script : MonoBehaviour {
    Color normal;
    Color highlight;
	// Use this for initialization
	void Start () {
        normal = GetComponent<Image>().color;
	}
    
    public void TurnOnHighlight() {
        GetComponent<Image>().color = Colors.buttonHighlight;
    }

    public void TurnOffHighlight() {
        GetComponent<Image>().color = normal;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
