using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteCameraMovement : MonoBehaviour
{
    public NetworkGamepad pad;
    public float moveSpeed = 5f;
    public float deadzone = 0.05f; // valores menores que isso serão ignorados

    // Valores de calibração do centro
    private float centerX = 0.0f;
    private float centerY = 0.0f;
    private bool calibrated = false;

    void Update()
    {
        if (!calibrated)
        {
            // Calibra o centro quando o analógico está parado
            centerX = pad.lx;
            centerY = pad.ly;
            calibrated = true;
            Debug.Log($"Centro calibrado: lx={centerX:F3}, ly={centerY:F3}");
        }

        // Subtrai o centro real
        float x = pad.lx - centerX;
        float z = pad.ly - centerY;

        // Deadzone para ignorar pequenos drifts
        if (Mathf.Abs(x) < deadzone) x = 0f;
        if (Mathf.Abs(z) < deadzone) z = 0f;

        Vector3 inputLocal = new Vector3(x, 0, z);
        Vector3 movementDirection = transform.rotation * inputLocal;
        movementDirection.y = 0;

        Vector3 rawInput = new Vector3(x, 0, z);
        //Vector3 input = new Vector3(x, 0, z);
        //Vector3 movementDirection = Vector3.ProjectOnPlane(rawInput, Vector3.up).normalized;

        /*if (rawInput.magnitude > 0)
        {
            // Aplica o movimento no ESPAÇO DO MUNDO (Space.World)
            transform.Translate(movementDirection * moveSpeed * Time.deltaTime, Space.World);
        }*/
        if (movementDirection.magnitude > 0)
        {
            // Aplica o movimento
            // Nota: O Space.World não é necessário aqui, mas funciona.
            // O transform.Translate com um vetor já transformado funciona bem.
            transform.Translate(movementDirection * moveSpeed * Time.deltaTime, Space.World);
        }
        //transform.Translate(input * moveSpeed * Time.deltaTime, Space.World);
    }
}

