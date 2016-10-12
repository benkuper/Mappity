using UnityEngine;
using System.Collections;

public class MappityPointHandler : MonoBehaviour {

    MappityProj proj;

    public Camera mainCamera;

    public enum EditMode { Space, Image};
    EditMode editMode;

    //SPACE POINTS
    Vector3 targetSpacePoint;

    //IMAGE POINTS
    int targetImagePointIndex;
    Vector2 mouseImageTargetOffset;
    bool editingImageTarget;


    //SPACE VIZ
    [Header("Space Viz")]
    public Material spaceTargetMat;
    [Range(0, 20)]
    public float spaceTargetSize;

    //IMAGE VIZ
    [Header("Image Viz")]
    public Material imageLineMat;
    public Material imageTargetMat;
    public Material imageArrowMat;

    public bool showImageArrow;
    public bool showImageCrosshair;

    [Range(0, 20f)]
    public float imageLineSize;
    [Range(0, 50f)]
    public float imageTargetSize;
    [Range(0, 50f)]
    public float imageArrowSize;



    //Unity methods

    void Start () {
        editMode = EditMode.Space;
	}
	
	void Update () {
        if (proj == null) return;
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        switch (editMode)
        {
            case EditMode.Space:
                updateSpaceMode();
                break;

            case EditMode.Image:
                updateImageMode();
                break;

        }
	}

    void OnEnable()
    {
        Camera.onPostRender += postRenderHandler;
    }

    void OnDisable()
    {
        Camera.onPostRender -= postRenderHandler;
    }


    /////////// SPACE

    void updateSpaceMode()
    {
        Ray r = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int calibMask = 1 << LayerMask.NameToLayer("calib");
        if (Physics.Raycast(r, out hit, 100f, calibMask ))
        {
            targetSpacePoint = getClosestVertexPos(hit);
        }

        if (Input.GetMouseButtonDown(1) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 imagePoint = mainCamera.WorldToScreenPoint(targetSpacePoint);
            proj.togglePointFromSpace(targetSpacePoint, imagePoint);
        }
    }
    
    Vector3 getClosestVertexPos(RaycastHit hit)
    {
        Mesh m = hit.collider.GetComponent<MeshFilter>().mesh;
        Vector3 result = Vector3.zero;
        float minDist = 1000;
        for (int i = 0; i < m.vertices.Length; i++)
        {
            Vector3 realPos = hit.collider.GetComponent<Transform>().TransformPoint(m.vertices[i]);
            float dist = Vector3.Distance(hit.point, realPos);
            if (dist < minDist)
            {
                result = realPos;
                minDist = dist;
            }
        }

        return result;
    }


    /////////// IMAGE

    void updateImageMode()
    {
         if (Input.GetMouseButtonDown(0))
        {
            editingImageTarget = true;
            if (targetImagePointIndex >= 0)
            {
                mouseImageTargetOffset = proj.imagePoints[targetImagePointIndex] - (Vector2)Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            editingImageTarget = false;
        }

        if (!editingImageTarget)
        {
            targetImagePointIndex = getClosestTarget();
        }else if (targetImagePointIndex >= 0)
        {
            proj.updateImagePoint(targetImagePointIndex, (Vector2)Input.mousePosition + mouseImageTargetOffset);
        }


        //viz
        if (Input.GetMouseButtonDown(1))
        {
            showImageCrosshair = !showImageCrosshair;
        }
    }


    int getClosestTarget()
    {
        int result = -1;
        float minDist = 1000;

        int numPoints = proj.numPoints;
        for (int i = 0; i < numPoints; i++)
        {
            float dist = Vector2.Distance(Input.mousePosition, proj.imagePoints[i]);
            if (dist < minDist)
            {
                result = i;
                minDist = dist;
            }
        }

        return result;
    }

    /////////// MODE AND VARIABLES
    public void setMainCamera(Camera cam)
    {
        this.mainCamera = cam;
    }

    public void setEditMode(EditMode mode)
    {
        editMode = mode;
        switch(editMode)
        {
            case EditMode.Space:
                break;

            case EditMode.Image:
                break;
        }

        clearEditing();
    }

    public void setProj(MappityProj _proj)
    {
       proj = _proj;
    }

    public void clearEditing()
    {
        editingImageTarget = false;
    }

    public void resetCalibration()
    {
        if (proj == null) return;

        for (int i = 0; i < proj.numPoints; i++)
        {
            proj.updateImagePoint(i,mainCamera.WorldToScreenPoint(proj.spacePoints[i]));
        }
    }
    
    ///////////////   VIZ
    
    void postRenderHandler(Camera cam)
    {
        if (!enabled) return;
        if (proj == null) return;
        
        switch(editMode)
        {
            case EditMode.Space:
                postRenderSpace(cam);
                break;

            case EditMode.Image:
                postRenderImage(cam);
                break;
        }
    }

    void postRenderSpace(Camera cam)
    {
        if (cam != mainCamera) return;

        int numPoints = proj.numPoints;

        Vector2 relScreenFac = new Vector2(1f / Screen.width, 1f / Screen.height);

        GL.PushMatrix();
        GL.LoadOrtho();

        spaceTargetMat.SetPass(0);
        GL.Begin(GL.QUADS);

        Vector2 relTargetPos = Vector2.Scale(mainCamera.WorldToScreenPoint(targetSpacePoint), relScreenFac);
        GL.Color(Color.yellow);
        drawImage(relTargetPos,spaceTargetSize);

        for (int i = 0; i < numPoints; i++)
        {

            Vector2 relPos = Vector2.Scale(mainCamera.WorldToScreenPoint(proj.spacePoints[i]), relScreenFac);

            GL.Color(proj.spacePoints[i] == targetSpacePoint ? Color.red : new Color(1f, .2f, 0));
            drawImage(relPos,spaceTargetSize);

        }

        GL.End();
        GL.PopMatrix();
    }

    void postRenderImage(Camera cam)
    {
        if (cam != proj.cam) return;
        GL.PushMatrix();
        GL.LoadOrtho();

        Vector2 relMousePos = new Vector2(Input.mousePosition.x * 1f / Screen.width, Input.mousePosition.y * 1f / Screen.height);

        if (showImageCrosshair && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            float tx1 = relMousePos.x - imageLineSize * 1f / Screen.width;
            float tx2 = relMousePos.x + imageLineSize * 1f / Screen.width;
            float ty1 = relMousePos.y + imageLineSize * 1f / Screen.height;
            float ty2 = relMousePos.y - imageLineSize * 1f / Screen.height;

            imageLineMat.SetPass(0);
            GL.Begin(GL.QUADS);

            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex(new Vector2(tx1, 1));
            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex(new Vector2(tx2, 1));
            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex(new Vector2(tx2, 0));
            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex(new Vector2(tx1, 0));

            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex(new Vector2(0, ty1));
            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex(new Vector2(1, ty1));
            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex(new Vector2(1, ty2));
            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex(new Vector2(0, ty2));

            GL.End();
        }


        Vector2 screenFacVector = new Vector2(1f / Screen.width, 1f / Screen.height);

        imageTargetMat.SetPass(0);
        GL.Begin(GL.QUADS);
        int numPoints = proj.numPoints;

        for (int i = 0; i < numPoints; i++)
        {

            Vector2 relPos = Vector2.Scale(proj.imagePoints[i], screenFacVector);

            if (targetImagePointIndex == i)
            {
                GL.Color(Color.green);
                Vector2 relSpacePos = Vector2.Scale(proj.cam.WorldToScreenPoint(proj.spacePoints[i]), new Vector2(1f / proj.cam.pixelWidth, 1f / proj.cam.pixelHeight));
                drawImage(relSpacePos, imageTargetSize);
            }

            GL.Color(targetImagePointIndex == i ? (editingImageTarget ? Color.red : Color.yellow) : Color.white);
            drawImage(relPos,imageTargetSize);
        }

        GL.End();


        if (targetImagePointIndex >= 0 && showImageArrow && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 relPos = Vector2.Scale(proj.imagePoints[targetImagePointIndex], screenFacVector);
            float angle = -Mathf.Atan2(relPos.x - relMousePos.x, relPos.y - relMousePos.y) + Mathf.PI;
            Vector2 angleOffset = new Vector2(Mathf.Cos(angle) * imageArrowSize / Screen.width, Mathf.Sin(angle) * imageArrowSize / Screen.height);


            imageArrowMat.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color(editingImageTarget ? Color.red : Color.yellow);

            GL.TexCoord(new Vector3(1, 1, 0));
            GL.Vertex(relMousePos - angleOffset);

            GL.TexCoord(new Vector3(1, 0, 0));
            GL.Vertex(relPos - angleOffset);

            GL.TexCoord(new Vector3(0, 0, 0));
            GL.Vertex(relPos + angleOffset);

            GL.TexCoord(new Vector3(0, 1, 0));
            GL.Vertex(relMousePos + angleOffset);

            GL.End();
        }

        GL.PopMatrix();
    }



    //GL Drawing Util
     void drawImage(Vector2 centerPos, float size)
    {
        float tw = size / Screen.width;
        float th = size / Screen.height;

        GL.TexCoord(new Vector3(0, 1, 0));
        GL.Vertex(centerPos + new Vector2(-tw, th));

        GL.TexCoord(new Vector3(1, 1, 0));
        GL.Vertex(centerPos + new Vector2(tw, th));

        GL.TexCoord(new Vector3(1, 0, 0));
        GL.Vertex(centerPos + new Vector2(tw, -th));

        GL.TexCoord(new Vector3(0, 0, 0));
        GL.Vertex(centerPos + new Vector2(-tw, -th));
    }
}
