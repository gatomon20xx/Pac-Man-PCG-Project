# from flask import Flask, request, jsonify
import json
import socket
import os
from deepface import DeepFace
import base64
import struct
import numpy as np
import cv2


HOST = ''
PORT = 5000
fieldnames = ["happy", "sad", "angry", "disgust", "fear", "surprise", "neutral"]

def get_unique_folder(parent, folder_name):
    base_path = os.path.join(parent, folder_name)
    if not os.path.exists(base_path):
        os.makedirs(base_path)
        return base_path
    k = 1
    while True:
        p = os.path.join(parent, f"{folder_name}_{k}")
        if not os.path.exists(p):
            os.makedirs(p)
            return p
        k += 1

def create_csv_file(csv_file_path, fieldnames):
    try:
        os.makedirs(os.path.dirname(csv_file_path), exist_ok=True)
        write_header = not os.path.exists(csv_file_path)
        with open(csv_file_path, 'a+', newline='') as f:
            w = csv.DictWriter(f, fieldnames=fieldnames, delimiter=';')
            if write_header:
                w.writeheader()
    except Exception as e:
        print("Error creating file:", str(e), flush=True)

def write_data_to_csv(csv_file_path, fieldnames, rows):
    try:
        with open(csv_file_path, 'a+', newline='') as f:
            w = csv.DictWriter(f, fieldnames=fieldnames, delimiter=';')
            w.writerows(rows)
    except Exception as e:
        print("Error writing to file:", str(e), flush=True)

def send_json_line(conn, obj):
    line = json.dumps(obj, ensure_ascii=False) + "\n"
    conn.sendall(line.encode("utf-8"))

def recvall(sock, length):
    data = b''
    while len(data) < length:
        packet = sock.recv(length - len(data))
        if not packet:
            return None
        data += packet
    return data

def main():
    # Make path to log data
    global PROJECT_PATH, OBSERVATIONS_LOG_PATH
    base = os.path.join(os.getcwd(), "EmotionData")
    os.makedirs(base, exist_ok=True)
    PROJECT_PATH = get_unique_folder(base, "SendToProctor")
    emo_csv = os.path.join(PROJECT_PATH, 'EmotionalScores.csv')
    create_csv_file(emo_csv, fieldnames)

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
            # Get the length of the image before processing it.
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
                }) + "\n"

            write_data_to_csv(emo_csv, fieldnames, emo_score)

            # Send the emotions as a message.
            conn.sendall(message.encode("utf-8"))

        except Exception as e:
            return send_json_line(conn, {"error": str(e)})

if __name__ == "__main__":
    main()