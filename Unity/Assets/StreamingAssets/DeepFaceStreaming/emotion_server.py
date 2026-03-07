# from flask import Flask, request, jsonify
import json
import socket
from deepface import DeepFace
import base64
import numpy as np
import cv2


HOST = ''
PORT = 5000

def send_json_line(conn, obj):
    line = json.dumps(obj, ensure_ascii=False) + "\n"
    conn.sendall(line.encode("utf-8"))

# app = Flask(__name__)

# @app.route("/analyze", methods=["POST"])
def analyze(conn):
    try:
        # Access the image data in the Flask request, then decode it to a proper image file.
        # data = request.json["image"]

        json_data = conn.recv(4096).decode('utf-8')
        parsed_data = json.loads(data)
        data = parsed_data["image"]

        img_bytes = base64.b64decode(data)
        img_array = np.frombuffer(img_bytes, np.uint8)
        img = cv2.imdecode(img_array, cv2.IMREAD_COLOR)

        # Get the predicted emotions from the image
        result = DeepFace.analyze(img, actions=["emotion"], enforce_detection=False)
        return send_json_line(conn, result)
        # return jsonify(result)

    except Exception as e:
        # Return the error and the error code.
        # return jsonify({"error": str(e)}), 500
        return send_json_line(conn, {"error": str(e)})

def main():
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.bind((HOST, PORT))
    s.listen(1)
    print('Server starts, waiting for connection...', flush=True)
    conn, addr = s.accept()
    print('Connected by', addr, flush=True)

    while True:
        analyze(conn)
    # app.run(host="127.0.0.1", port=5000)