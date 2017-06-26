using System;
using Microsoft.Azure.NotificationHubs;

namespace SendingApp
{
	class MainClass
	{
		
		public const string ConnectionString = "Endpoint=sb://humbertopushnotificationhub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=SDbXi9h0KS9Mi6Q2fj4Wpd25356eDj8cZzeEM1/fiRg=";
		public const string NotificationHubPath = "humbertopushnotificationhub";


		public static void Main(string[] args)
		{
			var hub = NotificationHubClient.CreateClientFromConnectionString(ConnectionString, NotificationHubPath);
			var json = "{\"data\":{\"message\":\"Notification Hub test notification from desktop in Inbursa\"}}";
			hub.SendGcmNativeNotificationAsync(json).Wait();
		    json = "{\"aps\":{\"alert\":\" Message from desktop in Inbursa\"}}";
		    hub.SendAppleNativeNotificationAsync(json).Wait();


		}
	}
}
