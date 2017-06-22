using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MappityProjPanel : MonoBehaviour {

    [Header("UI")]
    public RawImage feedbackImage;
    public MappityPointHandler pointHandler;
    public MouseOrbitImproved orbit;

    [Header("Shader Viz")]
    public Shader wireShader;

    MappityProj currentProj;
    Text title;
    Transform controlContainer;

    Toggle wireToggle;

    Toggle edit2D;
    Toggle crosshair;
    Toggle arrow;

    Slider lineSlider;
    Slider targetSlider;
    Slider arrowSlider;

    Image errorImg;
    Text errorText;

    public float errorMax = 10;

	// Use this for initialization
	void Start () {
        title = transform.Find("ProjTitle").GetComponent<Text>();
        controlContainer = transform.Find("Controls").GetComponent<Transform>();

        wireToggle = controlContainer.Find("WireToggle").GetComponent<Toggle>();

        edit2D = controlContainer.Find("Edit2DPointsToggle").GetComponent<Toggle>();
        crosshair = controlContainer.Find("CrossHairToggle").GetComponent<Toggle>();
        arrow = controlContainer.Find("ArrowToggle").GetComponent<Toggle>();

        arrowSlider = controlContainer.Find("ArrowSlider").GetComponent<Slider>();
        lineSlider = controlContainer.Find("LineSlider").GetComponent<Slider>();
        targetSlider = controlContainer.Find("TargetSlider").GetComponent<Slider>();

        errorText = controlContainer.Find("ErrorFeedback/Error").GetComponent<Text>();
        errorImg = controlContainer.Find("ErrorFeedback/ErrorBG/ErrorImg").GetComponent<Image>();

        updateUI();
	}

    
	// Update is called once per frame
	void Update () {
	    if(currentProj != null)
        {
            errorText.text = "Error : " + currentProj.calibrationError.ToString("F2");
            float clampError = Mathf.Clamp((float)currentProj.calibrationError, 0f, errorMax);
            errorImg.transform.localScale = new Vector3(clampError / errorMax, 1, 1);
            errorImg.color = Color.Lerp(Color.green, Color.red, clampError / errorMax);
        }
	}

    public void setCurrentProj(MappityProj proj)
    {
        if (proj == currentProj) return;

        if(currentProj != null)
        {
        }

        currentProj = proj;

        if(currentProj != null)
        {
            currentProj.recalibrate();
        }

        pointHandler.setProj(currentProj);
        updateUI();
    }

   
    public void resetCalibration()
    {
        pointHandler.resetCalibration();
    }


    void updateUI()
    {
        title.text = currentProj != null ? "Editing Projector #" + currentProj.id : "Choose a proj to edit";
        controlContainer.gameObject.SetActive(currentProj != null);

        updateControls();
    }



    public void updateControls()
    {
        crosshair.interactable = edit2D.isOn;
        arrow.interactable = edit2D.isOn;
        lineSlider.interactable = edit2D.isOn;
        arrowSlider.interactable = edit2D.isOn;
        targetSlider.interactable = edit2D.isOn;

        pointHandler.setEditMode(edit2D.isOn ? MappityPointHandler.EditMode.Image : MappityPointHandler.EditMode.Space);

        if (edit2D.isOn)
        {
            if (currentProj != null) feedbackImage.texture = currentProj.renderTexture;
            feedbackImage.enabled = true;

            pointHandler.imageLineSize = lineSlider.value;
            pointHandler.imageTargetSize = targetSlider.value;
            pointHandler.imageArrowSize = arrowSlider.value;
            pointHandler.showImageArrow = arrow.isOn;
            pointHandler.showImageCrosshair = crosshair.isOn;
            wireToggleHandler(wireToggle.isOn);

            if (orbit != null) orbit.enabled = false;
        }
        else
        {
            feedbackImage.texture = null;
            feedbackImage.enabled = false;


            if(orbit != null) orbit.enabled = true;
        }

    }

    //Handlers

    public void wireToggleHandler(bool value)
    {
        if (currentProj != null)
        {
            currentProj.setReplacementShader(value ? wireShader : null);
        }
    }

    public void displayToggleHandler(bool value)
    {
        if (currentProj != null)
        {
            currentProj.setDisplay(value);
        }
    }

    public void crossHairToggle(bool value)
    {
        if (currentProj == null) return;
        pointHandler.showImageCrosshair = value;
    }

    public void arrowToggle(bool value)
    {
        if (currentProj == null) return;
        pointHandler.showImageArrow = value;
    }

    public float targetSliderValue { set { if (currentProj != null) pointHandler.imageTargetSize = value; } }
    
    public float lineSliderValue { set { if (currentProj != null) pointHandler.imageLineSize = value; } }
    
    public float arrowSliderValue { set { if (currentProj != null) pointHandler.imageArrowSize = value; } }

   
}
