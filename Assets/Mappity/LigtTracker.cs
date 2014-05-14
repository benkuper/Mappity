using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LigtTracker : MonoBehaviour {

	[Range(0.0f,1.0f)]
	public float trackingSpeed = 1.0f;
	public float linePosition = 0.0f;
	public float lineWidth = 3f;
	Texture lineTex;
	
	int numLights = 6;
	Vector2[] lightPos;
	Vector2[] lightDetectionCount;
	
	public bool tracking;
	public bool finished;
	public bool trackVertical;
	float timeOffset;
	
	public int threshold = 170;
		
	public Texture trackTex;
	
	public bool updateMappity ;
	public Mappity mappity; 
	
	public int[] correspondance = {3,2,1,0,5,3};
	
	// Use this for initialization
	void Start () {
		lightPos = new Vector2[numLights];
		lightDetectionCount = new Vector2[numLights];
		
		OSCHandler.Instance.Init();
		OSCHandler.Instance.CreateServer("Light", 12000);	
		Application.runInBackground = true;
	}
	
	// Update is called once per frame
	void Update () {
	
		if(Input.GetKeyDown(KeyCode.T))
		{
			startTracking();
		}
		
		if(tracking && !finished)
		{
			float oldLinePos = linePosition;
			
			linePosition = ((Time.time-timeOffset)*trackingSpeed)%1.00f;
			linePosition *= trackVertical?Screen.height:Screen.width;
			
			if(oldLinePos > linePosition){
				trackVertical = !trackVertical;
				if(trackVertical) 
				{
					finished = true;
					if(updateMappity) mappity.setImagePoints(lightPos,correspondance);
					camera.enabled = false;
				}
				
			}
			
			OSCHandler.Instance.UpdateLogs();
			Dictionary<string,ServerLog> servers = OSCHandler.Instance.Servers;
			
			foreach( KeyValuePair<string, ServerLog> item in servers )
			{
				if(item.Value.log.Count > 0) 
				{
					int lastPacketIndex = item.Value.packets.Count - 1;
					object[] values = item.Value.packets[lastPacketIndex].Data.ToArray();
					for(int i=0;i<values.Length;i++)
					{
						int curVal = (int)values[i];
						if(curVal > threshold)
						{
							if(trackVertical)
							{
								lightDetectionCount[i].y ++;
								lightPos[i].y = (lightPos[i].y*(lightDetectionCount[i].y-1) + linePosition) / lightDetectionCount[i].y;
							}else
							{
								lightDetectionCount[i].x ++;
								lightPos[i].x = (lightPos[i].x*(lightDetectionCount[i].x-1) + linePosition) / lightDetectionCount[i].x;
							}
						}
					}
					
				}
			}
		}
		
	}
	
	public void startTracking()
	{
		for (int i=0;i<numLights;i++)
		{
			lightPos[i] = Vector2.zero;
			lightDetectionCount[i] = Vector2.zero;
		}
		
		camera.enabled = true;
		finished = false;
		timeOffset = Time.time;
	}
	
	void OnGUI()
	{
		for(int i=0;i<numLights;i++)
		{
			if(lightPos[i].magnitude > 0)
			{
				Vector2 pos = lightPos[i];
				//Drawing.DrawLine(new Vector2(lightPos[i].x*Screen.width,0),new Vector2(lightPos[i].x*Screen.width,Screen.height),Color.red*.5f,lineWidth,true);
				//Drawing.DrawLine(new Vector2(-Screen.width/2,lightPos[i].y*Screen.height),new Vector2(Screen.width/2,lightPos[i].y*Screen.height),Color.red*.5f,lineWidth,true);
				GUI.DrawTexture(new Rect(pos.x-trackTex.width/2,pos.y-trackTex.height/2,trackTex.width,trackTex.height),trackTex);
				GUI.Label (new Rect(pos.x-trackTex.width/2,pos.y-trackTex.height/2-10,50,50),i.ToString());
				
			}
		}
		
		if(tracking)
		{
			if(trackVertical) Drawing.DrawLine(new Vector2(-Screen.width/2,linePosition),new Vector2(Screen.width/2,linePosition),Color.white,lineWidth,true);
			else Drawing.DrawLine(new Vector2(linePosition,0),new Vector2(linePosition,Screen.height),Color.white,lineWidth,true);
			
		}
		
		
	}
}
