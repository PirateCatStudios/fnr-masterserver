using CommandLine;

namespace MasterServer
{
	class CommandLineOptions
	{
		[Option('p', "port", Default = "15940", HelpText = "The port the server will listen on")]
		public string Port { get; set; }

		[Option('h', "host", Default = "0.0.0.0", HelpText = "The ip address the server will listen on")]
		public string Host { get; set; }

		[Option('e', "elorange", Default = 0)]
		public int EloRange { get; set; }

		[Option('d', "daemon", Default = false)]
		public bool IsDaemon {get; set;}
	}
}
