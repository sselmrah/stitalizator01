using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using stitalizator01.Models;
using System.Net.Mail;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;


using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;



namespace stitalizator01
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static System.Timers.Timer timer = new System.Timers.Timer(60000); // This will raise the event every one minute.
        //public static System.Timers.Timer timer10 = new System.Timers.Timer(90000); 
        public static ApplicationDbContext db = new ApplicationDbContext();
        public static TimeSpan utcMoscowShift = TimeSpan.FromHours(3);
        private int minutesElapsed = 0;
        string key = "385340523:AAFPdWdVpE_oI4gLn8Z0XCb2_q-zaVVzP24";
        public BackgroundWorker bw;
        //private DateTime lastWarningTimeStamp = DateTime.UtcNow-TimeSpan.FromDays(1);

        protected void Application_Start()
        {

            AreaRegistration.RegisterAllAreas();
            //var ctx = new StitalizatorContext();
            //ctx.Database.Initialize(true);
            //initializeBetsDB(new ApplicationDbContext());
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);



            this.bw = new BackgroundWorker();
            this.bw.DoWork += this.bw_DoWork; // метод bw_DoWork будет работать асинхронно
            this.bw.RunWorkerAsync();


            //timer10.Enabled = true;
            //timer10.Elapsed += new System.Timers.ElapsedEventHandler(timer10_Elapsed);



        }

        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker; // получаем ссылку на класс вызвавший событие
            
            try
            {
                var Bot = new Telegram.Bot.TelegramBotClient(key); // инициализируем API
                await Bot.SetWebhookAsync("");
                //Bot.SetWebhook(""); // Обязательно! убираем старую привязку к вебхуку для бота
                int offset = 0; // отступ по сообщениям
                while (true)
                {
                    var updates = await Bot.GetUpdatesAsync(offset); // получаем массив обновлений
                    foreach (var update in updates) // Перебираем все обновления
                    {
                         
                        var message = update.Message;
                        if (message != null)
                        {
                            //Добавляем ChatId
                            string curUserName = message.Chat.Username;
                            ApplicationUser curUser = db.Users.Where(u => u.TelegramUserName == curUserName).FirstOrDefault();
                            if (curUser != null)
                            {
                                if (curUser.TelegramChatId == 0)
                                {
                                    curUser.TelegramChatId = message.Chat.Id;
                                    db.Entry(curUser).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }

                            if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                            {
                                
                                if (message.Text == "/saysomething")
                                {
                                    // в ответ на команду /saysomething выводим сообщение
                                    ReplyKeyboardHide rkh = new ReplyKeyboardHide();

                                    await Bot.SendTextMessageAsync(message.Chat.Id, "тест",
                                           replyToMessageId: message.MessageId, replyMarkup: rkh);
                                }
                                if (message.Text == "/mybets")
                                {
                                    string senderUserName = message.From.Username;
                                    DateTime dt = DateTime.UtcNow.Date;
                                    Debug.Print("Sender: " + senderUserName);
                                    string reply = "";
                                    reply = "А вот твоя панама, " + senderUserName + "!";
                                    List<Bet> bets = getBetsByTelegramUserNameAndDate(senderUserName, dt);
                                    InlineKeyboardMarkup kb = createKeabordFromBets(bets);
                                    await Bot.SendTextMessageAsync(message.Chat.Id, reply, false, false, message.MessageId, kb);

                                }
                            }
                        }
                        else
                        {
                            if (update.CallbackQuery.Data != null)
                            {
                                if (update.CallbackQuery.Data.Length > 1)
                                {

                                    InlineKeyboardMarkup kb = createNumericInlineKb();
                                    ReplyKeyboardHide rkh = new ReplyKeyboardHide();
                                    rkh.HideKeyboard = true;
                                    Bet b = db.Bets.Find(Convert.ToInt32(update.CallbackQuery.Data));
                                    string reply = "Сколько ставим на " + b.Program.ProgTitle + "?";
                                    //await Bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, reply, false, false, update.CallbackQuery.Message.MessageId, kb);
                                    await Bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, reply, false, false, update.CallbackQuery.Message.MessageId, rkh);
                                }
                                else
                                {
                                    
                                }
                            }
                        }
                        offset = update.Id + 1;
                    }

                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message); // если ключ не подошел - пишем об этом в консоль отладки
            }
        }




        public List<Bet> getBetsByTelegramUserNameAndDate(string tUserName, DateTime curDate)
        {
            List<Bet> bets = db.Bets.Where(b => b.ApplicationUser.TelegramUserName == tUserName & b.Program.TvDate == curDate).ToList();

            return bets;
        }

        public InlineKeyboardMarkup createNumericInlineKb()
        {
            
            //List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>();
            

            InlineKeyboardButton[][] rows = new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[] { new InlineKeyboardButton(text: "7"), new InlineKeyboardButton(text: "8"), new InlineKeyboardButton(text: "9") },
                new InlineKeyboardButton[] { new InlineKeyboardButton(text: "4"), new InlineKeyboardButton(text: "5"), new InlineKeyboardButton(text: "6") },
                new InlineKeyboardButton[] { new InlineKeyboardButton(text: "1"), new InlineKeyboardButton(text: "2"), new InlineKeyboardButton(text: "3") },
                new InlineKeyboardButton[] { new InlineKeyboardButton(text: "0"), new InlineKeyboardButton(text: ",") }
            };
            InlineKeyboardMarkup kb = new InlineKeyboardMarkup(rows);
            
            return kb;
        }
        public ReplyKeyboardMarkup createNumericReplyKb()
        {

            //List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>();


            KeyboardButton[][] rows = new KeyboardButton[][]
            {
                new KeyboardButton[] { new KeyboardButton(text: "7"), new KeyboardButton(text: "8"), new KeyboardButton(text: "9") },
                new KeyboardButton[] { new KeyboardButton(text: "4"), new KeyboardButton(text: "5"), new KeyboardButton(text: "6") },
                new KeyboardButton[] { new KeyboardButton(text: "1"), new KeyboardButton(text: "2"), new KeyboardButton(text: "3") },
                new KeyboardButton[] { new KeyboardButton(text: "0"), new KeyboardButton(text: ",") }
            };
            ReplyKeyboardMarkup kb = new ReplyKeyboardMarkup(rows);

            return kb;
        }


        public InlineKeyboardMarkup createKeabordFromBets(List<Bet> bets)
        {
            InlineKeyboardMarkup kb = new InlineKeyboardMarkup();
            
            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>();
            foreach (Bet b in bets)
            {
                InlineKeyboardButton curButton = new InlineKeyboardButton();
                curButton.Text = b.Program.ProgTitle + " (" + b.Program.ChannelCode + ", " + b.Program.TimeStart.ToString("HH:mm") + ")";
                curButton.CallbackData = b.BetID.ToString();                
                InlineKeyboardButton[] row = new InlineKeyboardButton[1];
                row[0] = curButton;
                rows.Add(row);
            }

            kb.InlineKeyboard = rows.ToArray();            

            return kb;
        }


        private void Contact()
        {
            DateTime curTime = DateTime.UtcNow;
            List<ApplicationUser> users = db.Users.ToList();
            List<Bet> userBets = new List<Bet>();
            //var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var message = new MailMessage();

            foreach (ApplicationUser user in users)
            {
                userBets = db.Bets.Where(b => b.ApplicationUser.UserName == user.UserName & b.Program.TvDate == curTime.Date & b.BetSTIplus == 0).ToList();

                if (user.UserName != "admin")
                {
                    if (userBets.Count() > 0)
                    {
                        message.To.Add(new MailAddress(user.Email));
                    }
                }
            }
            if (message.To.Count() > 0)
            {
                //message.To.Add(new MailAddress("amosendz@gmail.com"));  // replace with valid value 
                message.From = new MailAddress("stitalizator@gmail.com");  // replace with valid value
                message.Subject = "Нужно сделать ставки на " + curTime.Date.ToString("dd.MM.yyyy");
                message.Body = "В далеком светлом будущем здесь будет перечень ставок.";
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "stitalizator@gmail.com",  // replace with valid value
                        Password = "945549Co"  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                    //return RedirectToAction("Sent");
                    //return Content("Sent!");
                }
            }
            else
            {
               // return Content("Все молодцы! Отправлять некому!");
            }
        }

        private void sendPersonalizedEmail(string email, List<string> betName)
        {
            
            var message = new MailMessage();
            message.To.Add(new MailAddress(email));
            message.From = new MailAddress("stitalizator@gmail.com");  // replace with valid value
            message.Subject = "Нужно сделать ставки!";
            string text = "<p>Заканчивается прием ставок на следующие программы:</p>";
            foreach(string s in betName)
            {
                text += "<p>" + s + "</p>";
            }
            text += "<br><p><a href=\"http://stitalizator.azurewebsites.net\">" + "Перейти к выставлению ставок</a></p>";
            message.Body = text;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "stitalizator@gmail.com",  // replace with valid value
                    Password = "945549Co"  // replace with valid value
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
                //return RedirectToAction("Sent");
                //return Content("Sent!");
            }
        }

        private void timer10_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DateTime now = DateTime.UtcNow + utcMoscowShift;
            DateTime later = now + TimeSpan.FromMinutes(4);//19);
            //List<Bet> burningBets = db.Bets.Where(b => b.Program.TvDate == now.Date & b.Program.TimeStart < (now+TimeSpan.FromHours(1))).ToList();
            List<ApplicationUser> users = db.Users.ToList();
            foreach (ApplicationUser user in users)
            {
                List<Bet> burningBets = db.Bets.Where(b => b.ApplicationUser.Id == user.Id & b.BetSTIplus==0 & b.Program.TimeStart > now & b.Program.TimeStart < later).ToList();   
                if (burningBets.Count()>0)
                {
                    List<string> bets2send = new List<string>();
                    foreach(Bet b in burningBets)
                    {
                        string betDescription = b.Program.ProgTitle + "(" + b.Program.TimeStart.ToString("HH:mm")+") "+b.Program.ChannelCode;
                        bets2send.Add(betDescription);
                    }
                    //sendPersonalizedEmail(user.Email, bets2send);
                    if (user.TelegramChatId > 0)
                    {
                        sendTelegramUpdate(user.TelegramUserName, bets2send);
                    }
                }
            }

            /*
            if (lastWarningTimeStamp.Date < now.Date)
            {                
                if (burningBets.Count > 0)
                {
                    Contact();
                    lastWarningTimeStamp = now;    
                }
            }
            */
        }

        async void sendTelegramUpdate(string telegramUserName, List<string> betName)
        {
            var Bot = new Telegram.Bot.TelegramBotClient(key);
            await Bot.SetWebhookAsync("");
            
            string text = "Нужно сделать ставки: ";
            foreach (string s in betName)
            {
                text += "\n" + s;
            }
            InlineKeyboardButton curButton = new InlineKeyboardButton(text: "Перейти к выставлению ставок");
            curButton.Url = "http://stitalizator.azurewebsites.net";
            InlineKeyboardButton[][] rows = new InlineKeyboardButton[][]
            {                
                new InlineKeyboardButton[] { curButton},                
            };
            InlineKeyboardMarkup kb = new InlineKeyboardMarkup(rows);
            
            ApplicationUser curUser = db.Users.Where(u => u.TelegramUserName == telegramUserName).FirstOrDefault();
            if (curUser.TelegramChatId > 0)
            {
                await Bot.SendTextMessageAsync(chatId: curUser.TelegramChatId, text: text,replyMarkup: kb);
            }

        }


        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Московское время
            DateTime now = DateTime.UtcNow + utcMoscowShift;

            var expiredList = db.Bets.Where(b => b.Program.TimeStart < now);
            foreach (Bet bet in expiredList)
            {
                bet.IsLocked = true;
            }
            db.SaveChanges();
            minutesElapsed++;
            if (minutesElapsed==30)
            { 
                DateTime later = now + TimeSpan.FromMinutes(29);
                //List<Bet> burningBets = db.Bets.Where(b => b.Program.TvDate == now.Date & b.Program.TimeStart < (now+TimeSpan.FromHours(1))).ToList();
                List<ApplicationUser> users = db.Users.ToList();
                foreach (ApplicationUser user in users)
                {
                    List<Bet> burningBets = db.Bets.Where(b => b.ApplicationUser.Id == user.Id & b.BetSTIplus == 0 & b.Program.TimeStart > now & b.Program.TimeStart < later).ToList();
                    if (burningBets.Count() > 0)
                    {
                        List<string> bets2send = new List<string>();
                        foreach (Bet b in burningBets)
                        {
                            string betDescription = b.Program.ProgTitle + "(" + b.Program.TimeStart.ToString("HH:mm") + ") " + b.Program.ChannelCode;
                            bets2send.Add(betDescription);
                        }
                        //sendPersonalizedEmail(user.Email, bets2send);
                        if (user.TelegramChatId > 0)
                        {
                            sendTelegramUpdate(user.TelegramUserName, bets2send);
                        }
                    }
                }
                minutesElapsed = 0;
            }

        }

        /*
        private void initializeBetsDB(ApplicationDbContext context)
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
        }
         */
    }
}
