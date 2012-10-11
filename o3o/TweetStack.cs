using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Twitterizer;
using Twitterizer.Core;
using System.Xml.Serialization;
using System.Net;

namespace o3o
{
    //Quick tutorial:
    //First, Create a new user and authenticate it and such.
    //then, make sure you have all the internal tweetstack events hooked.
    //That's all pretty much, if you want to send a tweet just use TwitterInteraction, which is also inside User.

    public class TweetStack
    {
        //vars
        
        public TwitterInteraction Twitter;
        public Twitterizer.Streaming.TwitterStream Tweetstream;
        UserDatabase.User privOAuth;



        public TweetStack(UserDatabase.User OAuth)
        {
            privOAuth = OAuth;
            Twitter = new TwitterInteraction(privOAuth);
            //and attempt to load the keys from setting
            //If that gone well, and streaming tweets were requested, try initialize streaming tweets.

            Twitterizer.Streaming.StreamOptions Streamopts = new Twitterizer.Streaming.StreamOptions();
            Streamopts.UseCompression = false;
            Streamopts.Count = 0;
            StartStream(Streamopts);
        }

        public void StartStream(Twitterizer.Streaming.StreamOptions Streamopts)
        {
            Tweetstream = new Twitterizer.Streaming.TwitterStream(privOAuth.GetOAuthToken(), "o3o", Streamopts);
            Tweetstream.StartUserStream(
                new Twitterizer.Streaming.InitUserStreamCallback(FriendsCallback),
                new Twitterizer.Streaming.StreamStoppedCallback(StreamStoppedcallback),
                new Twitterizer.Streaming.StatusCreatedCallback(StatuscreatedCallback),
                new Twitterizer.Streaming.StatusDeletedCallback(statusdeletedCallback),
                new Twitterizer.Streaming.DirectMessageCreatedCallback(DMcreatedCallback),
                new Twitterizer.Streaming.DirectMessageDeletedCallback(DMDeletedtCallback),
                new Twitterizer.Streaming.EventCallback(eventCallback));
        }

        #region TwitterStreamStuff
        //Tweet stream stuff~!
        
        void FriendsCallback(Twitterizer.TwitterIdCollection input)
        {
            //Don't need this yet
            
        }

        void StreamStoppedcallback(Twitterizer.Streaming.StopReasons stopreason)
        {
            //What happen??!??!?!??!!??!1//1/1/111oneone
            //Restart dat SHEET OF PAPER
            TwitterStatus notification = new TwitterStatus();
            notification.Text = "Stream died! Restarting stream.. Poke a developer if this happens a lot, or get a better connection.";
            notification.User = new TwitterUser();
            notification.User.ScreenName = "Internal message system";
            NewTweet(notification, privOAuth);
            StartStream(new Twitterizer.Streaming.StreamOptions());
        }

        public delegate void newtweetDel(TwitterStatus status, UserDatabase.User _usr);
        public event newtweetDel NewTweet;
        void StatuscreatedCallback(TwitterStatus status)
        {
            if (NewTweet != null)
                NewTweet(status, privOAuth);
        }


        public delegate void TweetDeletedDel(Twitterizer.Streaming.TwitterStreamDeletedEvent DeleteReason);
        public event TweetDeletedDel TweetDeleted;
        void statusdeletedCallback(Twitterizer.Streaming.TwitterStreamDeletedEvent deletedreason)
        {
            if(TweetDeleted != null)
                TweetDeleted(deletedreason);
        }

        public delegate void DMReceivedDel(TwitterDirectMessage DM, UserDatabase.User _usr);
        public event DMReceivedDel DMReceived;
        void DMcreatedCallback(TwitterDirectMessage incomingDM)
        {
            if(DMReceived != null)
                DMReceived(incomingDM, privOAuth);
        }

        public delegate void DMDeletedDel(Twitterizer.Streaming.TwitterStreamDeletedEvent DR);
        public event DMDeletedDel DMDeleted;
        void DMDeletedtCallback(Twitterizer.Streaming.TwitterStreamDeletedEvent DeleteReason)
        {
            if (DMDeleted != null)
                DMDeleted(DeleteReason);
        }

        delegate void twittereventdel(Twitterizer.Streaming.TwitterStreamEvent a);
        event twittereventdel twitterevent;
        void eventCallback(Twitterizer.Streaming.TwitterStreamEvent eventstuff)
        {
            if (twitterevent != null)
                twitterevent(eventstuff);
        }
        #endregion

        //This is basically just a barebone wrapper to keep things simple for you.
        /// <summary>
        /// This class contains shit to interact with twitter other than receiving tweets through streaming.
        /// e.g Sending tweets, updating profile, and getting static versions of the current timeline and mentions and such.    
        /// </summary>

        public class TwitterInteraction
        {
            UserDatabase.User privOAuth;
            public TwitterInteraction(UserDatabase.User _oauth)
            {
                privOAuth = _oauth;
            }

            public void SendTweet(string tweet)
            {
                if (tweet.Count() < 140)
                    try
                    {
                        Twitterizer.TwitterResponse<Twitterizer.TwitterStatus> response = Twitterizer.TwitterStatus.Update(privOAuth.GetOAuthToken(), tweet);
                        if (!(response.Result == RequestResult.Success))
                        {
                            System.Windows.Forms.MessageBox.Show("error: " + response.Result, "error", System.Windows.Forms.MessageBoxButtons.OK);
                        }
                    }
                    catch (WebException e)
                    {
                        System.Windows.Forms.MessageBox.Show("error: " + e.Message,"error",System.Windows.Forms.MessageBoxButtons.OK);
                        //TwitterStatus notification = new TwitterStatus();  CED GO FIX THIS 
                        //notification.Text = "error: "+e.Message;           Error An object reference is required for the non-static field, method, or property 'o3o.TweetStack.NewTweet'	
                        //notification.User = new TwitterUser();
                        //notification.User.ScreenName = "Internal message system";
                        //TweetStack.NewTweet(notification, privOAuth);
                        
                    }
                else
                    throw new Exception("Status update too long! Make sure it's less than 140 characters!"); // also replace this with system message
            }

            //This is all deprecated since streaming tweets were introduced.
            //Funny, I'm deprecating things before we even released it :D~! I'm such a horrible coder
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
            public TwitterUser GetUser(string _UserName)
            {
                return TwitterUser.Show(privOAuth.GetOAuthToken(), _UserName).ResponseObject;
            }
            
            public void favorite(decimal Id)
            {
                StatusUpdateOptions options = new StatusUpdateOptions();

                Twitterizer.TwitterResponse<TwitterStatus> response = Twitterizer.TwitterFavorite.Create(privOAuth.GetOAuthToken(), Id);
                if (!(response.Result == RequestResult.Success))
                {
                    //throw new Exception("fav failed: " + response.ErrorMessage);
                    System.Windows.Forms.MessageBox.Show("error: " + response.ErrorMessage, "error, fav failed", System.Windows.Forms.MessageBoxButtons.OK);
                }
            }

            public void unfavorite(decimal Id)
            {
                StatusUpdateOptions options = new StatusUpdateOptions();

                Twitterizer.TwitterResponse<TwitterStatus> response = Twitterizer.TwitterFavorite.Delete(privOAuth.GetOAuthToken(), Id);
                if (!(response.Result == RequestResult.Success))
                {
                    //throw new Exception("unfav failed: " + response.ErrorMessage);
                    System.Windows.Forms.MessageBox.Show("error: " + response.ErrorMessage, "error, unfav failed", System.Windows.Forms.MessageBoxButtons.OK);
                }
            }

            public void Reply(decimal Id, string tweet)
            {
                StatusUpdateOptions options = new StatusUpdateOptions();
                options.InReplyToStatusId = Id;
                Twitterizer.TwitterResponse<TwitterStatus> response = Twitterizer.TwitterStatus.Update(privOAuth.GetOAuthToken(), tweet,options);
                if (!(response.Result == RequestResult.Success))
                {
                    //throw new Exception("reply failed: " + response.ErrorMessage);
                    System.Windows.Forms.MessageBox.Show("error: " + response.ErrorMessage, "error", System.Windows.Forms.MessageBoxButtons.OK);
                }
            }

            public void Retweet(decimal id)
            {
                Twitterizer.TwitterResponse<TwitterStatus> response = Twitterizer.TwitterStatus.Retweet(privOAuth.GetOAuthToken(), id);
                if (!(response.Result == RequestResult.Success))
                {
                    System.Windows.Forms.MessageBox.Show("error: " + response.ErrorMessage, "error, retweet failed", System.Windows.Forms.MessageBoxButtons.OK);
                }
            }
            
        }




        
    }


    /// <summary>
    /// o3o can now have multiple users!
    /// Every user has their own tweetstack, which can send and receive tweets, DMs, mentions, etc.
    /// Events and such have to be hooked per user.
    /// Static retrieval of tweets is no longer supported, and everything will be done through streaming tweets.
    /// </summary>
    public class UserDatabase
    {
        public List<User> Users = new List<User>();
        string filename = "Users.xml";

        public void WipeUsers()
        {
            Users.Clear();

            //save(); // why do you rely on if (!System.IO.File.Exists(_filename)) when you are saving a empty xml
            System.IO.File.Delete(System.IO.Directory.GetCurrentDirectory() + "\\Users.xml");

        }
        public  bool save(string _filename = null)
        {
            if (_filename == null)
                _filename = filename;

            XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
            System.IO.FileStream fstream = new System.IO.FileStream(_filename, System.IO.FileMode.OpenOrCreate);
            serializer.Serialize(fstream, Users);
            fstream.Flush();
            fstream.Close();
            return true;
        }

        public bool load(string _filename = null)
        {
            if (_filename == null)
                _filename = filename;
            if (!System.IO.File.Exists(_filename))
                return false;
            XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
            System.IO.FileStream fstream = new System.IO.FileStream(_filename, System.IO.FileMode.Open);
            Users = (List<User>)serializer.Deserialize(fstream);
            foreach (User usr in Users)
            {
                if (!string.IsNullOrWhiteSpace(usr.AccessToken))
                    usr.Initialize();
            }
            fstream.Flush();
            fstream.Close();
            return true;
        }

        public void CreateUser(string asdf = null)
        {
            User usr = new User();
            
            usr.AuthenticateTwitter();
            usr.CreationDate = DateTime.Now;
            
            Users.Add(usr);
            usr.Initialize();
            save();
        }

        [Serializable]
        public class User
        {
            public User()
            {

            }

            [XmlIgnore]
            public TweetStack tweetStack;


            public void Initialize()
            {
                tweetStack = new TweetStack(this);
            }

            public void AuthenticateTwitter()
            {
                GetOAuthForm f = new GetOAuthForm(CONSUMER_KEY, CONSUMER_SECRET);
                f.ShowDialog();
                if (f.OAuthTokenResponse != null)
                {
                    AccessToken = f.OAuthTokenResponse.Token;
                    AccessTokenSecret = f.OAuthTokenResponse.TokenSecret;
                    UserDetails = f.OAuthTokenResponse;
                }
            }

            [XmlElement]
            public DateTime CreationDate { get; set; }
            [XmlElement]
            public string AccessToken { get; set; }
            [XmlElement]
            public string AccessTokenSecret { get; set; }
            [XmlElement]
            public OAuthTokenResponse UserDetails { get; set; }
            
            public OAuthTokens GetOAuthToken()
            {
                OAuthTokens OAuth = new OAuthTokens();
                OAuth.AccessToken = AccessToken;
                OAuth.AccessTokenSecret = AccessTokenSecret;
                OAuth.ConsumerKey = CONSUMER_KEY;
                OAuth.ConsumerSecret = CONSUMER_SECRET;
                return OAuth;
            }
        }



        //SET THESE Or the program won't work.
        public const string CONSUMER_KEY = "QfId2BP2FJB5v7Y0IUl4UA";
        public const string CONSUMER_SECRET = "g8xCgUzfCVTnyfUaHeFdHgYHBNrE5fTmF9YAANlRM";
        //And yes I realize it's horrible to keep those in the sourcecode, but .NET can be decompiled regardless, so where the hell are we supposed to keep them?
        //Perhaps an authentication server? I could easily make one but that'd give us so much power and responsibility, do we want that? If the auth server goes offline then the clients will stop working with it.


        //Put simple, all you have to do is make the user run AuthenticateTwitter() to pair the app with their account,
        //I'll take care of the rest :3
    }
}
