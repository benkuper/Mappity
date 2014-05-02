using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		const int size = 200;
		IntPtr memorySource = Marshal.AllocHGlobal(size);
		IntPtr memoryTarget = Marshal.AllocHGlobal(size);
		
		
		CopyMemory(memoryTarget,memorySource, size);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	[DllImport("msvcrt.dll", EntryPoint = "memcpy", SetLastError = false)]
	public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
}
