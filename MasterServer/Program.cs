using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CommandLine;
using CommandLine.Text;

namespace MasterServer
{
	internal static class Program
	{
		private const string DEFAULT_HOST = "0.0.0.0";
		private const ushort DEFAULT_PORT = 15940;

		private static bool isDaemon = false;
		private static string host = DEFAULT_HOST;
		private static ushort port = DEFAULT_PORT;
		private static int eloRange = 0;

		private static bool showHelp = false;

		private static MasterServer server;

		private static void Main(string[] args)
		{
			string read = string.Empty;

			var commandParser = new Parser(with => with.HelpWriter = null);
			var parserResult = commandParser.ParseArguments<CommandLineOptions>(args);

			parserResult.WithParsed(options => {
				isDaemon = options.IsDaemon;
				host = options.Host;
				ushort.TryParse(options.Port, out port);
				eloRange = options.EloRange;
			}).WithNotParsed(errs => DisplayHelp(parserResult, errs));

			if (showHelp)
				return;
			
			if(!isDaemon)
			{
				if (host == DEFAULT_HOST)
				{
					Console.WriteLine("Entering nothing will choose defaults.");
					Console.WriteLine("Enter Host IP (Default: " + GetLocalIpAddress() + "):");
					read = Console.ReadLine();
					host = string.IsNullOrEmpty(read) ? GetLocalIpAddress() : read;
				}

				if (port == DEFAULT_PORT)
				{
					Console.WriteLine("Enter Port (Default: 15940):");
					read = Console.ReadLine();
					if (string.IsNullOrEmpty(read))
						port = 15940;
					else
						ushort.TryParse(read, out port);
				}
			}

			Console.WriteLine("Hosting ip [{0}] on port [{1}]", host, port);

			if (!isDaemon)
				PrintHelp();

			server = new MasterServer(host, port)
			{
				EloRange = eloRange
			};
			server.ToggleLogging();

			while (true)
			{
				if (!isDaemon)
				{
					if (HandleConsoleInput())
						break;
				}

			}
		}

		private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errors)
		{
			HelpText helpText = null;
			if (errors.IsVersion())
			{
				helpText = HelpText.AutoBuild(result);

			}
			else if(errors.IsHelp())
			{
				helpText = HelpText.AutoBuild(result, h => {
					h.AdditionalNewLineAfterOption = false;
					return HelpText.DefaultParsingErrorsHandler(result, h);
				});
			}

			if (helpText != null)
			{
				showHelp = true;
				Console.WriteLine(helpText);
			}
		}

		private static bool HandleConsoleInput()
		{
			string read = Console.ReadLine();
			read = string.IsNullOrEmpty(read) ? read : read.ToLower();

			switch (read)
			{
				case null:
					return false;

				case "s":
				case "stop":
					lock (server)
					{
						Console.WriteLine("Server stopped.");
						server.Dispose();
					}
					break;

				case "l":
				case "log":
					if (server.ToggleLogging())
						Console.WriteLine("Logging has been enabled");
					else
						Console.WriteLine("Logging has been disabled");
					break;

				case "r":
				case "restart":
					lock (server)
					{
						if (server.IsRunning)
						{
							Console.WriteLine("Server stopped.");
							server.Dispose();
						}
					}

					Console.WriteLine("Restarting...");
					Console.WriteLine("Hosting ip [{0}] on port [{1}]", host, port);
					server = new MasterServer(host, port);
					break;

				case "q":
				case "quit":
					lock (server)
					{
						Console.WriteLine("Quitting...");
						server.Dispose();
					}
					return true;

				case "h":
				case "help":
					PrintHelp();
					break;

				default:
					if (read.StartsWith("elo"))
					{
						int index = read.IndexOf("=", StringComparison.Ordinal);
						string val = read.Substring(index + 1, read.Length - (index + 1));
						if (int.TryParse(val.Replace(" ", string.Empty), out index))
						{
							Console.WriteLine("Elo range set to {0}", index);
							if (index == 0)
								Console.WriteLine("Elo turned off");
							server.EloRange = index;
						}
						else
							Console.WriteLine("Invalid elo range provided (Must be an integer)\n");
					}
					else
						Console.WriteLine("Command not recognized, please try again");

					break;
			}

			return false;
		}

		private static void PrintHelp()
		{
			Console.WriteLine(@"Commands Available
(s)top - Stops hosting
(r)estart - Restarts the hosting service even when stopped
(l)og - Toggles logging (starts enabled)
(q)uit - Quits the application
(h)elp - Get a full list of commands");
		}

	    /// <summary>
	    /// Return the Local IP-Address
	    /// </summary>
	    /// <returns></returns>
	    private static string GetLocalIpAddress()
	    {
	        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
	        foreach (IPAddress ip in hostEntry.AddressList)
	        {
	            if (ip.AddressFamily == AddressFamily.InterNetwork)
		            return ip.ToString();
	        }

	        throw new Exception("No network adapters with an IPv4 address in the system!");
	    }
    }
}
