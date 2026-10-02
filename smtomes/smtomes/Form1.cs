using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks; // 添加这行
using System.Text.RegularExpressions;
using System.Timers;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace smtomes
{
    public partial class Form1 : Form
    {
        SQL MYSQL = new SQL(); //数据库实例

        private SerialPort serialPort1;
        private SerialPort serialPort2;
        private SerialPort serialPort3;
        private SerialPort serialPort4;
        private System.Net.Sockets.TcpClient tcpClient1;
        private System.Net.Sockets.TcpListener tcpListener1;
        private System.Net.Sockets.TcpClient tcpClient2;
        private System.Net.Sockets.TcpListener tcpListener2;
        private System.Net.Sockets.TcpClient tcpClient3;
        private System.Net.Sockets.TcpListener tcpListener3;
        private System.Net.Sockets.TcpClient tcpClient4;
        private System.Net.Sockets.TcpListener tcpListener4;
        
        private DateTime lastSerialPort1ReceiveTime;
        private DateTime lastSerialPort3ReceiveTime;
        private string firstLine;
        private string secondLine;
        private System.Threading.Timer timeoutTimer;
        private string firstLine2;
        private string secondLine2;
        private System.Threading.Timer timeoutTimer2;
        private System.Threading.Timer bjy_mes1_resetTimer;
        private System.Threading.Timer bjy_mes2_resetTimer;
        private System.Threading.Timer panel4_resetTimer;
        private System.Threading.Timer panel9_resetTimer;

        private bool cb1_dm_isT = true;
        private bool cb2_dm_isT = true;

        public static int bt_sdsc = 0;
     

        //public static string com1sel, com2sel,com3sel,com4sel;


        public Form1()
        {
            InitializeComponent();


            serialPort1 = new SerialPort();
            serialPort2 = new SerialPort();
            serialPort3 = new SerialPort();
            serialPort4 = new SerialPort();
            
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);
            serialPort2.DataReceived += new SerialDataReceivedEventHandler(serialPort2_DataReceived);
            serialPort3.DataReceived += new SerialDataReceivedEventHandler(serialPort3_DataReceived);
            serialPort4.DataReceived += new SerialDataReceivedEventHandler(serialPort4_DataReceived);
            
            tcpClient1 = new System.Net.Sockets.TcpClient();
            tcpClient2 = new System.Net.Sockets.TcpClient();
            tcpClient3 = new System.Net.Sockets.TcpClient();
            tcpClient4 = new System.Net.Sockets.TcpClient();
        }




        private void Form1_Load(object sender, EventArgs e)
        {

            // 初始化面板显示状态
            panel_1.Visible = true;
            panel_2.Visible = false;
            panel_3.Visible = true;
            panel_4.Visible = false;
            panel1_5.Visible = true;
            panel1_6.Visible = false;
            panel_7.Visible = true;
            panel1_8.Visible = false;

            csinit();



            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭已打开的串口
            try
            {
                if (serialPort1 != null && serialPort1.IsOpen)
                {
                    serialPort1.Close();
                }
                if (serialPort2 != null && serialPort2.IsOpen)
                {
                    serialPort2.Close();
                }
                if (serialPort3 != null && serialPort3.IsOpen)
                {
                    serialPort3.Close();
                }
                if (serialPort4 != null && serialPort4.IsOpen)
                {
                    serialPort4.Close();
                }
            }
            catch (Exception ex)
            {
                // 忽略关闭时的错误
            }

            // 关闭已打开的TCP连接
            try
            {
                // 关闭TCP客户端
                if (tcpClient1 != null && tcpClient1.Connected)
                {
                    tcpClient1.Close();
                }
                if (tcpClient2 != null && tcpClient2.Connected)
                {
                    tcpClient2.Close();
                }
                if (tcpClient3 != null && tcpClient3.Connected)
                {
                    tcpClient3.Close();
                }
                if (tcpClient4 != null && tcpClient4.Connected)
                {
                    tcpClient4.Close();
                }

                // 关闭TCP服务端
                if (tcpListener1 != null)
                {
                    tcpListener1.Stop();
                }
                if (tcpListener2 != null)
                {
                    tcpListener2.Stop();
                }
                if (tcpListener3 != null)
                {
                    tcpListener3.Stop();
                }
                if (tcpListener4 != null)
                {
                    tcpListener4.Stop();
                }
            }
            catch (Exception ex)
            {
                // 忽略关闭时的错误
            }
        }

        private void csinit()
        {
            string strsql = "select top 1 * from config"; //公共SQL数据库查询字符串变量
            DataTable dt = MYSQL.GetDataTable(strsql);

            txtApiUrl.Text = dt.Rows[0]["apiurl"].ToString();
            txtEmployee.Text = dt.Rows[0]["username"].ToString();
            txtEquipMac.Text = dt.Rows[0]["sbid"].ToString();
            txtApiUrl2.Text = dt.Rows[0]["apiurl2"].ToString();
            txtEmployee2.Text = dt.Rows[0]["username2"].ToString();
            txtEquipMac2.Text = dt.Rows[0]["sbid2"].ToString();

            mes_wt1.Text = dt.Rows[0]["meswt1"].ToString();
            mes_wt2.Text = dt.Rows[0]["meswt2"].ToString();

            string com1sel = dt.Rows[0]["com1sel"].ToString();
            string com2sel = dt.Rows[0]["com2sel"].ToString();
            string com3sel = dt.Rows[0]["com3sel"].ToString();
            string com4sel = dt.Rows[0]["com4sel"].ToString();

            // 自动检测串口并填充下拉菜单
            FillSerialPortComboBox(comsel1, com1sel);
            FillSerialPortComboBox(comsel2, com2sel);
            FillSerialPortComboBox(comsel3, com3sel);
            FillSerialPortComboBox(comsel4, com4sel);

            josel1.SelectedIndex = 0;
            josel2.SelectedIndex = 0;
            josel3.SelectedIndex = 0;
            josel4.SelectedIndex = 0;


            if (dt.Rows[0]["tcptx1"].ToString() == "Y") radioButton_tcp1.Checked = true;
            else radioButton_serial1.Checked = true;

            if (dt.Rows[0]["tcptx2"].ToString() == "Y") radioButton_tcp2.Checked = true;
            else radioButton_serial2.Checked = true;

            if (dt.Rows[0]["tcptx3"].ToString() == "Y") radioButton_tcp3.Checked = true;
            else radioButton_serial3.Checked = true;

            if (dt.Rows[0]["tcptx4"].ToString() == "Y") radioButton6.Checked = true;
            else radioButton_serial4.Checked = true;

            if (dt.Rows[0]["tcpserver1"].ToString() == "Y") radioButton_server1.Checked = true;
            else radioButton_client1.Checked = true;
            if (dt.Rows[0]["tcpserver2"].ToString() == "Y") radioButton_server2.Checked = true;
            else radioButton_client2.Checked = true;
            if (dt.Rows[0]["tcpserver3"].ToString() == "Y") radioButton_server3.Checked = true;
            else radioButton_client3.Checked = true;
            if (dt.Rows[0]["tcpserver4"].ToString() == "Y") radioButton_server4.Checked = true;
            else radioButton_client4.Checked = true;

            tb_tcp_ip1.Text = dt.Rows[0]["ip1"].ToString();
            tb_tcp_ip2.Text = dt.Rows[0]["ip2"].ToString();
            tb_tcp_ip3.Text = dt.Rows[0]["ip3"].ToString();
            tb_tcp_ip4.Text = dt.Rows[0]["ip4"].ToString();

            tb_tcp_port1.Text = dt.Rows[0]["port1"].ToString();
            tb_tcp_port2.Text = dt.Rows[0]["port2"].ToString();
            tb_tcp_port3.Text = dt.Rows[0]["port3"].ToString();
            tb_tcp_port4.Text = dt.Rows[0]["port4"].ToString();

            cb1_dm.Checked = dt.Rows[0]["dm1"].ToString() == "Y";
            cb2_dm.Checked = dt.Rows[0]["dm2"].ToString() == "Y";


        }


        private void FillSerialPortComboBox(ComboBox comboBox,string comh)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add(comh);
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBox.Items.Add(port);
            }
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }


        }
        
        private void opencom1_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.PortName = comsel1.SelectedItem.ToString();
                    serialPort1.BaudRate = int.Parse(btl.Text);
                    serialPort1.DataBits = 8;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Parity = Parity.None;
                    
                    serialPort1.Open();
                    opencom1.Text = "关闭串口";
                    comsel1.Enabled = false;
                    UpdateUnifiedLog("串口1", "状态", string.Format("打开成功: {0}", serialPort1.PortName));
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("串口1", "错误", string.Format("打开失败: {0}", ex.Message));
                    MessageBox.Show("打开串口失败：" + ex.Message);
                }
            }
            else
            {
                try
                {
                    string portName = serialPort1.PortName;
                    serialPort1.Close();
                    opencom1.Text = "打开串口";
                    comsel1.Enabled = true;
                    UpdateUnifiedLog("串口1", "状态", string.Format("关闭成功: {0}", portName));
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("串口1", "错误", string.Format("关闭失败: {0}", ex.Message));
                    MessageBox.Show("关闭串口失败：" + ex.Message);
                }
            }
            UpdateReadCodeStatus();
        }
        
        private void opencom2_Click(object sender, EventArgs e)
        {
            if (!serialPort2.IsOpen)
            {
                try
                {
                    serialPort2.PortName = comsel2.SelectedItem.ToString();
                    serialPort2.BaudRate = int.Parse(textBox1.Text);
                    serialPort2.DataBits = 8;
                    serialPort2.StopBits = StopBits.One;
                    serialPort2.Parity = Parity.None;
                    
                    serialPort2.Open();
                    opencom2.Text = "关闭串口";
                    comsel2.Enabled = false;
                    UpdateUnifiedLog("串口2", "状态", string.Format("打开成功: {0}", serialPort2.PortName));
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("串口2", "错误", string.Format("打开失败: {0}", ex.Message));
                    MessageBox.Show("打开串口失败：" + ex.Message);
                }
            }
            else
            {
                try
                {
                    string portName = serialPort2.PortName;
                    serialPort2.Close();
                    opencom2.Text = "打开串口";
                    comsel2.Enabled = true;
                    UpdateUnifiedLog("串口2", "状态", string.Format("关闭成功: {0}", portName));
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("串口2", "错误", string.Format("关闭失败: {0}", ex.Message));
                    MessageBox.Show("关闭串口失败：" + ex.Message);
                }
            }
            UpdateReadCodeStatus();
        }
        
        private void opencom3_Click(object sender, EventArgs e)
        {
            if (!serialPort3.IsOpen)
            {
                try
                {
                    serialPort3.PortName = comsel3.SelectedItem.ToString();
                    serialPort3.BaudRate = int.Parse(textBox11.Text);
                    serialPort3.DataBits = 8;
                    serialPort3.StopBits = StopBits.One;
                    serialPort3.Parity = Parity.None;
                    
                    serialPort3.Open();
                    opencom3.Text = "关闭串口";
                    comsel3.Enabled = false;
                    UpdateUnifiedLog("串口3", "状态", string.Format("打开成功: {0}", serialPort3.PortName));
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("串口3", "错误", string.Format("打开失败: {0}", ex.Message));
                    MessageBox.Show("打开串口失败：" + ex.Message);
                }
            }
            else
            {
                try
                {
                    string portName = serialPort3.PortName;
                    serialPort3.Close();
                    opencom3.Text = "打开串口";
                    comsel3.Enabled = true;
                    UpdateUnifiedLog("串口3", "状态", string.Format("关闭成功: {0}", portName));
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("串口3", "错误", string.Format("关闭失败: {0}", ex.Message));
                    MessageBox.Show("关闭串口失败：" + ex.Message);
                }
            }
            UpdateReadCodeStatus();
        }
        
        private void opencom4_Click(object sender, EventArgs e)
        {
            if (!serialPort4.IsOpen)
            {
                try
                {
                    serialPort4.PortName = comsel4.SelectedItem.ToString();
                    serialPort4.BaudRate = int.Parse(textBox6.Text);
                    serialPort4.DataBits = 8;
                    serialPort4.StopBits = StopBits.One;
                    serialPort4.Parity = Parity.None;
                    
                    serialPort4.Open();
                    opencom4.Text = "关闭串口";
                    comsel4.Enabled = false;
                    UpdateUnifiedLog("串口4", "状态", string.Format("打开成功: {0}", serialPort4.PortName));
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("串口4", "错误", string.Format("打开失败: {0}", ex.Message));
                    MessageBox.Show("打开串口失败：" + ex.Message);
                }
            }
            else
            {
                try
                {
                    string portName = serialPort4.PortName;
                    serialPort4.Close();
                    opencom4.Text = "打开串口";
                    comsel4.Enabled = true;
                    UpdateUnifiedLog("串口4", "状态", string.Format("关闭成功: {0}", portName));
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("串口4", "错误", string.Format("关闭失败: {0}", ex.Message));
                    MessageBox.Show("关闭串口失败：" + ex.Message);
                }
            }
            UpdateReadCodeStatus();
        }
        
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);

            try
            {
                SerialPort sp = (SerialPort)sender;
                string data = sp.ReadExisting();
                
                if (!string.IsNullOrEmpty(data))
                {
                    lastSerialPort1ReceiveTime = DateTime.Now;
                    
                    // 记录串口1数据接收日志
                    UpdateUnifiedLog("串口1", "接收", data);
                    
                    if (txtReceived1.InvokeRequired)
                    {
                        txtReceived1.Invoke(new Action<string>(UpdateReceivedText), "T面码：" + data);
                    }
                    else
                    {
                        UpdateReceivedText("T面码：" + data);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUnifiedLog("串口1", "错误", string.Format("数据接收错误: {0}", ex.Message));
                MessageBox.Show("串口数据接收错误：" + ex.Message);
            }
        }
        
        private void serialPort2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);

            try
            {
                SerialPort sp = (SerialPort)sender;
                string data = sp.ReadExisting();
                
                if (!string.IsNullOrEmpty(data))
                {
                    // 记录串口2数据接收日志
                    UpdateUnifiedLog("串口2", "接收", data);
                    
                    if (txtReceived1.InvokeRequired)
                    {
                        txtReceived1.Invoke(new Action<string>(UpdateReceivedText), "B面码：" + data);
                    }
                    else
                    {
                        UpdateReceivedText("B面码：" + data);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUnifiedLog("串口2", "错误", string.Format("数据接收错误: {0}", ex.Message));
                MessageBox.Show("串口数据接收错误：" + ex.Message);
            }
        }
        
        private void UpdateReceivedText(string text)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(firstLine))
                {
                    firstLine = text;
                    if (timeoutTimer == null)
                    {
                        timeoutTimer = new System.Threading.Timer(TimeoutCallback, null, 5000, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        timeoutTimer.Change(5000, System.Threading.Timeout.Infinite);
                    }
                }
                else if (string.IsNullOrEmpty(secondLine))
                {
                    if ((firstLine.StartsWith("T面码：") && text.StartsWith("T面码：")) ||
                        (firstLine.StartsWith("B面码：") && text.StartsWith("B面码：")))
                    {
                        firstLine = text;
                    }
                    else
                    {
                        secondLine = text;
                        if (timeoutTimer != null)
                        {
                            timeoutTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        }
                    }
                }
                else
                {
                    firstLine = null;
                    secondLine = null;
                    firstLine = text;
                    if (timeoutTimer == null)
                    {
                        timeoutTimer = new System.Threading.Timer(TimeoutCallback, null, 5000, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        timeoutTimer.Change(5000, System.Threading.Timeout.Infinite);
                    }
                }
                
                UpdateTextBox();
            }
        }
        
        private void UpdateTextBox()
        {
            if (txtReceived1.InvokeRequired)
            {
                txtReceived1.Invoke(new Action(UpdateTextBox));
            }
            else
            {
                txtReceived1.Clear();
                if (!string.IsNullOrEmpty(firstLine))
                {
                    txtReceived1.AppendText(firstLine + Environment.NewLine);
                }
                if (!string.IsNullOrEmpty(secondLine))
                {
                    txtReceived1.AppendText(secondLine + Environment.NewLine);
                }
                txtReceived1.ScrollToCaret();
                
                bool containsT = txtReceived1.Text.Contains("T面码");
                bool containsB = txtReceived1.Text.Contains("B面码");
                bool bothPresent = containsT && containsB;
                bool hasData = containsT || containsB;
                bool hasNoRead = txtReceived1.Text.Contains("NOREAD");
                
                bool shouldPush = false;
                if (cb1_dm.Checked)
                {
                    shouldPush = hasData && !hasNoRead;
                }
                else
                {
                    shouldPush = bothPresent && !hasNoRead;
                }
                
                if (shouldPush)
                {
                    panel4.BackColor = System.Drawing.Color.Green;
                    if (panel4_resetTimer != null)
                    {
                        panel4_resetTimer.Dispose();
                    }
                    panel4_resetTimer = new System.Threading.Timer((state) =>
                    {
                        if (panel4.InvokeRequired)
                        {
                            panel4.Invoke(new Action(() => panel4.BackColor = System.Drawing.Color.Gray));
                        }
                        else
                        {
                            panel4.BackColor = System.Drawing.Color.Gray;
                        }
                    }, null, 5000, System.Threading.Timeout.Infinite);
                }
                else
                {
                    panel4.BackColor = System.Drawing.Color.Gray;
                }
                
                if (shouldPush)
                {
                    string tCode = "";
                    string bCode = "";

                    string[] lines = txtReceived1.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (line.Contains("T面码："))
                        {
                            tCode = line.Substring(line.IndexOf("T面码：") + 4);
                        }
                        else if (line.Contains("B面码："))
                        {
                            bCode = line.Substring(line.IndexOf("B面码：") + 4);
                        }
                    }

                    string tCodeTrimmed = string.IsNullOrEmpty(tCode.Trim()) ? " " : tCode.Trim();
                    string bCodeTrimmed = string.IsNullOrEmpty(bCode.Trim()) ? " " : bCode.Trim();
                    string combinedCode = tCodeTrimmed + "#@#" + bCodeTrimmed;
                    txtBarcode.Text = combinedCode;

                    if (cb1_dm.Checked && timeoutTimer != null)
                    {
                        timeoutTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    }

                    string[] tsjg = tsmes(1);
                    if (tsjg[0] == "OK")
                    {
                        bjy_mes1.BackColor = System.Drawing.Color.Green;
                        if (bjy_mes1_resetTimer != null)
                        {
                            bjy_mes1_resetTimer.Dispose();
                        }
                        bjy_mes1_resetTimer = new System.Threading.Timer((state) =>
                        {
                            if (bjy_mes1.InvokeRequired)
                            {
                                bjy_mes1.Invoke(new Action(() => bjy_mes1.BackColor = System.Drawing.Color.Gray));
                            }
                            else
                            {
                                bjy_mes1.BackColor = System.Drawing.Color.Gray;
                            }
                        }, null, 5000, System.Threading.Timeout.Infinite);
                    }
                    else if (tsjg[0] == "NG")
                    {
                        bjy_mes1.BackColor = System.Drawing.Color.Red;
                        if (bjy_mes1_resetTimer != null)
                        {
                            bjy_mes1_resetTimer.Dispose();
                        }
                        bjy_mes1_resetTimer = new System.Threading.Timer((state) =>
                        {
                            if (bjy_mes1.InvokeRequired)
                            {
                                bjy_mes1.Invoke(new Action(() => bjy_mes1.BackColor = System.Drawing.Color.Gray));
                            }
                            else
                            {
                                bjy_mes1.BackColor = System.Drawing.Color.Gray;
                            }
                        }, null, 5000, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        bjy_mes1.BackColor = System.Drawing.Color.Gray;
                    }

                    if (cb1_dm.Checked)
                    {
                        firstLine = null;
                        secondLine = null;
                    }
                }
            }
        }
        
        private void TimeoutCallback(object state)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(firstLine) && string.IsNullOrEmpty(secondLine))
                {
                    firstLine = null;
                    secondLine = null;
                    UpdateTextBox();
                }
            }
        }
        
        private void UpdateReceivedText2(string text)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(firstLine2))
                {
                    firstLine2 = text;
                    if (timeoutTimer2 == null)
                    {
                        timeoutTimer2 = new System.Threading.Timer(TimeoutCallback2, null, 5000, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        timeoutTimer2.Change(5000, System.Threading.Timeout.Infinite);
                    }
                }
                else if (string.IsNullOrEmpty(secondLine2))
                {
                    if ((firstLine2.StartsWith("T面码：") && text.StartsWith("T面码：")) ||
                        (firstLine2.StartsWith("B面码：") && text.StartsWith("B面码：")))
                    {
                        firstLine2 = text;
                    }
                    else
                    {
                        secondLine2 = text;
                        if (timeoutTimer2 != null)
                        {
                            timeoutTimer2.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        }
                    }
                }
                else
                {
                    firstLine2 = null;
                    secondLine2 = null;
                    firstLine2 = text;
                    if (timeoutTimer2 == null)
                    {
                        timeoutTimer2 = new System.Threading.Timer(TimeoutCallback2, null, 5000, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        timeoutTimer2.Change(5000, System.Threading.Timeout.Infinite);
                    }
                }
                
                UpdateTextBox2();
            }
        }
        
        private void UpdateTextBox2()
        {
            if (txtReceived2.InvokeRequired)
            {
                txtReceived2.Invoke(new Action(UpdateTextBox2));
            }
            else
            {
                txtReceived2.Clear();
                if (!string.IsNullOrEmpty(firstLine2))
                {
                    txtReceived2.AppendText(firstLine2 + Environment.NewLine);
                }
                if (!string.IsNullOrEmpty(secondLine2))
                {
                    txtReceived2.AppendText(secondLine2 + Environment.NewLine);
                }
                txtReceived2.ScrollToCaret();
                
                bool containsT = txtReceived2.Text.Contains("T面码");
                bool containsB = txtReceived2.Text.Contains("B面码");
                bool bothPresent = containsT && containsB;
                bool hasData = containsT || containsB;
                bool hasNoRead = txtReceived2.Text.Contains("NOREAD");
                
                bool shouldPush = false;
                if (cb2_dm.Checked)
                {
                    shouldPush = hasData && !hasNoRead;
                }
                else
                {
                    shouldPush = bothPresent && !hasNoRead;
                }
                
                if (shouldPush)
                {
                    panel9.BackColor = System.Drawing.Color.Green;
                    if (panel9_resetTimer != null)
                    {
                        panel9_resetTimer.Dispose();
                    }
                    panel9_resetTimer = new System.Threading.Timer((state) =>
                    {
                        if (panel9.InvokeRequired)
                        {
                            panel9.Invoke(new Action(() => panel9.BackColor = System.Drawing.Color.Gray));
                        }
                        else
                        {
                            panel9.BackColor = System.Drawing.Color.Gray;
                        }
                    }, null, 5000, System.Threading.Timeout.Infinite);
                }
                else
                {
                    panel9.BackColor = System.Drawing.Color.Gray;
                }
                
                if (shouldPush)
                {
                    string tCode = "";
                    string bCode = "";
                    
                    string[] lines = txtReceived2.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (line.Contains("T面码："))
                        {
                            tCode = line.Substring(line.IndexOf("T面码：") + 4);
                        }
                        else if (line.Contains("B面码："))
                        {
                            bCode = line.Substring(line.IndexOf("B面码：") + 4);
                        }
                    }
                    
                    string tCodeTrimmed = string.IsNullOrEmpty(tCode.Trim()) ? " " : tCode.Trim();
                    string bCodeTrimmed = string.IsNullOrEmpty(bCode.Trim()) ? " " : bCode.Trim();
                    string combinedCode = tCodeTrimmed + "#@#" + bCodeTrimmed;
                    txtBarcode2.Text = combinedCode;

                    if (cb2_dm.Checked && timeoutTimer2 != null)
                    {
                        timeoutTimer2.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    }

                    string[] tsjg = tsmes(2);
                    if (tsjg[0] == "OK")
                    {
                        bjy_mes2.BackColor = System.Drawing.Color.Green;
                        if (bjy_mes2_resetTimer != null)
                        {
                            bjy_mes2_resetTimer.Dispose();
                        }
                        bjy_mes2_resetTimer = new System.Threading.Timer((state) =>
                        {
                            if (bjy_mes2.InvokeRequired)
                            {
                                bjy_mes2.Invoke(new Action(() => bjy_mes2.BackColor = System.Drawing.Color.Gray));
                            }
                            else
                            {
                                bjy_mes2.BackColor = System.Drawing.Color.Gray;
                            }
                        }, null, 5000, System.Threading.Timeout.Infinite);
                    }
                    else if (tsjg[0] == "NG")
                    {
                        bjy_mes2.BackColor = System.Drawing.Color.Red;
                        if (bjy_mes2_resetTimer != null)
                        {
                            bjy_mes2_resetTimer.Dispose();
                        }
                        bjy_mes2_resetTimer = new System.Threading.Timer((state) =>
                        {
                            if (bjy_mes2.InvokeRequired)
                            {
                                bjy_mes2.Invoke(new Action(() => bjy_mes2.BackColor = System.Drawing.Color.Gray));
                            }
                            else
                            {
                                bjy_mes2.BackColor = System.Drawing.Color.Gray;
                            }
                        }, null, 5000, System.Threading.Timeout.Infinite);
                    }
                    else
                    {
                        bjy_mes2.BackColor = System.Drawing.Color.Gray;
                    }

                    if (cb2_dm.Checked)
                    {
                        firstLine2 = null;
                        secondLine2 = null;
                    }
                }
            }
        }
        
        private void TimeoutCallback2(object state)
        {
            lock (this)
            {
                if (!string.IsNullOrEmpty(firstLine2) && string.IsNullOrEmpty(secondLine2))
                {
                    firstLine2 = null;
                    secondLine2 = null;
                    UpdateTextBox2();
                }
            }
        }
        
        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            System.Windows.Forms.Panel panel = sender as System.Windows.Forms.Panel;
            if (panel != null)
            {
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, panel.Width, panel.Height);
                    panel.Region = new System.Drawing.Region(path);
                }
            }
        }
        
        private void panel9_Paint(object sender, PaintEventArgs e)
        {
            System.Windows.Forms.Panel panel = sender as System.Windows.Forms.Panel;
            if (panel != null)
            {
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, panel.Width, panel.Height);
                    panel.Region = new System.Drawing.Region(path);
                }
            }
        }
        
        private void bjy_mes1_Paint(object sender, PaintEventArgs e)
        {
            System.Windows.Forms.Panel panel = sender as System.Windows.Forms.Panel;
            if (panel != null)
            {
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, panel.Width, panel.Height);
                    panel.Region = new System.Drawing.Region(path);
                }
            }
        }
        
        private void bjy_mes2_Paint(object sender, PaintEventArgs e)
        {
            System.Windows.Forms.Panel panel = sender as System.Windows.Forms.Panel;
            if (panel != null)
            {
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, panel.Width, panel.Height);
                    panel.Region = new System.Drawing.Region(path);
                }
            }
        }
        
        private void serialPort3_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                string data = sp.ReadExisting();
                
                if (!string.IsNullOrEmpty(data))
                {
                    lastSerialPort3ReceiveTime = DateTime.Now;
                    
                    // 记录串口3数据接收日志
                    UpdateUnifiedLog("串口3", "接收", data);
                    
                    if (txtReceived2.InvokeRequired)
                    {
                        txtReceived2.Invoke(new Action<string>(UpdateReceivedText2), "T面码：" + data);
                    }
                    else
                    {
                        UpdateReceivedText2("T面码：" + data);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUnifiedLog("串口3", "错误", string.Format("数据接收错误: {0}", ex.Message));
                MessageBox.Show("串口数据接收错误：" + ex.Message);
            }
        }
        
        private void serialPort4_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                string data = sp.ReadExisting();
                
                if (!string.IsNullOrEmpty(data))
                {
                    // 记录串口4数据接收日志
                    UpdateUnifiedLog("串口4", "接收", data);
                    
                    if (txtReceived2.InvokeRequired)
                    {
                        txtReceived2.Invoke(new Action<string>(UpdateReceivedText2), "B面码：" + data);
                    }
                    else
                    {
                        UpdateReceivedText2("B面码：" + data);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUnifiedLog("串口4", "错误", string.Format("数据接收错误: {0}", ex.Message));
                MessageBox.Show("串口数据接收错误：" + ex.Message);
            }
        }
        

        

        
        private void radioButton_serial1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_serial1.Checked)
            {
                panel_1.Visible = true;
                panel_2.Visible = false;
            }
        }

        private void radioButton_tcp1_CheckedChanged(object sender, EventArgs e)
        {
             if (radioButton_tcp1.Checked)
            {
                panel_1.Visible = false;
                panel_2.Visible = true;
            }
        }

        private void radioButton_serial2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_serial2.Checked)
            {
                panel_3.Visible = true;
                panel_4.Visible = false;
            }
        }

        private void radioButton_tcp2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_tcp2.Checked)
            {
                panel_3.Visible = false;
                panel_4.Visible = true;
            }
        }

        private void radioButton_serial3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_serial3.Checked)
            {
                panel1_5.Visible = true;
                panel1_6.Visible = false;
            }
        }

        private void radioButton_tcp3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_tcp3.Checked)
            {
                panel1_5.Visible = false;
                panel1_6.Visible = true;
            }
        }

        private void radioButton_serial4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_serial4.Checked)
            {
                panel_7.Visible = true;
                panel1_8.Visible = false;
            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                panel_7.Visible = false;
                panel1_8.Visible = true;
            }
        }
        
        private void btn_tcp1_start_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioButton_client1.Checked)
                {
                    if (tcpClient1.Connected)
                    {
                        tcpClient1.Close();
                        btn_tcp1_start.Text = "启动TCP";
                        UpdateUnifiedLog("TCP客户端1", "状态", "关闭成功");
                    }
                    else
                    {
                        string ipAddress = tb_tcp_ip1.Text;
                        int port = int.Parse(tb_tcp_port1.Text);
                        
                        tcpClient1.Connect(ipAddress, port);
                        btn_tcp1_start.Text = "关闭TCP";
                        UpdateUnifiedLog("TCP客户端1", "状态", string.Format("启动成功: {0}:{1}", ipAddress, port));
                        
                        System.Threading.Thread receiveThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(HandleClientReceive1));
                        receiveThread.IsBackground = true;
                        receiveThread.Start(tcpClient1);
                    }
                }
                else if (radioButton_server1.Checked)
                {
                    if (tcpListener1 != null && tcpListener1.Server != null && tcpListener1.Server.IsBound)
                    {
                        tcpListener1.Stop();
                        btn_tcp1_start.Text = "启动TCP";
                        UpdateUnifiedLog("TCP服务端1", "状态", "关闭成功");
                    }
                    else
                    {
                        string ipAddress = tb_tcp_ip1.Text;
                        int port = int.Parse(tb_tcp_port1.Text);
                        
                        tcpListener1 = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse(ipAddress), port);
                        tcpListener1.Start();
                        btn_tcp1_start.Text = "关闭TCP";
                        UpdateUnifiedLog("TCP服务端1", "状态", string.Format("启动成功: {0}:{1}", ipAddress, port));
                        
                        System.Threading.Thread listenThread = new System.Threading.Thread(new System.Threading.ThreadStart(ListenForClients));
                        listenThread.IsBackground = true;
                        listenThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUnifiedLog("TCP1", "错误", string.Format("操作失败: {0}", ex.Message));
                MessageBox.Show("TCP通讯错误：" + ex.Message);
            }
            UpdateReadCodeStatus();
        }
        
        private void ListenForClients()
        {
            while (tcpListener1 != null && tcpListener1.Server != null && tcpListener1.Server.IsBound)
            {
                try
                {
                    System.Net.Sockets.TcpClient client = tcpListener1.AcceptTcpClient();
                    System.Threading.Thread clientThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(HandleClientComm));
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch
                {
                    break;
                }
            }
        }
        
        private void HandleClientComm(object clientObj)
        {
            System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)clientObj;
            System.Net.Sockets.NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    string data = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    
                    // 记录TCP服务端1数据接收日志
                    UpdateUnifiedLog("TCP服务端1", "接收", data);
                    
                    string formattedData = "T面码：" + data;
                    
                    if (txtReceived1.InvokeRequired)
                    {
                        txtReceived1.Invoke(new Action<string>(UpdateReceivedText), formattedData);
                    }
                    else
                    {
                        UpdateReceivedText(formattedData);
                    }
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("TCP服务端1", "错误", string.Format("数据接收错误: {0}", ex.Message));
                    break;
                }
            }
            
            client.Close();
        }
        
        private void HandleClientReceive1(object clientObj)
        {
            System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)clientObj;
            System.Net.Sockets.NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    string data = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    
                    // 记录TCP客户端1数据接收日志
                    UpdateUnifiedLog("TCP客户端1", "接收", data);
                    
                    string formattedData = "T面码：" + data;
                    
                    if (txtReceived1.InvokeRequired)
                    {
                        txtReceived1.Invoke(new Action<string>(UpdateReceivedText), formattedData);
                    }
                    else
                    {
                        UpdateReceivedText(formattedData);
                    }
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("TCP客户端1", "错误", string.Format("数据接收错误: {0}", ex.Message));
                    break;
                }
            }
        }
        
        private void btn_tcp2_start_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioButton_client2.Checked)
                {
                    if (tcpClient2.Connected)
                    {
                        tcpClient2.Close();
                        btn_tcp2_start.Text = "启动TCP";
                        UpdateUnifiedLog("TCP客户端2", "状态", "关闭成功");
                    }
                    else
                    {
                        string ipAddress = tb_tcp_ip2.Text;
                        int port = int.Parse(tb_tcp_port2.Text);
                        
                        tcpClient2.Connect(ipAddress, port);
                        btn_tcp2_start.Text = "关闭TCP";
                        UpdateUnifiedLog("TCP客户端2", "状态", string.Format("启动成功: {0}:{1}", ipAddress, port));
                        
                        System.Threading.Thread receiveThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(HandleClientReceive2));
                        receiveThread.IsBackground = true;
                        receiveThread.Start(tcpClient2);
                    }
                }
                else if (radioButton_server2.Checked)
                {
                    if (tcpListener2 != null && tcpListener2.Server != null && tcpListener2.Server.IsBound)
                    {
                        tcpListener2.Stop();
                        btn_tcp2_start.Text = "启动TCP";
                        UpdateUnifiedLog("TCP服务端2", "状态", "关闭成功");
                    }
                    else
                    {
                        string ipAddress = tb_tcp_ip2.Text;
                        int port = int.Parse(tb_tcp_port2.Text);
                        
                        tcpListener2 = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse(ipAddress), port);
                        tcpListener2.Start();
                        btn_tcp2_start.Text = "关闭TCP";
                        UpdateUnifiedLog("TCP服务端2", "状态", string.Format("启动成功: {0}:{1}", ipAddress, port));
                        
                        System.Threading.Thread listenThread = new System.Threading.Thread(new System.Threading.ThreadStart(ListenForClients2));
                        listenThread.IsBackground = true;
                        listenThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUnifiedLog("TCP2", "错误", string.Format("操作失败: {0}", ex.Message));
                MessageBox.Show("TCP通讯错误：" + ex.Message);
            }
            UpdateReadCodeStatus();
        }
        
        private void ListenForClients2()
        {
            while (tcpListener2 != null && tcpListener2.Server != null && tcpListener2.Server.IsBound)
            {
                try
                {
                    System.Net.Sockets.TcpClient client = tcpListener2.AcceptTcpClient();
                    System.Threading.Thread clientThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(HandleClientComm2));
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch
                {
                    break;
                }
            }
        }
        
        private void HandleClientComm2(object clientObj)
        {
            System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)clientObj;
            System.Net.Sockets.NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    string data = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    
                    // 记录TCP服务端2数据接收日志
                    UpdateUnifiedLog("TCP服务端2", "接收", data);
                    
                    string formattedData = "B面码：" + data;
                    
                    if (txtReceived1.InvokeRequired)
                    {
                        txtReceived1.Invoke(new Action<string>(UpdateReceivedText), formattedData);
                    }
                    else
                    {
                        UpdateReceivedText(formattedData);
                    }
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("TCP服务端2", "错误", string.Format("数据接收错误: {0}", ex.Message));
                    break;
                }
            }
            
            client.Close();
        }
        
        private void HandleClientReceive2(object clientObj)
        {
            System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)clientObj;
            System.Net.Sockets.NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    string data = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    
                    // 记录TCP客户端2数据接收日志
                    UpdateUnifiedLog("TCP客户端2", "接收", data);
                    
                    string formattedData = "B面码：" + data;
                    
                    if (txtReceived1.InvokeRequired)
                    {
                        txtReceived1.Invoke(new Action<string>(UpdateReceivedText), formattedData);
                    }
                    else
                    {
                        UpdateReceivedText(formattedData);
                    }
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("TCP客户端2", "错误", string.Format("数据接收错误: {0}", ex.Message));
                    break;
                }
            }
        }
        
        private void UpdateReadCodeStatus()
        {
            bool serial1Open = serialPort1.IsOpen;
            bool serial2Open = serialPort2.IsOpen;
            bool tcp1Connected = tcpClient1.Connected || (tcpListener1 != null && tcpListener1.Server != null && tcpListener1.Server.IsBound);
            bool tcp2Connected = tcpClient2.Connected || (tcpListener2 != null && tcpListener2.Server != null && tcpListener2.Server.IsBound);
            
            bool serial3Open = serialPort3.IsOpen;
            bool serial4Open = serialPort4.IsOpen;
            bool tcp3Connected = tcpClient3.Connected || (tcpListener3 != null && tcpListener3.Server != null && tcpListener3.Server.IsBound);
            bool tcp4Connected = tcpClient4.Connected || (tcpListener4 != null && tcpListener4.Server != null && tcpListener4.Server.IsBound);
            
            bool serialBothOpen1 = serial1Open && serial2Open;
            bool tcpBothConnected1 = tcp1Connected && tcp2Connected;
            
            bool serialBothOpen2 = serial3Open && serial4Open;
            bool tcpBothConnected2 = tcp3Connected && tcp4Connected;
            
            if (serialBothOpen1 || tcpBothConnected1)
            {
                lb_dmjg1.Text = "读码已开启";
            }
            else
            {
                lb_dmjg1.Text = "读码未开启";
            }
            
            if (serialBothOpen2 || tcpBothConnected2)
            {
                lb_dmjg2.Text = "读码已开启";
            }
            else
            {
                lb_dmjg2.Text = "读码未开启";
            }
            
            bool track1InUse = serial1Open || serial2Open || tcp1Connected || tcp2Connected;
            bool track2InUse = serial3Open || serial4Open || tcp3Connected || tcp4Connected;
            
            cb1_dm.Enabled = !track1InUse;
            cb2_dm.Enabled = !track2InUse;
        }
        
        private void btn_tcp3_start_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioButton_client3.Checked)
                {
                    if (tcpClient3.Connected)
                    {
                        tcpClient3.Close();
                        btn_tcp3_start.Text = "启动TCP";
                        UpdateUnifiedLog("TCP客户端3", "状态", "关闭成功");
                    }
                    else
                    {
                        string ipAddress = tb_tcp_ip3.Text;
                        int port = int.Parse(tb_tcp_port3.Text);
                        
                        tcpClient3.Connect(ipAddress, port);
                        btn_tcp3_start.Text = "关闭TCP";
                        UpdateUnifiedLog("TCP客户端3", "状态", string.Format("启动成功: {0}:{1}", ipAddress, port));
                        
                        System.Threading.Thread receiveThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(HandleClientReceive3));
                        receiveThread.IsBackground = true;
                        receiveThread.Start(tcpClient3);
                    }
                }
                else if (radioButton_server3.Checked)
                {
                    if (tcpListener3 != null && tcpListener3.Server != null && tcpListener3.Server.IsBound)
                    {
                        tcpListener3.Stop();
                        btn_tcp3_start.Text = "启动TCP";
                        UpdateUnifiedLog("TCP服务端3", "状态", "关闭成功");
                    }
                    else
                    {
                        string ipAddress = tb_tcp_ip3.Text;
                        int port = int.Parse(tb_tcp_port3.Text);
                        
                        tcpListener3 = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse(ipAddress), port);
                        tcpListener3.Start();
                        btn_tcp3_start.Text = "关闭TCP";
                        UpdateUnifiedLog("TCP服务端3", "状态", string.Format("启动成功: {0}:{1}", ipAddress, port));
                        
                        System.Threading.Thread listenThread = new System.Threading.Thread(new System.Threading.ThreadStart(ListenForClients3));
                        listenThread.IsBackground = true;
                        listenThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUnifiedLog("TCP3", "错误", string.Format("操作失败: {0}", ex.Message));
                MessageBox.Show("TCP通讯错误：" + ex.Message);
            }
            UpdateReadCodeStatus();
        }
        
        private void ListenForClients3()
        {
            while (tcpListener3 != null && tcpListener3.Server != null && tcpListener3.Server.IsBound)
            {
                try
                {
                    System.Net.Sockets.TcpClient client = tcpListener3.AcceptTcpClient();
                    System.Threading.Thread clientThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(HandleClientComm3));
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch
                {
                    break;
                }
            }
        }
        
        private void HandleClientComm3(object clientObj)
        {
            System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)clientObj;
            System.Net.Sockets.NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    string data = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    
                    // 记录TCP服务端3数据接收日志
                    UpdateUnifiedLog("TCP服务端3", "接收", data);
                    
                    string formattedData = "T面码：" + data;
                    
                    if (txtReceived2.InvokeRequired)
                    {
                        txtReceived2.Invoke(new Action<string>(UpdateReceivedText2), formattedData);
                    }
                    else
                    {
                        UpdateReceivedText2(formattedData);
                    }
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("TCP服务端3", "错误", string.Format("数据接收错误: {0}", ex.Message));
                    break;
                }
            }
            
            client.Close();
        }
        
        private void btn_tcp4_start_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioButton_client4.Checked)
                {
                    if (tcpClient4.Connected)
                    {
                        tcpClient4.Close();
                        btn_tcp4_start.Text = "启动TCP";
                        UpdateUnifiedLog("TCP客户端4", "状态", "关闭成功");
                    }
                    else
                    {
                        string ipAddress = tb_tcp_ip4.Text;
                        int port = int.Parse(tb_tcp_port4.Text);
                        
                        tcpClient4.Connect(ipAddress, port);
                        btn_tcp4_start.Text = "关闭TCP";
                        UpdateUnifiedLog("TCP客户端4", "状态", string.Format("启动成功: {0}:{1}", ipAddress, port));
                        
                        System.Threading.Thread receiveThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(HandleClientReceive4));
                        receiveThread.IsBackground = true;
                        receiveThread.Start(tcpClient4);
                    }
                }
                else if (radioButton_server4.Checked)
                {
                    if (tcpListener4 != null && tcpListener4.Server != null && tcpListener4.Server.IsBound)
                    {
                        tcpListener4.Stop();
                        btn_tcp4_start.Text = "启动TCP";
                        UpdateUnifiedLog("TCP服务端4", "状态", "关闭成功");
                    }
                    else
                    {
                        string ipAddress = tb_tcp_ip4.Text;
                        int port = int.Parse(tb_tcp_port4.Text);
                        
                        tcpListener4 = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse(ipAddress), port);
                        tcpListener4.Start();
                        btn_tcp4_start.Text = "关闭TCP";
                        UpdateUnifiedLog("TCP服务端4", "状态", string.Format("启动成功: {0}:{1}", ipAddress, port));
                        
                        System.Threading.Thread listenThread = new System.Threading.Thread(new System.Threading.ThreadStart(ListenForClients4));
                        listenThread.IsBackground = true;
                        listenThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUnifiedLog("TCP4", "错误", string.Format("操作失败: {0}", ex.Message));
                MessageBox.Show("TCP通讯错误：" + ex.Message);
            }
            UpdateReadCodeStatus();
        }
        
        private void ListenForClients4()
        {
            while (tcpListener4 != null && tcpListener4.Server != null && tcpListener4.Server.IsBound)
            {
                try
                {
                    System.Net.Sockets.TcpClient client = tcpListener4.AcceptTcpClient();
                    System.Threading.Thread clientThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(HandleClientComm4));
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch
                {
                    break;
                }
            }
        }
        
        private void HandleClientComm4(object clientObj)
        {
            System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)clientObj;
            System.Net.Sockets.NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    string data = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    
                    // 记录TCP服务端4数据接收日志
                    UpdateUnifiedLog("TCP服务端4", "接收", data);
                    
                    string formattedData = "B面码：" + data;
                    
                    if (txtReceived2.InvokeRequired)
                    {
                        txtReceived2.Invoke(new Action<string>(UpdateReceivedText2), formattedData);
                    }
                    else
                    {
                        UpdateReceivedText2(formattedData);
                    }
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("TCP服务端4", "错误", string.Format("数据接收错误: {0}", ex.Message));
                    break;
                }
            }
            
            client.Close();
        }
        
        private void HandleClientReceive3(object clientObj)
        {
            System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)clientObj;
            System.Net.Sockets.NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    string data = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    
                    // 记录TCP客户端3数据接收日志
                    UpdateUnifiedLog("TCP客户端3", "接收", data);
                    
                    string formattedData = "T面码：" + data;
                    
                    if (txtReceived2.InvokeRequired)
                    {
                        txtReceived2.Invoke(new Action<string>(UpdateReceivedText2), formattedData);
                    }
                    else
                    {
                        UpdateReceivedText2(formattedData);
                    }
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("TCP客户端3", "错误", string.Format("数据接收错误: {0}", ex.Message));
                    break;
                }
            }
        }
        
        private void HandleClientReceive4(object clientObj)
        {
            System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)clientObj;
            System.Net.Sockets.NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[1024];
            int bytesRead;
            
            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    
                    string data = System.Text.Encoding.Default.GetString(buffer, 0, bytesRead);
                    
                    // 记录TCP客户端4数据接收日志
                    UpdateUnifiedLog("TCP客户端4", "接收", data);
                    
                    string formattedData = "B面码：" + data;
                    
                    if (txtReceived2.InvokeRequired)
                    {
                        txtReceived2.Invoke(new Action<string>(UpdateReceivedText2), formattedData);
                    }
                    else
                    {
                        UpdateReceivedText2(formattedData);
                    }
                }
                catch (Exception ex)
                {
                    UpdateUnifiedLog("TCP客户端4", "错误", string.Format("数据接收错误: {0}", ex.Message));
                    break;
                }
            }
        }


        private string[] tsmes(int s)
        {
            // 创建取消标志
            bool isCancelled = false;
            System.Threading.Timer timeoutTimer = null;
            int meswt = 0;

            string[] jg = new string[2];
            try
            {
                string apiUrl = "";
                string employee = "";
                string barcode = "";
                string equipMac = "";

                int testResult = 1;

                if (s == 1)  //判断是第1轨还是第2轨调用
                {
                    // 获取用户输入的参数
                    apiUrl = txtApiUrl.Text;
                    employee = txtEmployee.Text;
                    equipMac = txtEquipMac.Text;
                    barcode = txtBarcode.Text;

                    // **********更新lb_tsmesjg1标签内容************************常规方法更新，会导致显示异常**************
                    if (!lb_tsmesjg1.IsHandleCreated)
                    {
                        lb_tsmesjg1.CreateControl();
                    }
                    // 确保在UI线程执行
                    if (lb_tsmesjg1.InvokeRequired)
                    {
                        lb_tsmesjg1.Invoke(new Action(() =>
                        {
                            lb_tsmesjg1.Text = "MES推送中";
                            lb_tsmesjg1.Refresh();  // 强制立即重绘
                            Application.DoEvents();   // 处理队列中的消息
                        }));
                    }
                    else
                    {
                        lb_tsmesjg1.Text = "MES推送中";
                        lb_tsmesjg1.Refresh();
                        Application.DoEvents();
                    }
                    // **********更新lb_tsmesjg1标签内容************************常规方法更新，会导致显示异常**************


                }
                else if (s == 2)
                {
                    apiUrl = txtApiUrl2.Text;
                    employee = txtEmployee2.Text;
                    equipMac = txtEquipMac2.Text;
                    barcode = txtBarcode2.Text;

                    // **********更新lb_tsmesjg1标签内容************************常规方法更新，会导致显示异常**************
                    if (!lb_tsmesjg2.IsHandleCreated)
                    {
                        lb_tsmesjg2.CreateControl();
                    }
                    // 确保在UI线程执行
                    if (lb_tsmesjg2.InvokeRequired)
                    {
                        lb_tsmesjg2.Invoke(new Action(() =>
                        {
                            lb_tsmesjg2.Text = "MES推送中";
                            lb_tsmesjg2.Refresh();  // 强制立即重绘
                            Application.DoEvents();   // 处理队列中的消息
                        }));
                    }
                    else
                    {
                        lb_tsmesjg2.Text = "MES推送中";
                        lb_tsmesjg2.Refresh();
                        Application.DoEvents();
                    }
                    // **********更新lb_tsmesjg1标签内容************************常规方法更新，会导致显示异常**************
                }



                // 构建请求数据
                var requestData = new Dictionary<string, object>
                    {
                        { "ApiType", "GsdMoveController" },
                        { "Parameters", new List<Dictionary<string, object>>
                            {
                                new Dictionary<string, object>
                                {
                                    { "Value", new Dictionary<string, object>
                                        {
                                            { "Employee", employee },
                                            { "Barcode", barcode },
                                            { "EquipMac", equipMac },
                                            { "TestResult", testResult }
                                        }
                                    }
                                }
                            }
                        },
                        { "Method", "CameraCastPlateTopBot" },
                        { "Context", new Dictionary<string, object>
                            {
                                { "InvOrgId", 1 }
                            }
                        }
                    };


                //// 根据条件修改
                //if (bt_sdsc != 0)
                //{
                //    requestData["Method"] = "CameraCastPlateTopBot";
                //    var valueDict = (Dictionary<string, object>)((List<Dictionary<string, object>>)requestData["Parameters"])[0]["Value"];
                //    //valueDict["Employee"] = null;
                //    //valueDict["Barcode"] = null;
                //    //valueDict["EquipMac"] = null;
                //    //valueDict["TestResult"] = 0;

                //    bt_sdsc = 0;  //复位
                //}



                // 序列化请求数据为JSON
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string jsonData = serializer.Serialize(requestData);

                // 显示发送的JSON数据
                //txtRequestJson.Text = jsonData;

                if (s == 1)  //判断是第1轨还是第2轨调用
                {
                    postdata_mes1.Text = "发送时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Environment.NewLine + jsonData;
                    meswt = Convert.ToInt32(mes_wt1.Text) * 1000;  //MES响应等待时间
                    WriteLog(1, "1轨推送MES内容：" + jsonData);
                }
                else if (s == 2)
                {
                    postdata_mes2.Text = "发送时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Environment.NewLine + jsonData;
                    meswt = Convert.ToInt32(mes_wt2.Text) * 1000;  //MES响应等待时间
                    WriteLog(2, "2轨推送MES内容：" + jsonData);
                }


                //UpdateUnifiedLog("网口", "发送", "JSON数据");



                // 使用WebClient发送请求
                using (WebClient client = new WebClient())
                {
                    // 设置请求头
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";


                    // 发送POST请求
                    //string responseContent = client.UploadString(apiUrl, "POST", jsonData);


                    //**************************这现超时退出发功能*****************************************************
                    //获取回应等待时间    
                    //meswt = Convert.ToInt32(mes_wt1.Text)*1000;
                    // 创建超时计时器##########################################################
                    timeoutTimer = new System.Threading.Timer(state =>
                    {
                        if (!isCancelled)
                        {
                            isCancelled = true;
                            client.CancelAsync(); // 取消请求
                        }
                    }, null, meswt, System.Threading.Timeout.Infinite);

                    client.Encoding = Encoding.UTF8;

                    // 发送POST请求
                    string responseContent = client.UploadString(apiUrl, "POST", jsonData);
                    // 如果已取消，不再处理响应##################################################
                    if (isCancelled) return jg;
                    //***********************************************************************************************


                    // 显示响应结果
                    //UpdateUnifiedLog("网口", "接收", "MES返回数据");

                    if (s == 1)  //判断是第1轨还是第2轨调用
                    {
                        txtResponse1.Text = "接收时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Environment.NewLine + responseContent;
                        var jsonObj = JsonConvert.DeserializeObject(responseContent);
                        string oneLineJson = JsonConvert.SerializeObject(jsonObj, Formatting.None);
                        WriteLog(1, "1轨MES返回内容：" + oneLineJson);
                    }
                    else if (s == 2)
                    {
                        txtResponse2.Text = "接收时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Environment.NewLine + responseContent;
                        var jsonObj = JsonConvert.DeserializeObject(responseContent);
                        string oneLineJson = JsonConvert.SerializeObject(jsonObj, Formatting.None);
                        WriteLog(2, "2轨MES返回内容：" + oneLineJson);
                    }

                    //测试时使用
                    string myjson = @"{
                                            ""Success"": true,
                                            ""Message"": null,
                                            ""Result"": {
                                                ""IsSuccess"": false,
                                                ""resultCode"": ""OK"",
                                                ""resultMsg"": """",
                                                ""Data"": null,
                                                ""DataString"": null
                                            },
                                            ""Context"": {
                                                ""Ticket"": null,
                                                ""InvOrgId"": null
                                            }
                                        }";
                    jg = ExtractResultValues(myjson);


                    //正式环境使用
                    //jg = ExtractResultValues(responseContent);

                    // 请求结束后恢复lb_tsmesjg1为"等待推送MES"
                    if (s == 1)
                    {
                        if (lb_tsmesjg1.InvokeRequired)
                        {
                            lb_tsmesjg1.Invoke(new Action(() => lb_tsmesjg1.Text = "等待推送MES"));
                        }
                        else
                        {
                            lb_tsmesjg1.Text = "等待推送MES";
                        }
                    }
                    // 请求结束后恢复lb_tsmesjg1为"等待推送MES"
                    if (s == 2)
                    {
                        if (lb_tsmesjg2.InvokeRequired)
                        {
                            lb_tsmesjg2.Invoke(new Action(() => lb_tsmesjg2.Text = "等待推送MES"));
                        }
                        else
                        {
                            lb_tsmesjg2.Text = "等待推送MES";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // 显示错误信息
                //txtResponse.Text = "错误: " + ex.Message;
                txtUnifiedLog.AppendText("错误: " + ex.Message);
                
                // 异常情况下也需要恢复lb_tsmesjg1为"等待推送MES"
                if (s == 1)
                {
                    if (lb_tsmesjg1.InvokeRequired)
                    {
                        lb_tsmesjg1.Invoke(new Action(() => lb_tsmesjg1.Text = "等待推送MES"));
                    }
                    else
                    {
                        lb_tsmesjg1.Text = "等待推送MES";
                    }
                }
                // 异常情况下也需要恢复lb_tsmesjg1为"等待推送MES"
                if (s == 2)
                {
                    if (lb_tsmesjg2.InvokeRequired)
                    {
                        lb_tsmesjg2.Invoke(new Action(() => lb_tsmesjg2.Text = "等待推送MES"));
                    }
                    else
                    {
                        lb_tsmesjg2.Text = "等待推送MES";
                    }
                }

            }

            return jg;
        }



        public static string[] ExtractResultValues(string jsonString)
        {
            try
            {
                JObject jsonObject = JObject.Parse(jsonString);

                string[] result = new string[2];

                if (jsonObject["Result"] != null)
                {
                    result[0] = jsonObject["Result"]["resultCode"] != null ? jsonObject["Result"]["resultCode"].ToString() : null;
                    result[1] = jsonObject["Result"]["resultMsg"] != null ? jsonObject["Result"]["resultMsg"].ToString() : null;
                }

                return result;
            }
            catch
            {
                return new string[] { null, null };
            }
        }




        private void btn_savemescs_Click(object sender, EventArgs e)
        {
            string tcptx1, tcpserver1, ip1, port1;
            string tcptx2, tcpserver2, ip2, port2;
            string tcptx3, tcpserver3, ip3, port3;
            string tcptx4, tcpserver4, ip4, port4;

            string com1sel, com2sel, com3sel, com4sel;

            string meswt1, meswt2;

            if (radioButton_serial1.Checked == true) tcptx1 = "N";
            else tcptx1 = "Y";
            if (radioButton_server1.Checked == true) tcpserver1 = "Y";
            else tcpserver1 = "N";


            if (radioButton_serial2.Checked == true) tcptx2 = "N";
            else tcptx2 = "Y";
            if (radioButton_server2.Checked == true) tcpserver2 = "Y";
            else tcpserver2 = "N";


            if (radioButton_serial3.Checked == true) tcptx3 = "N";
            else tcptx3 = "Y";
            if (radioButton_server3.Checked == true) tcpserver3 = "Y";
            else tcpserver3 = "N";


            if (radioButton_serial4.Checked == true) tcptx4 = "N";
            else tcptx4 = "Y";
            if (radioButton_server4.Checked == true) tcpserver4 = "Y";
            else tcpserver4 = "N";


            string dm1 = cb1_dm.Checked ? "Y" : "N";
            string dm2 = cb2_dm.Checked ? "Y" : "N";

            string udsql = "update  config set meswt1='" + mes_wt1.Text + "',meswt2='" + mes_wt2.Text + "',  " +
                           "  com1sel='" + comsel1.Text + "',com2sel='" + comsel2.Text + "',com3sel='" + comsel3.Text + "',com4sel='" + comsel4.Text + "'," +
                           "  apiurl='" + txtApiUrl.Text + "',apiurl2='" + txtApiUrl2.Text + "',username='" + txtEmployee.Text + "',username2='" + txtEmployee2.Text + "', " +
                           "  sbid='" + txtEquipMac.Text + "',sbid2='" + txtEquipMac2.Text + "', tcptx1='" + tcptx1 + "', tcptx2='" + tcptx2 + "' ,tcptx3='" + tcptx3 + "', tcptx4='" + tcptx4 + "', " +
                           "  tcpserver1='" + tcpserver1 + "',tcpserver2='" + tcpserver2 + "',tcpserver3='" + tcpserver3 + "',tcpserver4='" + tcpserver4 + "'," +
                           "  ip1='" + tb_tcp_ip1.Text + "', ip2='" + tb_tcp_ip2.Text + "',ip3='" + tb_tcp_ip3.Text + "',ip4='" + tb_tcp_ip4.Text + "', " +
                           "  port1='" + tb_tcp_port1.Text + "', port2='" + tb_tcp_port2.Text + "',port3='" + tb_tcp_port3.Text + "',port4='" + tb_tcp_port4.Text + "', " +
                           "  dm1='" + dm1 + "',dm2='" + dm2 + "' where id=1";

            MYSQL.execute(udsql);
            MessageBox.Show("读码模式和MES参数保存成功");


        }


        // 日志记录功能函数
        public void WriteLog(int number, string content)
        {
            try
            {
                // 获取当前日期，格式为yyyyMMdd
                string date = DateTime.Now.ToString("yyyyMMdd");
                // 构建文件名：年月日-传入数值
                string fileName = string.Format("{0}-{1}.txt", date, number);
                // 构建LOG文件夹路径，位于软件当前目录
                string logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LOG");
                // 构建完整的文件路径
                string filePath = Path.Combine(logFolderPath, fileName);

                // 检查LOG文件夹是否存在，不存在则创建
                if (!Directory.Exists(logFolderPath))
                {
                    Directory.CreateDirectory(logFolderPath);
                }
                else
                {
                    // 清理一个月前的日志文件
                    CleanupOldLogFiles(logFolderPath);
                }

                CleanupOldLogFiles(logFolderPath);

                // 获取当前系统时间，格式为yyyy-MM-dd HH:mm:ss.fff
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                // 构建日志内容：时间 + 内容
                string logEntry = string.Format("[{0}] {1}{2}", timestamp, content, Environment.NewLine);

                // 追加写入日志内容，不覆盖原有内容
                File.AppendAllText(filePath, logEntry, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // 记录错误信息到统一日志
                UpdateUnifiedLog("系统", "错误", string.Format("写入日志失败: {0}", ex.Message));
            }
        }

        // 清理一个月前的日志文件
        private void CleanupOldLogFiles(string logFolderPath)
        {
            try
            {
                // 获取一个月前的日期
                DateTime oneMonthAgo = DateTime.Now.AddMonths(-1);

                // 获取LOG文件夹下的所有txt文件
                string[] logFiles = Directory.GetFiles(logFolderPath, "*.txt");

                // 遍历所有日志文件
                foreach (string logFile in logFiles)
                {
                    // 获取文件名（不含路径）
                    string fileName = Path.GetFileName(logFile);

                    // 检查文件名格式是否符合要求（如：20260316-1.txt）
                    if (fileName.Length >= 10 && fileName.Substring(8, 1) == "-")
                    {
                        // 提取文件名中的日期部分（前8个字符）
                        string datePart = fileName.Substring(0, 8);

                        DateTime fileDate;
                        // 尝试解析日期
                        if (DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out fileDate))
                        {
                            // 检查文件日期是否超过一个月
                            if (fileDate < oneMonthAgo)
                            {
                                // 删除超过一个月的文件
                                File.Delete(logFile);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录错误信息到统一日志
                UpdateUnifiedLog("系统", "错误", string.Format("清理日志文件失败: {0}", ex.Message));
            }
        }


        // 统一日志更新方法
        private void UpdateUnifiedLog(string portName, string logType, string message)
        {
            if (txtUnifiedLog.InvokeRequired)
            {
                txtUnifiedLog.Invoke(new Action<string, string, string>(UpdateUnifiedLog), portName, logType, message);
            }
            else
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                //string logEntry = $"[{timestamp}] [{portName}] [{logType}] {message}{Environment.NewLine}";
                string logEntry = string.Format("[{0}] [{1}] [{2}] {3}{4}",
                                                timestamp,
                                                portName,
                                                logType,
                                                message,
                                                Environment.NewLine);
                txtUnifiedLog.AppendText(logEntry);
                txtUnifiedLog.ScrollToCaret(); // 自动滚动到最新日志
            }
        }

        private void btnManualUpload1_Click(object sender, EventArgs e)
        {
            bt_sdsc = 1; //手动上传标记
            string[] tsjg = tsmes(1);
            if (tsjg[0] == "OK")
            {
                bjy_mes1.BackColor = System.Drawing.Color.Green;
                if (bjy_mes1_resetTimer != null)
                {
                    bjy_mes1_resetTimer.Dispose();
                }
                bjy_mes1_resetTimer = new System.Threading.Timer((state) =>
                {
                    if (bjy_mes1.InvokeRequired)
                    {
                        bjy_mes1.Invoke(new Action(() => bjy_mes1.BackColor = System.Drawing.Color.Gray));
                    }
                    else
                    {
                        bjy_mes1.BackColor = System.Drawing.Color.Gray;
                    }
                }, null, 5000, System.Threading.Timeout.Infinite);
            }
            else if (tsjg[0] == "NG")
            {
                bjy_mes1.BackColor = System.Drawing.Color.Red;
                if (bjy_mes1_resetTimer != null)
                {
                    bjy_mes1_resetTimer.Dispose();
                }
                bjy_mes1_resetTimer = new System.Threading.Timer((state) =>
                {
                    if (bjy_mes1.InvokeRequired)
                    {
                        bjy_mes1.Invoke(new Action(() => bjy_mes1.BackColor = System.Drawing.Color.Gray));
                    }
                    else
                    {
                        bjy_mes1.BackColor = System.Drawing.Color.Gray;
                    }
                }, null, 5000, System.Threading.Timeout.Infinite);
            }
            else
            {
                bjy_mes1.BackColor = System.Drawing.Color.Gray;
            }
        }

        private void btnManualUpload2_Click(object sender, EventArgs e)
        {
            bt_sdsc = 1; //手动上传标记
            string[] tsjg = tsmes(2);
            if (tsjg[0] == "OK")
            {
                bjy_mes2.BackColor = System.Drawing.Color.Green;
                if (bjy_mes2_resetTimer != null)
                {
                    bjy_mes2_resetTimer.Dispose();
                }
                bjy_mes2_resetTimer = new System.Threading.Timer((state) =>
                {
                    if (bjy_mes2.InvokeRequired)
                    {
                        bjy_mes2.Invoke(new Action(() => bjy_mes2.BackColor = System.Drawing.Color.Gray));
                    }
                    else
                    {
                        bjy_mes2.BackColor = System.Drawing.Color.Gray;
                    }
                }, null, 5000, System.Threading.Timeout.Infinite);
            }
            else if (tsjg[0] == "NG")
            {
                bjy_mes2.BackColor = System.Drawing.Color.Red;
                if (bjy_mes2_resetTimer != null)
                {
                    bjy_mes2_resetTimer.Dispose();
                }
                bjy_mes2_resetTimer = new System.Threading.Timer((state) =>
                {
                    if (bjy_mes2.InvokeRequired)
                    {
                        bjy_mes2.Invoke(new Action(() => bjy_mes2.BackColor = System.Drawing.Color.Gray));
                    }
                    else
                    {
                        bjy_mes2.BackColor = System.Drawing.Color.Gray;
                    }
                }, null, 5000, System.Threading.Timeout.Infinite);
            }
            else
            {
                bjy_mes2.BackColor = System.Drawing.Color.Gray;
            }
        }

        private void cb1_dm_CheckedChanged(object sender, EventArgs e)
        {
            if (cb1_dm.Checked)
            {
                cb1_dm_isT = !cb1_dm_isT;
                SetTrack1FaceEnabled(cb1_dm_isT);
            }
            else
            {
                EnableAllTrack1Controls();
            }
        }

        private void cb2_dm_CheckedChanged(object sender, EventArgs e)
        {
            if (cb2_dm.Checked)
            {
                cb2_dm_isT = !cb2_dm_isT;
                SetTrack2FaceEnabled(cb2_dm_isT);
            }
            else
            {
                EnableAllTrack2Controls();
            }
        }

        private void EnableAllTrack1Controls()
        {
            communication_mode.Enabled = true;
            radioButton_serial1.Enabled = true;
            radioButton_tcp1.Enabled = true;
            groupBox1.Enabled = true;
            radioButton_serial2.Enabled = true;
            radioButton_tcp2.Enabled = true;
            panel_1.Enabled = true;
            panel_2.Enabled = true;
            comsel1.Enabled = true;
            opencom1.Enabled = true;
            btn_tcp1_start.Enabled = true;
            tb_tcp_ip1.Enabled = true;
            tb_tcp_port1.Enabled = true;
            radioButton_client1.Enabled = true;
            radioButton_server1.Enabled = true;
            panel_3.Enabled = true;
            panel_4.Enabled = true;
            comsel2.Enabled = true;
            opencom2.Enabled = true;
            btn_tcp2_start.Enabled = true;
            tb_tcp_ip2.Enabled = true;
            tb_tcp_port2.Enabled = true;
            radioButton_client2.Enabled = true;
            radioButton_server2.Enabled = true;
        }

        private void EnableAllTrack2Controls()
        {
            groupBox3.Enabled = true;
            radioButton_serial3.Enabled = true;
            radioButton_tcp3.Enabled = true;
            groupBox2.Enabled = true;
            radioButton_serial4.Enabled = true;
            radioButton6.Enabled = true;
            panel1_5.Enabled = true;
            panel1_6.Enabled = true;
            comsel3.Enabled = true;
            opencom3.Enabled = true;
            btn_tcp3_start.Enabled = true;
            tb_tcp_ip3.Enabled = true;
            tb_tcp_port3.Enabled = true;
            radioButton_client3.Enabled = true;
            radioButton_server3.Enabled = true;
            panel_7.Enabled = true;
            panel1_8.Enabled = true;
            comsel4.Enabled = true;
            opencom4.Enabled = true;
            btn_tcp4_start.Enabled = true;
            tb_tcp_ip4.Enabled = true;
            tb_tcp_port4.Enabled = true;
            radioButton_client4.Enabled = true;
            radioButton_server4.Enabled = true;
        }

        private void SetTrack1FaceEnabled(bool enableT)
        {
            bool enableB = !enableT;

            communication_mode.Enabled = enableT;
            radioButton_serial1.Enabled = enableT;
            radioButton_tcp1.Enabled = enableT;
            
            groupBox1.Enabled = enableB;
            radioButton_serial2.Enabled = enableB;
            radioButton_tcp2.Enabled = enableB;

            panel_1.Enabled = enableT;
            panel_2.Enabled = enableT;
            comsel1.Enabled = enableT;
            opencom1.Enabled = enableT;
            btn_tcp1_start.Enabled = enableT;
            tb_tcp_ip1.Enabled = enableT;
            tb_tcp_port1.Enabled = enableT;
            radioButton_client1.Enabled = enableT;
            radioButton_server1.Enabled = enableT;

            panel_3.Enabled = enableB;
            panel_4.Enabled = enableB;
            comsel2.Enabled = enableB;
            opencom2.Enabled = enableB;
            btn_tcp2_start.Enabled = enableB;
            tb_tcp_ip2.Enabled = enableB;
            tb_tcp_port2.Enabled = enableB;
            radioButton_client2.Enabled = enableB;
            radioButton_server2.Enabled = enableB;
        }

        private void SetTrack2FaceEnabled(bool enableT)
        {
            bool enableB = !enableT;

            groupBox3.Enabled = enableT;
            radioButton_serial3.Enabled = enableT;
            radioButton_tcp3.Enabled = enableT;

            groupBox2.Enabled = enableB;
            radioButton_serial4.Enabled = enableB;
            radioButton6.Enabled = enableB;

            panel1_5.Enabled = enableT;
            panel1_6.Enabled = enableT;
            comsel3.Enabled = enableT;
            opencom3.Enabled = enableT;
            btn_tcp3_start.Enabled = enableT;
            tb_tcp_ip3.Enabled = enableT;
            tb_tcp_port3.Enabled = enableT;
            radioButton_client3.Enabled = enableT;
            radioButton_server3.Enabled = enableT;

            panel_7.Enabled = enableB;
            panel1_8.Enabled = enableB;
            comsel4.Enabled = enableB;
            opencom4.Enabled = enableB;
            btn_tcp4_start.Enabled = enableB;
            tb_tcp_ip4.Enabled = enableB;
            tb_tcp_port4.Enabled = enableB;
            radioButton_client4.Enabled = enableB;
            radioButton_server4.Enabled = enableB;
        }

    }
}
