﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DTUGateWay
{
    public partial class FrmDownload : Form
    {
        public FrmDownload()
        {
            InitializeComponent();
            downloadWorker = new DownloadDataWorker();
            downloadWorker.valueChanged += downloadWorker_valueChanged;
            CheckForIllegalCrossThreadCalls = false;
        }

        void downloadWorker_valueChanged(object sender, ValueEventArgs e)
        {
            //throw new NotImplementedException();
            this.progressBar1.Value += e.Value;
        }

        private DownloadDataWorker downloadWorker;

        private void fileBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            if (fileTypeCb.SelectedIndex == 0)
            {
                op.Filter = "bin文件(*.bin)|*.bin";
            }
            else
            {
                op.Filter = "bin文件(*.bin)|*.bin";
            }
            if (op.ShowDialog() == DialogResult.OK)
            {
                this.fileTxt.Text = op.FileName;
            }
            else
            {
                this.fileTxt.Text = "";
            }
        }

        private int count = 0;
        private int index = 0;

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            if (isConnected == false)
            {
                MessageBox.Show("与服务器连接状态为失败，无法下发文件！");
                return;
            }
            string filePath = this.fileTxt.Text;
            if (filePath == null || filePath.Equals(""))
            {
                MessageBox.Show("没有选择程序文件！");
                return;
            }
            string ext = Path.GetExtension(filePath);
            if (!ext.Equals(".bin"))
            {
                MessageBox.Show("错误，程序文件格式不正确！");
                return;
            }
            FileInfo fileInfo = new FileInfo(filePath);
            this.progressBar1.Maximum = (int)fileInfo.Length;

            FileStream fs = new FileStream(filePath, FileMode.Open);
            sr = fs;
            readCount = 0;

            if ((fileInfo.Length % packetSize) == 0)
            {
                count = (int)fileInfo.Length / packetSize;
            }
            else
            {
                count = (int)fileInfo.Length / packetSize + 1;
            }
            downloadApp();
            this.totalFrameLabel.Text = count.ToString();
            this.downloadBtn.Enabled = false;
        }

        private void downloadApp()
        {
            if (sr == null)
            {
                return;
            }
            byte[] buffer = new byte[packetSize];
            int read = sr.Read(buffer, 0, packetSize);
            //MessageBox.Show("read====" + read);
            if (read > 0)
            {
                readCount += read;
                //还要更新进度条控件
                ValueEventArgs e = new ValueEventArgs();
                e.Value = read;
                downloadWorker.onValueChanged(e);
                 byte[] sendBuffer;
                //port.sendProtocol(sendBuf, sendBuf.Length);
                 if (isFirstSend == true)
                 {
                     sendBuffer = new byte[1 + packetSize];
                     sendBuffer[0] = fileType;
                     Array.Copy(buffer, 0, sendBuffer, 1, packetSize);

                 }
                 else
                 {
       //Debug.WriteLine("the read is \r\n" + read);
                     sendBuffer = new byte[read];
                     Array.Copy(buffer, 0, sendBuffer, 0, read);
                    // sendBuffer = buffer;
                 }
                 string DeviceNo = DeviceModule.GetFullDeviceNoByID(device.Id); 
                CmdToDtuSendFile cmd = new CmdToDtuSendFile();
                cmd.AddressField = DeviceNo.Substring(0, 12) + Convert.ToInt32(DeviceNo.Substring(12, 3)).ToString("X").PadLeft(2, '0');
                cmd.StationType = (byte)device.StationType;
                cmd.StationCode = device.StationType == 2 ? device.StationCode : 0;
                cmd.Sum = (short)count;
                cmd.Curr = (short)(index + 1);
                cmd.Content = sendBuffer;
                cmd.RawDataChar = cmd.WriteMsg();
                cmd.RawDataStr = HexStringUtility.ByteArrayToHexString(cmd.RawDataChar);

                byte[] cmd_send = cmd.RawDataChar;
                client.send(cmd_send, 0, cmd_send.Length);
                index++;
                this.timer1.Start();
                this.currentFrameLabel.Text = index.ToString();
            }
            else
            {
                readCount = 0;
                sr.Close();
                sr = null;

                MessageBox.Show("下载成功！");
                //将申请的资源释放掉
                this.fileTxt.Text = "";
                this.progressBar1.Value = 0;
                count = 0;
                index = 0;
                this.totalFrameLabel.Text = "0";
                this.currentFrameLabel.Text = "0";
                this.timer1.Stop();
                this.downloadBtn.Enabled = true;
            }

        }

        private void FrmDownload_Shown(object sender, EventArgs e)
        {

        }
    }
}
