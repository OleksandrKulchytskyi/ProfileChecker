using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DNSProfileChecker.Common.Implementation
{
	public sealed class FolderReorderManager : IReorderManager
	{
		private AggregateException aggExc;

		public bool Reorder(System.IO.DirectoryInfo[] folders)
		{
			Ensure.Argument.NotNull(folders, "folders parameter cannot be a null.");
			bool result = true;
			bool isSequenseCorreted = false;
			if (folders.Length == 0)
				return result;

			List<string> expected = Enumerable.Range(1, folders.Length).Select(x => "session" + x).ToList();
			string parentFolder = folders[folders.Length - 1].Parent.FullName;

			List<Exception> excList = new List<Exception>();

			while (!isSequenseCorreted)
			{
				for (int i = folders.Length - 1; i >= 0; i--)
				{
					DirectoryInfo current = folders[i];
					int number = int.Parse(current.Name.Remove(0, "session".Length));
					if (i > 0)
					{
						DirectoryInfo nextDir = folders[i - 1];
						int lowerNumber = int.Parse(nextDir.Name.Remove(0, "session".Length));
						if (number - 1 != lowerNumber)
						{
							DirectoryInfo newDi = new DirectoryInfo(Path.Combine(parentFolder, "session" + (lowerNumber + 1).ToString()));
							if (!newDi.Exists)
							{
								try
								{
									newDi.Create();
									newDi.CopyFrom(current);
									current.Delete(true);
								}
								catch (Exception exc) { excList.Add(exc); }
								finally { try { if (newDi.Exists) newDi.Delete(true); } catch { } }

							}
							break;
						}
					}
				}
				folders = new DirectoryInfo(parentFolder).GetDirectories("session*", SearchOption.TopDirectoryOnly).OrderBy(f => int.Parse(f.Name.Remove(0, "session".Length))).ToArray();
				isSequenseCorreted = expected.Except(folders.Select(f => f.Name)).Any() == false;
			}

			if (excList.Count > 0)
			{
				result = false;
				aggExc = new AggregateException(excList);
			}

			return result;
		}

		public AggregateException Errors
		{
			get
			{
				return aggExc;
			}
		}
	}
}
