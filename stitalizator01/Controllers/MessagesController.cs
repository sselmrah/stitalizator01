using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using stitalizator01.Models;
using Newtonsoft.Json;

using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace stitalizator01.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 
        ApplicationDbContext db = MvcApplication.db;
        

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            //MvcApplication.db.Users.Where(u => u.)
            /*
            ConversationStarter cs = new ConversationStarter();
            cs.ToId = activity.From.Id;
            cs.ToName = activity.From.Name;
            cs.FromId = activity.Recipient.Id;
            cs.FromName = activity.Recipient.Name;
            cs.ServiceUrl = activity.ServiceUrl;
            cs.ChannelId = activity.ChannelId;
            cs.ConversationId = activity.Conversation.Id;
            */


            if (activity.Type == ActivityTypes.Message)
            {
                //await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                
                if (activity.Text != null)
                {
                    if (activity.Text.Length >= 9)
                    {
                        if (activity.Text.Substring(0, 9).ToLower() == "register ")
                        {
                            registerUserForChannel(activity);
                        }
                    }
                    if (activity.ChannelId=="telegram")
                    {
                        if (activity.Text == "/mybets")
                        {
                            sendUserBetsTelegram(activity);
                            //Activity reply = activity.CreateReply("!!!");
                            //connector.Conversations.ReplyToActivity(reply);
                        }
                        else if (activity.Text == "test")
                        {
                            manualTeleSend("amosendz", activity);
                        }
                        if (activity.Text.Length>=5)
                        {
                            if (activity.Text.Substring(0,5)=="betId")
                            {
                                selectBetTelegram(activity);
                            }
                        }
                        if (Regex.IsMatch(activity.Text, "^(?=.*\\d)\\d*[\\.\\,]?\\d*$"))
                        {
                            placeBetTelegram(activity);
                        }
                    }
                    else if (activity.ChannelId=="skype")
                    {

                    }
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private void registerUserForChannel(Activity activity)
        {
            string givenUserName = activity.Text.Substring(9);
            var curUser = db.Users.Where(u => u.UserName.ToLower() == givenUserName.ToLower()).FirstOrDefault();
            //ApplicationUser curUser = getUserFromActivity(activity);
            if (curUser != null)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                ConversationStarter cs = new ConversationStarter();
                cs.ToId = activity.From.Id;
                cs.ToName = activity.From.Name;
                cs.FromId = activity.Recipient.Id;
                cs.FromName = activity.Recipient.Name;
                cs.ServiceUrl = activity.ServiceUrl;
                cs.ChannelId = activity.ChannelId;
                cs.ConversationId = activity.Conversation.Id;
                cs.ApplicationUser = curUser;
                cs.LastTimeUsed = DateTime.UtcNow - TimeSpan.FromDays(1);


                bool found = false;
                var css = db.CSs.Where(c => c.ApplicationUser.UserName == curUser.UserName);
                ConversationStarter csToRemove = new ConversationStarter();
                List<ConversationStarter> csList = css.ToList();
                foreach (ConversationStarter c in csList)
                {
                    if (c.ChannelId == cs.ChannelId)
                    {
                        found = true;
                        csToRemove = c;
                        break;
                    }
                }
                if (found)
                {                   
                    db.CSs.Remove(csToRemove);
                }
                

                db.CSs.Add(cs);
                db.SaveChanges();                    


                string text = "Для пользователя " + curUser.UserName + " зарегистрирован канал связи:\nUserName - " + cs.ToName + ", UserId - " + cs.ToId + ", ChannelID - " + cs.ChannelId;
                Activity reply = activity.CreateReply(text);
                connector.Conversations.ReplyToActivity(reply);
            }
        }

        private void sendUserBetsTelegram(Activity activity)
        {
            ApplicationUser curUser = getUserFromActivity(activity);
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (curUser != null)
            {
                Activity reply = activity.CreateReply("");
                DateTime curDt = DateTime.UtcNow + MvcApplication.utcMoscowShift;
                List<Bet> bets = getBetsByUserDay(curUser, curDt);
                TeleBot tb = new TeleBot();
                InlineKeyboardMarkup kb = tb.createKeabordFromBets(bets,true);
                string jsonKb = JsonConvert.SerializeObject(kb);
                string text = "";
                if (bets.Count>0)
                {
                    text = "Ставки на " + curDt.ToString("dd.MM.yyyy");
                }
                else
                {
                    text = "Открытых ставок на " + curDt.ToString("dd.MM.yyyy") + " нет";
                }

                reply.ChannelData = new TelegramChannelData()
                {
                    method = "sendMessage",
                    parameters = new TelegramParameters()
                    {
                        text = text,
                        parse_mode = "Markdown",
                        reply_markup = jsonKb
                    }
                };
                connector.Conversations.ReplyToActivity(reply);
            }
        }

        private void selectBetTelegram(Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            Activity reply = activity.CreateReply("");
            string betId = activity.Text.Substring(6);
            Bet b = db.Bets.Find(Convert.ToInt32(betId));
            ApplicationUser curUser = getUserFromActivity(activity);
            if (b.ApplicationUser == curUser)
            {
                curUser.TelegramBetId = Convert.ToInt32(betId);
                db.SaveChanges();
                string text = "Сколько ставим на \"" + b.Program.ProgTitle + "\"?";

                reply.ChannelData = new TelegramChannelData()
                {
                    method = "sendMessage",
                    parameters = new TelegramParameters()
                    {
                        text = text
                    }
                };
                connector.Conversations.ReplyToActivity(reply);
            }
        }

        private void placeBetTelegram(Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            Activity reply = activity.CreateReply("");
            ApplicationUser curUser = getUserFromActivity(activity);
            string betStr = activity.Text.Replace(',', '.');
            if (curUser.TelegramBetId > 0)
            {
                Bet curBet = db.Bets.Find(curUser.TelegramBetId);
                if (!curBet.IsLocked)
                {
                    float telegramBet = Convert.ToSingle(betStr, CultureInfo.InvariantCulture.NumberFormat);
                    curBet.BetSTIplus = telegramBet;
                    db.SaveChanges();
                    if (curBet.BetSTIplus == telegramBet)
                    {
                        string text = "Принято: \n\"" + curBet.Program.ProgTitle + "\" - " + curBet.BetSTIplus.ToString();
                        reply.ChannelData = new TelegramChannelData()
                        {
                            method = "sendMessage",
                            parameters = new TelegramParameters()
                            {
                                text = text
                            }
                        };
                        connector.Conversations.ReplyToActivity(reply);
                    }
                    else
                    {
                        string text = "Что-то пошло не так. Ставка не принята.";
                        reply.ChannelData = new TelegramChannelData()
                        {
                            method = "sendMessage",
                            parameters = new TelegramParameters()
                            {
                                text = text
                            }
                        };
                        connector.Conversations.ReplyToActivity(reply);
                    }
                    curUser.TelegramBetId = 0;
                    db.SaveChanges();
                }
                else
                {
                    string text = "Ставка \"" + curBet.Program.ProgTitle + "\" заблокирована. Поставить не удалось.";
                    reply.ChannelData = new TelegramChannelData()
                    {
                        method = "sendMessage",
                        parameters = new TelegramParameters()
                        {
                            text = text
                        }
                    };
                    connector.Conversations.ReplyToActivity(reply);
                }
            }
        }

        
        public void manualTeleSend(string userName, Activity mainActivity)
        {
            DateTime now = DateTime.UtcNow + MvcApplication.utcMoscowShift;
            List<Bet> allbets = db.Bets.Where(b => b.ApplicationUser.TelegramUserName == userName & b.Program.TvDate == now.Date & b.BetSTIplus == 0 & !b.IsLocked ).ToList();
            string allbetsstr = allbets.Count().ToString() + "; ";
            List<ConversationStarter> css = db.CSs.ToList();

            if (css.Count() > 0)
            {
                foreach (ConversationStarter cs in css)
                { 
                    if (cs.ChannelId == "telegram" & cs.ApplicationUser.TelegramUserName == userName)
                    {
                        ApplicationUser curUser = cs.ApplicationUser;


                        var userAccount = new ChannelAccount(cs.ToId, cs.ToName);
                        var botAccount = new ChannelAccount(cs.FromId, cs.FromName);
                        var connector = new ConnectorClient(new Uri(cs.ServiceUrl));

                        Activity activity = new Activity();
                        activity.From = botAccount;
                        activity.Recipient = userAccount;
                        activity.Conversation = new ConversationAccount(id: cs.ConversationId);
                        activity.Type = ActivityTypes.Message;
                        activity.Id = "1";

                        string text = "Нужно сделать ставки!";
                        TeleBot tb = new TeleBot();
                        InlineKeyboardMarkup kb = tb.createKeabordFromBets(allbets, true);
                        string jsonKb = JsonConvert.SerializeObject(kb);
                        activity.ChannelData = new TelegramChannelData()
                        {
                            method = "sendMessage",
                            parameters = new TelegramParameters()
                            {
                                text = text,
                                parse_mode = "Markdown",
                                reply_markup = jsonKb
                            }
                        };

                        allbetsstr += activity.Recipient.Name.ToString() + "-" + activity.Recipient.Id.ToString() + ". ";
                        try
                        {
                            connector.Conversations.SendToConversation(activity);
                        }
                        catch (Exception ex)
                        {
                            allbetsstr = "The error is: "+ex.Message;
                            var connector2 = new ConnectorClient(new Uri(mainActivity.ServiceUrl));
                            Activity reply = mainActivity.CreateReply("");
                            
                            reply.ChannelData = new TelegramChannelData()
                            {
                                method = "sendMessage",
                                parameters = new TelegramParameters()
                                {
                                    text = allbetsstr
                                }
                            };
                            connector2.Conversations.ReplyToActivity(reply);
                        }
                    }
                }
            }   
        }

        
        private void basicTelegramReminder(ConversationStarter cs)
        {
            ApplicationUser curUser = cs.ApplicationUser;


            var userAccount = new ChannelAccount(cs.ToId, cs.ToName);
            var botAccount = new ChannelAccount(cs.FromId, cs.FromName);
            var connector = new ConnectorClient(new Uri(cs.ServiceUrl));

            Activity activity = new Activity();
            activity.Type = ActivityTypes.Message;
            activity.From = botAccount;
            activity.Recipient = userAccount;
            activity.Conversation = new ConversationAccount(id: cs.ConversationId);
            activity.Id = "1";
            string text = "Нужно сделать ставки!";

            DateTime curDate = (DateTime.UtcNow + MvcApplication.utcMoscowShift).Date;
            List<Bet> bets = db.Bets.Where(b => b.ApplicationUser.UserName == curUser.UserName & b.Program.TvDate == curDate & !b.IsLocked).ToList();
            TeleBot tb = new TeleBot();
            InlineKeyboardMarkup kb = tb.createKeabordFromBets(bets, true);
            string jsonKb = JsonConvert.SerializeObject(kb);
            activity.ChannelData = new TelegramChannelData()
            {
                method = "sendMessage",
                parameters = new TelegramParameters()
                {
                    text = text,
                    parse_mode = "Markdown",
                    reply_markup = jsonKb
                }
            };
            connector.Conversations.SendToConversation(activity);
        }
        

        private ApplicationUser getUserFromActivity(Activity activity)
        {
            ApplicationUser curUser = null;
            //string givenUserName = activity.Text.Substring(9);
            //curUser = db.Users.Where(u => u.UserName.ToLower() == givenUserName.ToLower()).FirstOrDefault();
            var cs = db.CSs.Where(c => c.ToId == activity.From.Id).FirstOrDefault();
            curUser = cs.ApplicationUser;

            return curUser;
        }
        public List<Bet> getBetsByUserDay(ApplicationUser curUser, DateTime curDate)
        {
            curDate = curDate.Date;
            List<Bet> bets = db.Bets.Where(b => b.ApplicationUser.UserName == curUser.UserName & b.Program.TvDate == curDate & !b.IsLocked).ToList();
            return bets;
        }

    }
}
