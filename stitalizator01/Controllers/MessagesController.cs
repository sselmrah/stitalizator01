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
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                
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
                            Activity reply = activity.CreateReply("!!!");
                            connector.Conversations.ReplyToActivity(reply);
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
            //string givenUserName = activity.Text.Substring(9);
            //var curUser = db.Users.Where(u => u.UserName.ToLower() == givenUserName.ToLower()).FirstOrDefault();
            ApplicationUser curUser = getUserFromActivity(activity);
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
                DateTime curDt = DateTime.Now + MvcApplication.utcMoscowShift;
                List<Bet> bets = getBetsByUserDay(curUser, curDt);
                TeleBot tb = new TeleBot();
                InlineKeyboardMarkup kb = tb.createKeabordFromBets(bets);
                string jsonKb = JsonConvert.SerializeObject(kb);

                reply.ChannelData = new TelegramChannelData()
                {
                    method = "sendMessage",
                    parameters =
                    {
                        text = "Ставки на " + curDt.ToString("dd.MM.yyyy"),
                        reply_markup = jsonKb
                    }
                };
                connector.Conversations.ReplyToActivity(reply);
            }
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
