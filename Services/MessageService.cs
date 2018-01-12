﻿using Codewars_Bot.Contracts;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codewars_Bot.Services
{
	public class MessageService : IMessageService
	{

		public async Task<string> MessageHandler(Activity activity)
		{
			var databaseConnectionService = new DatabaseConnectionService();
			var requestContent = new
			{
				UserId = activity.From.Id,
				UserName = activity.From.Name,
				Message = activity.Text
			};

			databaseConnectionService.AuditMessageInDatabase(JsonConvert.SerializeObject(requestContent));

			switch (activity.Text)
			{
				case "/weekly_rating":
					return GetWeeklyRating();
				case "/total_rating":
					return GetGeneralRating();
				case "/weekly_rating_channel":
					return GetWeeklyRatingForChannel();
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
				return string.Empty;

			var databaseConnectionService = new DatabaseConnectionService();
			var codewarsConnectionService = new CodewarsConnectionService();
			var regex = new Regex(@"^\w+$", RegexOptions.IgnoreCase);

			if (!regex.Match(activity.Text).Success)
			{
				return $@"Логін Codewars має містити букви, цифри і знак '_' <br/><br/>
					Якщо ви хотіли дати команду боту -- перевірте, чи в ту сторону стоїть слеш на початку.
					Певні, що це таки ваш нік? Пишіть йому: @maksim36ua";
			}

			var userFromDb = databaseConnectionService.GetUserById(int.Parse(activity.From.Id));

			if (userFromDb != null)
				return $"Ви вже зареєстровані в рейтингу Codewars під ніком {userFromDb.CodewarsUsername}";

			var user = new UserModel
			{
				CodewarsUsername = activity.Text,
				TelegramUsername = activity.From.Name,
				TelegramId = int.Parse(activity.From.Id)
			};

			var codewarsUser = await codewarsConnectionService.GetCodewarsUser(user.CodewarsUsername);

			if (codewarsUser == null)
			{
				return $"Користувач {user.CodewarsUsername} не зареєстрований на Codewars";
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

		private string GetWeeklyRatingForChannel()
		{
			var databaseConnectionService = new DatabaseConnectionService();
			var rating = databaseConnectionService.GetWeeklyRating();

			return rating + @"<br/><br/>Зареєструватись в клані і почати набирати бали можна тут: @itkpi_codewars_bot. 
				Якщо маєте питання чи баг репорт -- пишіть йому: @maksim36ua";
		}

		private string ShowFaq()
		{
			return @"Вітаємо в клані ІТ КРІ на Codewars! 
			<br/><br/>codewars.com -- це знаменитий сайт з задачами для програмістів, за розв'язок яких нараховуються бали.
			От цими балами ми і будемо мірятись в кінці кожного тижня. 
			<br/><br/>Бот створений для того, щоб зробити реєстрацію в клані максимально швидкою і приємною.
			Щоб долучитись до рейтингу треба: 
				<br/>1) Зареєструватись на https://codewars.com 
				<br/>2) Надіслати сюди ваш нікнейм в Codewars.
			<br/><br/>Бали оновлюються раз на годину. Також доступні дві команди: 
				<br/>1) /weekly_rating показує поточний рейтинг за цей тиждень. 
				<br/>2) /total_rating відображає загальну кількість балів в кожного користувача.
			<br/><br/>Запрошуйте друзів в клан і гайда рубитись!
			<br/><br/>P.S: якщо знайшли багу або маєте зауваження -- пишіть йому @maksim36ua";
		}
	}
}