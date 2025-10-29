using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class MpuHandler : MonoBehaviour
{
    public float smoothing = 2.0f; // maior = movimento mais suave
    public KeyCode recalibrateKey = KeyCode.R; // tecla para zerar rotação

    private UdpClient client;
    private Thread receiveThread;
    private Vector3 currentRotation = Vector3.zero;
    private Vector3 calibratedOffset = Vector3.zero;
    private bool firstCalibrated = false;

    void Start()
    {
        // Escuta na porta 7000 (a mesma usada no Raspberry)
        client = new UdpClient(7000);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("Recebendo dados MPU6050 via UDP (porta 7000)...");
    }

    void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = System.Text.Encoding.UTF8.GetString(data);

                string[] values = text.Split(',');
                if (values.Length == 3)
                {
                    float pitch = float.Parse(values[0]);
                    float roll = float.Parse(values[1]);
                    float yaw = float.Parse(values[2]);

                    // Mapeamento corrigido (MPU → Unity)
                    // Roll -> X, Yaw -> Y, Pitch -> Z
                    currentRotation = new Vector3(roll, -yaw, pitch);

                    // Calibração inicial (zera na primeira leitura)
                    if (!firstCalibrated)
                    {
                        calibratedOffset = currentRotation;
                        firstCalibrated = true;
                        Debug.Log("Calibração inicial realizada!");
                    }
                }
            }
            catch { }
        }
    }

    void Update()
    {
        // Permite recalibrar pressionando "R"
        if (Input.GetKeyDown(recalibrateKey))
        {
            calibratedOffset = currentRotation;
            Debug.Log("Recalibração manual executada!");
        }

        if (firstCalibrated)
        {
            // Aplica o offset (zera a rotação inicial)
            Vector3 correctedRotation = currentRotation - calibratedOffset;

            // Suaviza e aplica rotação ao objeto
            Quaternion targetRotation = Quaternion.Euler(correctedRotation);
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                1.0f - Mathf.Exp(-smoothing * Time.deltaTime)
            );
        }
    }

    void OnApplicationQuit()
    {
        if (client != null)
            client.Close();
        if (receiveThread != null)
            receiveThread.Abort();
    }
}
