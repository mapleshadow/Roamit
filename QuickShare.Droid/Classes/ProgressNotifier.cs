﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Media;
using System.Threading.Tasks;

namespace QuickShare.Droid.Classes
{
    internal class ProgressNotifier
    {
        readonly TimeSpan _minimumTimeBetweenNotifs = TimeSpan.FromSeconds(0.5);
        readonly TimeSpan _minimumTimeBetweenFinalNotifAndPrev = TimeSpan.FromSeconds(1.5);
        readonly TimeSpan _timeoutSpan = TimeSpan.FromSeconds(20);

        // We have just one progress notification, but 
        // more than one finish notifications.
        static int id = Notification.GetNewNotifId();
        Context context;
        NotificationManager notificationManager;
        NotificationCompat.Builder builder;

        DateTime lastProgressNotif = DateTime.MinValue;

        Guid lastActivityGuid = Guid.Empty;

        string failedMessage;

        public ProgressNotifier(Context _context, string _failedMessage)
        {
            context = _context;
            failedMessage = _failedMessage;

            notificationManager = NotificationManager.FromContext(_context);
        }

        public void SendInitialNotification(string title, string text)
        {
            builder = new NotificationCompat.Builder(context)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetProgress(0, 0, true);

            notificationManager.Notify(id, builder.Build());
            lastProgressNotif = DateTime.Now;

            Timeout();
        }

        private void Timeout()
        {
            var guid = Guid.NewGuid();
            lastActivityGuid = guid;
            Timeout(guid);
        }

        private async void Timeout(Guid guid)
        {
            await Task.Delay(_timeoutSpan);

            if (lastActivityGuid != guid)
                return;

            lastActivityGuid = Guid.Empty;

            notificationManager.Cancel(id);

            builder = new NotificationCompat.Builder(context)
                .SetContentTitle(failedMessage)
                .SetContentText("")
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetProgress(0, 0, false);

            notificationManager.Notify(Notification.GetNewNotifId(), builder.Build());
        }

        public void UpdateTitle(string title)
        {
            if ((DateTime.Now - lastProgressNotif) < _minimumTimeBetweenNotifs)
                return;

            builder.SetContentTitle(title);

            notificationManager.Notify(id, builder.Build());
            lastProgressNotif = DateTime.Now;

            Timeout();
        }


        public void SetProgressValue(int max, int value, string title)
        {
            if ((DateTime.Now - lastProgressNotif) < _minimumTimeBetweenNotifs)
                return;

            int percent = (100 * value) / max;

            builder.SetProgress(max, value, false)
                .SetContentTitle(title)
                .SetContentText($"{percent}%");

            notificationManager.Notify(id, builder.Build());
            lastProgressNotif = DateTime.Now;

            Timeout();
        }

        public void MakeIndetermine(string text = "")
        {
            builder.SetProgress(0, 0, true)
                .SetContentText(text);

            notificationManager.Notify(id, builder.Build());

            Timeout();
        }

        public async Task FinishProgress(string title, string text)
        {
            lastActivityGuid = Guid.Empty;

            if ((DateTime.Now - lastProgressNotif) < _minimumTimeBetweenFinalNotifAndPrev)
                await Task.Delay(DateTime.Now - lastProgressNotif);

            notificationManager.Cancel(id);

            builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                .SetPriority((int)NotificationPriority.Max)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetProgress(0, 0, false)
                .SetStyle(new NotificationCompat.BigTextStyle()
                    .SetBigContentTitle(title)
                    .BigText(text));

            notificationManager.Notify(Notification.GetNewNotifId(), builder.Build());
        }

        public async Task FinishProgress(string title, string text, Intent intent, Context _context)
        {
            lastActivityGuid = Guid.Empty;

            if ((DateTime.Now - lastProgressNotif) < _minimumTimeBetweenFinalNotifAndPrev)
                await Task.Delay(DateTime.Now - lastProgressNotif);

            notificationManager.Cancel(id);

            builder = new NotificationCompat.Builder(_context)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                .SetPriority((int)NotificationPriority.Max)
                .SetContentTitle(title)
                .SetContentText(text)
                .SetContentIntent(PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.UpdateCurrent))
                .SetProgress(0, 0, false)
                .SetStyle(new NotificationCompat.BigTextStyle()
                    .SetBigContentTitle(title)
                    .BigText(text));
            
            notificationManager.Notify(Notification.GetNewNotifId(), builder.Build());
        }

        internal async Task ClearProgressNotification()
        {
            lastActivityGuid = Guid.Empty;

            if ((DateTime.Now - lastProgressNotif) < _minimumTimeBetweenFinalNotifAndPrev)
                await Task.Delay(DateTime.Now - lastProgressNotif);

            notificationManager.Cancel(id);
        }
    }
}