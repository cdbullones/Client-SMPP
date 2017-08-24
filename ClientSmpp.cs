using JamaaTech.Smpp.Net.Client;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;

namespace ClientSmpp
{
    public partial class ClientSmpp : ServiceBase
    {

        //Connection SMPP
        SmppClient client = new SmppClient();
        
		//Message SMPP
        TextMessage msg = new TextMessage();

        //timer
        private Timer timer1 = null;
        int count = 0;
        int attempts = 0;

        //Parameters Message
        string numOriginating = "090017288";
        string numReceipient = "+58 416 6426522";
        string textMessage = "Hello Wold!!!";

        public ClientSmpp()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Library.WriteErrorLog("Service is Started");

            try
            {
                //Parameters
                SmppConnectionProperties properties = client.Properties;
                
                //Properties SMPP
                properties.SystemID = ConfigurationManager.AppSettings["SystemID"].ToString();
                properties.Password = ConfigurationManager.AppSettings["Password"].ToString();
                properties.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]); //IP port to use
                properties.Host = ConfigurationManager.AppSettings["Host"].ToString(); //SMSC host name or IP Address
                properties.SystemType = ConfigurationManager.AppSettings["SystemType"].ToString();
                properties.DefaultServiceType = ConfigurationManager.AppSettings["DefaultServiceType"].ToString();

                //Resume a lost connection after 30 seconds
                client.AutoReconnectDelay = 3000;

                //Send Enquire Link PDU every 15 seconds
                client.KeepAliveInterval = 15000;

                //Start smpp client
                client.Start();

                //Test Changes in conection
                client.ConnectionStateChanged += client_ConnectionStateChanged;

                //Receiving messages
                client.MessageReceived += client_MessageReceived;

            }
            catch (Exception ex)
            {
                Library.WriteErrorLog(ex);
            }
        }

        protected override void OnStop()
        {
            client.Shutdown();
            timer1.Enabled = false;
            Library.WriteErrorLog("Service Closed");
        }


        //Changes in conection
        private void client_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            switch (e.CurrentState)
            {
                case SmppConnectionState.Closed:
                    //Connection to the remote server is lost
                    //Do something here
                    Library.WriteErrorLog("Connection to the remote server is lost");
                    e.ReconnectInteval = 60000; //Try to reconnect after 1 min
                    attempts = attempts + 1;
                    if (attempts >= 3)
                    {
                        client.Shutdown();
                        timer1.Enabled = false;
                        Library.WriteErrorLog("Service Stopped");
                    }
                    break;
                case SmppConnectionState.Connected:
                    //A successful connection has been established
                    Library.WriteErrorLog("Connection SMPP Client (" + client.Properties.SystemID + ") Started");
                    timer1 = new Timer();
                    this.timer1.Interval = 20000; //20 segundos
                    this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_connection);
                    timer1.Enabled = true;

                    //Sending Messages
                    msg.DestinationAddress = numReceipient; //Receipient number
                    msg.SourceAddress = numOriginating; //Originating number
                    msg.Text = textMessage;
                    msg.RegisterDeliveryNotification = true; //I want delivery notification for this message
                    
                    client.SendMessage(msg, 1000);
                    Library.WriteErrorLog("Message Sent Successfully! Receipient number:" + numReceipient + "Message:" + textMessage);

                    break;
                case SmppConnectionState.Connecting:
                    //A connection attemp is still on progress
                    Library.WriteErrorLog("A connection attemp is still on progress");
                    break;
            }
        }

        //Time Connection
        private void timer_connection(object sender, ElapsedEventArgs e)
        {
            //Write code here to do some job depends on your requirement
            string Message = "Time Connection Done Successfully : " + count + " Seconds";
            Library.WriteErrorLog(Message);
            count = count + 20;
        }

        private void client_MessageReceived(object sender, MessageEventArgs e)
        {
            TextMessage msg = e.ShortMessage as TextMessage;
            Library.WriteErrorLog("Receiving messages");
            Library.WriteErrorLog(msg.Text); //Display message
        }

    }
}
