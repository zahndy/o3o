using System;
using System.Windows.Forms;

namespace o3o
{
    public partial class GetOAuthForm : Form
    {
        Twitterizer.OAuthTokenResponse otokenresp;
        string CONSUMERKEY;
        string CONSUMERSECRET;
        
        public GetOAuthForm(string _CONSUMERKEY, string _CONSUMERSECRET)
        {
            //Still have to make this work'n'stuff.
            InitializeComponent();
            otokenresp = Twitterizer.OAuthUtility.GetRequestToken(_CONSUMERKEY,_CONSUMERSECRET, "oob");
            
            CONSUMERKEY = _CONSUMERKEY;
            CONSUMERSECRET = _CONSUMERSECRET;
        }

        private void GetOAuthForm_Load(object sender, EventArgs e)
        {
            Browserform.Navigate("http://twitter.com/oauth/authorize?oauth_token=" + otokenresp.Token);
            Browserform.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(Browserform_DocumentCompleted);

        }

        void Browserform_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //maybe a BIT hacky, bit it seems to work pretty flawless so far.
            if (e.Url.ToString() == "https://twitter.com/oauth/authorize")
            {
                try
                {
                    string pin = Browserform.Document.GetElementById("oauth_pin").GetElementsByTagName("code")[0].InnerText;
                    Twitterizer.OAuthTokenResponse otokenrespverified = Twitterizer.OAuthUtility.GetAccessToken(CONSUMERKEY, CONSUMERSECRET, otokenresp.Token, pin);
                    privOAUTHSUCCESS = otokenrespverified;
                    MessageBox.Show("Authentication successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    success = true;
                    this.Close();
                }
                catch (Exception) { };
            }
        }
        public bool success = false;
        private Twitterizer.OAuthTokenResponse privOAUTHSUCCESS;
        public Twitterizer.OAuthTokenResponse OAuthTokenResponse { get { return privOAUTHSUCCESS; } }

    }
}
