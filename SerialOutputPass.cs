﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArdupilotMega.Comms;
using System.Net.Sockets;
using System.Threading;

namespace ArdupilotMega
{
    public partial class SerialOutputPass : Form
    {
        static TcpListener listener;
        // Thread signal. 
        public static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        public SerialOutputPass()
        {
            InitializeComponent();

            CMB_serialport.Items.AddRange(SerialPort.GetPortNames());
            CMB_serialport.Items.Add("TCP Host");
            CMB_serialport.Items.Add("TCP Client");

            if (MainV2.comPort.MirrorStream != null && MainV2.comPort.MirrorStream.IsOpen || listener != null)
            {
                BUT_connect.Text = "Stop";
            }
        }

        private void BUT_connect_Click(object sender, EventArgs e)
        {
            if (MainV2.comPort.MirrorStream != null && MainV2.comPort.MirrorStream.IsOpen || listener != null)
            {
                MainV2.comPort.MirrorStream.Close();
                BUT_connect.Text = "Connect";
            }
            else
            {
                try
                {
                    switch (CMB_serialport.Text)
                    {
                        case "TCP Host":
                            MainV2.comPort.MirrorStream = new Comms.TcpSerial();
                            listener = new TcpListener(14550);
                            listener.Start(0);
                            listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), listener);
                            BUT_connect.Text = "Stop";
                            return;
                        case "TCP Client":
                            MainV2.comPort.MirrorStream = new Comms.TcpSerial();
                            break;
                        default:
                            MainV2.comPort.MirrorStream = new SerialPort();
                            MainV2.comPort.MirrorStream.PortName = CMB_serialport.Text;
                            break;
                    }
                }
                catch { CustomMessageBox.Show("Invalid PortName"); return; }
                try
                {
                    MainV2.comPort.MirrorStream.BaudRate = int.Parse(CMB_baudrate.Text);
                }
                catch { CustomMessageBox.Show("Invalid BaudRate"); return; }
                try
                {
                    MainV2.comPort.MirrorStream.Open();
                }
                catch { CustomMessageBox.Show("Error Connecting\nif using com0com please rename the ports to COM??"); return; }
            }
        }

        void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            // End the operation and display the received data on  
            // the console.
            TcpClient client = listener.EndAcceptTcpClient(ar);

            ((Comms.TcpSerial)MainV2.comPort.MirrorStream).client = client;

            listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), listener);
        }
    }
}