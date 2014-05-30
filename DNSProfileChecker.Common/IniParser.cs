using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSProfileChecker.Common
{
	public class IniFileParser
	{
		/// <summary>
		/// Get from DB Setup ini file the collection of sections with all text lines associated with it
		/// </summary>
		/// <param name="filePath">The path to the DB Setup ini file</param>
		/// <returns>Collection of sections</returns>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.UnauthorizedAccessException"></exception>
		/// <exception cref="System.IO.FileNotFoundException"></exception>
		public static Dictionary<string, List<string>> GetSectionsDictionary(string filePath)
		{
			Dictionary<string, List<string>> sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			StringBuilder fileContent = new StringBuilder(); //This object will contains the full text of INI file

			if (File.Exists(filePath))
			{
				//Read DB Setup ini file
				string data = null;
				try
				{
					data = File.ReadAllText(filePath);
				}
				catch (IOException ex) { throw; }
				catch (UnauthorizedAccessException ex) { throw; }
				using (StringReader sr = new StringReader(data))
				{
					string line = null;
					string sectionName = string.Empty;
					List<string> sectionList = null;

					while ((line = sr.ReadLine()) != null) //Read each line until the end of the file
					{
						//line = line.Trim();
						fileContent.AppendLine(line);

						//Check if this is the start of the section
						if (line.StartsWithIgnoreSpaces("[", false) && line.EndsWithIgnoreSpaces("]", false))
						{
							if (sectionList != null) sectionList = null; //Unitialize section list

							sectionName = GetSectionName(line); //Get section name
							sectionList = new List<string>(); //Initialize section list
							sections.Add(sectionName, sectionList); //Fill sections dictionary
							continue;
						}

						if (sectionList != null) sectionList.Add(line);
					}
				}
				fileContent.Clear();
				fileContent = null;
			}
			else
			{
				throw new FileNotFoundException("This file was not found on machine", filePath);
			}

			return sections;
		}

		public static Dictionary<string, List<KeyValuePair<string, string>>> GetSingleSection(string filePath, string section)
		{
			Dictionary<string, List<KeyValuePair<string, string>>> sections = new Dictionary<string, List<KeyValuePair<string, string>>>();

			if (File.Exists(filePath))
			{
				using (StreamReader sr = new StreamReader(filePath))
				{
					string line = null;
					string sectionName = string.Empty;
					List<KeyValuePair<string, string>> sectionData = null;
					int found = 0;
					while ((line = sr.ReadLine()) != null) //Read each line until the end of the file
					{
						//Check if this is the start of the section
						if (line.StartsWithIgnoreSpaces("[", false) && line.EndsWithIgnoreSpaces("]", false) &&
							(sectionName = GetSectionName(line)).Equals(section, StringComparison.OrdinalIgnoreCase))
						{
							found = 1;
							if (sectionData != null)
								sectionData = null; //make section list rootless

							sectionData = new List<KeyValuePair<string, string>>(); //Initialize section list
							sections.Add(sectionName, sectionData); //Fill sections dictionary
							continue;
						}
						else if (line.StartsWithIgnoreSpaces("[") && line.EndsWithIgnoreSpaces("]") &&
								found == 1)
							break;

						if (sectionData != null)
						{
							int indx=line.IndexOf('=');
							string key = line.Substring(0, indx);
							string value = line.Substring(indx+1);
							KeyValuePair<string, string> pair = new KeyValuePair<string, string>(key,value);
							sectionData.Add(pair);
						}
					}
				}
			}
			else
			{
				throw new FileNotFoundException("This file was not found on machine", filePath);
			}

			return sections;
		}

		/// <summary>
		/// Get Section list from specified file
		/// </summary>
		/// <param name="filePath">The path to the DB Setup ini file</param>
		/// <returns>Collection of sections</returns>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.UnauthorizedAccessException"></exception>
		/// <exception cref="System.IO.FileNotFoundException"></exception>
		public static List<string> GetSectionsList(string filePath)
		{
			List<string> sections = new List<string>();

			if (File.Exists(filePath))
			{
				//Read DB Setup ini file
				string data = null;
				try
				{
					data = File.ReadAllText(filePath);
				}
				catch (IOException ex) { throw; }
				catch (UnauthorizedAccessException ex) { throw; }
				using (StringReader sr = new StringReader(data))
				{
					string line = null;
					while ((line = sr.ReadLine()) != null) //Read each line until the end of the file
					{
						if (StringExtension.RegexSection.IsMatch(line))
							sections.Add(line);
					}
				}
			}
			else
			{
				throw new FileNotFoundException("This file wasn't found on the machine", filePath);
			}

			return sections;
		}

		/// <summary>
		/// Get the name of the section which is enclosed in brackets
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static string GetSectionName(string line)
		{
			return line.Trim(new char[] { '[', ']' });
		}
	}
}
