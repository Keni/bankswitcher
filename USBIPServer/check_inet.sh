#!/bin/bash

if ping -c5 192.168.11.254; then
    echo "ping success"
else
    shutdown -r 'no ping.'
fi

if pgrep -f "python usbipserver.py" &>/dev/null; then
    echo "usbipserver is running"
    exit
else
    python3 usbipserver.py
fi
