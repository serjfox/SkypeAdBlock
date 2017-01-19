using System;
using System.Threading;

namespace SkypeAdBlock
{
	class Program
	{
		private static void Main()
		{
			var adBlocker = new AdBlocker();

			Console.Write("Процесс выполняется от имени Администритора: ");
			var isAdmin = adBlocker.IsAdministratorMode();
			WriteColored(isAdmin);

			Console.Write("Путь к файлу hosts определен: ");
			string hostsPath;
			var hosts = adBlocker.FindHosts(out hostsPath);
			WriteColored(hosts);

			Console.Write("Путь к файлу конфигурации Skype определен: ");
			string skypeConfigPath;
			var appData = adBlocker.FindSkypeConfig(out skypeConfigPath);
			WriteColored(appData);

			if (!isAdmin || !hosts || !appData)
			{
				Console.Write("Дальнейшая работа невозможна");
				Console.ReadKey();
				return;
			}

			var skypeIsRuning = adBlocker.SkypeIsRuning();
			if (skypeIsRuning)
			{
				Console.WriteLine();
				
				if (!QuestionHelper.AskYesNoQuertion("Для внесения изменений Skype будет закрыт. Продожить?"))
					adBlocker.ExitWithoutMakingChanges();

				var closeSkypeProcessesThread = new Thread(adBlocker.CloseSkypeProcess);
				closeSkypeProcessesThread.Start();

				adBlocker.SkypeIsClosed += delegate
				{
					skypeIsRuning = false;
				};

				do
				{
					ClearLine(string.Format("Завершение процесса Skype {0}", Spinner.Next()));
					Thread.Sleep(150);

				} while (skypeIsRuning);

				ClearLine(string.Empty);
				Console.WriteLine("Skype закрыт");
			}

			Console.WriteLine();

			if (!QuestionHelper.AskYesNoQuertion("Заблокировать рекламу в Skype?"))
				adBlocker.ExitWithoutMakingChanges();

			Console.WriteLine();

			Console.Write("Записи внесены в файл hosts: ");
			var hostsAdded = adBlocker.AddHosts(hostsPath);
			WriteColored(hostsAdded);

			string backupPath;
			var skypeConfigBackuped = adBlocker.BackupSkypeConfig(skypeConfigPath, out backupPath);

			Console.Write("Файл конфигурации Skype обновлен: ");
			var cofigUpdated = adBlocker.UpdateSkypeConfig(skypeConfigPath);
			WriteColored(cofigUpdated);

			if (skypeConfigBackuped)
			{
				Console.Write("Создан бэкап файла концигурации Skype: ");
				Console.Write(backupPath);
			}

			Console.WriteLine();
			if (QuestionHelper.AskYesNoQuertion("Запустить Skype?"))
				adBlocker.StartSkypeProcess();
		}

		private static void ClearLine(string textInstead)
		{
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, Console.CursorTop - 1);
			Console.Write(textInstead);
		}

		private static void WriteColored(bool status)
		{
			Console.ForegroundColor = status ? ConsoleColor.Green : ConsoleColor.Red;
			Console.WriteLine(status ? "Да" : "Нет"); 

			Console.ForegroundColor = ConsoleColor.Gray;
		}

		protected class Spinner
		{
			private static int _current;
			private static readonly string[] Symbols =
			{
				"        ", 
				".       ", 
				" .      ", 
				"  .     ", 
				".  .    ", 
				" .  .   ", 
				"  . .   ", 
				".  ..   ", 
				" . ..   ", 
				"  ...   ", 
				"  ...   ", 
				"  ...   ", 
				"  ...   ", 
				"  .. .  ", 
				"  ..  . ", 
				"  . .  .", 
				"   . .  ", 
				"   .  . ", 
				"    .  .", 
				"     .  ", 
				"      . ", 
				"       ."
			};

			public Spinner()
			{
				_current = -1;
			}

			public static string Next()
			{
				var symbol = Symbols[++_current];

				if (_current == Symbols.Length - 1)
					_current = -1;

				return symbol;
			}
		}
	}
}
