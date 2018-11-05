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

        public SelectBank(string usbipServer, string pingResource, 
            PictureBox statusBank, PictureBox statusKeys, PictureBox statusInet, SectionData section)
        {
            if (LoginForm.test != false)
            {
                Thread threadLoading = new Thread(showLoadingForm);
                threadLoading.Start();                

                loadingForm.labelText = "Проверка связи с сервером ключей";

                try
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

                        //loadingForm.buttonCancel.Enabled = true;

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

                                    // Запуск IE от обычного пользователя (Спасибо опредленным банком за это)
                                    try
                                    {
                                        if (section.Keys["url"] != null)
                                        {
                                            ProcessStartInfo browser = new ProcessStartInfo("C:\\Program Files\\Internet Explorer\\iexplore.exe");
                                            browser.Arguments = section.Keys["url"];
                                            browser.UserName = "user";
                                            browser.UseShellExecute = false;

                                            Process.Start(browser);
                                        }
                                    }
                                    catch
                                    {
                                    }
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
