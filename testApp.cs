using Modules;
using Newtonsoft.Json;
using RabbitMQ;
using RabbitMQforNETCore.Models;
using RabbitMQforNETCore.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RabbitMQforNETCore
{
    public partial class testApp : Form
    {
        private readonly SystemDefinitions queue = new SystemDefinitions();        // User / Queue / Echange tanımları
        private IRabbitMQProcess RabbitMQProcess;                       //Kullanılacak RabbitMQ yöntemi (Gateway)

        public string SenRec { get; set; }
        public string ExchangeType { get; set; }

        public testApp()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Thread / [TASK] / Form kontrollerine doğrudan erisime izin verir.
            Control.CheckForIllegalCrossThreadCalls = false;

            PrepareRabbitMQ();
            PrepareApplication();
        }
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            OperationResult op;
            queue.UserName = textUserName.Text;
            queue.Password = textPassword.Text;
            queue.HostName = textHostName.Text;
            queue.RoutingKey = textRoutingKey.Text;
            queue.Exchange = textExchange.Text;
            queue.QueueName = textQueueName.Text;

            op = RabbitMQProcess.Login(queue);

            cmbDirection.Visible = !op.Result;
            cmbProvider.Visible = !op.Result;
            btnConnect.Visible = !op.Result;

            ClearResultMessage();
            textResult.Text = JsonConvert.SerializeObject(op);

            if (cmbDirection.Text == "Consumer") // consumer ise kanalı dinlemeye hemen başlamalı
            {
                btnGetMessage.PerformClick();
            }
        }

        private void BtnSendMessage_Click(object sender, EventArgs e)
        {
            OperationResult op;

            Dictionary<string, object> header = new Dictionary<string, object>();
            if (cmbProvider.Text == "HeaderExchange")
            {
                var headList = textHeader.Text.Split(";");
                foreach (var item in headList)
                {
                    var keyValue = item.Split("-").ToList();
                    var key = keyValue.FirstOrDefault();
                    var value = keyValue.LastOrDefault();
                    header.Add(key, value);
                }
            }
            var sendModel = new SendModel
            {
                Message = textMessage.Text,
                Header = header,
            };

            op = RabbitMQProcess.Send(sendModel);

            ClearResultMessage();
            textResult.Text = JsonConvert.SerializeObject(op);
        }

        private void BtnGetMessage_Click(object sender, EventArgs e)
        {
            OperationResult op;

            //MessageCallBack Object;

            #region Dönen mesajı event ile alabildiğimiz gibi bu şekilde register olarakta alabiliriz.
            //Object.Object = this;
            //Object.MethodName = "MessageCallBack";

            //op = RabbitMQProcess.Register(Object);
            #endregion

            Dictionary<string, object> header = new Dictionary<string, object>();
            if (cmbProvider.SelectedIndex == (int)ExchangeDesc.HeaderExchange)
            {
                header.Add("x-match", "any");
                var headList = textHeader.Text.Split(";");
                foreach (var item in headList)
                {
                    var keyValue = item.Split("-").ToList();
                    var key = keyValue.FirstOrDefault();
                    var value = keyValue.LastOrDefault();
                    header.Add(key, value);
                }
            }

            op = RabbitMQProcess.Listen(header);

            ClearResultMessage();
            textResult.Text = JsonConvert.SerializeObject(op);
        }

        private void CmbProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetQueueInformation();
        }

        private void CmbDirection_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrepareApplication();
            SetQueueInformation();
        }

        private void PrepareRabbitMQ()
        {
            Dictionary<string, int> comboDirectionSource = new Dictionary<string, int>()
            {
                {Enum.GetName(typeof(SendRec),0), 0},
                {Enum.GetName(typeof(SendRec),1), 1}
            };
            cmbDirection.DataSource = new BindingSource(comboDirectionSource, null);
            cmbDirection.DisplayMember = "Key";
            cmbDirection.ValueMember = "Value";

            //QueueDeclare / DirectExchangePublisher / TopicExchangePublisher / HeaderExchangePublisher / FanoutExchangePublisher
            Dictionary<string, int> cmbProviderSource = new Dictionary<string, int>()
            {
                {Enum.GetName(typeof(ExchangeDesc),0), 0},
                {Enum.GetName(typeof(ExchangeDesc),1), 1},
                {Enum.GetName(typeof(ExchangeDesc),2), 2},
                {Enum.GetName(typeof(ExchangeDesc),3), 3},
                {Enum.GetName(typeof(ExchangeDesc),4), 4}
            };

            cmbProvider.DataSource = new BindingSource(cmbProviderSource, null);
            cmbProvider.DisplayMember = "Key";
            cmbProvider.ValueMember = "Value";

            cmbProvider.SelectedIndex = 0;
            cmbDirection.SelectedIndex = 0;
        }

        private void ClearResultMessage()
        {
            textResult.Text = "";
        }

        private void QD_MessageReceived(string Message)
        {
            try
            {
                textMessageArrived.Text = Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void MessageCallBack(string Message)
        {
            textMessageArrived.Text = Message;
        }

        private void SetQueueInformation()
        {
            bool status = true;

            switch (cmbProvider.SelectedIndex)
            {
                case (int)ExchangeDesc.QueueDeclare:
                    QueueDeclare qd = new QueueDeclare();
                    qd.MessageReceived += QD_MessageReceived;
                    RabbitMQProcess = new RabbitMQProcess(qd);

                    textQueueName.Text = "Queue-QueueDeclare";
                    textExchange.Text = "";
                    textHeader.Text = "";
                    textRoutingKey.Text = "";

                    break;
                case (int)ExchangeDesc.DirectExchange:
                    DirectExchange de = new DirectExchange();
                    de.MessageReceived += QD_MessageReceived;
                    RabbitMQProcess = new RabbitMQProcess(de);

                    textQueueName.Text = "Queue-DirectExchange";
                    textExchange.Text = "DirectExchangeQueue";
                    textHeader.Text = "";
                    textRoutingKey.Text = "";

                    break;
                case (int)ExchangeDesc.TopicExchange:
                    TopicExchange te = new TopicExchange();
                    te.MessageReceived += QD_MessageReceived;
                    RabbitMQProcess = new RabbitMQProcess(te);

                    textQueueName.Text = "Queue-TopicExchange";
                    textExchange.Text = "TopicExchangeQueue";
                    textHeader.Text = "";
                    textRoutingKey.Text = "";

                    break;
                case (int)ExchangeDesc.HeaderExchange:
                    HeaderExchange he = new HeaderExchange();
                    he.MessageReceived += QD_MessageReceived;
                    RabbitMQProcess = new RabbitMQProcess(he);

                    textQueueName.Text = "Queue-HeaderExchange";
                    textExchange.Text = "HeaderExchangeQueue";
                    textHeader.Text = "";
                    textRoutingKey.Text = "";

                    break;
                case (int)ExchangeDesc.FanoutExchange:
                    FanoutExchange fe = new FanoutExchange();
                    fe.MessageReceived += QD_MessageReceived;
                    RabbitMQProcess = new RabbitMQProcess(fe);

                    textQueueName.Text = "Queue-FanoutExchange";
                    textExchange.Text = "FanoutExchangeQueue";
                    textHeader.Text = "";
                    textRoutingKey.Text = "";

                    break;
            }

            textExchange.Enabled = status;
            textRoutingKey.Enabled = status;
            textHeader.Enabled = status;

            textQueueName.Enabled = !status;

            ClearResultMessage();
        }

        private void PrepareApplication()
        {
            bool status = true;
            textMessage.Text = "";
            textResult.Text = "";
            textMessageArrived.Text = "";

            btnConnect.Visible = status;
            textQueueName.Enabled = !status;

            switch (cmbDirection.SelectedIndex)
            {
                case (int)SendRec.Publisher:
                    btnGetMessage.Visible = !status;
                    btnSendMessage.Visible = status;
                    textMessageArrived.Visible = !status;
                    textMessage.Visible = status;
                    break;
                case (int)SendRec.Consumer:
                    btnGetMessage.Visible = status;
                    btnSendMessage.Visible = !status;
                    textMessage.Visible = !status;
                    textMessageArrived.Visible = status;
                    break;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            var op = RabbitMQProcess.Close();

            if (op.Result)
            {
                PrepareApplication();

                cmbDirection.Visible = op.Result;
                cmbProvider.Visible = op.Result;
                btnConnect.Visible = op.Result;
            }
        }
    }
}
