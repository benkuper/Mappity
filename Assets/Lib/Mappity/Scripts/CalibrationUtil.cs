using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using System.Collections.Generic;

public class CalibrationUtil {
    MCvPoint3D32f[][] objectPoints;
    PointF[][] imagePoints;

    Size imageSize;
    IntrinsicCameraParameters intrinsics;
    CALIB_TYPE calibrationType;
    MCvTermCriteria termCriteria;

    ExtrinsicCameraParameters[] extrinsics;

    //CV <> Unity conversion
    Matrix4x4 cvExtrinsics;
    Matrix4x4 cvIntrinsics;

    Camera cam;

    public int getNumPoints { get { return objectPoints[0].Length; } }

    public double calibrationError;

    
    public CalibrationUtil(Camera cam)
    {
        this.cam = cam;

        objectPoints = new MCvPoint3D32f[1][];
        objectPoints[0] = new MCvPoint3D32f[0];

        imagePoints = new PointF[1][];
        imagePoints[0] = new PointF[0];

        intrinsics = new IntrinsicCameraParameters();
        imageSize = new Size(Screen.width, Screen.height);

        //Settings based on Mapamok default settings
        calibrationType = CALIB_TYPE.CV_CALIB_USE_INTRINSIC_GUESS
                | CALIB_TYPE.CV_CALIB_FIX_PRINCIPAL_POINT //required to work properly !!
                | CALIB_TYPE.CV_CALIB_FIX_ASPECT_RATIO
                | CALIB_TYPE.CV_CALIB_FIX_K1
                | CALIB_TYPE.CV_CALIB_FIX_K2
                | CALIB_TYPE.CV_CALIB_FIX_K3
                | CALIB_TYPE.CV_CALIB_FIX_K4
                | CALIB_TYPE.CV_CALIB_FIX_K5
                | CALIB_TYPE.CV_CALIB_ZERO_TANGENT_DIST
                ;

        termCriteria = new MCvTermCriteria();
        
    }

    public void updatePoints(List<Vector3> _spacePoints, List<Vector2> _imagePoints)
    {
        if (_spacePoints.Count != objectPoints[0].Length)
        {
            objectPoints[0] = new MCvPoint3D32f[_spacePoints.Count];
            imagePoints[0] = new PointF[_spacePoints.Count];
        }


        for (int i = 0; i < objectPoints[0].Length; i++)
        {
            objectPoints[0][i] = new MCvPoint3D32f(_spacePoints[i].x, _spacePoints[i].y, _spacePoints[i].z);
            imagePoints[0][i] = new PointF(_imagePoints[i].x, _imagePoints[i].y);
        }

        if(objectPoints[0].Length >= 4) calibrate();
    }

    public void calibrate()
    {
        if (objectPoints[0].Length < 4)
        {
            Debug.LogWarning("Need more than 4 points for calibration, not calibrating");
            return;
        }

        setIntrinsics();
        calibrationError = CameraCalibration.CalibrateCamera(objectPoints, imagePoints, imageSize, intrinsics, calibrationType, termCriteria, out extrinsics);

        cvExtrinsics = convertExtrinsics(extrinsics[0].ExtrinsicMatrix);
        cvIntrinsics = convertIntrinsics(intrinsics.IntrinsicMatrix);

        updateCameraParams();
    }

    //3 - update cam with result
    public void updateCameraParams()
    {
        cam.projectionMatrix = cvIntrinsics;
        cam.worldToCameraMatrix = cvExtrinsics;
    }

    //CV Utils
    void setIntrinsics()
    {
        double aov = cam.fieldOfView;
        double f = imageSize.Width * Mathf.Deg2Rad * aov; // i think this is wrong, but it's optimized out anyway
        Vector2 c = new Vector2(imageSize.Width / 2, imageSize.Height / 2);

        intrinsics.IntrinsicMatrix[0, 0] = f;
        intrinsics.IntrinsicMatrix[0, 1] = 0;
        intrinsics.IntrinsicMatrix[0, 2] = c.x;
        intrinsics.IntrinsicMatrix[1, 0] = 0;
        intrinsics.IntrinsicMatrix[1, 1] = f;
        intrinsics.IntrinsicMatrix[1, 2] = c.y;
        intrinsics.IntrinsicMatrix[2, 0] = 0;
        intrinsics.IntrinsicMatrix[2, 1] = 0;
        intrinsics.IntrinsicMatrix[2, 2] = 1;
    }

    public Matrix4x4 convertIntrinsics(Matrix<double> mat)
    {
        Matrix4x4 cm = CVMatToMat4x4(mat);
        float far = cam.farClipPlane;
        float near = cam.nearClipPlane;

        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = cm[0, 0] / cm[0, 2];
        m[1, 1] = cm[1, 1] / cm[1, 2];
        m[2, 2] = -(far + near) / (far - near);
        m[2, 3] = -(2 * far * near) / (far - near);
        m[3, 2] = -1;

        return m;
    }

    public Matrix4x4 convertExtrinsics(Matrix<double> mat)
    {
        Matrix4x4 m = CVMatToMat4x4(mat);

        //m = m.transpose;

        //Invert some signs to conform to unity matrix
        m[2, 0] = -m[2, 0];
        m[2, 1] = -m[2, 1];
        m[2, 2] = -m[2, 2];
        m[2, 3] = -m[2, 3];

        m[3, 3] = 1;

        return m;
    }

    public Matrix4x4 CVMatToMat4x4(Matrix<double> mat)
    {
        Matrix4x4 m = new Matrix4x4();
        for (int i = 0; i < mat.Rows; i++)
        {
            for (int j = 0; j < mat.Cols; j++)
            {
                m[i, j] = (float)mat[i, j];
            }
        }

        return m;
    }

}
