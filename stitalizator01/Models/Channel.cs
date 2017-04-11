using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class Channel
    {
        private int _channelID;
        private int _channelCode;
        private string _channelName;
        private string _channelTag;


        public int ChannelID
        {
            get { return _channelID; }
            set { _channelID = value; }
        }

        public int ChannelCode
        {
            get { return _channelCode; }
            set { _channelCode = value; }
        }


        public string ChannelTag
        {
            get { return _channelTag; }
            set { _channelTag = value; }
        }

        public string ChannelName
        {
            get { return _channelName; }
            set { _channelName = value; }
        }


    }
}