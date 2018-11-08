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
        
        string networkInterface;
        string mask;
        string gateway;
        string dns;
        string usbipServerIP;
        string usbipServerPort;
        string pingResource;

        int top = 50;
        int left = 15;
        int currentBank = 0;
        bool counterRow = true;

        public static LoadingForm loadingForm = new LoadingForm();
        
        public MainForm()
        {           
            if (LoginForm.test)
            {
                InitializeComponent();                

                // Парсим конфиг
                var parser = new FileIniDataParser();
                IniData banksData = new IniData();
                try
                {
                    banksData = parser.ReadFile("config.ini");
                }
                catch
                {
                    errorAndExit("Возникла ошибка отсутствия файла конфигурации", "Отсутствует файл конфигурации");
                }

                // Проверка запущена ли программа с админскими правами
                try
                {
                    string currentPath = Path.GetDirectoryName(Application.ExecutablePath);
                    string pathVariables = Environment.GetEnvironmentVariable("PATH");
                    if (!pathVariables.Contains(currentPath))
                        Environment.SetEnvironmentVariable("PATH", pathVariables + @";" + currentPath, EnvironmentVariableTarget.Machine);
                }
                catch
                {
                    errorAndExit("Программа запущена без прав администратор", "Программа запущена без прав администратор");
                }

                if (banksData.Sections.Count > 1 && banksData["Options"]["interface"] != null
                    && banksData["Options"]["mask"] != null && banksData["Options"]["gateway"] != null && banksData["Options"]["usbip_server_ip"] != null
                    && banksData["Options"]["usbip_server_ip"] != null  && banksData["Options"]["ping_resource"] != null)
                {                    
                    networkInterface = banksData["Options"]["interface"];
                    mask = banksData["Options"]["mask"];
                    gateway = banksData["Options"]["gateway"];
                    dns = banksData["Options"]["dns"];
                    usbipServerIP = banksData["Options"]["usbip_server_ip"];
                    usbipServerPort = banksData["Options"]["usbip_server_port"];
                    pingResource = banksData["Options"]["ping_resource"];
                    
                    // Заполнение формы
                    foreach (SectionData bank in banksData.Sections.Skip(1))
                    {
                        // Формирование кнопки
                        Button buttonBank = new Button
                        {
                            Left = left,
                            Top = top,
                            Width = 250,
                            Text = bank.Keys["name"]
                        };
                        banks.Add(buttonBank);
                        
                        // Параметры тултипа для кнопки
                        string toolTipCaption = "ID: " + bank.Keys["id"] +
                                                "\nIP: " + bank.Keys["ip"] +
                                                "\nID ключа: " + bank.Keys["usb_key_id"];

                        if (bank.Keys["usb_key2_id"] != null)
                            toolTipCaption += "\nID второго ключа: " + bank.Keys["usb_key2_id"];

                        if (bank.Keys["phone"] != null)
                            toolTipCaption += "\nНомер сим-карты: " + bank.Keys["phone"];

                        ToolTip info = new ToolTip();
                        info.InitialDelay = 1000;
                        info.AutoPopDelay = 60000;
                        info.SetToolTip(buttonBank, toolTipCaption);

                        // Статус кнопки
                        PictureBox statusBank = new PictureBox();
                        PictureBox statusKeys = new PictureBox();
                        PictureBox statusInet = new PictureBox();

                        statusBank.Image = Properties.Resources.led_grey;
                        statusKeys.Image = Properties.Resources.led_grey;
                        statusInet.Image = Properties.Resources.led_grey;

                        statusBank.SizeMode = PictureBoxSizeMode.StretchImage;
                        statusBank.Size = new Size(20, 20);
                        statusKeys.SizeMode = PictureBoxSizeMode.StretchImage;
                        statusKeys.Size = new Size(20, 20);
                        statusInet.SizeMode = PictureBoxSizeMode.StretchImage;
                        statusInet.Size = new Size(20, 20);

                        listStatusBank.Add(statusBank);
                        listStatusKeys.Add(statusKeys);
                        listStatusInet.Add(statusInet);

                        statusBank.Location = new Point(buttonBank.Location.X + buttonBank.Width + 34, buttonBank.Location.Y + 2);
                        statusKeys.Location = new Point(statusBank.Location.X + statusBank.Width + 31, statusBank.Location.Y);
                        statusInet.Location = new Point(statusKeys.Location.X + statusKeys.Width + 27, statusKeys.Location.Y);

                        // Обработка нажатия кнопки                         
                        buttonBank.Click += (sender, args) =>
                        {
                            if (bank.Keys["id"] != null && bank.Keys["ip"] != null && bank.Keys["usb_key_id"] != null)
                            {
                                logToFile("Выбран банк: " + bank.Keys["name"] + ", ID: " + bank.Keys["id"]);

                                // Очищение формы и выбор текущего банка
                                listStatusBank[currentBank].Image = Properties.Resources.led_grey;
                                listStatusKeys[currentBank].Image = Properties.Resources.led_grey;
                                listStatusInet[currentBank].Image = Properties.Resources.led_grey;
                                currentBank = Int32.Parse(bank.Keys["id"]) - 1;

                                // Зажигаем кнопку, что банк активировался
                                listStatusBank[currentBank].Image = Properties.Resources.led_green;

                                string keys = bank.Keys["usb_key_id"];
                                if (bank.Keys["usb_key2_id"] != null)
                                    keys += ", " + bank.Keys["usb_key2_id"];

                                this.Enabled = false;

                                loadingForm.labelText = "Применение параметров сетевого адаптера";

                                Thread ThreadLoadingDialog = new Thread(LoadingThreadDialog);
                                ThreadLoadingDialog.Start();

                                dismountKeys();
                                
                                // Запускаем поток настройки сетевого интерфейса
                                Thread ThreadNetwork = new Thread(() =>
                                {
                                    Network network = new Network(networkInterface, mask, gateway, dns, pingResource, bank);
                                });
                                ThreadNetwork.Start();

                                // Запускаем поток подключения ключей
                                Thread threadUsbIPClient = new Thread(() =>
                                {
                                    loadingForm.labelText = "Инициализация ключей на стороне сервера";

                                    if (usbipClientThread(keys))
                                    {
                                        Thread ThreadSelectBank = new Thread(() =>
                                        {
                                            SelectBank selectBank = new SelectBank(usbipServerIP, pingResource, statusBank, statusKeys, statusInet, bank);
                                        });
                                        ThreadNetwork.Join();
                                        ThreadSelectBank.Start();
                                        ThreadSelectBank.Join();
                                    }
                                    else
                                    {
                                        statusKeys.Image = Properties.Resources.led_red;
                                        MainForm.logToFile("Сервер вернул ошибку");
                                        MessageBox.Show("Ошибка на стороне сервера", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                });
                                threadUsbIPClient.Start();
                                threadUsbIPClient.Join();
                                ThreadLoadingDialog.Abort();
                                this.Enabled = true;
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
                                Top = 34,
                                AutoSize = true
                            };

                            Label labelOnOff = new Label
                            {
                                Text = "Вкл/Выкл",
                                Left = left + 265,
                                Top = 34,
                                AutoSize = true
                            };

                            Label labelKeys = new Label
                            {
                                Text = "Ключи",
                                Left = left + 325,
                                Top = 34,
                                AutoSize = true
                            };

                            Label labelInternet = new Label
                            {
                                Text = "Интернет",
                                Left = left + 365,
                                Top = 34,
                                AutoSize = true
                            };

                            this.Controls.Add(labelBank);
                            this.Controls.Add(labelOnOff);
                            this.Controls.Add(labelKeys);
                            this.Controls.Add(labelInternet);
                            counterRow = false;
                        }

                        if (!LoginForm.hr)
                        {
                            addButton();

                            newLine();
                        }
                        else if (LoginForm.hr && bank.Keys["hr"] == "1")
                        {
                            addButton();

                            newLine();
                        }

                        // Функция переноса кнопок на новую строку и формирование по 20 столбцов, но не более 2 столбоцов по 20 кнопок
                        void newLine()
                        {
                            top += buttonBank.Height + 2;

                            if (banks.Count % 20 == 0)
                            {
                                this.Size = new Size(950, 600);
                                left += 450;
                                top = 50;
                                counterRow = true;
                            }
                        }

                        // Функция добовления кнопок на форму
                        void addButton()
                        {
                            this.Controls.Add(buttonBank);
                            this.Controls.Add(statusBank);
                            this.Controls.Add(statusKeys);
                            this.Controls.Add(statusInet);
                        }
                    }
                }
                else if (banksData.Sections.Count > 41)
                {
                    errorAndExit("Слишком много банков добавленно", "Превышен лимит банков. Обратитесь за новой лицензией :)");
                }
                else
                {
                    errorAndExit("Вызвана ошибка содержимого файла конфигурации", "Ошибка конфигурации приложения");
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

        private void dismountKeys()
        {
            dismountKeys("-d 1");
            dismountKeys("-d 2");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            dismountKeys();

            MainForm.logToFile("Приложение было закрыто");
        }

        private void LoadingThreadDialog()
        {
            loadingForm.ShowDialog();
        }

        private bool usbipClientThread(string keys)
        {
            UsbipClient usbipClient = new UsbipClient();
            return usbipClient.client(usbipServerIP, Int32.Parse(usbipServerPort), keys);
        }

        private void errorAndExit(string log, string message)
        {
            logToFile(log);
            MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        private void rebootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UsbipClient usbipClient = new UsbipClient();

            if (usbipClient.client(usbipServerIP, Int32.Parse(usbipServerPort), "reboot"))
            {
                MessageBox.Show("Сервер был перезагружен, это займет 1-2 минуты", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
