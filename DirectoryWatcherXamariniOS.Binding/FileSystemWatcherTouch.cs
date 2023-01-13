//
// FileSystemWatcherTouch.cs
// DirectoryWatcherXamariniOS
//
// Created by dan (dan@mcneel.com) on 1/18/2014
// Robert McNeel & Associates.
//
using System;
using System.Diagnostics;

namespace DirectoryWatcherXamariniOS
{
	public class FileSystemWatcherTouch : IDisposable, IDirectoryWatcher
	{
		#region members
		private string m_path;
		private bool m_IsWatching;
		public event EventHandler Changed;
		//TODO: public event EventHandler Created;
		//TODO: public event EventHandler Deleted;
		//TODO: public event EventHandler Disposed;
		//TODO: public event EventHandler Error;
		//TODO: public event EventHandler Renamed;
		#endregion

		#region properties
		/// <value> Gets or sets the path of the directory to watch. </value>
		public string Path {
			get { 
				return m_path; 
			}

			set {
				m_path = value;
				if (m_IsWatching)
					ResetWatcher ();
			}
		}

		/// <value> Gets or sets a value indicating whether the object is enabled. </value>
		public bool EnableRaisingEvents { get; set; }

		/// <value> Gets a value indicating whether the object can raise an event. </value>
		public bool CanRaiseEvents { get { return EnableRaisingEvents; } }

		/// <value> The DirectoryWatcher wrapper around the native Obj-C DirectoryWatcher class. </value>
		private DirectoryWatcher DocumentsDirectoryWatcher { get; set; }

		/// <value> The DirectoryChangedDelegate wrapper around the native Obj-C DirectoryChangedDelegate class. </value>
		private DirectoryChangedDelegate DocumentsDirectoryChangedDelegate { get; set; }

		#endregion

		#region constructors and disposal
		/// <summary>
		/// <para>Listens to file system change notifications and raises events when a directory, or file in a directory, changes.</para>
		/// <para>NOTE: This class only implements a subset of the full .NET FileSystemWatcher class.  In actuality, this class is 
		/// a wrapper around the native DirectoryWatcher class in Objective-C and will only return events for directory changes.
		/// </para>
		/// </summary>
		public FileSystemWatcherTouch (string path)
		{
			Path = path;

			if (!(String.IsNullOrWhiteSpace(m_path))) {
				try {
					this.DocumentsDirectoryChangedDelegate = new DirectoryChangedDelegate (this);
					DocumentsDirectoryWatcher = DirectoryWatcher.WatchFolderWithPath (Path, DocumentsDirectoryChangedDelegate);
					EnableRaisingEvents = true;
					m_IsWatching = true;
				} catch (Exception e) {
					Console.WriteLine (e.Message);
					EnableRaisingEvents = false;
					m_IsWatching = false;
					return;
				}
			} else {
				Console.WriteLine ("WARNING: You must provide a valid path.");
				EnableRaisingEvents = false;
				m_IsWatching = false;
			}

		}

		/// <summary>
		/// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
		/// </summary>
		~ FileSystemWatcherTouch () { Dispose (false); }

		/// <summary>
		/// Actively reclaims unmanaged resources that this instance uses.
		// </summary>
		public void Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// For derived class implementers.
		/// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
		/// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
		/// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
		/// <para>Also, you must call the base virtual method within your overriding method.</para>
		/// </summary>
		/// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			// Free managed resources...but only if called from Dispose
			// (If called from Finalize then the objects might not exist anymore)
			if (disposing) {
				DocumentsDirectoryWatcher.Invalidate ();
				DocumentsDirectoryWatcher = null;
				DocumentsDirectoryChangedDelegate = null;
				Changed = null;
			}	
		}
		#endregion

		#region methods
		/// <summary>
		/// Raises the Changed event.
		/// </summary>
    public void OnChanged (DirectoryWatcherXamariniOS.DirectoryWatcher folderWatcher){
			if (CanRaiseEvents) {
				EventHandler changedHandler = Changed;
				if (changedHandler != null)
					changedHandler (this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Resets the DirectoryWatcher to watch a different path
		/// </summary>
		private void ResetWatcher ()
		{
			try {
				DocumentsDirectoryWatcher.Invalidate();
				DocumentsDirectoryWatcher = DirectoryWatcher.WatchFolderWithPath (Path, DocumentsDirectoryChangedDelegate);
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
			}
		}
		#endregion

	}
}