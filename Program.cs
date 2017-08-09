using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DuplicateFinder
{
	class Program
	{
		static void Main(string[] args)
		{
			if ((args.Length != 1) && (args.Length != 2))
			{
				Console.WriteLine("DuplicateFinder <directory> [properties:...]");
				return;
			}

			var dir = new DirectoryInfo(args[0]);
			if (!dir.Exists)
			{
				Console.WriteLine($"ERROR! Directory #{args[0]} does not exist");
				return;
			}

			if (args.Length == 2)
			{
				var extraProperties = args[1];
				if (!extraProperties.StartsWith("properties:"))
				{
					Console.Write("Error! Optional parameter must start with 'properties:'");
					return;
				}
				Dictionary<String, String> parsedGlobalProperties = Parse(extraProperties);
				var finder = new DuplicateFinder(dir, parsedGlobalProperties);
				finder.Search();
			}
		}

		/// <summary>
		/// Parses the command line parameter for extra properties which will be sent into
		/// the MSBuild API for loading project files. This will be a string that starts with
		/// 'properties:'
		/// and has pairs of properties and values, delimited by semicolons.
		/// The pairs of properties and values are seperated by equal characters: '='.
		/// For instance:
		/// 'properties:Configuration=Debug;Platform=AnyCPU;Country=USA'
		/// </summary>
		/// <param name="extraProperties">String containing the raw properites that came from the command line</param>
		/// <returns>A dictionary with the property value pairs</returns>
		private static Dictionary<string, string> Parse(string extraProperties)
		{
			var temp = extraProperties.Replace("properties:", "");
			String[] splits = temp.Split(';');
			var result = new Dictionary<String, String>();
			foreach (String s in splits)
			{
				String[] pair = s.Split('=');
				result[pair[0]] = pair[1];
			}
			return result;
		}
	}
}
