using System;
using System.Collections.Generic;
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
				PathToAssets = PathToAssets+Path.DirectorySeparatorChar;
			try
			{
				var files = Directory.GetFiles(PathToAssets, "*", SearchOption.AllDirectories).ToList();
				var cachePath = Path.Combine(PathToAssets, "FilesCache.txt");
				if (File.Exists(cachePath))
					File.Delete(cachePath);

				if (!files.Contains(cachePath)) 
					files.Add(cachePath);
				var relativeFiles = (from file in files where file.Contains(PathToAssets) 
									 select file.Replace(PathToAssets, string.Empty)).ToList();
				using (File.Create(cachePath))
				{}
				File.AppendAllLines(cachePath, relativeFiles);
				return true;
			}
			catch (Exception exception)
			{
				Log.LogMessage(MessageImportance.High, "Error adding files to the file cache.", exception);

				return false;
			}
		}

		/// <summary>
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private static String MakeRelativePath(String fromPath, String toPath)
		{
			if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
			if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

			var fromUri = new Uri(fromPath);
			var toUri = new Uri(toPath);

			if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

			var relativeUri = fromUri.MakeRelativeUri(toUri);
			var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (toUri.Scheme.ToUpperInvariant() == "FILE")
			{
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}
	}
}
