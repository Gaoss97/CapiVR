#! python3.8
import socket
import cv2
import numpy as np
from cvzone.HandTrackingModule import HandDetector
import asyncio
import websockets

UDP_IP = "0.0.0.0"
UDP_PORT = 5005
MAX_DGRAM = 65507
sock2 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = ("127.0.0.1", 5052)
width, height = 1280, 720
detector = HandDetector(maxHands=1, detectionCon=0.5)



sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind((UDP_IP, UDP_PORT))

print("Servidor UDP rodando em", UDP_IP, UDP_PORT)

data_buffer = b""
PORT = 5007
sock3 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock3.bind(("", PORT))
while True:
    data, addr = sock3.recvfrom(1024)
    if data == b"who_is_unity":
        print("Raspberry pediu IP, respondendo...")
        sock3.sendto(b"i_am_unity", addr)
        break

while True:
    try:
        packet, addr = sock.recvfrom(MAX_DGRAM)
        if packet == b'__end__':  # fim do frame
            nparr = np.frombuffer(data_buffer, np.uint8)
            frame = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
            frame = cv2.flip(frame, 1)
            if frame is not None:
                hands, img = detector.findHands(frame)
                data = []
                if hands:
                    hand = hands[0]
                    lmList = hand['lmList']
                    for lm in lmList:
                        data.extend([lm[0], height - lm[1], lm[2]])

                    # Envia landmarks
                    sock2.sendto(str.encode(str(data)), serverAddressPort)
                cv2.imshow("Image", img)

                
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break
            data_buffer = b""
        else:
            data_buffer += packet
    except Exception as e:
        print("Erro:", e)
        continue

cv2.destroyAllWindows()
sock.close()




