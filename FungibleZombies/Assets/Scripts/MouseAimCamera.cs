using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseAimCamera : MonoBehaviour {
    public GameObject target;
    public float rotateSpeed = 3;
    Vector3 offset;
     
    void Start() {
      offset = target.transform.position - transform.position;
    }
     
    void LateUpdate() {

      SoldierControl sc = target.GetComponent<SoldierControl>();
      if (!sc.movingBackwardsOrSides)
      {
        float horizontal = Input.GetAxis("Mouse X") * rotateSpeed;
        if (!sc.IsDead())
        {
          target.transform.Rotate(0, horizontal, 0);
        }

        float desiredAngle = target.transform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
        transform.position = target.transform.position - (rotation * offset);

        transform.LookAt(target.transform);
      }
      else
      {
        transform.position = target.transform.position - offset;
        transform.LookAt(target.transform);
      }
    }
}
