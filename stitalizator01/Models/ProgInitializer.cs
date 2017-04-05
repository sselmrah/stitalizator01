using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;


namespace stitalizator01.Models
{
    public class ProgInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<StitalizatorContext>
    {
        public void InitializerDatabase(StitalizatorContext context)
        {
            if (context.Database.Exists())
            {
                string x = "";
            }
            else
            {
                context.Database.Create();
            }
        }
        protected override void Seed(StitalizatorContext context)
        {
            var programs = new List<Program>
            {
                new Program{ProgTitle="Вечерние новости", TvDate=DateTime.Parse("04.04.2017"), TimeStart=DateTime.Parse("04.04.2017 18:00:00"), TimeEnd=DateTime.Parse("04.04.2017 18:25:00"), ChannelCode=10},
                new Program{ProgTitle="Первая студия", TvDate=DateTime.Parse("04.04.2017"), TimeStart=DateTime.Parse("04.04.2017 18:25:00"), TimeEnd=DateTime.Parse("04.04.2017 21:00:00"), ChannelCode=10},
                new Program{ProgTitle="Время", TvDate=DateTime.Parse("04.04.2017"), TimeStart=DateTime.Parse("04.04.2017 21:00:00"), TimeEnd=DateTime.Parse("04.04.2017 21:30:00"), ChannelCode=10},
                new Program{ProgTitle="Волчье солнце", TvDate=DateTime.Parse("04.04.2017"), TimeStart=DateTime.Parse("04.04.2017 21:30:00"), TimeEnd=DateTime.Parse("04.04.2017 23:35:00"), ChannelCode=10},
            };
            programs.ForEach(p => context.Programs.Add(p));
            context.SaveChanges();

            //base.Seed(context);
        }
    }
}