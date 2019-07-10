using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIManager : MonoBehaviour {

    GameObject agentRepPf;
    List<GameObject> agentReps = new List<GameObject>();
    const float width = 80;

    // Use this for initialization
    void Start () {
        //agentRepPf = (GameObject)Resources.Load("agentrepresentation_pf");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateTurnOrder(World world)
    {
        if (agentReps.Count > 0) {
            foreach (GameObject agentRep in agentReps) {
                Destroy(agentRep);
            }
        }

        List<Agent> agents = world.Agents().OrderBy(x => x.turnPriority).ToList();


        for (int i = 0; i < agents.Count; i++) {
            if (agentRepPf == null) {
                agentRepPf = (GameObject)Resources.Load("agentrepresentation_pf");
            }
            GameObject agentRep = Instantiate(agentRepPf);
            agentRep.GetComponent<RectTransform>().SetParent(this.GetComponent<RectTransform>());
            agentRep.GetComponent<RectTransform>().offsetMin = new Vector2(13 + i * width, 36);
            agentRep.GetComponent<RectTransform>().offsetMax = new Vector2(-347 + i * width, -984);
            agentRep.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            agentRep.GetComponent<Image>().color = agents[i].GetComponent<SpriteRenderer>().color;
            agentReps.Add(agentRep);
        }
        
    }
}
