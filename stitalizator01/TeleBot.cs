using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using stitalizator01.Models;


using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Globalization;

namespace stitalizator01
{
    public class TeleBot
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public static TimeSpan utcMoscowShift = TimeSpan.FromHours(3);
        private static string key = "385340523:AAFPdWdVpE_oI4gLn8Z0XCb2_q-zaVVzP24";
        public static System.Timers.Timer timer = new System.Timers.Timer(1000);
        private Telegram.Bot.TelegramBotClient Bot = new Telegram.Bot.TelegramBotClient(key);
        private int offset = 0; // отступ по сообщениям
        private List<ApplicationUser> users = new List<ApplicationUser>();
        private List<Bet> allbets = new List<Bet>();
        private Regex exp = new Regex("^(?=.*\\d)\\d*[\\.\\,]?\\d*$");
        private long secondsElapsed = 0;

        public async void StartBot()
        {
            try
            {
                await Bot.SetWebhookAsync("");
                users = db.Users.ToList();
                allbets = db.Bets.Where(b => b.BetSTIplus == 0 & !b.IsLocked).ToList();

                timer.Enabled = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            }
            catch
            {

            }

        }

        private async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            secondsElapsed += 1;
            DateTime now = DateTime.UtcNow + utcMoscowShift;
            if (secondsElapsed == 60*60)
            {
                foreach (ApplicationUser user in users)
                {
                    if (user.TelegramChatId > 0)
                    {
                        await sendTelegramUpdateBets(Bot, user);
                    }
                }
                allbets = db.Bets.Where(b => b.BetSTIplus == 0 & !b.IsLocked).ToList();
                secondsElapsed = 0;
            }

            if ((secondsElapsed == 15*60) | (secondsElapsed == 30*60) | (secondsElapsed == 30*60))
            {
                allbets = db.Bets.Where(b => b.BetSTIplus == 0 & !b.IsLocked).ToList();
            }

            var updates = await Bot.GetUpdatesAsync(offset);
            if (updates != null)
            {
                if (updates.Count() > 0)
                {
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
                                else if (message.Text == "/saysomething")
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
                                    await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: reply, replyMarkup: kb);

                                }
                                else if (Regex.IsMatch(message.Text, "^(?=.*\\d)\\d*[\\.\\,]?\\d*$"))
                                {
                                    string betStr = message.Text.Replace(',', '.');
                                    if (curUser.TelegramBetId > 0)
                                    {
                                        Bet curBet = db.Bets.Find(curUser.TelegramBetId);
                                        if (!curBet.IsLocked)
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
                                            string reply = "Ставка \"" + curBet.Program.ProgTitle + "\" заблокирована. Поставить не удалось.";
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: reply);
                                        }
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

                                if (update.CallbackQuery.Data.Length > 5)
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
                                    await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Что-то пошло не так...");
                                }
                            }
                        }
                        offset = update.Id + 1;
                    }
                }
            }


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
                curButton.Url = "https://stitalizator.azurewebsites.net";
                InlineKeyboardButton[] row = new InlineKeyboardButton[1];
                row[0] = curButton;
                rows.Add(row);
            }


            kb.InlineKeyboard = rows.ToArray();

            return kb;
        }

        public List<Bet> getBetsByTelegramUserNameAndDate(string tUserName, DateTime curDate)
        {
            List<Bet> bets = db.Bets.Where(b => b.ApplicationUser.TelegramUserName == tUserName & b.Program.TvDate == curDate & !b.IsLocked).ToList();

            return bets;
        }
        
        async Task sendTelegramUpdateBets(Telegram.Bot.TelegramBotClient Bot, ApplicationUser user)
        {
            if (user.TelegramChatId > 0)
            {
                //var Bot = new Telegram.Bot.TelegramBotClient(key);
                //await Bot.SetWebhookAsync("");
                //string telegramUserName = user.TelegramUserName;
                string text = "Нужно сделать ставки: ";
                DateTime now = DateTime.UtcNow + utcMoscowShift;
                DateTime later = now + TimeSpan.FromMinutes(120);
                bool timeToRemind = false;
                //List<string> bets2send = new List<string>();
                //List<Bet> burningBets = db.Bets.Where(b => b.ApplicationUser == user & b.BetSTIplus == 0 & !b.IsLocked).ToList();
                List<Bet> bets2send = new List<Bet>();
                foreach (Bet b in allbets)
                {
                    if (b.ApplicationUser.TelegramUserName == user.TelegramUserName & b.Program.TimeStart >= now & b.Program.TvDate == now.Date)
                    {
                        //string betDescription = b.Program.ProgTitle + "(" + b.Program.TimeStart.ToString("HH:mm") + ") " + b.Program.ChannelCode;
                        //bets2send.Add(betDescription);
                        bets2send.Add(b);
                        if (b.Program.TimeStart < later)
                        {
                            timeToRemind = true;
                        }
                    }
                }

                InlineKeyboardMarkup kb = createKeabordFromBets(bets2send, true);

                if (timeToRemind)
                {
                    await Bot.SendTextMessageAsync(chatId: user.TelegramChatId, text: text, replyMarkup: kb);
                }
            }
        }
    }
}