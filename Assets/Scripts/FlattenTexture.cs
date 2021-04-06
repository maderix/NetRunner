using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlattenTexture : MonoBehaviour
{
    public bool isChanged;
    public Texture2D srcTexture;
    private Texture2D targetTexture;
    
    // Start is called before the first frame update
    void Start()
    {
        isChanged = false;
        targetTexture = new Texture2D(srcTexture.width * srcTexture.height,1,TextureFormat.RGBA32,true);
        targetTexture.wrapMode = TextureWrapMode.Repeat;
        targetTexture.filterMode = FilterMode.Point;
        var mat = new Material(Shader.Find("Standard"));
        //gameObject.GetComponent<MeshRenderer>().material = mat;
        flattenTexture(srcTexture);
    }

    // Update is called once per frame
    void Update()
    {
        if (isChanged)
        {
            flattenTexture(srcTexture);
            isChanged = false;
        }
        
    }

    void flattenTexture(Texture2D src)
    {
        Color32[] pixels = srcTexture.GetPixels32();
        for(int i = 0; i < pixels.Length; i++)
        {
            targetTexture.SetPixels32(pixels);
        }
        targetTexture.Apply();
        gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", targetTexture);
    }

}
