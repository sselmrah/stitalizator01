using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class Program
    {
        
        private int _programID;
        private string _progTitle;
        private DateTime _tvDate;
        private DateTime _timeStart;
        private DateTime? _timeEnd;
        private string _channelCode;
        private float? _shareSti;
        private float? _shareStiMob;
        private double? _shareStiPlus;
        private float? _shareMos18;
        private float? _shareRus18;
        private string _progDescr;
        private string _progCat;
        private bool _isBet;
        private bool _isHorse;

        public bool IsHorse
        {
            get { return _isHorse; }
            set { _isHorse = value; }
        }

        [DisplayName("Ставка")]
        public bool IsBet
        {
            get { return _isBet; }
            set { _isBet = value; }
        }        

               
        public int ProgramID
        {
            get { return _programID; }
            set { _programID = value; }
        }

        [DisplayName("Название канала")]
        public string ChannelCode
        {
            get { return _channelCode; }
            set { _channelCode = value; }
        }
        [DisplayName("Название программы")]
        public string ProgTitle
        {
            get { return _progTitle; }
            set { _progTitle = value; }
        }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]        
        [DisplayName("Дата")]
        public DateTime TvDate
        {
            get { return _tvDate; }
            set { _tvDate = value; }
        }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]        
        [DisplayName("Время начала")]
        public DateTime TimeStart
        {
            get { return _timeStart; }
            set { _timeStart = value; }
        }
        
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]        
        [DisplayName("Время окончания")]
        public DateTime? TimeEnd
        {
            get { return _timeEnd; }
            set { _timeEnd = value; }
        }
        [DisplayName("Описание")]
        public string ProgDescr
        {
            get { return _progDescr; }
            set { _progDescr = value; }
        }
        [DisplayName("Категории")]
        public string ProgCat
        {
            get { return _progCat; }
            set { _progCat = value; }
        }
        [DisplayName("Доля СТИ+")]
        public double? ShareStiPlus
        {
            get { return _shareStiPlus; }
            set { _shareStiPlus = value; }
        }
        [DisplayName("Доля СТИ-Моб")]
        public float? ShareStiMob
        {
            get { return _shareStiMob; }
            set { _shareStiMob = value; }
        }
        [DisplayName("Доля СТИ")]
        public float? ShareSti
        {
            get { return _shareSti; }
            set { _shareSti = value; }
        }
        [DisplayName("Доля Mediascope-Mos")]
        public float? ShareMos18
        {
            get { return _shareMos18; }
            set { _shareMos18 = value; }
        }
        [DisplayName("Доля СТИ+")]
        public float? ShareRus18
        {
            get { return _shareRus18; }
            set { _shareRus18 = value; }
        }





    }
}