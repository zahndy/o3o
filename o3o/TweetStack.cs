using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Twitterizer;
using Twitterizer.Core;
namespace o3o
{
    public class TweetStack
    {
        //vars
        OAuthTokens OAuth;
        public TweetStack(bool _LoadOAuth)
        {
            if (_LoadOAuth)
                OAuth = LoadOAuth();
        }

        public OAuthTokens LoadOAuth()
        {
            OAuthTokens privOAuth = new OAuthTokens();
            if (Properties.Settings.Default.OAuth_AccessToken == "")
                throw new Exception("Accesstoken empty! Are there any saved credentials at all? You need to reauthenticate!");
            privOAuth.AccessToken = Properties.Settings.Default.OAuth_AccessToken;
            privOAuth.AccessTokenSecret = Properties.Settings.Default.OAuth_AccessTokenSecret;
            privOAuth.ConsumerKey = Properties.Settings.Default.OAuth_ConsumerKey;
            privOAuth.ConsumerSecret = Properties.Settings.Default.OAuth_ConsumerSecret;
            return privOAuth;
        }

        public OAuthTokens AuthenticateTwitter()
        {

        }
    }
}
