using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraWASD : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            move += transform.forward;
        if (Input.GetKey(KeyCode.S))
            move -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            move -= transform.right;
        if (Input.GetKey(KeyCode.D))
            move += transform.right;

        move.y = 0;
        transform.position += move.normalized * moveSpeed * Time.deltaTime;
    }
}
