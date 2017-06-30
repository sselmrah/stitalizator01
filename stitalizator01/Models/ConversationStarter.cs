using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class ConversationStarter
    {
        private int _conversationStarterID;
        private string _toId;
        private string _toName;
        private string _fromId;
        private string _fromName;
        private string _serviceUrl;
        private string _channelId;
        private string _conversationId;
        private DateTime _lastTimeUsed;

        public virtual ApplicationUser ApplicationUser { get; set; }
        public int ConversationStarterID { get => _conversationStarterID; set => _conversationStarterID = value; }
        
        public string ToId { get => _toId; set => _toId = value; }
        public string ToName { get => _toName; set => _toName = value; }
        public string FromId { get => _fromId; set => _fromId = value; }
        public string FromName { get => _fromName; set => _fromName = value; }
        public string ServiceUrl { get => _serviceUrl; set => _serviceUrl = value; }
        public string ChannelId { get => _channelId; set => _channelId = value; }
        public string ConversationId { get => _conversationId; set => _conversationId = value; }
        public DateTime LastTimeUsed { get => _lastTimeUsed; set => _lastTimeUsed = value; }
    }
}