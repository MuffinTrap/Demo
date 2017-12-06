
using System;

using OpenTK.Graphics.OpenGL;

using OpenTK.Audio.OpenAL;

namespace OpenTkConsole
{
	static class Logger
	{


		public enum ErrorState
		{
			Critical,	// Program cannot continue. Code error
			Limited,	// Something is missing. Data error
			Unoptimal,	// Stupid or conflicting settings. User error
			NoErrors
		}

		public enum LogType
		{ 
			Error,		// Display an error, must cause one of above
			Warning,	// Display a warning
			Info,		// Information
			Phase		// Phase of execution
		}

		static public ErrorState ProgramErrorState { get; private set; }

		static public void LogInfo(string message)
		{
			PrintLogType(LogType.Info);
			Console.WriteLine(message);
		}

		static public void LogPhase(string message)
		{
			PrintLogType(LogType.Phase);
			Console.WriteLine(message);
		}

		static public void LogWarning(string message)
		{
			PrintLogType(LogType.Warning);
			Console.WriteLine(message);
			ProgramErrorState = ErrorState.Unoptimal;
		}

		static public void LogError(ErrorState severity, string message)
		{
			PrintLogType(LogType.Error);
			Console.WriteLine(message);
			ProgramErrorState = severity;
		}

		static private void PrintLogType(LogType logtype)
		{
			switch (logtype)
			{
				case LogType.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write("Error: ");
					break;
				case LogType.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write("Warning: ");
					break;
				case LogType.Phase:
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write("++++ ");
					break;
				case LogType.Info:
					Console.ForegroundColor = ConsoleColor.White;
					Console.Write("Info: ");
					break;
			}
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		static Logger()
		{
			ProgramErrorState = ErrorState.NoErrors;
		}
	}


	static class Error
	{

		static public bool checkGLError(string place)
		{
			bool errorFound = false;
			while (GL.GetError() != ErrorCode.NoError)
			{
				Logger.LogError(Logger.ErrorState.Critical, ("GL error in " + place));
				errorFound = true;
			}
			return errorFound;
		}

		static public bool checkALError(string place)
		{
			bool errorFound = false;
			while (AL.GetError() != ALError.NoError)
			{
				Logger.LogError(Logger.ErrorState.Critical, ("AL error in " + place));
				errorFound = true;
			}
			return errorFound;
		}
	}
}