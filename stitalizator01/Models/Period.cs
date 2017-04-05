using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class Period
    {
        private int _periodID;
        private DateTime _begDate;
        private DateTime _endDate;
        private string _periodDescription;
        private int _userID;

        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public string PeriodDescription
        {
            get { return _periodDescription; }
            set { _periodDescription = value; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        public DateTime BegDate
        {
            get { return _begDate; }
            set { _begDate = value; }
        }

        public int PeriodID
        {
            get { return _periodID; }
            set { _periodID = value; }
        }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}