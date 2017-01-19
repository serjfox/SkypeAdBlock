using System;
using System.Collections.Generic;
using System.Linq;

namespace SkypeAdBlock
{
	public static class QuestionHelper
	{
		public static int AskQuertion(string question, List<string> answers)
		{
			if (string.IsNullOrWhiteSpace(question) || !answers.Any())
				return -1;

			Console.WriteLine(question);
			for (var i = 1; i <= answers.Count; i++)
			{
				Console.WriteLine("{0}) {1}",i, answers[i]);
			}

			var answer = GetAnswer(answers.Count);
			return answer;
		}

		public static bool AskYesNoQuertion(string question)
		{
			Console.WriteLine(question);
			Console.WriteLine("1) Да");
			Console.WriteLine("2) Нет");

			var answer = GetAnswer(2);
			return answer == 1;
		}

		private static int GetAnswer(int questionsNumber)
		{
			Console.WriteLine("Введите номер и нажмите Enter:");

			ConsoleKeyInfo key;
			var line = string.Empty;
			var choise = 0;
			do
			{
				key = Console.ReadKey(true);

				if ((key.Key == ConsoleKey.Enter))
					continue;

				if (key.Key != ConsoleKey.Backspace)
				{
					if (!int.TryParse(key.KeyChar.ToString(), out choise))
						continue;

					line += key.KeyChar.ToString();
					if (!int.TryParse(line, out choise))
						continue;

					if (!Enumerable.Range(1, questionsNumber).Contains(choise))
						continue;

					Console.Write(key.KeyChar);
				}
				else
				{
					if (key.Key != ConsoleKey.Backspace)
						continue;

					Console.SetCursorPosition(0, Console.CursorTop);
					Console.Write(new string(' ', Console.WindowWidth));
					Console.SetCursorPosition(0, Console.CursorTop - 1);
					line = string.Empty;
				}
			}
			while (key.Key != ConsoleKey.Enter);

			return choise;
		}
	}
}
