using Unity.VisualScripting;
using UnityEngine;

public class Control : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void exit() { 
    
        Application.Quit();
    }
}
