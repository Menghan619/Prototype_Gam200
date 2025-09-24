using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 5f;

    private Vector3 targetPos;
    private bool isMoving = false;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);//changing the z axis to follow the camera
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            targetPos = new Vector3(worldPos.x, worldPos.y, transform.position.z);
            isMoving = true;
        }
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if(Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                isMoving = false;
            }
        }
    }
}
