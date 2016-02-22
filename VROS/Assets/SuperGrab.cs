using UnityEngine;
using System.Collections;

public class SuperGrab : MonoBehaviour {
    public bool held;

    public bool icon;
    public float hightTresh = 0;

    public float trashTresh = float.NegativeInfinity;

    Vector3 startPos;
    public void Grab()
    {
        held = true;
        startPos = transform.position;
    }

    public GameObject spawnMe;

    public void Drop()
    {
        held = false;
        if (transform.position.y > hightTresh && spawnMe)
        {
            Instantiate(spawnMe, transform.position, transform.rotation);
        }

        if (spawnMe)
        {
            transform.position = startPos;
            transform.LookAt(Camera.main.transform.position);
            transform.forward *= -1;
        }

        if (transform.position.y < trashTresh)
            Destroy(gameObject);
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
