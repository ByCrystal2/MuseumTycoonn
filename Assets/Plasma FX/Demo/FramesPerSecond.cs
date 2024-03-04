using UnityEngine;
using System;


public class FramesPerSecond:MonoBehaviour
{
	public TextMesh _textMesh;
	public float updateInterval = 0.5f;
	
	float accum = 0.0f; // FPS accumulated over the interval
	int frames = 0; // Frames drawn over the interval
	float timeleft; // Left time for current interval
	
	public void Start()
	{
	    timeleft = updateInterval;  
	    _textMesh = transform.GetComponent<TextMesh>();
	}
	
	public void Update()
	{
		    timeleft -= Time.deltaTime;
		    accum += Time.timeScale/Time.deltaTime;
		    ++frames;
		    
		    // Interval ended - update GUI text and start new interval
		    if( timeleft <= 0.0f )
		    {
		        // display two fractional digits (f2 format)
		      	_textMesh.text = "FPS " + (accum/frames).ToString("f2");
		        timeleft = updateInterval;
		        accum = 0.0f;
		        frames = 0;
		        
		    }
  
	}
}