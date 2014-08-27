using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DNSProfileChecker.Common.Implementation
{
	public sealed class FolderReorderManager : IReorderManager
	{
		private readonly bool useMove;
		private AggregateException aggExc;

		public FolderReorderManager()
		{
			if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("reorderMethod"))
				useMove = System.Configuration.ConfigurationManager.AppSettings["reorderMethod"].IndexOf("move", StringComparison.OrdinalIgnoreCase) != -1;
			else
				useMove = false;
		}

		public bool Reorder(System.IO.DirectoryInfo[] folders)
		{
			Ensure.Argument.NotNull(folders, "folders parameter cannot be a null.");
			bool result = true;
			//bool isSequenseCorreted = false;
			if (folders.Length == 0)
				return result;
#if DEBUG
			System.Diagnostics.Debug.WriteLine(string.Format(" Started at {0}", DateTime.Now.ToString()));
#endif
			List<string> expected = Enumerable.Range(1, folders.Length).Select(x => "session" + x).ToList();
			string parentFolder = folders[folders.Length - 1].Parent.FullName;

			List<Exception> excList = new List<Exception>();
			int foldersCount = folders.Length;

			for (int i = 0; i < foldersCount; i++)
			{
				DirectoryInfo current = folders[i];
				if (String.Compare(current.Name, expected[i]) != 0)
				{
					DirectoryInfo newDi = new DirectoryInfo(Path.Combine(parentFolder, expected[i]));
					if (!newDi.Exists)
					{
						try
						{
							if (!useMove)
							{
								newDi.Create();
								newDi.CopyFrom(current);
								current.Delete(true);
							}
							else
								newDi.Move(current);
						}
						catch (Exception exc) { excList.Add(exc); }
					}
				}
			}

#if DEBUG
			System.Diagnostics.Debug.WriteLine(string.Format(" Fnished at {0}", DateTime.Now.ToString()));
#endif

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