using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayCubesCreator : MonoBehaviour
{
    public GameObject meshPrefab;
    public ArrayList meshes;
    public Vector3 shape;
    public float spacing;
    public Texture2D colorTexture;
    public bool isTextureUpdated;
    // Start is called before the first frame update
    void Start()
    {
        
        //initMeshes();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isTextureUpdated)
        {
            updateMeshes();
            isTextureUpdated = false;
        }
        
    }

    public void initMeshes()
    {
        meshes = new ArrayList();
        isTextureUpdated = false;
        var transform = gameObject.transform;
        float curX = transform.position.x;
        float curY = transform.position.y;
        float curZ = transform.position.z;
        for (int i = 0; i < shape.z; i++)
        {
            for (int j = 0; j < shape.y; j++)
            {
                for (int k = 0; k < shape.x; k++)
                {
                    var meshInstance = Instantiate<GameObject>(meshPrefab, new Vector3(curX, curY, curZ), new Quaternion());
                    meshInstance.transform.SetParent(gameObject.transform);
                    var material = new Material(Shader.Find("HDRP/Lit"));
                    meshInstance.GetComponent<Renderer>().material = material;
                    meshes.Add(meshInstance);
                    curX += spacing;
                }
                curX = transform.position.x;
                curY += spacing;
            }
            curX = transform.position.x;
            curY = transform.position.y;
            curZ += spacing;
        }
        curZ = transform.position.z;
    }

    void updateMeshes()
    {
        var pixels = colorTexture.GetPixels();
        for(int i = 0; i < meshes.Count; i++)
        {
            var mesh = meshes[i] as GameObject;
            mesh.GetComponent<Renderer>().material.SetColor("_BaseColor", pixels[i]);
        }
    }
}
