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
using System.Data.Entity;
using System.Threading;

namespace stitalizator01.Controllers
{
    //[BotAuthentication(MicrosoftAppId = "ed8d437a-c850-4738-a15f-534457ad8716", MicrosoftAppPassword = "jYAUUmDx1zWwfv3L1BpmOeR")]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 
        ApplicationDbContext db = MvcApplication.db;
        private static readonly log4net.ILog logM = log4net.LogManager.GetLogger("MessagesController.cs");
        string x = "";


        public static async Task<bool> AuthenticateBotRequest(HttpRequestMessage req, Activity activity, CancellationToken token)
        {
            var credProvider = new SettingsCredentialProvider();

            var authenticator = new BotAuthenticator(credProvider, JwtConfig.ToBotFromChannelOpenIdMetadataUrl, disableEmulatorTokens: false);


            var authenticated = await authenticator.TryAuthenticateAsync(req, new[] { activity }, token);

            if (authenticated)
            {
                MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
            }
            return authenticated;
        }

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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
                            logM.Info("User " + activity.From.Name + " has called MYBETS method");
                            //Activity reply = activity.CreateReply("!!!");
                            //connector.Conversations.ReplyToActivity(reply);
                        }
                        else if (activity.Text.ToLower() == "test")
                        {
                            DateTime now = (DateTime.UtcNow + MvcApplication.utcMoscowShift).Date;
                            try
                            {
                                //List<Bet> allbets = db.Bets.Where(b => b.ApplicationUser.TelegramUserName == "amosendz" & b.Program.TvDate == now & b.BetSTIplus == 0 & !b.IsLocked).ToList();
                                ////manualTeleSend("amosendz", activity, allbets);
                                //ApplicationUser curUser = getUserFromActivity(activity);
                                ////var _connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                //if (curUser != null)
                                //{
                                //    Activity reply = activity.CreateReply("");
                                //    //DateTime curDt = DateTime.UtcNow + MvcApplication.utcMoscowShift;
                                //    //List<Bet> bets = getBetsByUserDay(curUser, curDt);
                                //    //TeleBot tb = new TeleBot();
                                //    //InlineKeyboardMarkup kb = tb.createKeabordFromBets(bets, true);
                                //    //string jsonKb = JsonConvert.SerializeObject(kb);
                                //    string text = "Ответ";
                                //    //if (bets.Count > 0)
                                //    //{
                                //    //    text = "Ставки на " + curDt.ToString("dd.MM.yyyy");
                                //    //}
                                //    //else
                                //    //{
                                //    //    text = "Открытых ставок на " + curDt.ToString("dd.MM.yyyy") + " нет";
                                //    //}

                                //    reply.ChannelData = new TelegramChannelData()                                    
                                //    {
                                //        method = "sendMessage",
                                //        parameters = new TelegramParameters()
                                //        {

                                //            text = text//,                                            
                                //            //parse_mode = "Markdown",
                                //            //reply_markup = jsonKb
                                //        }
                                //    };
                                //    connector.Conversations.ReplyToActivity(reply);
                                //}
                                MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                Activity reply = activity.CreateReply("Test reply");
                                connector.Conversations.ReplyToActivity(reply);
                                //sendBasicResponse(activity);

                            }
                            catch (Exception ex)
                            {
                                logM.Error("Something went wrong during simpler Telegram test.", ex);
                            }
                        }
                        else if (activity.Text.Length>=5)
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
                    else
                    {
                        
                        try
                        {
                            MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
                            Activity reply = activity.CreateReply("Simpler Text");
                            connector.Conversations.ReplyToActivity(reply);
                        }
                        catch (Exception ex)
                        {
                            string xx = ex.Message;
                        }
                        //string x = reply.Text;
                        //connector.Conversations.ReplyToActivity(reply);
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
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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

        private void sendBasicResponse(Activity activity)
        {
            ApplicationUser curUser = getUserFromActivity(activity);
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (curUser != null)
            {
                Activity reply = activity.CreateReply("Simpler Text");                
                TeleBot tb = new TeleBot();
                string text = "Simple text";

                //reply.ChannelData = new TelegramChannelData()
                //{
                //    method = "sendMessage",
                //    parameters = new TelegramParameters()
                //    {
                //        text = text
                //    }
                //};
                
                try
                {
                    connector.Conversations.ReplyToActivity(reply);
                }
                catch (Exception ex)
                {
                    logM.Error("Inner problem of simpler Telegram test.", ex);
                }
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
                    isHorse(curBet); //Добавлено 24.10.17 + 2 метода внизу
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
                        string msg = "";
                        msg = "User "+ curUser.UserName + " has placed a bet " + curBet.BetSTIplus + " for program "+ curBet.Program.ProgTitle + " using Telegram";
                        logM.Info(msg);
                        //stitalizator01.MvcApplication.log.Info(String.Format("User {0} has placed a bet {1} for program {3} using Telegram", curUser.UserName, curBet.BetSTIplus, curBet.Program.ProgTitle));
                        //stitalizator01.MvcApplication.log.Info("Bet placed");

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
                        //stitalizator01.MvcApplication.log.Info(String.Format("User {0} FAILED to place a bet {1} for program {3} using Telegram for obscure reasons", curUser.UserName, curBet.BetSTIplus, curBet.Program.ProgTitle));
                        string msg = "";
                        msg = "User " + curUser.UserName + " FAILED to place a bet " + curBet.BetSTIplus + " for program " + curBet.Program.ProgTitle + " using Telegram";
                        logM.Info(msg);
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
                    string msg = "";
                    msg = "User " + curUser.UserName + " FAILED to place the LOCKED bet " + curBet.BetSTIplus + " for program " + curBet.Program.ProgTitle + " using Telegram";
                    logM.Info(msg);
                    //stitalizator01.MvcApplication.log.Info(String.Format("User {0} FAILED to place the LOCKED bet {1} for program {3} using Telegram as it was LOCKED", curUser.UserName, curBet.BetSTIplus, curBet.Program.ProgTitle));
                }
            }
        }

        
        public void manualTeleSend(string userName, Activity mainActivity,List<Bet> allbets)
        {
            DateTime now = DateTime.UtcNow + MvcApplication.utcMoscowShift;
            string allbetsstr = "";// allbets.Count().ToString() + "; ";
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

                        MicrosoftAppCredentials.TrustServiceUrl(cs.ServiceUrl, DateTime.UtcNow.AddDays(7));
                        var account = new MicrosoftAppCredentials("ed8d437a-c850-4738-a15f-534457ad8716", "jYAUUmDx1zWwfv3L1BpmOeR");
                        var connector = new ConnectorClient(new Uri(cs.ServiceUrl),account);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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
                            if (cs.ApplicationUser.TelegramUserName == "amosendz")
                            {
                                allbetsstr = "The error is: " + ex.Message;
                                var connector2 = new ConnectorClient(new Uri(mainActivity.ServiceUrl), account);
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

        private void isHorse(Bet bet)
        {
            var bets = db.Bets.Where(b => b.ProgramID == bet.ProgramID).FirstOrDefault();

            if (bets != null)
            {
                var tempRes = db.Bets.Where(b => (b.ProgramID == bet.ProgramID) & (b.BetSTIplus > 0));
                double maxBet = 0;
                double minBet = 0;
                if (tempRes.Count() > 0)
                {
                    //double maxBet = db.Bets.Where(b => (b.Program.ProgramID == bet.Program.ProgramID) & (b.BetSTIplus > 0)).Max(b => b.BetSTIplus);
                    //double minBet = db.Bets.Where(b => (b.Program.ProgramID == bet.Program.ProgramID) & (b.BetSTIplus > 0)).Min(b => b.BetSTIplus);
                    minBet = tempRes.Min(b => b.BetSTIplus);
                    maxBet = tempRes.Max(b => b.BetSTIplus);
                }
                Program program = db.Programs.Find(bet.ProgramID);
                Period curPeriod = getPeriodByDate(program.TvDate, false);
                if (Math.Abs(maxBet - minBet) >= 5)
                {
                    //Увеличиваем количество очков в розыгрыше при добавлении лошадки
                    if (!program.IsHorse)
                    {
                        curPeriod.ScoresGambled += 3;
                        db.Entry(curPeriod).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    program.IsHorse = true;
                    db.Entry(program).State = EntityState.Modified;
                    db.SaveChanges();
                    //var programs = db.Bets.Select(b => {b.IsHorse})
                    //collection.Select(c => { c.PropertyToSet = value; return c; }).ToList();
                }
                else
                {
                    //Уменьшаем количество очков в розыгрыше при удалении лошадки
                    if (program.IsHorse)
                    {
                        curPeriod.ScoresGambled -= 3;
                        db.Entry(curPeriod).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    program.IsHorse = false;
                    db.Entry(program).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        private Period getPeriodByDate(DateTime dt, bool metaPeriod = false)
        {
            Period period = new Period();
            period = db.Periods.Where(p => (p.BegDate <= dt.Date) & (p.EndDate >= dt.Date) & (p.IsMetaPeriod == metaPeriod)).FirstOrDefault();
            if (period == null)
            {
                period = db.Periods.Where(p => (p.IsMetaPeriod == metaPeriod)).FirstOrDefault();
            }
            return period;
        }

    }
}
