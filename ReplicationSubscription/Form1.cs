using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// These namespaces are required.
using Microsoft.SqlServer.Replication;
using Microsoft.SqlServer.Management.Common;

namespace ExpressMergePullSubscriptionProgressBar
{
    public partial class Form1 : Form
    {
        // Define the server, subscription, publication, and database names.
        string subscriberName = "PACIFIC\\SQLEXPRESS";
        string subscriptionDbName = "TestDB1";
        string publisherName = "WS2008R2_1";
        string publicationName = "TestMergePub1";
        string publicationDbName = "AdventureWorksLT";

        // Merge agent
        MergeSynchronizationAgent agent;

        // Sync BackgroundWorker
        BackgroundWorker syncBackgroundWorker;

        public Form1()
        {
            InitializeComponent();
            lblSubscriptionName.Text = "[" + subscriptionDbName + "] - [" + publisherName + "] - [" + publicationDbName + "]";
            lblPublicationName.Text = publicationName;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Instantiate a BackgroundWorker, subscribe to its events, and call RunWorkerAsync()
            syncBackgroundWorker = new BackgroundWorker();
            syncBackgroundWorker.WorkerReportsProgress = true;
            syncBackgroundWorker_DoWork();
           // syncBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(syncBackgroundWorker_ProgressChanged);
            //syncBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(syncBackgroundWorker_RunWorkerCompleted);

            // Disable the start button
            btnStart.Enabled = false;

            // Initialize the progress bar and status textbox
            pbStatus.Value = 0;
            tbLastStatusMessage.Text = String.Empty;

            pictureBoxStatus.Visible = true;
            pictureBoxStatus.Enabled = true;

            // Kick off a background operation to synchronize
            //syncBackgroundWorker.RunWorkerAsync();
        }

        // This event handler initiates the synchronization
        private void syncBackgroundWorker_DoWork()
        {
            // Connect to the Subscriber and synchronize

            // Create a connection to the Subscriber.
            ServerConnection conn = new ServerConnection(subscriberName);

            // Merge pull subscription
            MergePullSubscription subscription;

            try
            {
                // Connect to the Subscriber.
                conn.Connect();

                // Define the pull subscription.
                subscription = new MergePullSubscription();
                subscription.ConnectionContext = conn;
                subscription.DatabaseName = subscriptionDbName;
                subscription.PublisherName = publisherName;
                subscription.PublicationDBName = publicationDbName;
                subscription.PublicationName = publicationName;

                // If the pull subscription exists, then start the synchronization.
                if (subscription.LoadProperties())
                {
                    // Get the agent for the subscription.
                    agent = subscription.SynchronizationAgent;

                    // Set the required properties that could not be returned
                    // from the MSsubscription_properties table.
                    agent.PublisherSecurityMode = SecurityMode.Integrated;
                    agent.DistributorSecurityMode = SecurityMode.Integrated;
                    agent.Distributor = publisherName;

                    // Enable verbose merge agent output to file.
                    agent.OutputVerboseLevel = 4;
                    agent.Output = "C:\\TEMP\\mergeagent.log";

                    // Handle the Status event
                    //agent.Status += new AgentCore.StatusEventHandler(agent_Status);

                    // Synchronously start the Merge Agent for the subscription.
                    agent.Synchronize();
                }
                else
                {
                    // Do something here if the pull subscription does not exist.
                    throw new ApplicationException(String.Format(
                        "A subscription to '{0}' does not exist on {1}",
                        publicationName, subscriberName));
                }
            }
            catch (Exception ex)
            {
                // Implement appropriate error handling here.
                throw new ApplicationException("The subscription could not be " +
                    "synchronized. Verify that the subscription has " +
                    "been defined correctly.", ex);
            }
            finally
            {
                conn.Disconnect();
            }
        }

        //// This event handler handles the Status event and reports the agent progress.
        //public void agent_Status(object sender, StatusEventArgs e)
        //{
        //    syncBackgroundWorker.ReportProgress(Convert.ToInt32(e.PercentCompleted), e.Message.ToString());
        //}

        // This event handler updates the form with agent progress.
        //private void syncBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    // Set the progress bar percent completed
        //    pbStatus.Value = e.ProgressPercentage;

        //    // Append the last agent message
        //    tbLastStatusMessage.Text += e.UserState.ToString() + Environment.NewLine;

        //    // Scroll to end
        //    ScrollToEnd();
        //}

        // This event handler deals with the results of the background operation.
        //private void syncBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Cancelled == true)
        //    {
        //        tbLastStatusMessage.Text += "Canceled!" + Environment.NewLine;
        //        ScrollToEnd();
        //    }
        //    else if (e.Error != null)
        //    {
        //        tbLastStatusMessage.Text += "Error: " + e.Error.Message + Environment.NewLine;
        //        ScrollToEnd();
        //    }
        //    else
        //    {
        //        tbLastStatusMessage.Text += "Done!" + Environment.NewLine;
        //        ScrollToEnd();
        //    }

        //    btnStart.Enabled = true;
        //    pictureBoxStatus.Enabled = false;
        //}

        //private void ScrollToEnd()
        //{
        //    // Scroll to end
        //    tbLastStatusMessage.SelectionStart = tbLastStatusMessage.TextLength;
        //    tbLastStatusMessage.ScrollToCaret();
        //}

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
