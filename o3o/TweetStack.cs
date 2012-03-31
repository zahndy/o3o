using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Twitterizer;
using Twitterizer.Core;
namespace o3o
{

    //Quick tutorial:
    //Make a new instance of this class, like TweetStack o3o = new TweetStack(false);
    //Then run o3o.OAuth.AuthenticateTwitter() to authenticate the user for the first time (AND MAKE SURE TO RUN o3o.OAuth.SaveOAuth()!!!!!!!11oneone)
    //And run o3o.OAuth.LoadOAuth() for all future sessions
    //Then run o3o.Twitter.GetTimelineHome() to get all the tweets they'd normally see in their timeline, GetMentions() for the mentions,
    //And user o3o.Twitter.SendTweet("Something to tweet") to send tweets! (Max 140 chars, or it'll throw an error.)
    public class TweetStack
    {
        //vars
        public OAuthstuff OAuth;
        public TwitterInteraction Twitter;
        public Twitterizer.Streaming.TwitterStream Tweetstream;

        //SET THESE Or the program won't work.
        const string CONSUMER_KEY = "QfId2BP2FJB5v7Y0IUl4UA";
        const string CONSUMER_SECRET = "g8xCgUzfCVTnyfUaHeFdHgYHBNrE5fTmF9YAANlRM";
        //And yes I realize it's horrible to keep those in the sourcecode, but .NET can be decompiled regardless, so where the hell are we supposed to keep them?

        public TweetStack(bool _LoadOAuth, bool streaming = false)
        {
            OAuth = new OAuthstuff();
            Twitter = new TwitterInteraction(OAuth);
            OAuth.ConsumerKey = CONSUMER_KEY;
            OAuth.ConsumerSecret = CONSUMER_SECRET;
            if (_LoadOAuth)
                 OAuth.LoadOAuth();
            if (streaming && _LoadOAuth)
            {
                Twitterizer.Streaming.StreamOptions Streamopts = new Twitterizer.Streaming.StreamOptions();
                Streamopts.Count = 20;
                Tweetstream = new Twitterizer.Streaming.TwitterStream(OAuth.GetOAuthToken(), "o3o", Streamopts);
                Tweetstream.StartUserStream(
                    new Twitterizer.Streaming.InitUserStreamCallback(FriendsCallback),
                    new Twitterizer.Streaming.StreamStoppedCallback(StreamStoppedcallback),
                    new Twitterizer.Streaming.StatusCreatedCallback(StatuscreatedCallback), 
                    new Twitterizer.Streaming.StatusDeletedCallback(statusdeletedCallback), 
                    new Twitterizer.Streaming.DirectMessageCreatedCallback(DMcreatedCallback),
                    new Twitterizer.Streaming.DirectMessageDeletedCallback(DMDeletectCallback), 
                    new Twitterizer.Streaming.EventCallback(eventCallback));
            }
        }

        #region TwitterStreamStuff
        //Tweet stream stuff~!
        
        void FriendsCallback(Twitterizer.TwitterIdCollection input)
        {
            //Don't need this yet
        }

        void StreamStoppedcallback(Twitterizer.Streaming.StopReasons stopreason)
        {
            throw new Exception("Stream was stopped! Stop reason: " + stopreason.ToString());
        }

        public delegate void newtweetDel(TwitterStatus status);
        public event newtweetDel NewTweet;
        void StatuscreatedCallback(TwitterStatus status)
        {
            if (NewTweet != null)
                NewTweet(status);
        }

        public delegate void TweetDeletedDel(Twitterizer.Streaming.TwitterStreamDeletedEvent DeleteReason);
        public event TweetDeletedDel TweetDeleted;
        void statusdeletedCallback(Twitterizer.Streaming.TwitterStreamDeletedEvent deletedreason)
        {
            if(TweetDeleted != null)
                TweetDeleted(deletedreason);
        }

        public delegate void DMReceivedDel(TwitterDirectMessage DM);
        public event DMReceivedDel DMReceived;
        void DMcreatedCallback(TwitterDirectMessage incomingDM)
        {
            if(DMReceived != null)
                DMReceived(incomingDM);
        }

        void DMDeletectCallback(Twitterizer.Streaming.TwitterStreamDeletedEvent DeleteReason)
        {
            //Don't need this yet
        }

        void eventCallback(Twitterizer.Streaming.TwitterStreamEvent eventstuff)
        {
            //Don't need this yet
        }
        #endregion

        //This is basically just a barebone wrapper to keep things simple for you.
        public class TwitterInteraction
        {
            private OAuthstuff privOAuth;
            public TwitterInteraction(OAuthstuff _OAuth)
            {
                privOAuth = _OAuth;
            }

            public void SendTweet(string tweet)
            {
                if (tweet.Count() < 140)
                    Twitterizer.TwitterStatus.Update(privOAuth.GetOAuthToken(), tweet);
                else
                    throw new Exception("Status update too long! Make sure it's less than 140 characters!");
            }
            public Twitterizer.TwitterStatusCollection GetTimelineHome()
            {
                return Twitterizer.TwitterTimeline.UserTimeline(privOAuth.GetOAuthToken()).ResponseObject;
            }
            public Twitterizer.TwitterStatusCollection GetMentions()
            {
                return TwitterTimeline.Mentions(privOAuth.GetOAuthToken()).ResponseObject;
            }
            public TwitterStatusCollection GetPublicTimeline()
            {
                return TwitterTimeline.PublicTimeline(privOAuth.GetOAuthToken()).ResponseObject;
            }
            public TwitterStatusCollection GetRetweetsOfMe()
            {
                return TwitterTimeline.RetweetsOfMe(privOAuth.GetOAuthToken()).ResponseObject;
            }
            public TwitterStatusCollection GetTweets()
            {
                return TwitterTimeline.HomeTimeline(privOAuth.GetOAuthToken()).ResponseObject;
            }
            //More possibilities upcoming~
        }


        //Put simple, all you have to do is make the user run AuthenticateTwitter() to pair the app with their account,
        //then make sure to run SaveOAuth() to save it to the harddrive, 
        //and run LoadOAuth() the next time you want to use their account.
        //I'll take care of the rest :3
        public class OAuthstuff
        {
            private string privAccessToken;
            private string privAccessTokenSecret;
            private string privConsumerKey;
            private string privConsumerKeySecret;
            public OAuthstuff() { }
            public OAuthstuff(string _AccessToken, string _AccessTokenSecret, string _ConsumerKey, string _ConsumerKeySecret)
            {
                privAccessToken = _AccessToken;
                privAccessTokenSecret = _AccessTokenSecret;
                privConsumerKey = _ConsumerKey;
                privConsumerKeySecret = _ConsumerKeySecret;
            }


            public void AuthenticateTwitter()
            {
                GetOAuthForm f = new GetOAuthForm(CONSUMER_KEY, CONSUMER_SECRET);
                f.ShowDialog();
                if (f.OAuthTokenResponse != null)
                {
                    privAccessToken = f.OAuthTokenResponse.Token;
                    privAccessTokenSecret = f.OAuthTokenResponse.TokenSecret;
                    privConsumerKey = CONSUMER_KEY;
                    privConsumerKeySecret = CONSUMER_SECRET;
                }
            }

            public void SaveOAuth()
            {
                Properties.Settings.Default.OAuth_AccessToken = AccessToken;
                Properties.Settings.Default.OAuth_AccessTokenSecret = AccessTokenSecret;
                Properties.Settings.Default.OAuth_ConsumerKey = ConsumerKey;
                Properties.Settings.Default.OAuth_ConsumerSecret = ConsumerSecret;
                Properties.Settings.Default.Save();
            }

            public void LoadOAuth()
            {
                if (Properties.Settings.Default.OAuth_AccessToken == "")
                    throw new Exception("Accesstoken empty! Are there any saved credentials at all? You need to reauthenticate!");
                AccessToken = Properties.Settings.Default.OAuth_AccessToken;
                AccessTokenSecret = Properties.Settings.Default.OAuth_AccessTokenSecret;
                ConsumerKey = Properties.Settings.Default.OAuth_ConsumerKey;
                ConsumerSecret = Properties.Settings.Default.OAuth_ConsumerSecret;
            }

            public string AccessToken { get { return privAccessToken; } set { privAccessToken = value; } }
            public string AccessTokenSecret { get { return privAccessTokenSecret; } set { privAccessTokenSecret = value; } }
            public string ConsumerKey { get { return privConsumerKey; } set { privConsumerKey = value; } }
            public string ConsumerSecret { get { return privConsumerKeySecret; } set { privConsumerKeySecret = value; } }
            public OAuthTokens GetOAuthToken()
            {
                OAuthTokens privOAuth = new OAuthTokens();
                privOAuth.AccessToken = privAccessToken;
                privOAuth.AccessTokenSecret = privAccessTokenSecret;
                privOAuth.ConsumerKey = privConsumerKey;
                privOAuth.ConsumerSecret = privConsumerKeySecret;
                return privOAuth;
            }

        }
    }
}
