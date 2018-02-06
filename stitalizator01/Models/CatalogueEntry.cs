using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class CatalogueEntry
    {
        private int _catalogueEntryID;
        private DateTime _timing;
        private string _title;
        private bool _repeat;
        private DateTime _tVDate;
        private int _dow;
        private DateTime _begTime;
        private float _sti;
        private float _dm;
        private float _dr;
        private int _producerCode;
        private int _sellerCode;


        [DisplayName("Id")]
        public int CatalogueEntryID { get => _catalogueEntryID; set => _catalogueEntryID = value; }
        [DisplayName("Хронометраж")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString= "{0:HH:mm}",ApplyFormatInEditMode = true)]
        public DateTime Timing { get => _timing; set => _timing = value; }
        [DisplayName("Название")]
        public string Title { get => _title; set => _title = value; }
        [DisplayName("Повтор")]
        public bool Repeat { get => _repeat; set => _repeat = value; }
        [DisplayName("Дата")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime TVDate { get => _tVDate; set => _tVDate = value; }
        [DisplayName("День")]
        public int Dow { get => _dow; set => _dow = value; }
        [DisplayName("Время")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime BegTime { get => _begTime; set => _begTime = value; }
        [DisplayName("СТИ")]
        public float Sti { get => _sti; set => _sti = value; }
        [DisplayName("DM")]
        public float Dm { get => _dm; set => _dm = value; }
        [DisplayName("DR")]
        public float Dr { get => _dr; set => _dr = value; }
        [DisplayName("Дирекция")]
        public int ProducerCode { get => _producerCode; set => _producerCode = value; }
        [DisplayName("Производитель")]
        public int SellerCode { get => _sellerCode; set => _sellerCode = value; }



    }
}