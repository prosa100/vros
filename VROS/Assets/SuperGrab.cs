using UnityEngine;
using System.Collections;

public class SuperGrab : MonoBehaviour
{
    public bool held;

    public bool icon;
    public float hightTresh = 0;

    public float trashTresh = float.NegativeInfinity;
    //ghj
    Vector3 startPos;
    public void Grab()
    {
        held = true;
       
    }

    public bool isPolar = false;
    // j;'gsdfsjfksdj hi my name is carly hi hello my name is jason tu
    //System.out
    public GameObject spawnMe;
    //
    public void Drop()
    {
        held = false;
        var angle = Vector3.Angle((transform.position - Camera.main.transform.position), Camera.main.transform.forward);
        //print(angle);
        if (angle > hightTresh && spawnMe)
        {

            Instantiate(spawnMe, transform.position, transform.rotation);
        }
        //i can code!!!
        //o
        if (spawnMe)
        {
            transform.localPosition = startPos;
            transform.LookAt(Camera.main.transform.position);
            transform.forward *= -1;
        }
        //hELLOW wORLD COOL Hello I am coding!
        //Hi this is the codes!!
        //Look at my codes!
        if (angle > trashTresh)
        {
            Destroy(gameObject);
        }
    }
    //QWSBqwjqwddfdfddffd
    // Use this for initialization
    void Start()
    {
        startPos = transform.localPosition;
        //
    }

    // Update is called once per frame
    void Update()
    {

    }
}
