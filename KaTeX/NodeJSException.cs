#nullable enable
using JBSnorro;
using Microsoft.JSInterop;
using System;
using System.Text.RegularExpressions;
using System.IO;
using JBSnorro.Diagnostics;

namespace BlaTeX.JSInterop
{
	public class NodeJSException : JSException
	{
		public static NodeJSException Create(string message, string debugOutput)
		{
			if (BlatexNotFoundDueToMalformedPathException.Regex.IsMatch(message))
				return new BlatexNotFoundDueToMalformedPathException(message);

			if (BlatexNotFoundException.Regex.IsMatch(message))
				return new BlatexNotFoundException(message);

			return new NodeJSException(message, debugOutput);
		}
		public string DebugOutput { get; set; } = "";

		public NodeJSException(string message) : base(message) { }
		public NodeJSException(string message, Exception innerException) : base(message, innerException) { }
		public NodeJSException(string message, string debugOutput) : base(message)
		{
			this.DebugOutput = debugOutput;
		}
		public NodeJSException(string message, Exception innerException, string debugOutput) : base(message, innerException)
		{
			this.DebugOutput = debugOutput;
		}
	}

	public class BlatexNotFoundException : NodeJSException
	{
		// example: Error: Cannot find module 'C:gitBlaTeX/wwwroot/js/blatex_wrapper.js'
		// (?!\n)+ means anything that doesn't contain non-newline characters
		internal static Regex Regex { get; } = new Regex(@"Error: Cannot find module '[^\n]+(\/|\\)js(\/|\\)blatex_wrapper\.js'");
		private static readonly string userMessage = $"Cannot find '{string.Join("/", ".", "wwwroot", "js", "blatex_wrapper.js")}'"
#if DEBUG
			+ ". Build JS first, see readme.md"
#endif
			;

		public string AttemptedPath { get; }
		public BlatexNotFoundException(string message) : this(message, userMessage) { }
		protected BlatexNotFoundException(string message, string userMessage) : base(userMessage)
		{
			Contract.Requires(message != null, nameof(message));
			this.AttemptedPath = message?.Substring("Error: Cannot find module '", "'")!;
			Contract.Requires(this.AttemptedPath != null, "Message does not match Regex");
		}
	}

	public class BlatexNotFoundDueToMalformedPathException : NodeJSException
	{
		// example: Error: Cannot find module 'C:gitBlaTeX/wwwroot/js/blatex_wrapper.js'
		// (?!\n)+ means anything that doesn't contain non-newline characters
		internal static Regex Regex { get; } = new Regex(@"Error: Cannot find module '[A-Z]:[^\\][^\n]+(\/|\\)js(\/|\\)blatex_wrapper\.js'");
		private static readonly string userMessage = $"Cannot find '{string.Join("/", ".", "wwwroot", "js", "blatex_wrapper.js")}' because the path seems malformed. Backslashes seem to be missing";
		public BlatexNotFoundDueToMalformedPathException(string message) : base(message, userMessage) { }
	}
	public static class ExtensionsJBSnorro_0_0_10
	{
		/// <summary>
		/// Gets the string in <paramref name="s"/> between <paramref name="start"/> and <paramref name="end"/>.
		/// </summary>
		public static string? Substring(this string s, string start, string? end = null, int startIndex = 0)
		{
			Contract.Requires(s != null, nameof(s));
			Contract.Requires(start != null, nameof(start));
			Contract.Requires(end != null, nameof(end));
			int i = s.IndexOf(start, startIndex);
			if (i == -1)
				return null;

			if (end == null)
				return s[i..];

			int endIndex = s.IndexOf(end, i + start.Length);
			if (endIndex == -1)
				return null;

			return s[i..endIndex];
		}
	}
}
