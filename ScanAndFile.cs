using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ScanFilesMsBuildTask
{
	public class ScanAndFile : Task
	{
		/// Path for example C:\honourbound\Source\Code\DualityLauncher\obj\Release\assets (these files are
		///  copied here by a previous MsBuild Task)
		public string PathToAssets { get; set; }

		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.Low, "Create cache of files", PathToAssets);
			if (!Directory.Exists(PathToAssets))
				return false;
			if (!PathToAssets.EndsWith(Path.DirectorySeparatorChar.ToString()))
				PathToAssets = PathToAssets + Path.DirectorySeparatorChar;
			try
			{
				var files = Directory.GetFiles(PathToAssets, "*", SearchOption.AllDirectories).ToList();
				var cachePath = Path.Combine(PathToAssets, "FilesCache.txt");
				if (File.Exists(cachePath))
					File.Delete(cachePath);

				if (!files.Contains(cachePath))
					files.Add(cachePath);
				var relativeFiles = (from file in files
					where file.Contains(PathToAssets)
					select file.Replace(PathToAssets, string.Empty)).ToList();
				using (File.Create(cachePath))
				{
				}
				File.AppendAllLines(cachePath, relativeFiles);
				return true;
			}
			catch (Exception exception)
			{
				Log.LogMessage(MessageImportance.High, "Error adding files to the file cache.", exception);

				return false;
			}
		}
	}
}
