using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Cocona;


namespace Rephidock.CLI.RunWithColor;


public class RunWithColorApp : CoconaLiteConsoleAppBase {


	[PrimaryCommand]
	public async Task<int> MainAssync(
		[Argument(Description = "Commands and it's arguments (arg0 being the command)", Name="arg")]
		params string[] command
	) {

		// Check for os
		if (!OperatingSystem.IsWindows()) {
			Console.WriteLine("Only windows is supported.");
			return 1;
		}

		// Process arguments
		static string QuoteArgumentIfNeeded(string arg) {

			// Do not quote arguments which dont contain delimiters
			if (!arg.Contains(' ')) return arg;

			// Do not quote arguments which are quoted
			if (arg[0] == '\"' && arg[^1] == '\"') return arg;

			// Quote other arguments
			return $"\"{arg}\"";

		}

		// Define the process
		using Process process = new();
		process.StartInfo = new ProcessStartInfo {
			UseShellExecute = false,
			FileName = "cmd.exe",
			Arguments = "/C " + string.Join(' ', command.Select(QuoteArgumentIfNeeded)),
			WorkingDirectory = Environment.CurrentDirectory			
		};
		
		// Start the process
		bool hasStarted = process.Start();
		if (!hasStarted) {
			Console.WriteLine($"Process \"{command}\" could not be started.");
			return 1;
		}
		
		// Set console mode
		bool hasSetColored = NativeCalls.SetVirtualTerminalOutput(true);
		if (!hasSetColored) {
			process.Close();
			Console.WriteLine($"Could not enable colors. Native error code: {NativeCalls.GetLastErrorCode()}");
			return 1;
		}
	
		// Wait for it finish
		await process.WaitForExitAsync();
		return process.ExitCode;
	}

}
