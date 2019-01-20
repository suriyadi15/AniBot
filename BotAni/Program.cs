using BotAni.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotAni
{
    class Program
    {
        static TelegramBotClient botClient;
        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("744605487:AAHW4BRy_bkkT6Ocg0MZnFX9fg9q18wthOo");

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += BotOnMessageReceived;
            botClient.OnMessageEdited += BotOnMessageReceived;
            botClient.OnCallbackQuery += BotOnCallbackQueryReceived;
            //botClient.OnInlineQuery += BotOnInlineQueryReceived;
            //botClient.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            botClient.OnReceiveError += BotOnReceiveError;

            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
        }
        
        private static List<List<InlineKeyboardButton>> getAnimes()
        {
            return AniService.GetAnimes().Result
                        .Select(anime => new List<InlineKeyboardButton>{
                            InlineKeyboardButton.WithCallbackData(
                                anime.Title,
                                $"/anime {new Uri(anime.Url).Segments.Last().Trim('/')}"
                            )
                        })
                        .ToList();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return;

            switch (message.Text.Split(' ').First())
            {
                // send inline keyboard
                case "/list":
                    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                    List<List<InlineKeyboardButton>> data = getAnimes();

                    await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "List Anime",
                        replyMarkup: new InlineKeyboardMarkup(data.Take(10)));

                    data = data.Skip(10).ToList();
                    while (data.Any())
                    {
                        await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Next List Anime",
                        replyMarkup: new InlineKeyboardMarkup(data.Take(10)));
                        data = data.Skip(10).ToList();
                    }
                    break;
                default:
                    const string usage = @"
Usage:
/list   - get all list anime";

                    await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        usage,
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            //await botClient.AnswerCallbackQueryAsync(
            //                callbackQuery.Id,
            //                $"Received {callbackQuery.Data}");
            await botClient.SendChatActionAsync(callbackQuery.Message.Chat.Id, ChatAction.Typing);

            var dataQuery = callbackQuery.Data.Split(' ');
            switch (dataQuery[0])
            {
                case "/list":
                    if(dataQuery.Length == 3 && dataQuery[1] == "next")
                    {
                        int nextIndex = Convert.ToInt32(dataQuery[2]);
                        List<List<InlineKeyboardButton>> data = getAnimes()
                            .Skip(nextIndex - 1)
                            .Take(5)
                            .ToList();

                        if (data.Count >= 10)
                        {
                            data.Add(new List<InlineKeyboardButton> {
                                InlineKeyboardButton.WithCallbackData(
                                        "Next",
                                        $"/list next {nextIndex + 10}"
                                    )
                            });
                        }
                        InlineKeyboardMarkup listAnime = new InlineKeyboardMarkup(data);
                        await botClient.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            $"{(data.Count >= 10 ? "Next" : "Last")} List Anime",
                            replyMarkup: listAnime);
                        return;

                    }
                    break;
                case "/anime":
                    DataAnime anime = AniService.GetAnime(dataQuery[1]).Result;
                    if(anime != null)
                    {
                        await botClient.SendPhotoAsync(
                            callbackQuery.Message.Chat.Id,
                            new Telegram.Bot.Types.InputFiles.InputOnlineFile(anime.Image),
                            $@"{anime.Title}");
                        await botClient.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            $@"
description: {anime.Description}
type : {anime.Type}
status: {anime.Status}
release Date: {anime.ReleaseDate}
producer: {anime.Producer}");

                        if(anime.Episode != null && anime.Episode.Count > 0)
                        {
                            var episodes = anime.Episode;
                            while (episodes.Any())
                            {
                                await botClient.SendTextMessageAsync(
                                    callbackQuery.Message.Chat.Id,
                                    "Episode",
                                    replyMarkup: new InlineKeyboardMarkup(
                                        new [] {
                                            episodes.Take(10).Select(x => InlineKeyboardButton.WithCallbackData(
                                                x.No,
                                                $"/ep {(new Uri(x.Url).Segments.Last().Trim('/'))}"))
                                        }
                                    ));
                                episodes = episodes.Skip(10).ToList();
                            }
                            return;
                        }
                    }
                    break;
                case "/ep":
                    var episode = AniService.GetEpisode(dataQuery[1]).Result;
                    if(episode != null)
                    {
                        //var media = episode.Video.Select(x => new InputMediaVideo(new InputMedia(x)));
                        //await botClient.SendMediaGroupAsync(media, callbackQuery.Message.Chat.Id);
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                            $@"
Link Video:
{(string.Join('\n', episode.Video))}
");
                        return;
                    }
                    break;
            }
            await botClient.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Invalid Query data: {callbackQuery.Data}");
        }

        //private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        //{
        //    Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

        //    InlineQueryResultBase[] results = {
        //        new InlineQueryResultLocation(
        //            id: "1",
        //            latitude: 40.7058316f,
        //            longitude: -74.2581888f,
        //            title: "New York")   // displayed result
        //            {
        //                InputMessageContent = new InputLocationMessageContent(
        //                    latitude: 40.7058316f,
        //                    longitude: -74.2581888f)    // message if result is selected
        //            },

        //        new InlineQueryResultLocation(
        //            id: "2",
        //            latitude: 13.1449577f,
        //            longitude: 52.507629f,
        //            title: "Berlin") // displayed result
        //            {

        //                InputMessageContent = new InputLocationMessageContent(
        //                    latitude: 13.1449577f,
        //                    longitude: 52.507629f)   // message if result is selected
        //            }
        //    };

        //    await botClient.AnswerInlineQueryAsync(
        //        inlineQueryEventArgs.InlineQuery.Id,
        //        results,
        //        isPersonal: true,
        //        cacheTime: 0);
        //}

        //private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        //{
        //    Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        //}

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }
    }
}
