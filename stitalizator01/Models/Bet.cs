using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class Bet
    {
        private int _betID;
        private int _programID;
        //private int _userID;
        private float _betSTIplus;
        private float _betSTI;
        private float _betSTImob;
        private float _betMos18;
        private float _betRus18;
        private bool _isHorse;
        private DateTime _timeStamp;
        private int _attemptNo;
        private float _scoreClassic;
        private float _scoreOLS;
        private int _PeriodID;
        private bool _isLocked;


        public int PeriodID
        {
            get { return _PeriodID; }
            set { _PeriodID = value; }
        }

        public float ScoreOLS
        {
            get { return _scoreOLS; }
            set { _scoreOLS = value; }
        }

        public float ScoreClassic
        {
            get { return _scoreClassic; }
            set { _scoreClassic = value; }
        }

        public int AttemptNo
        {
            get { return _attemptNo; }
            set { _attemptNo = value; }
        }

        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        public bool IsHorse
        {
            get { return _isHorse; }
            set { _isHorse = value; }
        }

        public float BetRus18
        {
            get { return _betRus18; }
            set { _betRus18 = value; }
        }

        public float BetMos18
        {
            get { return _betMos18; }
            set { _betMos18 = value; }
        }

        public float BetSTImob
        {
            get { return _betSTImob; }
            set { _betSTImob = value; }
        }

        public float BetSTI
        {
            get { return _betSTI; }
            set { _betSTI = value; }
        }

        public float BetSTIplus
        {
            get { return _betSTIplus; }
            set { _betSTIplus = value; }
        }
        /*
        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }
        */

        public int ProgramID
        {
            get { return _programID; }
            set { _programID = value; }
        }


        public int BetID
        {
            get { return _betID; }
            set { _betID = value; }
        }

        [DisplayName("Заблокировано")]
        public bool IsLocked
        {
            get { return _isLocked; }
            set { _isLocked = value; }
        }


        public virtual Program Program { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Period> Periods { get; set; }
    }
}