using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MappityMatSwitcher : MonoBehaviour {

    Camera cam;
    public Shader baseShader;
    public Shader wireShader;
    public Shader uvShader;


	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
        
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.SetReplacementShader(wireShader, "");
    }

    void OnDisable()
    {
        cam = GetComponent<Camera>();
        cam.ResetReplacementShader();
    }
}
