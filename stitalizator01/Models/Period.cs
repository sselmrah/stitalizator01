using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        private bool _isMetaPeriod;
        private int _scoresGambled;

        [DisplayName("Очков в розыгрыше")]
        public int ScoresGambled
        {
            get { return _scoresGambled; }
            set { _scoresGambled = value; }
        }

        [DisplayName("Сезон")]
        public bool IsMetaPeriod
        {
            get { return _isMetaPeriod; }
            set { _isMetaPeriod = value; }
        }
        /*
        private int _userID;
        
        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }
        */
        
        [DisplayName("Описание")]
        public string PeriodDescription
        {
            get { return _periodDescription; }
            set { _periodDescription = value; }
        }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        [DisplayName("Дата окончания")]
        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        [DisplayName("Дата начала")]
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


        [DisplayName("Лидер")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}