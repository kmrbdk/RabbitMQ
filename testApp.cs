using Modules;
using Newtonsoft.Json;
using RabbitMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabbitMQforNETCore
{
    public partial class testApp : Form
    {
        private readonly SystemDefinitions queue = new SystemDefinitions();        // User / Queue / Echange tanımları
        private IRabbitMQProcess RabbitMQProcess;                       //Kullanılacak RabbitMQ yöntemi (Gateway)

        public testApp()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Thread / [TASK] / Form kontrollerine doğrudan erisime izin verir.
            Control.CheckForIllegalCrossThreadCalls = false;

            PrepareApplication();
            PrepareRabbitMQ();            
        }
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            OperationResult op;// = new OperationResult();
            queue.HostName = textHostName.Text;
            queue.Uri= textHostName.Text;
            op = RabbitMQProcess.Login(queue); 

            cmbDirection.Visible = !op.Result;
            cmbProvider.Visible = !op.Result;
            btnConnect.Visible = !op.Result;

            ClearResultMessage();
            textResult.Text = JsonConvert.SerializeObject(op);
        }

        private void BtnSendMessage_Click(object sender, EventArgs e)
        {
            OperationResult op;// = new OperationResult();
            op = RabbitMQProcess.Send(textMessage.Text);

            ClearResultMessage();
            textResult.Text = JsonConvert.SerializeObject(op);
        }

        private void BtnGetMessage_Click(object sender, EventArgs e)
        {
            OperationResult op;// = new OperationResult();

            MessageCallBack Object;

            #region Dönen mesajı event ile alabildiğimiz gibi bu şekilde register olarakta alabiliriz.
            //Object.Object = this;
            //Object.MethodName = "MessageCallBack";

            //op = RabbitMQProcess.Register(Object);
            #endregion

            op = RabbitMQProcess.Listen();

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
                {"Publisher", 0},
                {"Consumer", 1}
            };
            cmbDirection.DataSource = new BindingSource(comboDirectionSource, null);
            cmbDirection.DisplayMember = "Key";
            cmbDirection.ValueMember = "Value";

            //QueueDeclare / DirectExchangePublisher / TopicExchangePublisher / HeaderExchangePublisher / FanoutExchangePublisher
            Dictionary<string, int> cmbProviderSource = new Dictionary<string, int>()
            {
                {"QueueDeclare", 0},
                {"DirectExchangePublisher", 1},
                {"TopicExchangePublisher", 2},
                {"HeaderExchangePublisher", 3},
                {"FanoutExchangePublisher", 4}
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
            string PublishModel = cmbProvider.Text;
            bool status = true;

            queue.UserName = textUserName.Text;
            queue.Password = textPassword.Text;
            queue.HostName = textHostName.Text;
            queue.Exchange = textExchange.Text;
            queue.QueueName = textQueueName.Text;
            queue.RoutingKey = textRoutingKey.Text;
            queue.Uri = "";

            textExchange.Enabled = status;
            textRoutingKey.Enabled = status;

            textQueueName.Enabled = !status;

            switch (PublishModel)
            {
                case "QueueDeclare":
                    queue.QueueName = textQueueName.Text;
                    textExchange.Enabled = !status;
                    textRoutingKey.Enabled = !status;
                    textQueueName.Enabled = status;

                    QueueDeclare qd = new QueueDeclare();
                    qd.MessageReceived += QD_MessageReceived; //Mesajları Register ile alabildiğimiz gibi Event i de tanımşayabiiriz.

                    RabbitMQProcess = new RabbitMQProcess(qd);
                    break;
                    //case "My Sql":
                    //    msProcess = new MySqlProcess();
                    //            queue.Exchange = textExchange.Text;
                    //queue.RoutingKey = textRoutingKey.Text;
                    //    break;
                    //case "Mongo":
                    //    msProcess = new MongoProcess();
                    //    break;
                    //default:
                    //    msProcess = new MsSqlProcess();
                    //    break;
            }

            queue.Durable = true;
            queue.Exclusive = false;
            queue.AutoDelete = false;
            queue.Arguments = null;

            ClearResultMessage();
        }

        private void PrepareApplication()
        {
            bool status = true;
            textMessage.Text = "Hello RabbitMQ";
            textResult.Text = "";
            textMessageArrived.Text = "";

            btnConnect.Visible = status;                        
            textMessageArrived.Text = "";
            btnGetMessage.Visible = status;
            btnSendMessage.Visible = status;

            lblMessage.Visible = status;
            textMessage.Visible = status;

            //lblMessageArrived.Visible = status;
            textMessageArrived.Visible = status;

            textExchange.Enabled = status;
            textRoutingKey.Enabled = status;

            string PublishModel = cmbDirection.Text;
            switch (PublishModel)
            {
                case "Publisher":
                    textQueueName.Enabled = status;
                    btnGetMessage.Visible = !status;
                    //lblMessageArrived.Visible = !status;
                    textMessageArrived.Visible = !status;
                    break;
                case "Consumer":                    
                    btnSendMessage.Visible = !status;
                    //lblMessage.Visible = !status;
                    textMessage.Visible = !status;
                    break;
            }

            textExchange.Enabled = status;
            textRoutingKey.Enabled = status;
        }
    }
}
