using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml;

namespace SkypeAdBlock
{
	public class AdBlocker
	{
		public bool IsAdministratorMode()
		{
			var identity = WindowsIdentity.GetCurrent();
			return identity != null && (new WindowsPrincipal(identity))
				.IsInRole(WindowsBuiltInRole.Administrator);
		}

		public bool FindHosts(out string path)
		{
			var system32 = Environment.SystemDirectory;
			path = Path.Combine(system32, @"drivers\etc\hosts");

			return File.Exists(path);
		}

		public bool FindSkypeConfig(out string path)
		{
			path = string.Empty;

			var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var skypeFolder = Path.Combine(appData, @"Skype\");

			if (!Directory.Exists(skypeFolder))
				return false;

			var subDirictories = Directory.GetDirectories(skypeFolder);
			var allConfigs =
				subDirictories.Select(subDirictory => Path.Combine(subDirictory, "config.xml")).Where(File.Exists).ToList();

			if (!allConfigs.Any())
				return false;

			if (allConfigs.Count > 1)
			{
				Console.WriteLine();

				var answer =
					QuestionHelper.AskQuertion(
						"Найдено несколько файлов с конфигурациями. Выберите правильный:",
						allConfigs);

				Console.WriteLine("");
				path = allConfigs[answer];

				return true;
			}

			path = allConfigs.First();
			return true;
		}

		public bool AddHosts(string path)
		{
			var hosts = new[]
			{
				"127.0.0.1 rad.msn.com",
				"127.0.0.1 adriver.ru",
				"127.0.0.1 api.skype.com",
				"127.0.0.1 .skypeassets.com",
				"127.0.0.1 apps.skype.com"
			};

			var existingHosts = File.ReadAllLines(path);

			try
			{
				using (var stream = new StreamWriter(path, true, Encoding.Default))
				{
					foreach (var host in hosts.Where(host => !existingHosts.Any(x => x.Trim().Equals(host))))
					{
						stream.WriteLine(host);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}

			return true;
		}

		public bool BackupSkypeConfig(string path, out string backupPath)
		{
			backupPath = string.Empty;

			if (!File.Exists(path))
				return false;

			var dir = Path.GetDirectoryName(path);
			if (dir == null)
				return false;

			var fileName = Path.GetFileNameWithoutExtension(path);
			var extention = Path.GetExtension(path);
			var backupFileName = string.Format("{0} - backup{1}", fileName, extention);
			backupPath = Path.Combine(dir, backupFileName);

			if (File.Exists(backupPath))
				return false;

			try
			{
				File.Copy(path, backupPath);
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool UpdateSkypeConfig(string path)
		{
			var xmlDoc = new XmlDocument();

			xmlDoc.Load(path);
			var generalNode = xmlDoc.SelectSingleNode("//UI/General");

			if (generalNode == null)
				return false;

			var eastAdNode = generalNode["AdvertEastRailsEnabled"];
			if (eastAdNode == null)
				return false;

			eastAdNode.InnerText = "0";

			var adPlaceholder = generalNode["AdvertPlaceholder"];
			if (adPlaceholder == null)
				return false;

			adPlaceholder.InnerText = "0";

			xmlDoc.Save(path);

			return true;
		}

		public bool SkypeIsRuning()
		{
			return Process.GetProcessesByName("Skype").Any();
		}

		public void ExitWithoutMakingChanges()
		{
			Console.WriteLine();
			Console.WriteLine("Изменения не внесены");
			Console.WriteLine("Приложение завершается...");
			Thread.Sleep(3000);
			Environment.Exit(0);
		}

		public void StartSkypeProcess()
		{
			Process.Start("Skype.exe");
		}

		public void CloseSkypeProcess()
		{
			foreach (var process in Process.GetProcessesByName("Skype"))
			{
				process.CloseMainWindow();
			}

			while (SkypeIsRuning())
			{
				Thread.Sleep(100);
			}

			RaiseSkypeIsClosed();
		}

		public event EventHandler SkypeIsClosed;

		private void RaiseSkypeIsClosed()
		{
			if (SkypeIsClosed == null)
				return;

			SkypeIsClosed(this, EventArgs.Empty);
		}
	}
}
