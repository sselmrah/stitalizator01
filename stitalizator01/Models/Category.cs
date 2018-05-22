using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class Category
    {
        private int _id;
        private string _catName;
        private int _catNum;


        public int Id { get => _id; set => _id = value; }
        public string CatName { get => _catName; set => _catName = value; }
        public int CatNum { get => _catNum; set => _catNum = value; }
    }
}