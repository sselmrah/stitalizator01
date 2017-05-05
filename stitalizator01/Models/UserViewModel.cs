using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class GroupedUserViewModel
    {
        public List<UserViewModel> Users { get; set; }
        
    }
    public class UserViewModel
    {
        public string Username { get; set; }
        public List<string> Roles { get; set; }
    }

}