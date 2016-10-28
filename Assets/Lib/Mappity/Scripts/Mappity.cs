using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class Mappity : MonoBehaviour {

    public Camera mainCam;

    //Generation
    public GameObject projPrefab;
    public GameObject projTogglePrefab;
    

    //For ease of use
    Transform projectorContainer;
    List<MappityProj> projs;
    List<MappityProjToggle> projToggles;

    //ui
    Transform projectorListContainer;
    MappityProjPanel projPanel;

    [Header("UI")]
    public ToggleGroup projToggleGroup;
    public RawImage feedbackImage;


    //handler
    MappityPointHandler mph;

    bool _editMode;

    const int maxProj = 10; //Defined in SpoutSDK (SpoutSenderNames.h MaxSenders=10), must recompile if we want more


    int curProjID;

    void Start () {
        projs = new List<MappityProj>();
        projToggles = new List<MappityProjToggle>();
        projectorContainer = transform.FindChild("Projectors");
        projectorListContainer = projToggleGroup.transform;
        projPanel = GetComponentInChildren<MappityProjPanel>();

        if (mainCam == null) mainCam = Camera.main;
        
        mph = GetComponent<MappityPointHandler>();
        mph.setMainCamera(mainCam);


        curProjID = -1;
        setCurProj(-1);

        editMode = true;
        load();        
    }
	
	void Update () {
	
	}


    public void addProjector()
    {
        if (projs.Count >= maxProj-1) return;
        MappityProj proj = Instantiate(projPrefab).GetComponent<MappityProj>();
        projs.Add(proj);
       
        MappityProjToggle projToggle = Instantiate(projTogglePrefab).GetComponent<MappityProjToggle>();
        projToggle.setMappityAndIndex(this, projs.IndexOf(proj));

        projToggles.Add(projToggle);
        projToggle.GetComponentInChildren<Toggle>().group = projToggleGroup;
        projToggle.transform.SetParent(projectorListContainer, false);

        proj.transform.SetParent(projectorContainer, true);

        updateProjIds();
        updateCameraMasks();
    }

    public void removeProjector(int index)
    {
        if(curProjID == index)  setCurProj(-1);

        MappityProj proj = projs[index];
        projs.Remove(proj);
        Destroy(proj.gameObject);
       
        MappityProjToggle projToggle = projToggles[index];
        projToggleGroup.UnregisterToggle(projToggle.GetComponent<Toggle>());
        projToggles.Remove(projToggle);
        Destroy(projToggle.gameObject);
        updateProjIds();
    }

    void updateProjIds()
    {
        for (int i = 0; i < projs.Count; i++)
        {
            projs[i].setID(i + 1);
            projToggles[i].setMappityAndIndex(this, i);
        }
    }


    void setCurProj(int index)
    {
        if(curProjID != -1)
        {
            //MappityProj proj = projs[curProjID];
            projPanel.setCurrentProj(null);
            mainCam.GetComponent<MouseOrbitImproved>().enabled = true;
            feedbackImage.enabled = false;
        }

        curProjID = index;

        if (curProjID != -1)
        {
            MappityProj proj = projs[curProjID];
            projPanel.setCurrentProj(proj);
        }        
    }

    public void projToggleChanged(MappityProjToggle toggle, bool value)
    {
        
        if(value)
        {
            setCurProj(toggle.index);
        }else if(curProjID == toggle.index && !value)
        {
            setCurProj(-1);
        }
    }

    public void projRemovePressed(MappityProjToggle toggle)
    {
        removeProjector(toggle.index);
    }

    public void mouseEnterCanvas()
    {
        mainCam.GetComponent<MouseOrbitImproved>().enabled = false;
    }

    public void mouseExitCanvas()
    {
        mainCam.GetComponent<MouseOrbitImproved>().enabled = true;
    }

    

    public void save()
    {
        SaveData data = new SaveData();


        data["numProjectors"] = projs.Count;
        for(int i=0;i<projs.Count;i++)
        {
            string prefix = "proj" + i + "_";
            MappityProj proj = projs[i];
            data[prefix + "numPoints"] = proj.numPoints;
            for(int j=0;j<proj.numPoints;j++)
            {
                string prefixSpace = prefix + "spacePoint" + j;
                string prefixImage = prefix + "imagePoint" + j;
                data[prefixSpace] = proj.spacePoints[j];
                data[prefixImage] = proj.imagePoints[j];
                
            }
        }

        //Save the data
        data.Save(Application.dataPath + "/mappity.uml");

    }


    public void load()
    {

        SaveData data = SaveData.Load(Application.dataPath + "/mappity.uml");

        if (data == null) return;

        reset();

        int numProjVal = data.GetValue<int>("numProjectors");
        for (int i = 0; i < numProjVal; i++)
        {
            addProjector();
        }

        for(int i=0;i<projs.Count;i++)
        {
            string prefix = "proj" + i + "_";
            int numPoints = data.GetValue<int>(prefix + "numPoints");
            for(int j=0;j< numPoints;j++)
            {
                string prefixSpace = prefix + "spacePoint" + j;
                string prefixImage = prefix + "imagePoint" + j;
                Vector3 spacePoint = data.GetValue<Vector3>(prefixSpace);
                Vector2 imagePoint = data.GetValue<Vector2>(prefixImage);
                projs[i].addPoint(spacePoint, imagePoint);
            }

        }

       
    }
    
    public void reset()
    {
        while (projs.Count > 0) removeProjector(0);
    }

    public bool editMode { 
        get
        {
            return _editMode;
        }
        set
        {
            _editMode = value;

            if (!editMode)
            {
                foreach (MappityProj p in projs)
                {
                    p.setReplacementShader(null);
                }

                if (curProjID != -1)
                {
                    setCurProj(-1);
                }
            }

            updateCameraMasks();
        }
    }
    

    void updateCameraMasks()
    {
        int maskVal = 1 << LayerMask.NameToLayer("calib");
        if (!editMode) maskVal = ~maskVal;

        Camera[] cams = Camera.allCameras;
        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].cullingMask = maskVal;
        }
    }
}
