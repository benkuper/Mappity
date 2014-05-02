using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;

[RequireComponent(typeof(Camera))]
public class Mappity : MonoBehaviour {

	public int numPoints = 6;

	Vector3[][] unityObjectPoints;
	Vector2[][] unityImagePoints;

	MCvPoint3D32f[][] objectPoints;
	PointF[][] imagePoints;

	Size imageSize;
	IntrinsicCameraParameters intrinsics;
	CALIB_TYPE calibrationType;
	MCvTermCriteria termCriteria;
	ExtrinsicCameraParameters[] extrinsics;

	public bool autoCalibrate;
	
	//temp
	Matrix4x4 cvExtrinsics;
	Matrix4x4 cvIntrinsics;
	
	//ui
	public Texture targetTex;
	public Texture overTex;
	
	//testing
	public GameObject calibObject;
	
	// Use this for initialization
	void Start () {

		//init variables
		unityObjectPoints = new Vector3[1][];
		unityObjectPoints [0] = new Vector3[numPoints];
		unityImagePoints = new Vector2[1][];
		unityImagePoints [0] = new Vector2[numPoints];
		objectPoints = new MCvPoint3D32f[1][];
		objectPoints [0] = new MCvPoint3D32f[numPoints];
		imagePoints = new PointF[1][];
		imagePoints [0] = new PointF[numPoints];

		imageSize = new Size (Screen.width, Screen.height);
		intrinsics = new IntrinsicCameraParameters (); 

		//Settings based on Mapamok default settings
		calibrationType = CALIB_TYPE.CV_CALIB_USE_INTRINSIC_GUESS |
				//CALIB_TYPE.CV_CALIB_FIX_PRINCIPAL_POINT) | //not enabled by default in Mapamok
				CALIB_TYPE.CV_CALIB_FIX_ASPECT_RATIO |
				CALIB_TYPE.CV_CALIB_FIX_K1|
				CALIB_TYPE.CV_CALIB_FIX_K2 |
				CALIB_TYPE.CV_CALIB_FIX_K3 |
				CALIB_TYPE.CV_CALIB_ZERO_TANGENT_DIST;

		termCriteria = new MCvTermCriteria();
		
		//used for CV Matrix to unity camera position
		
		setPointsFromObject (calibObject);
	}
	
	void setPointsFromObject(GameObject o)
	{
		Mesh m = o.GetComponent<MeshFilter> ().sharedMesh;
		for (int i=0; i<numPoints; i++) {
			Vector3 op = calibObject.transform.TransformPoint(m.vertices[i]);
			unityObjectPoints[0][i] = op;
			Vector2 sp = camera.WorldToScreenPoint(op);
			unityImagePoints[0][i] = sp;

			objectPoints[0][i] = new MCvPoint3D32f(op.x,op.y,op.z);
			imagePoints[0][i] = new PointF(sp.x,sp.y);
		}
	}

	// Update is called once per frame
	void Update () {
		setPointsFromObject (calibObject);

		if (autoCalibrate) {
			calibrate ();
		}
		
		if(Input.GetKeyDown(KeyCode.A)) autoCalibrate = !autoCalibrate;
	}
	
	public void calibrate()
	{
		setIntrinsics ();
		CameraCalibration.CalibrateCamera (objectPoints, imagePoints, imageSize, intrinsics, calibrationType, termCriteria, out extrinsics);
		
		cvExtrinsics = convertExtrinsics(extrinsics[0].ExtrinsicMatrix);
		cvIntrinsics = convertIntrinsics(intrinsics.IntrinsicMatrix);
		
		updateCameraParams();
	}
	

	void OnGUI()
	{
		for (int i=0; i<numPoints; i++) {
			Vector2 p = unityImagePoints[0][i];
			p.y = Screen.height - p.y;
			GUI.DrawTexture(new Rect(p.x-targetTex.width/2,p.y-targetTex.height/2,targetTex.width,targetTex.height),targetTex);
		}
		
		GUI.TextField(new Rect(10,10,300,150),camera.projectionMatrix.ToString());
		GUI.TextField(new Rect(10,160,300,150),cvIntrinsics.ToString());
	}
	
	public void updateCameraParams()
	{
		Vector3 zero = new Vector3(cvExtrinsics[3,0],-cvExtrinsics[3,1],cvExtrinsics[3,2]);
		Quaternion q = Quaternion.LookRotation(cvExtrinsics.GetColumn(2), cvExtrinsics.GetColumn(1));
		
		//To convert to camera position when all is clean
		
		//testZero.transform.position = Vector3.zero;
		//testZero.transform.rotation = q;
		//Vector3 newT = testZero.transform.TransformPoint(zero);
		//testZero.transform.position = newT;
		
		//camera.projectionMatrix = cvIntrinsics;
	}

	//Utils
	void setIntrinsics()
	{
		double aov = camera.fieldOfView;
		double f = imageSize.Width * Mathf.Deg2Rad * aov; // i think this is wrong, but it's optimized out anyway
		Vector2 c =  new Vector2(imageSize.Width/2,imageSize.Height/2);

		intrinsics.IntrinsicMatrix[0,0] = f;
		intrinsics.IntrinsicMatrix[0,1] = 0;
		intrinsics.IntrinsicMatrix[0,2] = c.x;
		intrinsics.IntrinsicMatrix[1,0] = 0;
		intrinsics.IntrinsicMatrix[1,1] = f;
		intrinsics.IntrinsicMatrix[1,2] = c.y;
		intrinsics.IntrinsicMatrix[2,0] = 0;
		intrinsics.IntrinsicMatrix[2,1] = 0;
		intrinsics.IntrinsicMatrix[2,2] = 1;
	}
	
	public Matrix4x4 convertIntrinsics(Matrix<double> mat)
	{
		Matrix4x4 cm = CVMatToMat4x4(mat);
		float far = camera.farClipPlane;
		float near =camera.nearClipPlane;
		
		Matrix4x4 m = new Matrix4x4();
		m[0,0] = cm[0,0] / cm[0,2];
		m[1,1] = cm[1,1] / cm[1,2];
		m[2,2] = -(far+near)/(far-near);
		m[2,3] = -(2*far*near)/(far-near);
		m[3,2] = -1;
		
		return m;
	}
	
	public Matrix4x4 convertExtrinsics(Matrix<double> mat)
	{
		Matrix4x4 m = CVMatToMat4x4(mat);
		
		m = m.transpose;
		
		//Invert some signs to conform to unity matrix
		m[0,2] = -m[0,2];
		m[1,2] = -m[1,2];
		m[2,2] = -m[2,2];
	
		m[3,3] = 1;
		
		return m;
	}

	public Matrix4x4 CVMatToMat4x4(Matrix<double> mat)
	{
		Matrix4x4 m = new Matrix4x4 ();
		for (int i=0; i<mat.Rows; i++) {
			for (int j=0; j<mat.Cols; j++) {
				m [i, j] =(float)mat[i, j];
			}
		}
		
		return m;
	}
}
