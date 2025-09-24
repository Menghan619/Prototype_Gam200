using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class LookScript : MonoBehaviour
{

    private Camera mainCam;
    private Vector3 MousePos;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        MousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        Vector3 rotation = MousePos - transform.position;
        float rotateZ = Mathf.Atan2(rotation.y, rotation.x) *Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, rotateZ);
    }
}
