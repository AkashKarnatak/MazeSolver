using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WallEMovement : MonoBehaviour
{
    private const int LEFT = 0;
    private const int FORWARD = 1;
    private const int RIGHT = 2;
    private const int BACKWARD = 3;
    [SerializeField] private int MAXACTIONSTEPS = 50;  // MAXACTIONSTEPS = 1/Time.fixedDeltaTime
    private const float RANGE = 0.57f;   // NEEDS TO BE FIXED
    private float maxRayShootDistance = default;
    private bool isForwardBlocked = default;
    private bool isLeftBlocked = default;
    private bool isRightBlocked = default;
    private int actionState = default;
    private int actionCounter = default;
    private bool shouldThink = default;

    // Camera variables
    [SerializeField] private Camera wallECamera = default;
    [SerializeField] private Camera spectatorCamera = default;

    //AnimationController variables
    private Animator animator = default;
    private int isMovingForwardHash = default;
    private int isRotatingLeftHash = default;
    private int isRotatingRightHash = default;

    private Transform checkTrackersTransform = default;
    [SerializeField] private LayerMask layerMask = default;
    [SerializeField] private LayerMask mazeLayerMask = default;
    [SerializeField] private GameObject trackerGameObject = default;

    // Begin Debug statements -----------------------
    // To enable interruptGame from Editor, uncomment the lines from 340
    [SerializeField] private bool INTERUPTGAME = true;
    // End Debug statements -----------------------

    // Test variables
    private bool interruptGame = default;

    // Start is called before the first frame update
    void Start()
    {
        maxRayShootDistance = 0.755f;  // NEEDS TO BE FIXED
        isForwardBlocked = false;
        isLeftBlocked = false;
        isRightBlocked = false;
        actionState = 0;
        actionCounter = 0;
        shouldThink = true;
        checkTrackersTransform = GameObject.Find("CheckTrackers").transform;
        layerMask = LayerMask.GetMask("Tracker");
        mazeLayerMask = LayerMask.GetMask("Maze");

        // Enable spectator camera by default
        wallECamera.enabled = false;
        spectatorCamera.enabled = true;

        // AnimationController variables
        animator = GetComponent<Animator>();

        isMovingForwardHash = Animator.StringToHash("isMovingForward");
        isRotatingLeftHash = Animator.StringToHash("isRotatingLeft");
        isRotatingRightHash = Animator.StringToHash("isRotatingRight");
        
        // Debug variables
        interruptGame = true;
    }

    private void MoveForward() {
        transform.Translate(transform.forward/MAXACTIONSTEPS, Space.World);
    }

    private void TurnLeft() {
        transform.RotateAround(transform.position, transform.up, -90f/MAXACTIONSTEPS);
    }
    private void TurnRight() {
        transform.RotateAround(transform.position, transform.up, 90f/MAXACTIONSTEPS);
    }

    private void UTurn() {
        transform.RotateAround(transform.position, transform.up, 180f/MAXACTIONSTEPS);
    }

    private void ObserveSurroundings() {
        // Ray physics
        // Correction of Vector3.up/2 as WallE's origin is located on its feet.
        Ray forwardRay = new Ray(transform.position + Vector3.up/4, transform.forward);
        Ray leftRay = new Ray(transform.position + Vector3.up/4, -transform.right);
        Ray rightRay = new Ray(transform.position + Vector3.up/4, transform.right);
        RaycastHit forwardHit;
        RaycastHit leftHit;
        RaycastHit rightHit;
        if(Physics.Raycast(forwardRay, out forwardHit, maxRayShootDistance, mazeLayerMask)) {
            Debug.DrawLine(forwardRay.origin, forwardHit.point, Color.red);
            isForwardBlocked = true;
        } else {
            Debug.DrawLine(forwardRay.origin, forwardRay.origin + forwardRay.direction*maxRayShootDistance, Color.green);
            isForwardBlocked = false;
        }
        if(Physics.Raycast(leftRay, out leftHit, maxRayShootDistance, mazeLayerMask)) {
            Debug.DrawLine(leftRay.origin, leftHit.point, Color.red);
            isLeftBlocked = true;
        } else {
            Debug.DrawLine(leftRay.origin, leftRay.origin + leftRay.direction*maxRayShootDistance, Color.green);
            isLeftBlocked = false;
        }
        if(Physics.Raycast(rightRay, out rightHit, maxRayShootDistance, mazeLayerMask)) {
            Debug.DrawLine(rightRay.origin, rightHit.point, Color.red);
            isRightBlocked = true;
        } else {
            Debug.DrawLine(rightRay.origin, rightRay.origin + rightRay.direction*maxRayShootDistance, Color.green);
            isRightBlocked = false;
        }

        // Begin Debug statements -------------------------
        if(isForwardBlocked) { Debug.Log("FORWARD WALL"); }
        if(isLeftBlocked) { Debug.Log("Left WALL"); }
        if(isRightBlocked) { Debug.Log("Right WALL"); }
        // End Debug statements -------------------------
    }

    private void DetermineActionStateAtIntersection(int suggestedActionState, int wallPosition) {

        // Begin Debug statements -------------------------
        Debug.Log("Right now I am at an intersection---------------------------");
        // End Debug statements -------------------------

        // Stores the number of trackers in each direction
        int[] noOfTrackersInDirection = {
            0, // LEFT
            0, // FORWARD
            0, // RIGHT
            0  // BACKWARD
            };
        // Check for all trackers within 0.53m radius
        Collider[] colliders = Physics.OverlapSphere(checkTrackersTransform.position, RANGE, layerMask);
        foreach(Collider collider in colliders) {
            // Determine the direction of tracker and update noOfTrackersInDirection array
            int direction = (Mathf.RoundToInt(Vector3.SignedAngle(checkTrackersTransform.forward, collider.transform.position - checkTrackersTransform.position, transform.up)/90) + 5)%4;

            // This ain't the best way to deal with it. But I'll fix it later.

            // consider only the trackers which are not on the same tile as the player
            if(Vector3.Distance(collider.transform.position, checkTrackersTransform.position) > 0.52f) {
                noOfTrackersInDirection[direction]++;
                
                // Begin debug statements ----------------------------
                if(direction==FORWARD) {
                    Debug.Log("Tracker at FORWARD");
                } else if (direction == BACKWARD) {
                    Debug.Log("Tracker at BACKWARD");
                } else if (direction == LEFT) {
                    Debug.Log("Tracker at LEFT");
                } else {
                    Debug.Log("Tracker at RIGHT");
                }
                // End debug statements ----------------------------
            }


        }

        // No trackers --> Take suggested action
        if(noOfTrackersInDirection[LEFT]+noOfTrackersInDirection[FORWARD]+noOfTrackersInDirection[RIGHT] == 0) {
            actionState = suggestedActionState;
        } 
        // Trackers detected!!
        else {
            // Can I go back?
            if(noOfTrackersInDirection[BACKWARD] == 2) {
                // NO
                // Determine the available directions and take the road less travelled
                if(wallPosition == -1) {
                    // No blocking wall
                    // Three available directions. Choose the direction with least trackers.
                    if(noOfTrackersInDirection[LEFT] < noOfTrackersInDirection[FORWARD]) {
                        if(noOfTrackersInDirection[LEFT] < noOfTrackersInDirection[RIGHT]) {
                            actionState = LEFT;
                        } else {
                            actionState = RIGHT;
                        }
                    } else {
                        if(noOfTrackersInDirection[FORWARD] < noOfTrackersInDirection[RIGHT]) {
                            actionState = FORWARD;
                        } else {
                            actionState = RIGHT;
                        }
                    }
                } else {
                    // A wall!!
                    // Which direction should I take now?
                    int firstDirection = (wallPosition + 2)%3;
                    int secondDirection = (wallPosition + 4)%3;
                    // Determine the direction with least trackers
                    if(noOfTrackersInDirection[firstDirection] < noOfTrackersInDirection[secondDirection]) {
                        actionState = firstDirection;
                    } else {
                        actionState = secondDirection;
                    }
                }
            } else {
                // Yes
                // RETREAT!!
                actionState = BACKWARD;
            }
        }
    }

    private void Think() {
        // Begin Debug statements -------------------------
        Debug.Log("Looking around---------------------------");
        // End Debug statements -------------------------
        ObserveSurroundings();

        // Begin Debug statements -------------------------
        Debug.Log("Determining state---------------------------");
        // End Debug statements -------------------------
        // Where am I?
        // On a path
        if(!isForwardBlocked && isLeftBlocked && isRightBlocked) {
            actionState = FORWARD;
        } else if(isForwardBlocked && !isLeftBlocked && isRightBlocked) {
            actionState = LEFT;
        } else if(isForwardBlocked && isLeftBlocked && !isRightBlocked) {
            actionState = RIGHT;
        } else if(isForwardBlocked && isLeftBlocked && isRightBlocked) {
            //dead end
            actionState = BACKWARD;
        }
        // At an intersection
        else {

            // Begin Debug statements -------------------------
            Debug.Log("Droping tracker behind---------------------------");
            // End Debug statements -------------------------
            // Drop a tracker behind. Marks end of previous path.
            // 52% of forward so that the tracker lies in front of the tile.
            Instantiate(trackerGameObject, checkTrackersTransform.position-(checkTrackersTransform.forward * 0.55f), Quaternion.identity);


            int suggestedActionState;
            int wallPosition;

            // suggested action priority --> FORWARD > LEFT > RIGHT
            if(!isForwardBlocked && !isLeftBlocked && isRightBlocked) {
                suggestedActionState = FORWARD;
                wallPosition = RIGHT;
            } else if(!isForwardBlocked && isLeftBlocked && !isRightBlocked) {
                suggestedActionState = FORWARD;
                wallPosition = LEFT;
            } else if(isForwardBlocked && !isLeftBlocked && !isRightBlocked) {
                suggestedActionState = LEFT;
                wallPosition = FORWARD;
            } else {
                suggestedActionState = FORWARD;
                wallPosition = -1;
            }
            DetermineActionStateAtIntersection(suggestedActionState, wallPosition);

            // Begin Debug statements -------------------------
            Debug.Log("Droping tracker ahead---------------------------");
            // End Debug statements -------------------------


            // Drop a tracker to mark beginning of a new path.
            Vector3 dir;
            if(actionState == LEFT) {
                dir = -checkTrackersTransform.right;
            } else if(actionState == FORWARD) {
                dir = checkTrackersTransform.forward;
            } else if(actionState == RIGHT) {
                dir = checkTrackersTransform.right;
            } else {
                dir = -checkTrackersTransform.forward;
            }
            // 52% of dir so that the trackers lie just in front of the tile
            Instantiate(trackerGameObject, checkTrackersTransform.position+dir*0.55f, Quaternion.identity);
        }

        // // Set animation booleans
        // if(actionState == FORWARD) {
        //     animator.SetBool(isMovingForwardHash, true);
        // } else if(actionState == LEFT) {
        //     animator.SetBool(isRotatingLeftHash, true);
        // } else if(actionState == RIGHT) {
        //     animator.SetBool(isRotatingRightHash, true);
        // } else if(actionState == BACKWARD) {
        //     animator.SetBool(isRotatingLeftHash, true);
        // }

        // Begin Debug statements -------------------------
        Debug.Log("action state = " + actionState + " ---------------------------");
        // End Debug statements -------------------------

        shouldThink = false;

        // Begin debug statements --------------------------
        interruptGame = INTERUPTGAME;
        // End debug statements ----------------------------
    }

    private void Action() {
        if(actionState == FORWARD) {
            animator.SetBool(isMovingForwardHash, true);
            MoveForward();
        } else if(actionState == LEFT) {
            animator.SetBool(isRotatingLeftHash, true);
            TurnLeft();
        } else if(actionState == RIGHT) {
            animator.SetBool(isRotatingRightHash, true);
            TurnRight();
        } else if(actionState == BACKWARD) {
            animator.SetBool(isRotatingLeftHash, true);
            UTurn();
        }
        // Increment and check
        if(++actionCounter == MAXACTIONSTEPS) {
            // reset animation booleans
            animator.SetBool(isMovingForwardHash, false);
            animator.SetBool(isRotatingLeftHash, false);
            animator.SetBool(isRotatingRightHash, false);

            if(actionState == FORWARD) {
                shouldThink = true;
            } else {
                actionState = FORWARD;
            }
            actionCounter = 0;
        }
    }

    void Update() {        

        // For debugging purpose
        // if(Input.GetKeyDown(KeyCode.Space)) {
        //     interruptGame = !interruptGame;
        // }

        // switch cameras
        if(Input.GetKeyDown(KeyCode.C) && Time.timeScale != 0f) {
            wallECamera.enabled = !wallECamera.enabled;
            spectatorCamera.enabled = !spectatorCamera.enabled;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(shouldThink) {
            // decide an action state

            // Begin Debug statements -------------------------
            Debug.Log("Started Thinking---------------------------");
            // End Debug statements -------------------------
            Think();

            // Begin Debug statements -------------------------
            if(actionState==FORWARD) {
                Debug.Log("MOVE FORWARD");
            } else if (actionState == BACKWARD) {
                Debug.Log("MOVE BACKWARD");
            } else if (actionState == LEFT) {
                Debug.Log("MOVE LEFT");
            } else {
                Debug.Log("MOVE RIGHT");
            }
            // End Debug statements
        }
        if(!interruptGame) {
            // perform the action
            Action();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "End") {
            Debug.Log("Destination Reached");
            // Disable animations
            animator.SetBool(isMovingForwardHash, false);
            animator.SetBool(isRotatingLeftHash, false);
            animator.SetBool(isRotatingRightHash, false);
            // Disable WallE movement 
            INTERUPTGAME = true;
            interruptGame = INTERUPTGAME;
        }
        if(other.gameObject.tag == "Exit") {
            Debug.Log("Exit");
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Test functions
    // void RepositionAtCenter() {
    //     Ray forwardRay = new Ray(transform.position, transform.forward);
    //     Ray leftRay = new Ray(transform.position, -transform.right);
    //     Ray rightRay = new Ray(transform.position, transform.right);
    //     Ray backRay = new Ray(transform.position, -transform.forward);
    //     RaycastHit forwardHit;
    //     RaycastHit leftHit;
    //     RaycastHit rightHit;
    //     RaycastHit backHit;

    //     float forwardDistance, leftDistance, rightDistance, backDistance;
    //     if(Physics.Raycast(forwardRay, out forwardHit, maxRayShootDistance)) {
    //         // forwardDistance = forwardHit.point - transform.position;
    //     } else {
    //         // Debug.DrawLine(forwardRay.origin, forwardRay.origin + forwardRay.direction*maxRayShootDistance, Color.green);
    //         isForwardBlocked = false;
    //     }
    //     if(Physics.Raycast(leftRay, out leftHit, maxRayShootDistance)) {
    //         // Debug.DrawLine(leftRay.origin, leftHit.point, Color.red);
    //         isLeftBlocked = true;
    //     } else {
    //         // Debug.DrawLine(leftRay.origin, leftRay.origin + leftRay.direction*maxRayShootDistance, Color.green);
    //         isLeftBlocked = false;
    //     }
    //     if(Physics.Raycast(rightRay, out rightHit, maxRayShootDistance)) {
    //         // Debug.DrawLine(rightRay.origin, rightHit.point, Color.red);
    //         isRightBlocked = true;
    //     } else {
    //         // Debug.DrawLine(rightRay.origin, rightRay.origin + rightRay.direction*maxRayShootDistance, Color.green);
    //         isRightBlocked = false;
    //     }
    //     if(Physics.Raycast(backRay, out backHit, maxRayShootDistance)) {
    //         // Debug.DrawLine(backRay.origin, backHit.point, Color.red);
    //         isRightBlocked = true;
    //     } else {
    //         // Debug.DrawLine(backRay.origin, backRay.origin + backRay.direction*maxRayShootDistance, Color.green);
    //         isRightBlocked = false;
    //     }
    // }
}
