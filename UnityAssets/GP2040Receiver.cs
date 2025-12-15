using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets;
using System.Runtime.InteropServices; // Necessário para Marshaling/Structs
using System.Threading; // Necessário para threads
using UnityEngine;
/*public class NetworkGamepad : MonoBehaviour
{
    UdpClient client;
    public byte[] data = new byte[64];

    public float lx, ly;   // valores do analógico -1..1
    public bool buttonA;

    void Start()
    {
        client = new UdpClient(9000);
    }

    void Update()
    {
        if (client.Available > 0)
        {
            IPEndPoint ep = null;
            byte[] recv = client.Receive(ref ep);

            for (int i = 0; i < recv.Length && i < 64; i++)
                data[i] = recv[i];

            // Decodifica eixos do GP2040 (HID clássico)
            short rawLX = (data[1]);
            short rawLY = (data[2]);

            // Converte 0..65535 para -1..1
            lx = (rawLX - 165); //(rawLX - 32768) / 32768f;
            ly = (rawLY - 125);//(rawLY - 32768) / 32768f;

            // Exemplo botão A
            buttonA = (data[10] & 0x01) != 0;
        }
    }
}*/


public class NetworkGamepad : MonoBehaviour
{
    private UdpClient client;
    private Thread receiveThread;
    private IPEndPoint anyIP;

    // Use uma classe/struct para dados e adicione um Lock
    // Use 'volatile' para variáveis acessadas por threads diferentes
    private readonly object lockObject = new object();
    public volatile float lx, ly;
    public volatile bool buttonA;
    public int localPort = 9000;


    void Start()
    {
        anyIP = new IPEndPoint(IPAddress.Any, localPort);
        client = new UdpClient(localPort);

        // 1. Inicia o thread de recepção
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true; // Permite que o programa feche
        receiveThread.Start();
        Debug.Log("Thread de recepção UDP iniciada.");
    }

    // Método que roda na thread separada
    private void ReceiveData()
    {
        while (true) // Loop infinito na thread de background
        {
            try
            {
                // A recepção UDP é bloqueante (espera), mas isso não bloqueia o Unity!
                byte[] recv = client.Receive(ref anyIP);

                // --- CONSUMIR TODOS OS PACOTES EM FILA ---
                // Para garantir que sempre pegamos o MAIS RECENTE, 
                // lemos e descartamos todos os pacotes adicionais que já estão no buffer
                while (client.Available > 0)
                {
                    recv = client.Receive(ref anyIP); // Substitui pelo pacote mais recente
                }

                // --- PROCESSAMENTO DO PACOTE ---
                // Decodifica dados
                if (recv.Length >= 64)
                {
                    // Acessamos os dados com segurança
                    // Usando lock para garantir que a leitura no Update não ocorra no meio da escrita
                    lock (lockObject)
                    {
                        // Seus valores de índice e calibração:
                        short rawLX = (recv[1]);
                        short rawLY = (recv[2]);

                        // NOTA: Os valores de conversão (rawLX - 165) parecem arbitrários. 
                        // Verifique a calibração 0..255 (byte) para -1..1 (float).
                        // Se for byte (0 a 255): 
                        // float range = 255f / 2f; 
                        // lx = (rawLX - range) / range;

                        lx = (rawLX - 128f) / 128f; // Exemplo para escala 0-255 centrada em 128
                        ly = (rawLY - 128f) / 128f;

                        buttonA = (recv[10] & 0x01) != 0;
                    }
                }
            }
            catch (SocketException e)
            {
                // Tratamento de erro (ex: socket fechado)
                if (e.ErrorCode != 10004) // 10004 é "Socket closed", normal ao fechar
                {
                    Debug.LogError("Erro no thread de recepção UDP: " + e.Message);
                }
                break;
            }
            // Não precisa de Thread.Sleep. Deixe a thread bloquear na client.Receive()
        }
    }

    // 2. Destruição segura:
    void OnDestroy()
    {
        // Interrompe o thread ao sair do jogo
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        client.Close();
    }

    // O Update() do Unity agora APENAS LÊ os valores:
    void Update()
    {
        // Os valores lx, ly e buttonA já foram atualizados 
        // pela thread de background o mais rápido possível.
        // O outro script (RemoteCameraMovement) acessa esses valores diretamente.
    }
}


