using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour {

    float mainSpeed = 5.0f;   // regular speed
    float shiftAdd = 12.5f;   // multiplied by how long shift is held.  Basically running
    float maxShift = 50.0f;   // maximum speed when holding shift
    float camSens = 0.3f;     // How sensitive it with mouse
    private bool mouseCameraAngleActive = false;
    private Vector3 lastMouse = new Vector3(255, 255, 255); // kind of in the middle of the screen, rather than at the top (play)
    private float totalRun= 1.0f;

    void Update () {
        // Mouse camera angle (left shift is pressed)
        if (Input.GetKey(KeyCode.LeftControl)) {
          if (mouseCameraAngleActive) {
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
          }
          lastMouse =  Input.mousePosition;
          mouseCameraAngleActive = true;
        } else {
          mouseCameraAngleActive = false;
        }
       
        // Keyboard commands
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0){ // only move while a direction key is pressed
          if (Input.GetKey(KeyCode.LeftShift)) {
              totalRun += Time.deltaTime;
              p  = p * totalRun * shiftAdd;
              p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
              p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
              p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
          } else {
              totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
              p = p * mainSpeed;
          }
         
          p = p * Time.deltaTime;
          Vector3 newPosition = transform.position;
          if (Input.GetKey(KeyCode.Space)) { //If player wants to move on X and Z axis only
              transform.Translate(p);
              newPosition.x = transform.position.x;
              newPosition.z = transform.position.z;
              transform.position = newPosition;
          } else {
              transform.Translate(p);
          }
        }
    }
     
    private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey (KeyCode.W)){
            p_Velocity += new Vector3(0, 0 , 1);
        }
        if (Input.GetKey (KeyCode.S)){
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey (KeyCode.A)){
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey (KeyCode.D)){
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }
}