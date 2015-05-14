﻿// TicketDesk - Attribution notice
// Contributor(s):
//
//      Stephen Redd (stephen@reddnet.net, http://www.reddnet.net)
//
// This file is distributed under the terms of the Microsoft Public 
// License (Ms-PL). See http://opensource.org/licenses/MS-PL
// for the complete terms of use. 
//
// For any distribution that contains code from this file, this notice of 
// attribution must remain intact, and a copy of the license must be 
// provided to the recipient.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TicketDesk.PushNotifications.Common.Model
{

    [Table("PushNotificationItems", Schema = "notifications")]
    public class PushNotificationItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PushNotificationItemId { get; set; }

        [Index("IX_PushNotificationTarget", 0, IsUnique = true)]
        [Column(Order = 0)]
        public int ContentSourceId { get; set; }

        [Required]
        [Index("IX_PushNotificationTarget", 1, IsUnique = true)]
        [StringLength(100)]
        public string ContentSourceType { get; set; }

        [Required]
        [Index("IX_PushNotificationTarget", 2, IsUnique = true)]
        [StringLength(256)]
        public string SubscriberId { get; set; }

        [Required]
        [Index("IX_PushNotificationTarget", 3, IsUnique = true)]
        public int DestinationId { get; set; }

        public PushNotificationDestination Destination { get; set; }
        
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? ScheduledSendDate { get; set; }

        public PushNotificationItemStatus DeliveryStatus { get; set; }

        public int RetryCount { get; set; }

        public string MessageContent { get; set; }

        internal DateTimeOffset? GetSendDate(ApplicationPushNotificationSetting appSettings, SubscriberNotificationSetting userNoteSettings)
        {
            var send = ScheduledSendDate;//we'll leave this alone if consolidation isn't used

            if (userNoteSettings.IsEnabled && appSettings.AntiNoiseSettings.IsConsolidationEnabled && ScheduledSendDate.HasValue)
            {
                var now = DateTime.Now;

                var currentDelayMinutes = (now - CreatedDate).Minutes;

                var maxDelayMinutes = appSettings.AntiNoiseSettings.MaxConsolidationDelayMinutes;

                var delay = appSettings.AntiNoiseSettings.InitialConsolidationDelayMinutes;

                //ensure we don't bump thing over the max wait
                if (delay + currentDelayMinutes > maxDelayMinutes)
                {
                    delay = maxDelayMinutes - currentDelayMinutes;
                }

                send = now.AddMinutes(delay);
            }
            return send;
        }
    }
}
