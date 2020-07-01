using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Surbgirq.TeleBot.Services
{
    public interface IBotClient
    {
        TelegramBotClient Client { get; }
    }

}
