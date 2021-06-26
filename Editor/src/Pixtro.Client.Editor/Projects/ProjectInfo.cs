using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pixtro.Compiler;

namespace Pixtro.Client.Editor.Projects
{
	public sealed class ProjectInfo : IDisposable
	{
		public static readonly string Project_Filter = "Pixtro Project|*.pixProj";

		private static Version CurrentFormatVersion = new Version(1, 0, 0);


		public readonly string ProjectPath;
		public string ProjectDirectory => Path.GetDirectoryName(ProjectPath);
		private Version formatVersion;
		private BinaryFileWriter nodes;

		public bool BuiltRelease = false;

		public string Name => Path.GetFileNameWithoutExtension(ProjectPath);

		public ProjectInfo(string path)
		{
			ProjectPath = path;

			if (File.Exists(path))
			{
				InitFromFile(path);
			}
			else
			{
				Init();
			}

		}
		private void InitFromFile(string path)
		{
			try
			{
				var parsed = new BinaryFileParser(path, "pixtro");

				nodes = new BinaryFileWriter();
				nodes.Nodes = parsed.Nodes;
			}
			catch
			{
				// todo: do stuff if fails
			}

			var node = nodes["Version"];

			formatVersion = new Version(node.GetInteger("Major"), node.GetInteger("Minor"), node.GetInteger("Build"));
			
			if (formatVersion > CurrentFormatVersion)
			{
				// Oops, you opened up something from the future D:

				var result = MessageBox.Show("Are you a time traveller?  This version of pixtro is a newer version than expected and may not load correctly.  Do you want to proceed?", "Pixtro", MessageBoxButtons.YesNo);

				if (result == DialogResult.No)
				{
					throw new FormatException();
				}
			}

			if (formatVersion.Major != CurrentFormatVersion.Major)
			{
				// This shouldn't happen, but in case it does, figure out how to parse and reformat the data.  Version 2 shouldn't exist though
				throw new FormatException();

			}
			else if (formatVersion < CurrentFormatVersion)
			{
				// For any time something is added, removed, or changed.
				// Not used yet, but just in case
			}
			
		}
		private void Init()
		{
			nodes = new BinaryFileWriter();
			formatVersion = new Version(CurrentFormatVersion.ToString());

			BinaryFileNode node = nodes.AddNode("Version");

			node.AddAttribute("Major", CurrentFormatVersion.Major);
			node.AddAttribute("Minor", CurrentFormatVersion.Minor);
			node.AddAttribute("Build", CurrentFormatVersion.Build);

			Save();
		}

		public void CleanProject()
		{
			if (Directory.Exists(ProjectDirectory + "\\build"))
				Directory.Delete(ProjectDirectory + "\\build", true);
		}
		public void Save()
		{
			nodes.Save(ProjectPath, "pixtro");
		}

		public void Dispose()
		{
			nodes.Dispose();
		}
	}
}
