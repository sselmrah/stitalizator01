using stitalizator01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stitalizator01
{
    public class LeaderboardEntry
    {


        public virtual ApplicationUser ApplicationUser { get; set; }
        public float Score { get; set; }
    }
}