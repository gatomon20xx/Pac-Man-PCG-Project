# from flask import Flask, request, jsonify
import json
import socket
from deepface import DeepFace
import base64
import struct
import numpy as np
import cv2


HOST = ''
PORT = 5000

def send_json_line(conn, obj):
    line = json.dumps(obj, ensure_ascii=False) + "\n"
    conn.sendall(line.encode("utf-8"))

# app = Flask(__name__)

# @app.route("/analyze", methods=["POST"])

def recvall(sock, length):
    data = b''
    while len(data) < length:
        packet = sock.recv(length - len(data))
        if not packet:
            return None
        data += packet
    return data

def main():
    print('DeepFace environment is now trying to run.')
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.bind((HOST, PORT))
    s.listen(1)
    print('Server starts, waiting for connection...', flush=True)
    conn, addr = s.accept()
    print('Connected by', addr, flush=True)

    while True:
        length_data = recvall(conn, 4)
        if not length_data: break;
        try:
            # Access the image data in the Flask request, then decode it to a proper image file.
            # data = request.json["image"]
            
            length = struct.unpack("I", length_data)[0]
            img_bytes = recvall(conn, length)
            img = cv2.imdecode(np.frombuffer(img_bytes, np.uint8), cv2.IMREAD_COLOR)

            # Get the predicted emotions from the image
            result = DeepFace.analyze(img, actions=["emotion"], enforce_detection=False)
            print("Sending Result Now")
            emo_score = result[0]["emotion"]
            emote = result[0]["dominant_emotion"]
            message = json.dumps({
                "dominant_emotion": emote,
                "emotion": emo_score
                })
            conn.sendall(message.encode("utf-8"))
            # return jsonify(result)

        except Exception as e:
            # Return the error and the error code.
            # return jsonify({"error": str(e)}), 500
            return send_json_line(conn, {"error": str(e)})
    # app.run(host="127.0.0.1", port=5000)

if __name__ == "__main__":
    main()