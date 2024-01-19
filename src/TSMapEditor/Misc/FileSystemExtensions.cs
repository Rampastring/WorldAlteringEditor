using System;
using System.Windows.Forms;

namespace TSMapEditor.Misc
{
	public static class FileSystemExtensions
	{
		public const string AllFilesFilter = "All files|*.*";

		public static void OpenFile(
			Action<string> onOpened,
			Action onAborted = null,
			string filter = AllFilesFilter,
			bool checkFileExist = true,
			string initialDirectory = null
		) {
#if WINDOWS
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = string.IsNullOrEmpty(initialDirectory) ? Environment.CurrentDirectory : initialDirectory;

				openFileDialog.Filter = string.IsNullOrEmpty(filter) ? AllFilesFilter : filter;
				openFileDialog.CheckFileExists = checkFileExist;
				openFileDialog.RestoreDirectory = true;

				switch (openFileDialog.ShowDialog())
				{
					case DialogResult.OK:
						onOpened?.Invoke(openFileDialog.FileName);
						break;
					default:
						onAborted?.Invoke();
						break;
				}
			}
#else
            throw new NotImplementedException($"{nameof(FileSystemExtensions)}::{nameof(OpenFile)}");
#endif
		}
	}
}
