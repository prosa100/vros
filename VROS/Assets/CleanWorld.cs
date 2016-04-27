using UnityEngine;
using System.Collections;

public class CleanWorld : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var userCreatedObjects = GameObject.FindGameObjectsWithTag("Interface");
            foreach(var thing in userCreatedObjects)
            {
                Destroy(thing);
            }
        }
	}
}
