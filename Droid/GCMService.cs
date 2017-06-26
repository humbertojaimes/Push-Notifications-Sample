using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Android.Util;
using Gcm.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System;
using WindowsAzure.Messaging;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
//GET_ACCOUNTS is only needed for android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
namespace PushNotificationsSample.Droid
{

	[BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE }, Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK }, Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY }, Categories = new string[] { "@PACKAGE_NAME@" })]
	public class PushHandlerBroadcastReceiver : GcmBroadcastReceiverBase<GcmService>
	{
		public static string[] SENDER_IDS = new string[] { "44426910487" };
		public const string TAG = "MyBroadcastReceiver-GCM";
	}

	[Service]
	public class GcmService : GcmServiceBase
	{
		public static string RegistrationID { get; private set; }

		public GcmService()
			: base(PushHandlerBroadcastReceiver.SENDER_IDS) { }


		public const string ConnectionString = "Endpoint=sb://humbertopushnotificationhub.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=IlHehjlScZYNCmN0RIv/v2cBrKgOx2IAsUAFEDEdkd4=";
		public const string NotificationHubPath = "humbertopushnotificationhub";
		private NotificationHub Hub { get; set; }

		protected override void OnRegistered(Context context, string registrationId)
		{
			Log.Verbose(PushHandlerBroadcastReceiver.TAG, "GCM Registered: " + registrationId);
			RegistrationID = registrationId;

			createNotification("PushHandlerService-GCM Registered...",
								"The device has been Registered!");

			Hub = new NotificationHub(NotificationHubPath, ConnectionString,
										context);
			try
			{
				Hub.UnregisterAll(registrationId);
			}
			catch (Exception ex)
			{
				Log.Error(PushHandlerBroadcastReceiver.TAG, ex.Message);
			}

			//var tags = new List<string>() { "falcons" }; // create tags if you want
			var tags = new List<string>() { };

			try
			{
				var hubRegistration = Hub.Register(registrationId, tags.ToArray());
			}
			catch (Exception ex)
			{
				Log.Error(PushHandlerBroadcastReceiver.TAG, ex.Message);
			}
		}

		protected override void OnMessage(Context context, Intent intent)
		{
			Log.Info(PushHandlerBroadcastReceiver.TAG, "GCM Message Received!");

			var msg = new StringBuilder();

			if (intent != null && intent.Extras != null)
			{
				foreach (var key in intent.Extras.KeySet())
					msg.AppendLine(key + "=" + intent.Extras.Get(key).ToString());
			}

			string messageText = intent.Extras.GetString("message");
			if (!string.IsNullOrEmpty(messageText))
			{
				createNotification("New hub message!", messageText);
			}
			else
			{
				createNotification("Unknown message details", msg.ToString());
			}
		}


		void createNotification(string title, string desc)
		{
			//Create notification
			var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

			//Create an intent to show UI
			var uiIntent = new Intent(this, typeof(MainActivity));

			//Create the notification
			var notification = new Notification(Android.Resource.Drawable.SymActionEmail, title);

			//Auto-cancel will remove the notification once the user touches it
			notification.Flags = NotificationFlags.AutoCancel;

			//Set the notification info
			//we use the pending intent, passing our ui intent over, which will get called
			//when the notification is tapped.
			notification.SetLatestEventInfo(this, title, desc, PendingIntent.GetActivity(this, 0, uiIntent, 0));

			//Show the notification
			notificationManager.Notify(1, notification);
			dialogNotify(title, desc);
		}

		protected void dialogNotify(String title, String message)
		{

			MainActivity.CurrentActivity.RunOnUiThread(() =>
			{
				AlertDialog.Builder dlg = new AlertDialog.Builder(MainActivity.CurrentActivity);
				AlertDialog alert = dlg.Create();
				alert.SetTitle(title);
				alert.SetButton("Ok", delegate
				{
					alert.Dismiss();
				});
				alert.SetMessage(message);
				alert.Show();
			});
		}


		protected override void OnUnRegistered(Context context, string registrationId)
		{
			Log.Verbose(PushHandlerBroadcastReceiver.TAG, "GCM Unregistered: " + registrationId);

			createNotification("GCM Unregistered...", "The device has been unregistered!");
		}

		protected override bool OnRecoverableError(Context context, string errorId)
		{
			Log.Warn(PushHandlerBroadcastReceiver.TAG, "Recoverable Error: " + errorId);

			return base.OnRecoverableError(context, errorId);
		}

		protected override void OnError(Context context, string errorId)
		{
			Log.Error(PushHandlerBroadcastReceiver.TAG, "GCM Error: " + errorId);
		}

	}
}
