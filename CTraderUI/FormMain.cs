using System;
using System.Configuration;
using System.Windows.Forms;
namespace CTraderUI
{
    public partial class FormMain : Form
    {
        #region private members
        private bool isConnected;
        
        #endregion

        public FormMain()
        {
            isConnected = false;
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                Connect();
                isConnected = true;
                buttonConnect.Enabled = false;
            }
        }

        private void Connect()
        {
            string ApiKey = ConfigurationManager.AppSettings["APIKEY"];
            string ApiSecret = ConfigurationManager.AppSettings["APISECRET"];
            bool isLive = bool.Parse(ConfigurationManager.AppSettings["ISLIVE"]);

        }
    }
}
