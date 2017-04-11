
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;


[assembly: OwinStartupAttribute(typeof(stitalizator01.Startup))]
namespace stitalizator01
{
    
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
