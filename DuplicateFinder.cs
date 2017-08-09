using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;

namespace DuplicateFinder
{
	/// <summary>
	///     Finds Duplicate References in MSBuild project files.
	///     As input it takes a file directory to search in,
	///     and a Dictionary of global properties to use when opening
	///     the project files.
	///     All output will be to the commandline. It does not fix the
	///     problem, it only identifies the problems.
	///     This is how to use the class
	///     <example>
	///         var globals = new Dictionary<String, String>();
	///         globals["Configuration"] = "Debug";
	///         globals["Platform"] = "AnyCPU";
	///         var sb = new DuplicateFinder("C:\\example\\repository", globals);
	///         sb.Search();
	///         return sb.ErrorCount;
	///     </example>
	///     Whereupon the command line will contain errors (if any)
	/// </summary>
	public class DuplicateFinder
	{
		private readonly DirectoryInfo _inputDir;
		private readonly Dictionary<string, string> globals;

		public DuplicateFinder(DirectoryInfo dir, Dictionary<string, string> globalProperties)
		{
			_inputDir = dir;
			globals = globalProperties;
		}

		public int ErrorCount { get; set; }

		public void Search()
		{
			var files = Directory.GetFiles(_inputDir.FullName, "*.csproj", SearchOption.AllDirectories);
			foreach (var file in files)
			{
				ExamineFile(file);
			}

			if (ErrorCount > 0)
			{
				Console.WriteLine("ERROR: Found {0} duplicate references in directory: {1}", ErrorCount, _inputDir.FullName);
			}
		}

		private void ExamineFile(string file)
		{
			Project proj = null;
			try
			{
				proj = new Project(file, globals, "14.0");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return;
			}
			var root = proj.Xml;
			var items = root.Items;

			var refs = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			var foundDups = false;

			foreach (var item in items)
				if (item.ItemType == "Reference")
				{
					var raw = item.Include;
					var assemblyName = raw;
					if (raw.Contains(','))
					{
						assemblyName = raw.Substring(0, raw.IndexOf(','));
					}
					assemblyName = assemblyName.ToLower();

					if (refs.ContainsKey(assemblyName))
					{
						// Found a duplicate
						refs[assemblyName]++;
						foundDups = true;
						ErrorCount++;
					}
					else
					{
						refs[assemblyName] = 1;
					}
				}

			if (foundDups)
			{
				Console.WriteLine("File: {0}", file);
				foreach (var pair in refs)
				{
					if (pair.Value > 1)
					{
						var prev = Console.ForegroundColor;
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Duplicate Reference Found: {0}", pair.Key);
						Console.ForegroundColor = prev;
					}
				}
			}
		}
	}
}