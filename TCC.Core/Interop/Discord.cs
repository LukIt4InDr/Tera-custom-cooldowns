﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using TCC.Data;

namespace TCC.Interop
{
    public static class Discord
    {
        public static async void FireWebhook(string webhook, string message)
        {
            if (!await Firebase.RequestWebhookExecution(webhook)) return;
            var msg = new JObject
            {
                {"content", message},
                {"username", App.AppVersion },
                {"avatar_url", "http://i.imgur.com/8IltuVz.png" }
            };

            try
            {
                using (var client = Utils.GetDefaultWebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    client.UploadString(webhook, "POST", msg.ToString());
                }

            }
            catch (Exception e)
            {
                WindowManager.FloatingButton.NotifyExtended("TCC Discord notifier", "Failed to send Discord notification.", NotificationType.Error);
                Log.F($"Failed to execute webhook: {e}");
            }

        }
    }
}
