using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions;
using SpaceBot.Clients;
using SpaceBot.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace SpaceBot
{
    internal class SpacePhotoBot
    {
        TelegramBotClient botClient = new TelegramBotClient(Constants.BotID);
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions{ AllowedUpdates = { } };
        private static Dictionary<long, string> editingPhotoDates = new Dictionary<long, string>();

        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMe(); 
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            await Task.Delay(-1);
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellation)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}", _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellation)
        {
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await HandlerCallbackAsync(botClient, update.CallbackQuery);
            }
        }
        
        private async Task HandlerCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var data = callbackQuery.Data;

            if (data.StartsWith("EditNotes:"))
            {
                string date = data.Split(':')[1];
                editingPhotoDates[chatId] = date;
                await botClient.SendMessage(chatId, "Введіть зміни у форматі: Нова назва / новий опис");
                Console.WriteLine($"Очікується введення даних для зміни фотографії {date}");
            }
            else 
            if (data.StartsWith("DeletePhoto:"))
            {
                string date = data.Split(':')[1];
                SpaceClient spaceClient = new SpaceClient();
                await spaceClient.DeleteDatabasePhoto(date);
                await botClient.SendMessage(chatId, "Фотографію видалено з архіву");
                Console.WriteLine($"Видалено фотографію з датою {date}");
            }
            else 
            if (data.StartsWith("SaveThisPhoto:"))
            {
                string date = data.Split(':')[1];
                SpaceClient spaceClient = new SpaceClient();
                await spaceClient.PostDatabasePhoto(date);
                await botClient.SendMessage(chatId, "Фотографію збережено до архіву");
                Console.WriteLine($"Збережено фотографію з датою {date}");
            }
        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                await botClient.SendMessage(message.Chat.Id, "Виберіть команду /keyboard");
            }
            else
            if (message.Text == "/keyboard")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup
                    (
                    new[]
                        {
                        new KeyboardButton [] {"DailyPhoto", "RandomPhoto"},
                        new KeyboardButton [] { "ShowSavedPhotos" }
                        }
                    )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: replyKeyboardMarkup);
                return;
            }
            else
            if (message.Text == "DailyPhoto")
            {
                SpaceClient spaceClient = new SpaceClient();
                SpacePhoto photo = await spaceClient.GetPhoto();
                InlineKeyboardMarkup keyboardMarkup = new
                    (
                            InlineKeyboardButton.WithCallbackData("Save this photo", $"SaveThisPhoto:{photo.date}")
                    );
                await botClient.SendPhoto(message.Chat.Id, photo.url);
                await botClient.SendMessage(message.Chat.Id, $"{photo.title}\nДата завантаження: {photo.date}\n{photo.explanation}", replyMarkup: keyboardMarkup);
                Console.WriteLine($"Бот надіслав фотографію дня");
                return;
            }
            else
            if (message.Text == "RandomPhoto")
            {
                SpaceClient spaceClient = new SpaceClient();
                SpacePhoto photo = await spaceClient.GetRandomPhoto();
                InlineKeyboardMarkup keyboardMarkup = new
                    (
                            InlineKeyboardButton.WithCallbackData("Save this photo", $"SaveThisPhoto:{photo.date}")
                    );
                await botClient.SendPhoto(message.Chat.Id, photo.url);
                await botClient.SendMessage(message.Chat.Id, $"{photo.title}\nДата завантаження: {photo.date}\n{photo.explanation}", replyMarkup: keyboardMarkup);
                Console.WriteLine($"Бот надіслав випадкову фотографію");
                return;
            }
            else
            if (message.Text == "ShowSavedPhotos")
            {
                SpaceClient spaceClient = new SpaceClient();
                SpacePhoto[] photos = await spaceClient.GetDatabasePhoto();
                if (photos.Length == 0)
                {
                    await botClient.SendMessage(message.Chat.Id, "В базі даних відсутні фотографії");
                    Console.WriteLine("База даних пуста");
                }
                else 
                {
                    foreach (SpacePhoto photo in photos)
                    {
                        InlineKeyboardMarkup keyboardMarkup = new
                    (
                            InlineKeyboardButton.WithCallbackData("Edit notes", $"EditNotes:{photo.date}"),
                            InlineKeyboardButton.WithCallbackData("Delete photo", $"DeletePhoto:{photo.date}")
                    );
                        await botClient.SendPhoto(message.Chat.Id, photo.url);
                        await botClient.SendMessage(message.Chat.Id, $"{photo.title}\nДата завантаження: {photo.date}\n{photo.explanation}", replyMarkup: keyboardMarkup);
                    }
                    Console.WriteLine("Бот надіслав фотографії з бази даних");
                }
                return;
            }
            else
            if(editingPhotoDates.ContainsKey(message.Chat.Id))
            {
                string date = editingPhotoDates[message.Chat.Id];
                if (message.Text.Contains('/'))
                {
                    SpaceClient spaceClient = new SpaceClient();
                    string[] text = message.Text.Split('/');
                    string title = text[0].Trim();
                    string explanation = text[1].Trim();

                    await spaceClient.PutDatabasePhoto(title, explanation, date);
                    await botClient.SendMessage(message.Chat.Id, "Записи в архіві відредаговано");
                    editingPhotoDates.Remove(message.Chat.Id);
                    Console.WriteLine($"Відредаговано записи до фотографії {date}");
                }
                else
                {
                    await botClient.SendMessage(message.Chat.Id, "Формат запиту некоректний, спробуйте ще раз");
                    Console.WriteLine($"Некоректна спроба внесення редагувань до фотографії {date}");
                }
            }
        }
    }
}