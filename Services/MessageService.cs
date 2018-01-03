﻿using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Codewars_Bot.Services
{
	public class MessageService
	{
		public async Task<string> MessageHandler(Activity activity)
		{
			var databaseConnectionService = new DatabaseConnectionService();
			databaseConnectionService.AuditMessageInDatabase(JsonConvert.SerializeObject(activity));

			switch (activity.Text)
			{
				case "/weekly_rating":
					return GetWeeklyRating();
				case "/total_rating":
					return GetGeneralRating();
				case "/start":
					return ShowFaq();
				case "/show_faq":
					return ShowFaq();
				default:
					return await SaveNewUser(activity);
			}

		}

		private async Task<string> SaveNewUser(Activity activity)
		{
			if ((bool)activity.Conversation.IsGroup)
			{
				return string.Empty;
			}

			var databaseConnectionService = new DatabaseConnectionService();
			var codewarsConnectionService = new CodewarsConnectionService();

			var user = new UserModel
			{
				CodewarsUsername = activity.Text,
				TelegramUsername = activity.From.Name,
				TelegramId = int.Parse(activity.From.Id)
			};

			if (databaseConnectionService.CheckIfUserExists(user) || user.CodewarsUsername == "start")
			{
				return $"Користувач {user.CodewarsUsername} вже зареєстрований";
			}

			var codewarsUser = await codewarsConnectionService.GetCodewarsUser(user.CodewarsUsername);

			if (codewarsUser.Name == "NOT EXIST")
			{
				return $"Користувач {user.CodewarsUsername} не зареєстрований на codewars.com";
			}
			else
			{
				user.CodewarsFullname = codewarsUser.Name;
				user.Points = codewarsUser.Honor;
			}

			return databaseConnectionService.SaveUserToDatabase(user);
		}

		private string GetWeeklyRating()
		{
			var databaseConnectionService = new DatabaseConnectionService();
			return databaseConnectionService.GetWeeklyRating();
		}

		private string GetGeneralRating()
		{
			var databaseConnectionService = new DatabaseConnectionService();
			return databaseConnectionService.GetGeneralRating();
		}

		private string ShowFaq()
		{
			var response = new StringBuilder();

			response.Append("Вітаємо в клані ІТ КРІ на Codewars! <br/><br/>codewars.com -- це знаменитий сайт з задачами для програмістів, за розв'язок яких нараховуються бали. ");
			response.Append("От цими балами ми і будемо мірятись в кінці кожного тижня. <br/><br/>Цей бот створений для того, щоб зробити реєстрацію в клані максимально швидкою і приємною. ");
			response.Append("Щоб долучитись до рейтингу треба: <br/>1) Зареєструватись на https://codewars.com <br/>2) Надіслати сюди ваш нікнейм в Codewars.");
			response.Append("<br/><br/>Бали оновлюються раз на годину. Також доступні дві команди: <br/>1) /weekly_rating показує поточний рейтинг за цей тиждень. <br/>2) /total_rating відображає загальну кількість балів в кожного користувача");
			response.Append("<br/><br/>Запрошуйте друзів в клан і гайда рубитись!");
			response.Append("<br/><br/>P.S: якщо знайшли багу або маєте зауваження -- пишіть йому @maksim36ua");

			response.Append("");
			return response.ToString();
		}
	}
}