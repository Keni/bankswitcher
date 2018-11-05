#!/usr/bin/python3
import socket
import subprocess
import time
import logging
import os
import sys

clientIP = "ip"
clientIPTEST = "ip"
usbip = "/usr/sbin/usbip"
logging.basicConfig(filename="/var/log/usbipserver.log", level=logging.INFO)


def log(message):
    with open('/var/log/usbipserver.log', 'a') as f:
        f.write(time.strftime("%d.%m.%Y %H:%M:%S ") + message)


def process(command):
    with open('/var/log/usbipserver.log', 'a') as logfile:
        subprocess.Popen(usbip + command, shell=True, stderr=subprocess.STDOUT, stdout=logfile)


def mountKey(key):
    process(" unbind -b " + key)
    time.sleep(1)

    process(" bind -b " + key)
    time.sleep(1)

    process(" attach -r localhost -b " + key)
    time.sleep(2)

    process(" detach -p 0")


def reboot():
    os.system('/sbin/shutdown -r now')


def startServer():
    sock = socket.socket()
    sock.bind(('', 666))
    sock.listen(1)

    log("Server start\n")
    print("Server start")

    conn, address = sock.accept()

    if (clientIP in str(address)) or (clientIPTEST in str(address)):
        log("Connected: " + str(address) + "\n")
        print("Connected: " + str(address))

        while True:
            data = conn.recv(1024)

            if not data:
                break
            else:
                try:
                    key = data.decode('utf-8')

                    if "1-1.2" in key:
                        keysArray = key.split(',')

                        for i in range(len(keysArray)):
                            mountKey(keysArray[i])

                        conn.send('success'.encode())
                        print("Key attached")
                        log("Key attached\n")

                    elif key == "reboot":
                        conn.send('rebooting'.encode())
                        print("Rebooting")
                        log("Rebooting\n")
                        reboot()

                    else:
                        conn.send('errorCMD'.encode())
                        print("Error command")
                        log("Error command\n")

                except:
                    conn.send('errorSRV'.encode())
                    print("Server error")
                    log("Server error\n")
                    break

    else:
        log("Failed connected from: " + str(address) + "\n")
        print("Failed connected from: " + str(address))

    conn.close()
    time.sleep(2)


while True:
    startServer()
