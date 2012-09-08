using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace o3o
{
    public partial class UserAccounts : Form
    {
        public UserAccounts()
        {
            InitializeComponent();
            RefreshList();
        }
        void RefreshList()
        {
            listView_accounts.Items.Clear();
            foreach (UserDatabase.User usr in App.UsrDB.Users)
            {
                listView_accounts.Items.Add(usr.UserDetails.ScreenName);
            }
        }

        private void addNewAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.UsrDB.CreateUser();
            RefreshList();
        }

        private void listView_accounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            UserDatabase.User usr = App.UsrDB.Users.Find(u => u.UserDetails.ScreenName == listView_accounts.FocusedItem.Text);
            label_AccountInfo.Text =
                "User info:\n\n" +
                string.Format("Created on: {0}\n", usr.CreationDate.ToLongDateString()) +
                string.Format("Screen name: {0}\n", usr.UserDetails.ScreenName) +
                string.Format("User tokens: \n\tToken:{0}\n\tToken secret:{1}\n", usr.UserDetails.Token, usr.UserDetails.TokenSecret) +
                string.Format("User ID: {0}", usr.UserDetails.UserId)
                ;
        }
    }
}
