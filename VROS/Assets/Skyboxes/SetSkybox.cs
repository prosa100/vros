using UnityEngine;
using System.Collections;

public class SetSkybox : MonoBehaviour
{
    public Material mat;
   // public Cubemap Texture;

    // Use this for initialization
    void Start()
    {
        //mat.mainTexture = Texture;
        //mat.SetTexture("_MainTex", Texture);
        RenderSettings.skybox = mat;
       // Destroy(gameObject);
    }
}
