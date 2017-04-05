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
        private int _channelCode;
        private float? _shareSti;
        private float? _shareStiMob;
        private float? _shareStiPlus;
        private float? _shareMos18;
        private float? _shareRus18;
        private string _progDescr;









        
        public int ProgramID
        {
            get { return _programID; }
            set { _programID = value; }
        }

        [DisplayName("Код канала")]
        public int ChannelCode
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
        [DisplayName("Дата")]
        public DateTime TvDate
        {
            get { return _tvDate; }
            set { _tvDate = value; }
        }
        //[DisplayFormat(DataFormatString = "{HH:mm}", ApplyFormatInEditMode = true)]
        //[DataType(DataType.Time)]
        [DisplayName("Время начала")]
        public DateTime TimeStart
        {
            get { return _timeStart; }
            set { _timeStart = value; }
        }
        //[DisplayFormat(DataFormatString = "{HH:mm}", ApplyFormatInEditMode = true)]
        //[DataType(DataType.Time)]
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

        public float? ShareStiPlus
        {
            get { return _shareStiPlus; }
            set { _shareStiPlus = value; }
        }

        public float? ShareStiMob
        {
            get { return _shareStiMob; }
            set { _shareStiMob = value; }
        }

        public float? ShareSti
        {
            get { return _shareSti; }
            set { _shareSti = value; }
        }

        public float? ShareMos18
        {
            get { return _shareMos18; }
            set { _shareMos18 = value; }
        }
        public float? ShareRus18
        {
            get { return _shareRus18; }
            set { _shareRus18 = value; }
        }





    }
}