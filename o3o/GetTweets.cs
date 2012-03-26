using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Twitterizer;
using System.Threading;

namespace o3o
{
    class GetTweets
    {
        public MainWindow parent;
        public TweetStack o3o;
        public GetTweets(MainWindow prnt, TweetStack stack)
        {
            parent = prnt;
			get_tweets();
            o3o = stack;
        }
        
		void get_tweets()
        {
            //parent.TweetElements.Items.Clear();
            Twitterizer.TwitterStatusCollection response = o3o.Twitter.GetTweets();
            foreach (Twitterizer.TwitterStatus tweet in response)
            {
                FillHome(tweet.Text, tweet.User.ScreenName, tweet.CreatedDate, tweet.User.ProfileImageLocation, tweet.Id.ToString());
            }
            int index = 0;
            parent.Notification(response[index].Text, response[index].User.ScreenName, response[index].CreatedDate, response[index].User.ProfileImageLocation, response[index].Id.ToString());
            
        }
		
		 public void FillHome(string message, string user, DateTime date, string url, string id) 
        {
            TweetElement element = new TweetElement(parent);
            element.Tweet = message;
            element.name = user;
            element.Date = date.Month.ToString() + "/" + date.Day.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString();
            element.imagelocation = url;
            element.ID = id;
            //parent.TweetElements.Items.Add(element);
             
        }
    }
}
