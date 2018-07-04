using System;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading;
using IniParser.Model;
using System.Net.Sockets;
using System.Net;

namespace BankSwitcher
{
    class SelectBank
    {
        Ping ping = new Ping();

        private static bool working;

        private static LoadingForm loadingForm = new LoadingForm();

        public SelectBank(string networkInterface, string mask,
            string gateway, string dns, string usbipServer, string pingResource, 
            PictureBox statusBank, PictureBox statusKeys, PictureBox statusInet, SectionData section)
        {
            if (LoginForm.test != false)
            {
                Thread threadLoading = new Thread(showLoadingForm);
                threadLoading.Start();

                MainForm.logToFile("Установлен IP: " + section.Keys["ip"]);

                statusBank.Image = Properties.Resources.led_green;

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect(pingResource, 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    Console.WriteLine(endPoint.Address.ToString());

                    if (!endPoint.Address.ToString().Equals(section.Keys["ip"]))
                    {
                        string configAdapter = "interface ip set address " +
                        networkInterface + " static " + section.Keys["ip"] + " " + mask + " " + gateway;
                        string setDNS = "interface ip set dns " + networkInterface + " static " + dns;
                        string disableAdapter = "interface set interface " + networkInterface + " disable";
                        string enableAdapter = "interface set interface " + networkInterface + " enable";

                        loadingForm.labelText = "Применение параметров сетевого адаптера";

                        startNetsh(configAdapter);
                        startNetsh(setDNS);
                        startNetsh(disableAdapter);
                        startNetsh(enableAdapter);
                    }
                }

                loadingForm.labelText = "Проверка связи с сервером ключей";

                try
                {
                    PingReply pingReplyServer = ping.Send(usbipServer);

                    if (pingReplyServer.Status == IPStatus.Success)
                    {
                        loadingForm.labelText = "Подключение ключа";
                        
                        string usbipKey = " -a " + usbipServer + " -b " + section.Keys["usb_key_id"];
                        mountKeys(usbipKey, statusKeys);

                        if (section.Keys["usb_key2_id"] != null)
                        {
                            loadingForm.labelText = "Подключение второго ключа";
                            string usbipKey_2 = " -a " + usbipServer + " -b " + section.Keys["usb_key2_id"];
                            mountKeys(usbipKey_2, statusKeys);
                        }
                    }
                    else
                    {
                        MainForm.logToFile("Нет связи с сервером ключей");
                        statusKeys.Image = Properties.Resources.led_red;
                        MessageBox.Show("Нет связи с сервером ключей", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MainForm.logToFile("Нет связи с сервером ключей");
                    statusKeys.Image = Properties.Resources.led_red;
                    MessageBox.Show("Нет связи с сервером ключей", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                for (int i = 1; i <= 10; i++)
                {
                    loadingForm.labelText = "Проверка интернета";
                    try
                    {
                        loadingForm.labelText = "Попытка проверки интернета #" + i;

                        PingReply reply = ping.Send(pingResource);

                        if (reply.Status == IPStatus.Success)
                        {
                            MainForm.logToFile("Статус интернета: ОК");
                            statusInet.Image = Properties.Resources.led_green;

                            threadLoading.Abort();

                            if (working)
                            {
                                try
                                {
                                    Process.Start(@section.Keys["dir"]);
                                    Process.Start(section.Keys["url"]);
                                }
                                catch
                                {
                                }
                            }

                            break;
                        }
                        else
                        {
                            if (i == 10)
                            {
                                MainForm.logToFile("Статус интернета: Нет пинга");
                                statusInet.Image = Properties.Resources.led_red;
                                threadLoading.Abort();
                                
                                if (working)
                                {
                                    try
                                    {
                                        Process.Start(@section.Keys["dir"]);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }

                            Thread.Sleep(2000);
                        }
                    }
                    catch (PingException)
                    {
                        MainForm.logToFile("Статус интернета: Ошибка соединения");
                        statusInet.Image = Properties.Resources.led_red;
                        threadLoading.Abort();
                        break;
                    }

                }                
            }
            else
            {
                Environment.Exit(1);
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

        private static void mountKeys(string key, PictureBox statusKeys)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("usbip", key)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);

            Thread.Sleep(2000);

            loadingForm.labelText = "Проверка проброса ключей";

            Process[] pname = Process.GetProcessesByName("usbip");
            Console.WriteLine(pname.Length);
            if (pname.Length != 0)
            {
                MainForm.logToFile("Ключи проброшены");
                statusKeys.Image = Properties.Resources.led_green;
                working = true;
            }
            else
            {
                MainForm.logToFile("Ошибка проброса ключей");
                statusKeys.Image = Properties.Resources.led_red;
                working = false;
            }
        }

        private void showLoadingForm()
        {
            loadingForm.ShowDialog();           
        }
    }
}
