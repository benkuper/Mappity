using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using System.Collections.Generic;
using Spout;

[RequireComponent(typeof(Camera))]
public class MappityProj : MonoBehaviour
{
    CalibrationUtil calib;

    public Camera cam { get; private set; }
    public RenderTexture renderTexture { get { return cam.targetTexture; } }

    public List<Vector3> spacePoints { get; private set; }
    public List<Vector2> imagePoints { get; private set; }

    public int numPoints { get { return spacePoints.Count; } }
    public float calibrationError { get { return (float)calib.calibrationError; } }

    public int id { get; private set; }


    // Use this for initialization
    void Awake()
    {
        spacePoints = new List<Vector3>();
        imagePoints = new List<Vector2>();

        cam = GetComponent<Camera>();
        calib = new CalibrationUtil(cam);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }


    //Points Handling
    public void addPoint(Vector3 spacePoint, Vector3 imagePoint)
    {
        spacePoints.Add(spacePoint);
        imagePoints.Add(imagePoint);
        calib.updatePoints(spacePoints, imagePoints);
    }

    public bool removePointFromSpace(Vector3 spacePoint)
    {
        int index = spacePoints.IndexOf(spacePoint);

        if (index >= 0)
        {
            spacePoints.RemoveAt(index);
            imagePoints.RemoveAt(index);
            calib.updatePoints(spacePoints, imagePoints);
            return true;
        }

        return false;
    }

    public void togglePointFromSpace(Vector3 spacePoint, Vector2 imagePoint)
    {
        if (!removePointFromSpace(spacePoint)) addPoint(spacePoint, imagePoint);
    }

    public void updateSpacePoint(int index, Vector3 newPos)
    {
        spacePoints[index] = newPos;
        calib.updatePoints(spacePoints, imagePoints);
    }

    public void updateImagePoint(int index, Vector2 newPos)
    {
        imagePoints[index] = newPos;
        calib.updatePoints(spacePoints, imagePoints);
    }

    public void recalibrate()
    {
        calib.updatePoints(spacePoints, imagePoints);
    }

    //ID
    public void setID(int id)
    {
        if (this.id == id) return;
        this.id = id;

        GetComponent<SpoutCamSender>().sharingName = "MappityCam" + this.id;
        GetComponent<SpoutCamSender>().enabled = false;
        GetComponent<SpoutCamSender>().enabled = true; //force recreation of spoutTex with new name
    }

    public void setReplacementShader(Shader s)
    {
        if (s != null) cam.SetReplacementShader(s, "");
        else cam.ResetReplacementShader();
    }

    public void setDisplay(bool value)
    {
        GetComponent<SpoutCamSender>().enabled = value;
    }
    
}
