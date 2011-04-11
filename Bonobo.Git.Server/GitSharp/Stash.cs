// 
// Stash.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using GitSharp.Core;
using System.Text;

namespace GitSharp
{
	public class Stash
	{
		internal string PrevStashCommitId { get; private set; }
		internal string CommitId { get; private set; }
		internal string FullLine { get; private set; }
		internal StashCollection StashCollection { get; set; }
		
		/// <summary>
		/// Who created the stash
		/// </summary>
		public Author Author { get; private set; }
		
		/// <summary>
		/// Timestamp of the stash creation
		/// </summary>
		public DateTimeOffset DateTime { get; private set; }
		
		/// <summary>
		/// Stash comment
		/// </summary>
		public string Comment { get; private set; }
		
		private Stash ()
		{
		}
		
		internal Stash (string prevStashCommitId, string commitId, Author author, string comment)
		{
			this.PrevStashCommitId = prevStashCommitId;
			this.CommitId = commitId;
			this.Author = author;
			this.Comment = comment;
			this.DateTime = DateTimeOffset.Now;
			
			// Create the text line to be written in the stash log
			
			int secs = (int) (this.DateTime - new DateTimeOffset (1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds;
			Console.WriteLine ();
			
			TimeSpan ofs = this.DateTime.Offset;
			string tz = string.Format ("{0}{1:00}{2:00}", (ofs.Hours >= 0 ? '+':'-'), Math.Abs (ofs.Hours), Math.Abs (ofs.Minutes));
			
			StringBuilder sb = new StringBuilder ();
			sb.Append (prevStashCommitId ?? new string ('0', 40)).Append (' ');
			sb.Append (commitId).Append (' ');
			sb.Append (author.Name).Append (" <").Append (author.EmailAddress).Append ("> ");
			sb.Append (secs).Append (' ').Append (tz).Append ('\t');
			sb.Append (comment);
			FullLine = sb.ToString ();
		}

		
		internal static Stash Parse (string line)
		{
			// Parses a stash log line and creates a Stash object with the information
			
			Stash s = new Stash ();
			s.PrevStashCommitId = line.Substring (0, 40);
			if (s.PrevStashCommitId.All (c => c == '0')) // And id will all 0 means no parent (first stash of the stack)
				s.PrevStashCommitId = null;
			s.CommitId = line.Substring (41, 40);
			
			int i = line.IndexOf ('<');
			s.Author = new Author ();
			if (i != -1) {
				s.Author.Name = line.Substring (82, i - 82 - 1);
				i++;
				int i2 = line.IndexOf ('>', i);
				if (i2 != -1)
					s.Author.EmailAddress = line.Substring (i, i2 - i);
				
				i2 += 2;
				i = line.IndexOf (' ', i2);
				int secs = int.Parse (line.Substring (i2, i - i2));
				DateTime t = new DateTime (1970, 1, 1) + TimeSpan.FromSeconds (secs);
				string st = t.ToString ("yyyy-MM-ddTHH:mm:ss") + line.Substring (i + 1, 3) + ":" + line.Substring (i + 4, 2);
				s.DateTime = DateTimeOffset.Parse (st);
				s.Comment = line.Substring (i + 7);
			}
			s.FullLine = line;
			return s;
		}
		
		public void Apply ()
		{
			StashCollection.Apply (this);
		}
	}
	
	public class StashCollection: IEnumerable<Stash>
	{
		Repository _repo;
		
		internal StashCollection (Repository repo)
		{
			this._repo = repo;
		}
		
		FileInfo StashLogFile {
			get {
				string stashLog = Path.Combine (_repo.Directory, "logs");
				stashLog = Path.Combine (stashLog, "refs");
				return new FileInfo (Path.Combine (stashLog, "stash"));
			}
		}
		
		FileInfo StashRefFile {
			get {
				string file = Path.Combine (_repo.Directory, "refs");
				return new FileInfo (Path.Combine (file, "stash"));
			}
		}
		
		public Stash Create ()
		{
			return Create (null);
		}
		
		public Stash Create (string message)
		{
			var parent = _repo.CurrentBranch.CurrentCommit;
			Author author = new Author(_repo.Config["user.name"] ?? "unknown", _repo.Config["user.email"] ?? "unknown@(none).");
			
			if (message == null) {
				// Use the commit summary as message
				message = parent.ShortHash + " " + parent.Message;
				int i = message.IndexOfAny (new char[] { '\r', '\n' });
				if (i != -1)
					message = message.Substring (0, i);
			}
			
			// Create the index tree commit
			GitIndex index = _repo.Index.GitIndex;
			index.RereadIfNecessary();
			var tree_id = index.writeTree();
			Tree indexTree = new Tree(_repo, tree_id);
			string commitMsg = "index on " + _repo.CurrentBranch.Name + ": " + message;
			var indexCommit = Commit.Create(commitMsg + "\n", parent, indexTree, author);

			// Create the working dir commit
			tree_id = WriteWorkingDirectoryTree (parent.Tree.InternalTree, index);
			commitMsg = "WIP on " + _repo.CurrentBranch.Name + ": " + message;
			var wipCommit = Commit.Create(commitMsg + "\n", new Commit[] { parent, indexCommit }, new Tree(_repo, tree_id), author, author, DateTimeOffset.Now);
			
			string prevCommit = null;
			FileInfo sf = StashRefFile;
			if (sf.Exists)
				prevCommit = File.ReadAllText (sf.FullName);
			
			Stash s = new Stash (prevCommit, wipCommit.Hash, author, commitMsg);
			
			FileInfo stashLog = StashLogFile;
			File.AppendAllText (stashLog.FullName, s.FullLine + "\n");
			File.WriteAllText (sf.FullName, s.CommitId + "\n");
			
			// Wipe all local changes
			_repo.CurrentBranch.Reset (ResetBehavior.Hard);
			
			s.StashCollection = this;
			return s;
		}
		
		ObjectId WriteWorkingDirectoryTree (Core.Tree headTree, GitIndex index)
		{
			var writer = new ObjectWriter(_repo._internal_repo);
			var tree = new Core.Tree(_repo._internal_repo);
			WriteTree (writer, headTree, index, tree, _repo._internal_repo.WorkingDirectory);
			return writer.WriteTree (tree);
		}
		
		void WriteTree (ObjectWriter writer, Core.Tree headTree, GitIndex index, Core.Tree tree, DirectoryInfo dir)
		{
			foreach (var fsi in dir.GetFileSystemInfos ()) {
				if (fsi is FileInfo) {
					// Exclude untracked files
					string gname = _repo.ToGitPath (fsi.FullName);
					bool inIndex = index.GetEntry (gname) != null;
					bool inHead = headTree.FindBlobMember (gname) != null;
					if (inIndex || inHead) {
						var entry = tree.AddFile (fsi.Name);
						entry.Id = writer.WriteBlob ((FileInfo)fsi);
					}
				}
				else if (fsi.Name != Constants.DOT_GIT) {
					var child = tree.AddTree (fsi.Name);
					WriteTree (writer, headTree, index, child, (DirectoryInfo) fsi);
					child.Id = writer.WriteTree (child);
				}
			}
		}
		
		internal void Apply (Stash stash)
		{
			// Restore the working tree
			Commit wip = _repo.Get<Commit> (stash.CommitId);
			wip.Checkout();
			_repo._internal_repo.Index.write();
			
			// Restore the index
			Commit index = wip.Parents.Last ();
			_repo.Index.GitIndex.ReadTree (index.Tree.InternalTree);
			_repo.Index.GitIndex.write ();
		}
		
		public void Remove (Stash s)
		{
			List<Stash> stashes = ReadStashes ();
			Remove (stashes, s);
		}
		
		public void Pop ()
		{
			List<Stash> stashes = ReadStashes ();
			Stash last = stashes.Last ();
			last.Apply ();
			Remove (stashes, last);
		}
		
		public void Clear ()
		{
			if (StashRefFile.Exists)
				StashRefFile.Delete ();
			if (StashLogFile.Exists)
				StashLogFile.Delete ();
		}
		
		void Remove (List<Stash> stashes, Stash s)
		{
			int i = stashes.FindIndex (st => st.CommitId == s.CommitId);
			if (i != -1) {
				stashes.RemoveAt (i);
				if (stashes.Count == 0) {
					// No more stashes. The ref and log files can be deleted.
					StashRefFile.Delete ();
					StashLogFile.Delete ();
					return;
				}
				WriteStashes (stashes);
				if (i == stashes.Count) {
					// We deleted the head. Write the new head.
					File.WriteAllText (StashRefFile.FullName, stashes.Last ().CommitId + "\n");
				}
			}
		}
		
		public IEnumerator<Stash> GetEnumerator ()
		{
			return ReadStashes ().GetEnumerator ();
		}
		
		List<Stash> ReadStashes ()
		{
			// Reads the registered stashes
			// Results are returned from the bottom to the top of the stack
			
			List<Stash> result = new List<Stash> ();
			FileInfo logFile = StashLogFile;
			if (!logFile.Exists)
				return result;
			
			Dictionary<string,Stash> stashes = new Dictionary<string, Stash> ();
			Stash first = null;
			foreach (string line in File.ReadAllLines (logFile.FullName)) {
				Stash s = Stash.Parse (line);
				s.StashCollection = this;
				if (s.PrevStashCommitId == null)
					first = s;
				else
					stashes.Add (s.PrevStashCommitId, s);
			}
			while (first != null) {
				result.Add (first);
				stashes.TryGetValue (first.CommitId, out first);
			}
			return result;
		}
		
		void WriteStashes (List<Stash> list)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (var s in list) {
				sb.Append (s.FullLine);
				sb.Append ('\n');
			}
			File.WriteAllText (StashLogFile.FullName, sb.ToString ());
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}

