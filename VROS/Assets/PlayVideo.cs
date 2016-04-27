using UnityEngine;
using System.Collections;

public class PlayVideo : MonoBehaviour {
	// Use this for initialization
	void Start () {
        var mat = GetComponent<Renderer>().material;
        var movie = (MovieTexture)mat.mainTexture;
        var audio = GetComponent<AudioSource>();
        movie.Stop();
        audio.Stop();

        movie.Play();
        audio.Play();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
