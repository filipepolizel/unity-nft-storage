using UnityEngine;

public class ZombieCameraLogic : MonoBehaviour
{

    [SerializeField] private Transform m_target = null;
    private float m_distance = 2f;
    private float m_height = 1;
    private float m_lookAtAroundAngle = 180;

    private void LateUpdate()
    {
        if (m_target == null) { return; }

        float targetHeight = m_target.position.y + m_height;
        float currentRotationAngle = m_lookAtAroundAngle;

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        Vector3 position = m_target.position;
        position -= currentRotation * Vector3.forward * m_distance;
        position.y = targetHeight;

        transform.position = position;
        transform.LookAt(m_target.position + new Vector3(0, m_height, 0));
    }
}
