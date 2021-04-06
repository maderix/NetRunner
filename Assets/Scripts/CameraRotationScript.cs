using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationScript : MonoBehaviour
{
    public GameObject centerObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(centerObject.transform.position + new Vector3(0, 0.25f, 0), Vector3.up);
        transform.RotateAround(centerObject.transform.position , Vector3.up, 15 * Time.deltaTime);
    }
}
