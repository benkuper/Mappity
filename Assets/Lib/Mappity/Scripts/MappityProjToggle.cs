using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MappityProjToggle : MonoBehaviour {

    Mappity mappity;
    public int index;

    public void setMappityAndIndex(Mappity m, int _index)
    {
        mappity = m;
        index = _index;
        GetComponentInChildren<Toggle>().GetComponentInChildren<Text>().text = "Projector #" + (index + 1);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    public void onToggleChanged(bool value)
    {
        mappity.projToggleChanged(this,value);
    }

    public void onRemovePressed()
    {
        mappity.projRemovePressed(this);
        
    }
}
