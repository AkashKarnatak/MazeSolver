using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private LayerMask layerMask = default;
    // Start is called before the first frame update
    void Start()
    {
        layerMask = LayerMask.GetMask("Tracker");
    }

    // Update is called once per frame
    private void Update() {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.UpArrow)) {
            transform.Translate(transform.forward * Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.LeftArrow)) {
            transform.Translate(-transform.right * Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.RightArrow)) {
            transform.Translate(transform.right * Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.DownArrow)) {
            transform.Translate(-transform.forward * Time.deltaTime);
        }    
    }

    void FixedUpdate()
    {
        Collider[] colliderArray =  Physics.OverlapSphere(transform.position, 4, layerMask);
        if(colliderArray.Length > 0) {
            foreach(Collider c in colliderArray) {
                int direction = (Mathf.RoundToInt(Vector3.SignedAngle(transform.forward, c.transform.position - transform.position, transform.up)/90) + 5)%4;
                if(direction == 0) {
                    Debug.Log("LEFT");
                } else if(direction == 1){ 
                    Debug.Log("UP");
                } else if(direction == 2) {
                    Debug.Log("RIGHT");
                } else {
                    Debug.Log("DOWN");
                }
            }
        }
    }
}
