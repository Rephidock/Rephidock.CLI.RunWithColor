using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;


namespace Rephidock.CLI.RunWithColor;


// Based on
// https://stackoverflow.com/a/53391837

[SupportedOSPlatform("windows")]
public static partial class NativeCalls {

	#region //// Last Error

	[LibraryImport("kernel32.dll")]
	private static partial uint GetLastError();

	/// <summary>Returns result of GetLastError in kernel32.dll</summary>
	public static uint GetLastErrorCode() => GetLastErrorCode();

	#endregion

	#region //// Standard handles

	public enum StdHandle : int {
		INVALID_HANDLE_VALUE = -1,

		STD_INPUT_HANDLE = -10,
		STD_OUTPUT_HANDLE = -11,
		STD_ERROR_HANDLE = -12,
	}

	[LibraryImport("kernel32.dll", SetLastError = true)]
	internal static partial IntPtr GetStdHandle(int nStdHandle); //returns Handle

	#endregion

	#region //// Console Mode

	[SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "Different use implies different name")]
	public enum ConsoleMode : uint {

		ENABLE_EXTENDED_FLAGS = 0x0080,
		ENABLE_AUTO_POSITION = 0x100,

		ENABLE_ECHO_INPUT = 0x0004,
		ENABLE_INSERT_MODE = 0x0020,
		ENABLE_LINE_INPUT = 0x0002,
		ENABLE_MOUSE_INPUT = 0x0010,
		ENABLE_PROCESSED_INPUT = 0x0001,
		ENABLE_QUICK_EDIT_MODE = 0x0040,
		ENABLE_WINDOW_INPUT = 0x0008,
		ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200,

		//for screen buffer handle
		ENABLE_PROCESSED_OUTPUT = 0x0001,
		ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
		ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
		DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
		ENABLE_LVB_GRID_WORLDWIDE = 0x0010
	}

	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	/// <summary>
	/// Enables or disables <see cref="ConsoleMode.ENABLE_VIRTUAL_TERMINAL_PROCESSING"/>
	/// and <see cref="ConsoleMode.DISABLE_NEWLINE_AUTO_RETURN"/> mode flags for
	/// the current console.
	/// </summary>
	/// <returns>True if operation succeeded, false otherwise</returns>
	public static bool SetVirtualTerminalOutput(bool enabled) {
		return SetConsoleMode(
			StdHandle.STD_OUTPUT_HANDLE,
			ConsoleMode.ENABLE_VIRTUAL_TERMINAL_PROCESSING | ConsoleMode.DISABLE_NEWLINE_AUTO_RETURN,
			enabled: enabled
		);
	}

	/// <summary>Sets console mode flags for the current console</summary>
	/// <returns>True if operation succeeded, false otherwise</returns>
	public static bool SetConsoleMode(StdHandle handle, ConsoleMode mode, bool enabled) {

		// Get handle
		IntPtr stdHandle = GetStdHandle((int)handle);
		if (stdHandle == (IntPtr)StdHandle.INVALID_HANDLE_VALUE) {
			return false;
		}

		// Get flags
		if (!GetConsoleMode(stdHandle, out uint consoleMode)) {
			return false;
		}

		// Change flags
		if (enabled) {
			consoleMode |= (uint)mode;
		} else {
			consoleMode &= ~((uint)mode);
		}

		// Set flags
		if (!SetConsoleMode(stdHandle, consoleMode)) {
			return false;
		}

		return true;
	}

	#endregion

}