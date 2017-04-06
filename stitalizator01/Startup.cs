
using Microsoft.Owin;
using Owin;


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
