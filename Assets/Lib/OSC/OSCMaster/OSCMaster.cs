using UnityEngine;
using System.Collections;
using UnityOSC;


public class OSCMaster : MonoBehaviour {

    OSCServer server;
    public int port = 6000;

    OSCControllable[] controllables;
    
	// Use this for initialization
	void Start () {
        server = new OSCServer(port);
        server.PacketReceivedEvent += packetReceived;
        server.Connect();

        controllables = FindObjectsOfType<OSCControllable>();
	}

    void packetReceived(OSCPacket p)
    {
        //Debug.Log("Received packet");
        OSCMessage m = (OSCMessage)p;
        string[] addSplit = m.Address.Split(new char[] { '/' });

        if (addSplit.Length != 3) return;

        string target = addSplit[1];
        string property = addSplit[2];


        
        OSCControllable c = getControllableForID(target);
        if (c == null) return;
        
        
        c.setProp(property, m.Data);
    }

    OSCControllable getControllableForID(string id)
    {
        foreach(OSCControllable c in controllables)
        {
            if (c.oscName == id) return c;
        }
        return null;
    }
	
	// Update is called once per frame
	void Update () {
        server.Update();
	}


    void OnDestroy()
    {
        server.Close();
    }
}
