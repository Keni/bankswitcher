using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using IniParser;
using IniParser.Model;

namespace BankSwitcher
{
    public partial class MainForm : Form
    {
        List<Button> banks = new List<Button>();

        List<PictureBox> listStatusBank = new List<PictureBox>();
        List<PictureBox> listStatusKeys = new List<PictureBox>();
        List<PictureBox> listStatusInet = new List<PictureBox>();

        int top = 25;
        int left = 15;
        int currentBank = 0;
        bool counterRow = true;

        private static LoadingForm loadingForm = new LoadingForm();

        public MainForm()
        {
            if (LoginForm.test == true)
            {
                InitializeComponent();

                var parser = new FileIniDataParser();
                IniData banksData = new IniData();
                try
                {
                    banksData = parser.ReadFile("config.ini");
                }
                catch
                {
                    logToFile("Вызвана ошибка отсутствия файла конфигурации");
                    MessageBox.Show("Отсутствует файл конфигурации", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                try
                {
                    string currentPath = Path.GetDirectoryName(Application.ExecutablePath);
                    string pathVariables = Environment.GetEnvironmentVariable("PATH");
                    if (!pathVariables.Contains(currentPath))
                        Environment.SetEnvironmentVariable("PATH", pathVariables + @";" + currentPath, EnvironmentVariableTarget.Machine);
                }
                catch
                {
                    logToFile("Программа запущена без прав администратор");
                    MessageBox.Show("Программа запущена без прав администратор", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                if (banksData.Sections.Count > 1 && banksData["Options"]["interface"] != null
                    && banksData["Options"]["mask"] != null && banksData["Options"]["gateway"] != null && banksData["Options"]["usbip_server_ip"] != null
                    && banksData["Options"]["usbip_server_ip"] != null  && banksData["Options"]["ping_resource"] != null)
                {                    
                    string networkInterface = banksData["Options"]["interface"];
                    string mask = banksData["Options"]["mask"];
                    string gateway = banksData["Options"]["gateway"];
                    string dns = banksData["Options"]["dns"];
                    string usbipServerIP = banksData["Options"]["usbip_server_ip"];
                    string usbipServerPort = banksData["Options"]["usbip_server_port"];
                    string pingResource = banksData["Options"]["ping_resource"];

                    foreach (SectionData bank in banksData.Sections.Skip(1))
                    {
                        Button buttonBank = new Button
                        {
                            Left = left,
                            Top = top,
                            Width = 250,
                            Text = bank.Keys["name"]
                        };
                        banks.Add(buttonBank);

                        ToolTip info = new ToolTip();
                        info.InitialDelay = 1000;
                        info.AutoPopDelay = 60000;
                        info.SetToolTip(buttonBank,
                            "ID: " + bank.Keys["id"] +
                            "\nIP: " + bank.Keys["ip"] +
                            "\nID-ключа: " + bank.Keys["usb_key_id"] +
                            "\nНомер сим-карты: " + bank.Keys["phone"] +
                            "\nПароль от сим-карты: " + bank.Keys["sim_card_password"]
                            );

                        PictureBox statusBank = new PictureBox();
                        PictureBox statusKeys = new PictureBox();
                        PictureBox statusInet = new PictureBox();

                        listStatusBank.Add(statusBank);
                        listStatusKeys.Add(statusKeys);
                        listStatusInet.Add(statusInet);

                        statusBank.Image = Properties.Resources.led_grey;
                        statusKeys.Image = Properties.Resources.led_grey;
                        statusInet.Image = Properties.Resources.led_grey;

                        statusBank.SizeMode = PictureBoxSizeMode.StretchImage;
                        statusBank.Size = new Size(20, 20);
                        statusKeys.SizeMode = PictureBoxSizeMode.StretchImage;
                        statusKeys.Size = new Size(20, 20);
                        statusInet.SizeMode = PictureBoxSizeMode.StretchImage;
                        statusInet.Size = new Size(20, 20);

                        statusBank.Location = new Point(buttonBank.Location.X + buttonBank.Width + 34, buttonBank.Location.Y + 2);
                        statusKeys.Location = new Point(statusBank.Location.X + statusBank.Width + 31, statusBank.Location.Y);
                        statusInet.Location = new Point(statusKeys.Location.X + statusKeys.Width + 27, statusKeys.Location.Y);

                        buttonBank.Click += (sender, args) =>
                        {
                            if (bank.Keys["id"] != null && bank.Keys["ip"] != null && bank.Keys["usb_key_id"] != null)
                            {
                                Thread threadLoading = new Thread(showLoadingForm);
                                threadLoading.Start();

                                dismountKeys("-d 1");
                                dismountKeys("-d 2");

                                listStatusBank[currentBank].Image = Properties.Resources.led_grey;
                                listStatusKeys[currentBank].Image = Properties.Resources.led_grey;
                                listStatusInet[currentBank].Image = Properties.Resources.led_grey;
                                currentBank = Int32.Parse(bank.Keys["id"]) - 1;

                                logToFile("Выбран банк: " + bank.Keys["name"] + ", ID: " + bank.Keys["id"]);

                                loadingForm.labelText = "Инициализация ключей на стороне сервера";
                                new Thread(() =>
                                {
                                    UsbipClient usbipClient = new UsbipClient();

                                    if (usbipClient.client(usbipServerIP, Int32.Parse(usbipServerPort), bank.Keys["usb_key_id"]))
                                    {
                                        threadLoading.Abort();
                                        this.Invoke(new MethodInvoker(() => this.Enabled = false));
                                        SelectBank selectBank = new SelectBank(networkInterface, mask, gateway, dns, usbipServerIP, pingResource, statusBank, statusKeys, statusInet, bank);
                                        this.Invoke(new MethodInvoker(() => this.Enabled = true));
                                    }
                                    else
                                    {
                                        threadLoading.Abort();
                                        logToFile("Сервер ключей вернул ошибку, ключ не был инициализирован");
                                        MessageBox.Show("Сервер ключей вернул ошибку, ключ не был инициализирован", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }).Start();
                                
                            }
                            else
                            {
                                logToFile("Вызвана ошибка конфигурации банка");
                                MessageBox.Show("Ошибка конфигурации банка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);                            
                            }
                        };

                        if (counterRow)
                        {
                            Label labelBank = new Label
                            {
                                Text = "Банк",
                                Left = left,
                                Top = 9,
                                AutoSize = true
                            };

                            Label labelOnOff = new Label
                            {
                                Text = "Вкл/Выкл",
                                Left = left + 265,
                                Top = 9,
                                AutoSize = true
                            };

                            Label labelKeys = new Label
                            {
                                Text = "Ключи",
                                Left = left + 325,
                                Top = 9,
                                AutoSize = true
                            };

                            Label labelInternet = new Label
                            {
                                Text = "Интернет",
                                Left = left + 365,
                                Top = 9,
                                AutoSize = true
                            };

                            this.Controls.Add(labelBank);
                            this.Controls.Add(labelOnOff);
                            this.Controls.Add(labelKeys);
                            this.Controls.Add(labelInternet);
                            counterRow = false;
                        }

                        this.Controls.Add(buttonBank);
                        this.Controls.Add(statusBank);
                        this.Controls.Add(statusKeys);
                        this.Controls.Add(statusInet);

                        top += buttonBank.Height + 2;

                        if (banks.Count % 20 == 0)
                        {
                            this.Size = new Size(950, 600);
                            left += 450;
                            top = 25;
                            counterRow = true;
                        }
                    }
                }
                else if (banksData.Sections.Count > 41)
                {
                    logToFile("Слишком много банков добавленно");
                    MessageBox.Show("Превышен лимит банков. Обратитесь за новой лицензией :)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }
                else
                {
                    logToFile("Вызвана ошибка содержимого файла конфигурации");
                    MessageBox.Show("Ошибка конфигурации приложения", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }
            }
            else
            {
                Environment.Exit(1);
            }           
            
        }

        public static void logToFile(string message)
        {
            StringBuilder sb = new StringBuilder();

            sb.Length = 0;
            sb.Append(DateTime.Now + " " + message + Environment.NewLine);
            File.AppendAllText("log.txt", sb.ToString());
        }

        private void dismountKeys(string key)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("usbip", key)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);

            process.WaitForExit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            dismountKeys("-d 1");
            dismountKeys("-d 2");

            MainForm.logToFile("Приложение было закрыто");
        }

        private void showLoadingForm()
        {
            loadingForm.ShowDialog();
        }
    }
}
