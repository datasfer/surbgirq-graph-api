using Microsoft.Extensions.Options;
using Surbgirq.TeleBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Surbgirq.TeleBot.Services
{
    public class BotClient : IBotClient, IDisposable
    {
        private readonly BotConfiguration _config;

        public BotClient(IOptions<BotConfiguration> config)
        {
            _config = config.Value;
#if USE_PROXY
            var Proxy = new WebProxy(_config.Proxy.Host, _config.Proxy.Port) { UseDefaultCredentials = true };
            Client = new TelegramBotClient(_config.BotToken, webProxy: Proxy);
#else
            Client = new TelegramBotClient(_config.BotToken);

        
            Client.SetWebhookAsync($"https://bot.datasfer.com/{_config.BotToken}/WebHook").Wait();

#endif

        }

        public TelegramBotClient Client { get; }

        public void Dispose()
        {
            Client.DeleteWebhookAsync().Wait();
        }
    }
}
