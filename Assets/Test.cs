using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		memcpy(IntPtr.Zero, IntPtr.Zero, 0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	[DllImport("kernel32.dll")]
	static extern void memcpy(IntPtr destination, IntPtr source, uint length);
	
	public void testFunction()
	{
	}
}
