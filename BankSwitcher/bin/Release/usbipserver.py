import socket
import subprocess
import time

def mountKey(key):
    subprocess.call("usbip unbind -b " + key, shell=True, stderr=subprocess.STDOUT)
    time.sleep(1)
    subprocess.call("usbip bind -b " + key, shell=True, stderr=subprocess.STDOUT)
    time.sleep(1)
    subprocess.call("usbip attach -r localhost -b " + key, shell=True, stderr=subprocess.STDOUT)
    time.sleep(2)
    subprocess.call("usbip detach -p 0", shell=True, stderr=subprocess.STDOUT)

def startServer():
    sock = socket.socket()
    sock.bind(('', 666))
    sock.listen(1)
    conn, address = sock.accept()

    print('Connected:', address)

    while True:
        data = conn.recv(1024)

        if not data:
            break
        else:
            try:
                key = data.decode('utf-8')
                mountKey(key)
                conn.send('success'.encode())
            except:
                conn.send('error'.encode())
                break
    conn.close()
    time.sleep(5)

while True:
    startServer()
