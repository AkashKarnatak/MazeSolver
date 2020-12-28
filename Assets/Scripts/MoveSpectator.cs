using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpectator : MonoBehaviour
{
    [SerializeField] private Camera spectatorCamera = default;
    public float mouseSensitivity = 50f;
    public float movementSpeed = 2f;
    private float xRotation = default;
    private float yRotation = default;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        mouseSensitivity = 50f;
        movementSpeed = 2f;
        xRotation = 0f;
        yRotation = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Perform spectator movements only when spectator camera is enabled
        if(spectatorCamera.enabled) {
            // Translate spectator
            if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.UpArrow)) {
                transform.Translate(transform.forward * movementSpeed * Time.deltaTime, Space.World);
            }
            if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.LeftArrow)) {
                transform.Translate(-transform.right * movementSpeed * Time.deltaTime, Space.World);
            }
            if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.RightArrow)) {
                transform.Translate(transform.right * movementSpeed * Time.deltaTime, Space.World);
            }
            if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.DownArrow)) {
                transform.Translate(-transform.forward * movementSpeed * Time.deltaTime, Space.World);
            }    

            // Look Around
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            yRotation += mouseX;

            // Clamp top to bottom rotation
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}
