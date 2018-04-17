using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour {

    public GameObject buttonPf;
    public GameObject canvasPf;
    const int buttonCount = 4;

	void Start () {
		GridWorld world1 = new GridWorld(4, 4);
        GameObject canvas = Instantiate(canvasPf);
        
        for (int i = 0; i < buttonCount; i++) {
            GameObject firstButton = Instantiate(buttonPf);
            firstButton.transform.SetParent(canvas.transform, false);
            firstButton.transform.localPosition = new Vector3(-(Screen.width / 2) + (firstButton.GetComponent<RectTransform>().sizeDelta.x / 2) + firstButton.GetComponent<RectTransform>().sizeDelta.x * i, -(Screen.height / 2) + (firstButton.GetComponent<RectTransform>().sizeDelta.y / 2), 0);

        }

    }
}
