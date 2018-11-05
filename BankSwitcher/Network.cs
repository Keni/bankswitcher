using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BankSwitcher
{
    class Network
    {
        private static LoadingForm loadingForm = new LoadingForm();

        public Network(string networkInterface, string mask,
            string gateway, string dns, string pingResource, SectionData section)
        {
            MainForm.logToFile("Установлен IP: " + section.Keys["ip"]);
            
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect(pingResource, 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                
                if (!endPoint.Address.ToString().Equals(section.Keys["ip"]))
                {
                    string configAdapter = "interface ip set address " +
                    networkInterface + " static " + section.Keys["ip"] + " " + mask + " " + gateway;
                    string setDNS = "interface ip set dns " + networkInterface + " static " + dns;
                    string disableAdapter = "interface set interface " + networkInterface + " disable";
                    string enableAdapter = "interface set interface " + networkInterface + " enable";

                    //loadingForm.labelText = "Применение параметров сетевого адаптера";

                    startNetsh(enableAdapter);
                    startNetsh(configAdapter);
                    startNetsh(setDNS);
                    startNetsh(disableAdapter);
                    startNetsh(enableAdapter);
                }
            }
        }
        private void startNetsh(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("netsh", command)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            process.WaitForExit();
        }
    }
}
