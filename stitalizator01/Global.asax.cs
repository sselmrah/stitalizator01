﻿using System;
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

using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading.Tasks;

namespace stitalizator01
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static System.Timers.Timer timer = new System.Timers.Timer(60000); // This will raise the event every one minute.

        public static ApplicationDbContext db = new ApplicationDbContext();
        public static TimeSpan utcMoscowShift = TimeSpan.FromHours(3);
        private int minutesElapsed = 0;
        string key = "385340523:AAFPdWdVpE_oI4gLn8Z0XCb2_q-zaVVzP24";
        public BackgroundWorker bw;
        private List<ApplicationUser> users = db.Users.ToList();
        private List<Bet> allbets = db.Bets.Where(b => b.BetSTIplus == 0 & !b.IsLocked).ToList();



        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            this.bw = new BackgroundWorker();
            this.bw.DoWork += this.bw_DoWork; // метод bw_DoWork будет работать асинхронно
            this.bw.RunWorkerAsync();
        }

        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker; // получаем ссылку на класс вызвавший событие
            var exp = new Regex("^(?=.*\\d)\\d*[\\.\\,]?\\d*$");
            
            try
            {
                var Bot = new Telegram.Bot.TelegramBotClient(key); // инициализируем API
                await Bot.SetWebhookAsync("");

                Stopwatch watch = new Stopwatch();
                watch.Start();
                //Bot.SetWebhook(""); // Обязательно! убираем старую привязку к вебхуку для бота
                int offset = 0; // отступ по сообщениям
                DateTime now = DateTime.UtcNow + utcMoscowShift;
                //DateTime later = now + TimeSpan.FromMinutes(15);
                List<ApplicationUser> users = db.Users.ToList();
                //List<Bet> allbets = db.Bets.Where(b => b.BetSTIplus == 0 & b.Program.TimeStart > now).ToList();


                while (true)
                {
                    
                    if (watch.Elapsed >= TimeSpan.FromMinutes(30))
                    {
                        //await sendTelegramUpdate(Bot, "amosendz", new List<string> { "1253" });   
                        foreach (ApplicationUser user in users)
                        {
                            if (user.TelegramChatId > 0)
                            {
                                    await sendTelegramUpdateBets(Bot, user);
                            }  
                        } 
                        watch.Reset();
                        watch.Start();
                    }
                    
                    var updates = await Bot.GetUpdatesAsync(offset); // получаем массив обновлений
                    foreach (var update in updates) // Перебираем все обновления
                    {
                        ApplicationUser curUser;
                        var message = update.Message;
                        if (message != null)
                        {
                            string curUserName = message.Chat.Username;
                            curUser = db.Users.Where(u => u.TelegramUserName == curUserName).FirstOrDefault();
                            /*
                            //Добавляем ChatId
                            if (curUser != null)
                            {
                                if (curUser.TelegramChatId == 0)
                                {
                                    curUser.TelegramChatId = message.Chat.Id;
                                    db.Entry(curUser).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            */


                            if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                            {
                                if (message.Text == "/register")
                                {
                                    //Добавляем ChatId
                                    if (curUser != null)
                                    {
                                        curUser.TelegramChatId = message.Chat.Id;
                                        db.Entry(curUser).State = EntityState.Modified;
                                        db.SaveChanges();
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Регистрация прошла успешно!", replyToMessageId: message.MessageId);
                                    }

                                }
                                if (message.Text == "/saysomething")
                                {
                                    // в ответ на команду /saysomething выводим сообщение
                                    ReplyKeyboardHide rkh = new ReplyKeyboardHide();

                                    await Bot.SendTextMessageAsync(message.Chat.Id, "тест",
                                           replyToMessageId: message.MessageId, replyMarkup: rkh);
                                }
                                else if (message.Text == "/mybets")
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
                                else if (Regex.IsMatch(message.Text, "^(?=.*\\d)\\d*[\\.\\,]?\\d*$"))
                                {
                                    string betStr = message.Text.Replace(',','.');
                                    if (curUser.TelegramBetId>0)
                                    { 
                                        Bet curBet = db.Bets.Find(curUser.TelegramBetId);
                                        if (curBet.IsLocked)
                                        {
                                            float telegramBet = Convert.ToSingle(betStr, CultureInfo.InvariantCulture.NumberFormat);
                                            curBet.BetSTIplus = telegramBet;
                                            db.Entry(curBet).State = EntityState.Modified;
                                            db.SaveChanges();
                                            if (curBet.BetSTIplus == telegramBet)
                                            {
                                                string reply = "Принято: \n\"" + curBet.Program.ProgTitle + "\" - " + curBet.BetSTIplus.ToString();

                                                await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: reply);
                                            }
                                            else
                                            {
                                                await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Что-то пошло не так. Ставка не принята.");
                                            }
                                            curUser.TelegramBetId = 0;
                                            db.Entry(curUser).State = EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            string reply = "Ставка: \n\"" + curBet.Program.ProgTitle + "\" заблокирована. Поставить не удалось.";
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: reply);
                                        }
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Что-то пошло не так. Ставка не принята.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (update.CallbackQuery.Data != null)
                            {
                                string curUserName = update.CallbackQuery.Message.Chat.Username;
                                curUser = db.Users.Where(u => u.TelegramUserName == curUserName).FirstOrDefault();

                                if (update.CallbackQuery.Data.Length > 1)
                                {
                                    if (update.CallbackQuery.Data.Substring(0, 5) == "betId")
                                    {
                                        string betId = update.CallbackQuery.Data.Substring(6);
                                        Bet b = db.Bets.Find(Convert.ToInt32(betId));
                                        if (b.ApplicationUser == curUser)
                                        {
                                            curUser.TelegramBetId = Convert.ToInt32(betId);
                                            db.Entry(curUser).State = EntityState.Modified;
                                            db.SaveChanges();
                                            string reply = "Сколько ставим на \"" + b.Program.ProgTitle + "\"?";
                                            //await Bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, reply, false, false, update.CallbackQuery.Message.MessageId, kb);
                                            //await Bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, reply, false, false, update.CallbackQuery.Message.MessageId, rkh);
                                            await Bot.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id, text: reply);
                                        }
                                    }
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
            List<Bet> bets = db.Bets.Where(b => b.ApplicationUser.TelegramUserName == tUserName & b.Program.TvDate == curDate &! b.IsLocked).ToList();

            return bets;
        }

        public InlineKeyboardMarkup createKeabordFromBets(List<Bet> bets, bool includeLink = false)
        {
            InlineKeyboardMarkup kb = new InlineKeyboardMarkup();

            List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>();

            foreach (Bet b in bets)
            {
                InlineKeyboardButton curButton = new InlineKeyboardButton();
                string curValue = "";
                if (b.BetSTIplus > 0) { curValue = " - " + b.BetSTIplus.ToString(); }
                curButton.Text = b.Program.ProgTitle + " (" + b.Program.ChannelCode + ", " + b.Program.TimeStart.ToString("HH:mm") + ")" + curValue;
                curButton.CallbackData = "betId_" + b.BetID.ToString();
                InlineKeyboardButton[] row = new InlineKeyboardButton[1];
                row[0] = curButton;
                rows.Add(row);
            }
            if (includeLink)
            {
                InlineKeyboardButton curButton = new InlineKeyboardButton(text: "Перейти на сайт");
                curButton.Url = "http://stitalizator.azurewebsites.net";
                InlineKeyboardButton[] row = new InlineKeyboardButton[1];
                row[0] = curButton;
                rows.Add(row);
            }


            kb.InlineKeyboard = rows.ToArray();

            return kb;
        }

        
        private void sendPersonalizedEmail(string email, List<string> betName)
        {

            var message = new MailMessage();
            message.To.Add(new MailAddress(email));
            message.From = new MailAddress("stitalizator@gmail.com");  // replace with valid value
            message.Subject = "Нужно сделать ставки!";
            string text = "<p>Заканчивается прием ставок на следующие программы:</p>";
            foreach (string s in betName)
            {
                text += "<p>" + s + "</p>";
            }
            //text += "<br><p><a href=\"http://stitalizator.azurewebsites.net\">" + "Перейти к выставлению ставок</a></p>";
            
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

        async Task sendTelegramUpdate(Telegram.Bot.TelegramBotClient Bot,string telegramUserName, List<string> betName)
        {
            //var Bot = new Telegram.Bot.TelegramBotClient(key);
            //await Bot.SetWebhookAsync("");

            string text = "Нужно сделать ставки: ";
            foreach (string s in betName)
            {
                text += "\n\"" + s + "\"";
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
                await Bot.SendTextMessageAsync(chatId: curUser.TelegramChatId, text: text, replyMarkup: kb);
            }

        }

        async Task sendTelegramUpdateBets(Telegram.Bot.TelegramBotClient Bot, ApplicationUser user)
        {
            //var Bot = new Telegram.Bot.TelegramBotClient(key);
            //await Bot.SetWebhookAsync("");
            //string telegramUserName = user.TelegramUserName;
            string text = "Нужно сделать ставки: ";
            DateTime now = DateTime.UtcNow + utcMoscowShift;
            DateTime later = now + TimeSpan.FromMinutes(30);
            //List<string> bets2send = new List<string>();
            //List<Bet> burningBets = db.Bets.Where(b => b.ApplicationUser == user & b.BetSTIplus == 0 & !b.IsLocked).ToList();
            List<Bet> bets2send = new List<Bet>();
            foreach (Bet b in allbets)
            {
                if (b.ApplicationUser.TelegramUserName==user.TelegramUserName & b.Program.TimeStart >= now & b.Program.TvDate == now.Date)
                {
                    //string betDescription = b.Program.ProgTitle + "(" + b.Program.TimeStart.ToString("HH:mm") + ") " + b.Program.ChannelCode;
                    //bets2send.Add(betDescription);
                    bets2send.Add(b);
                }
            }
            
            InlineKeyboardMarkup kb = createKeabordFromBets(bets2send, true);

            if (user.TelegramChatId > 0 & bets2send.Count>0)
            {
                await Bot.SendTextMessageAsync(chatId: user.TelegramChatId, text: text, replyMarkup: kb);
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
          
            if (minutesElapsed == 15)
            {
                allbets = db.Bets.Where(b => b.BetSTIplus == 0 & !b.IsLocked).ToList();
                //DateTime later = now + TimeSpan.FromMinutes(50);
                //List<ApplicationUser> users = db.Users.ToList();
                /*
                //Рассылка по e-mail. Пока отключили
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
                        sendPersonalizedEmail(user.Email, bets2send);   
                    }
                }
                */
                minutesElapsed = 0;
            }
            
            /*
            if (minutesElapsed2 == 5)
            {
                DateTime later = now + TimeSpan.FromMinutes(30);

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
                        if (user.TelegramChatId > 0)
                        {
                            sendTelegramUpdate(user.TelegramUserName, bets2send);
                        }
                        minutesElapsed2 = 0;
                    }
                }
            }
            */
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
}
