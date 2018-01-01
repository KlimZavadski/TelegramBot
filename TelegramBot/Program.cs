using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class MainClass
    {
        private static readonly TelegramBotClient TelegramBot = new TelegramBotClient(Resources.TelegramBotToken);

        static void Main(string[] args)
        {
            TelegramBot.OnMessage += OnMessageReceived;
            TelegramBot.OnMessageEdited += OnMessageReceived;
            TelegramBot.OnCallbackQuery += OnCallbackQueryReceived;
            TelegramBot.OnInlineQuery += OnInlineQueryReceived;
            TelegramBot.OnInlineResultChosen += OnChosenInlineResultReceived;
            TelegramBot.OnReceiveError += OnErrorReceived;

            var me = TelegramBot.GetMeAsync().Result;

            Console.Title = me.FirstName;

            TelegramBot.StartReceiving();
            Console.ReadLine();
            TelegramBot.StopReceiving();
        }

        private static async void OnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage)
            {
                return;
            }

            var text = message.Text;
            Console.WriteLine($"OnMessageReceived: {message.From.FirstName}-{message.Text}");

            if (text.StartsWith("/start", StringComparison.InvariantCultureIgnoreCase))  // start
            {
                var usage = "Usage:\n" +
                    "/inline - send inline keyboard\n" +
                    "/keyboard - send custom keyboard\n" +
                    "/photo    - send a photo\n" +
                    "/request  - request location or contact\n";

                await TelegramBot.SendTextMessageAsync(message.Chat.Id, usage, replyMarkup: new ReplyKeyboardRemove());
            }
            if (message.Text.StartsWith("/inline", StringComparison.InvariantCultureIgnoreCase))  // send inline keyboard
            {
                await TelegramBot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                var keyboard = new InlineKeyboardMarkup(new[] {
                    new[] {  // first row
                        new InlineKeyboardCallbackButton("1.1", "callback_1.1"),
                        new InlineKeyboardCallbackButton("1.2", "callback_1.2"),
                    },
                    new[] {  // second row
                        new InlineKeyboardCallbackButton("2.1", "callback_2.1"),
                        new InlineKeyboardCallbackButton("2.2", "callback_2.2")
                    }
                });

                await Task.Delay(500);  // simulate longer running task

                await TelegramBot.SendTextMessageAsync(message.Chat.Id, "Choose", replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/keyboard", StringComparison.InvariantCultureIgnoreCase))  // send custom keyboard
            {
                var keyboard = new ReplyKeyboardMarkup(new[] {
                    new [] {  // first row
                        new KeyboardButton("1.1"),
                        new KeyboardButton("1.2")
                    },
                    new [] {  // last row
                        new KeyboardButton("2.1"),
                        new KeyboardButton("2.2")
                    }
                });

                await TelegramBot.SendTextMessageAsync(message.Chat.Id, "Choose", replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/photo", StringComparison.InvariantCultureIgnoreCase))  // send a photo
            {
                await TelegramBot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                //const string file = @"<FilePath>";

                //var fileName = file.Split('\\').Last();

                //using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                //{
                //    var fts = new FileToSend(fileName, fileStream);

                //    await TelegramBot.SendPhotoAsync(message.Chat.Id, fts, "Nice Picture");
                //}
            }
            else if (message.Text.StartsWith("/request", StringComparison.InvariantCultureIgnoreCase))  // request location or contact
            {
                var keyboard = new ReplyKeyboardMarkup(new[] {
                    new KeyboardButton("Location") {
                        RequestLocation = true
                    },
                    new KeyboardButton("Contact") {
                        RequestContact = true
                    }
                });

                await TelegramBot.SendTextMessageAsync(message.Chat.Id, "Who or Where are you?", replyMarkup: keyboard);
            }
            else
            {
                await TelegramBot.SendTextMessageAsync(message.Chat.Id, text);
            }
        }

        private static async void OnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var query = callbackQueryEventArgs.CallbackQuery;
            Console.WriteLine($"OnCallbackQueryReceived: {query.From.FirstName}-{query.Data}");

            await TelegramBot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }

        private static async void OnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            var query = inlineQueryEventArgs.InlineQuery;
            Console.WriteLine($"OnMessageReceived: {query.From.FirstName}-{query.Query}");

            InlineQueryResult[] results = {
                new InlineQueryResultLocation {
                    Id = "1",
                    Latitude = 40.7058316f, // displayed result
                    Longitude = -74.2581888f,
                    Title = "New York",
                    InputMessageContent = new InputLocationMessageContent {  // message if result is selected
                        Latitude = 40.7058316f,
                        Longitude = -74.2581888f,
                    }
                },
                new InlineQueryResultLocation {
                    Id = "2",
                    Longitude = 52.507629f, // displayed result
                    Latitude = 13.1449577f,
                    Title = "Berlin",
                    InputMessageContent = new InputLocationMessageContent {  // message if result is selected
                        Longitude = 52.507629f,
                        Latitude = 13.1449577f
                    }
                }
            };

            await TelegramBot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results, isPersonal: true, cacheTime: 0);
        }

        private static void OnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void OnErrorReceived(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }
    }
}
