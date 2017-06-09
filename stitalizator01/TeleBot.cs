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

namespace stitalizator01
{
    public class TeleBot
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public List<Bet> getBetsByTelegramUserNameAndDate(string tUserName, DateTime curDate)
        {
            List<Bet> bets = db.Bets.Where(b => b.ApplicationUser.TelegramUserName == tUserName & b.Program.TvDate.Date == curDate.Date).ToList();

            return bets;
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


    }
}