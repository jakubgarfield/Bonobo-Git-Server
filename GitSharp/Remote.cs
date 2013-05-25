// 
// Remote.cs
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
using System.Collections.Generic;
using GitSharp.Core.Transport;
using System.Collections;

namespace GitSharp
{
	public class Remote
	{
		RemoteConfig _config;
		Repository _repo;
		
		
		internal Remote (Repository repo, RemoteConfig config)
		{
			_config = config;
			_repo = repo;
		}
		
		public void Update ()
		{
			_config.Update (_repo._internal_repo.Config);
			_repo.Config.Persist ();
		}
		
		internal void Delete ()
		{
			_config.Delete (_repo._internal_repo.Config);
			_repo.Config.Persist ();
		}
		
        /// <summary>
        /// local name this remote configuration is recognized as
        /// </summary>
		public string Name {
			get { return _config.Name; }
			set { _config.Name = value; }
		}

        /// <summary>
        /// all configured URIs under this remote
        /// </summary>
		public IEnumerable<URIish> URIs {
			get { return _config.URIs; }
		}

        /// <summary>
        /// all configured push-only URIs under this remote.
        /// </summary>
		public IEnumerable<URIish> PushURIs {
			get { return _config.PushURIs; }
		}

        /// <summary>
        /// Remembered specifications for fetching from a repository.
        /// </summary>
		public IEnumerable<RefSpec> Fetch {
			get { return _config.Fetch; }
		}

        /// <summary>
        /// Remembered specifications for pushing to a repository.
        /// </summary>
		public IEnumerable<RefSpec> Push {
			get { return _config.Push; }
		}

        /// <summary>
        /// Override for the location of 'git-upload-pack' on the remote system.
        /// <para/>
        /// This value is only useful for an SSH style connection, where Git is
        /// asking the remote system to execute a program that provides the necessary
        /// network protocol.
        /// <para/>
        /// returns location of 'git-upload-pack' on the remote system. If no
        /// location has been configured the default of 'git-upload-pack' is
        /// returned instead.
        /// </summary>
		public string UploadPack {
			get { return _config.UploadPack; }
		}

        /// <summary>
        /// Override for the location of 'git-receive-pack' on the remote system.
        /// <para/>
        /// This value is only useful for an SSH style connection, where Git is
        /// asking the remote system to execute a program that provides the necessary
        /// network protocol.
        /// <para/>
        /// returns location of 'git-receive-pack' on the remote system. If no
        /// location has been configured the default of 'git-receive-pack' is
        /// returned instead.
        /// </summary>
		public string ReceivePack {
			get { return _config.ReceivePack; }
		}

        /// <summary>
        /// Get the description of how annotated tags should be treated during fetch.
        /// <para/>
        /// returns option indicating the behavior of annotated tags in fetch.
        /// </summary>
		public TagOpt TagOpt {
			get { return _config.TagOpt; }
			set { _config.SetTagOpt (value); }
		}

        /// <summary>
        /// mirror flag to automatically delete remote refs.
        /// <para/>
        /// true if pushing to the remote automatically deletes remote refs
        /// </summary>
		public bool Mirror {
			get { return _config.Mirror; }
		}
        /// <summary>
        /// timeout before willing to abort an IO call.
        /// <para/>
        /// number of seconds to wait (with no data transfer occurring)
        /// before aborting an IO read or write operation with this
        /// remote.  A timeout of 0 will block indefinitely.
        /// </summary>
		public int Timeout {
			get { return _config.Timeout; }
			set { _config.Timeout = value; }
		}
		
        /// <summary>
        /// Add a new URI to the end of the list of URIs.
        /// </summary>
        /// <param name="toAdd">the new URI to add to this remote.</param>
        /// <returns>true if the URI was added; false if it already exists.</returns>
		public bool AddURI(URIish toAdd)
		{
			return _config.AddURI (toAdd);
		}

        /// <summary>
        /// Remove a URI from the list of URIs.
        /// </summary>
        /// <param name="toRemove">the URI to remove from this remote.</param>
        /// <returns>true if the URI was added; false if it already exists.</returns>
		public bool RemoveURI(URIish toRemove)
		{
			return _config.RemoveURI (toRemove);
		}

        /// <summary>
        /// Add a new push-only URI to the end of the list of URIs.
        /// </summary>
        /// <param name="toAdd">the new URI to add to this remote.</param>
        /// <returns>true if the URI was added; false if it already exists.</returns>
		public bool AddPushURI(URIish toAdd)
		{
			return _config.AddPushURI (toAdd);
		}

        /// <summary>
        /// Remove a push-only URI from the list of URIs.
        /// </summary>
        /// <param name="toRemove">the URI to remove from this remote.</param>
        /// <returns>true if the URI was added; false if it already exists.</returns>
        public bool RemovePushURI(URIish toRemove)
        {
			return _config.RemovePushURI (toRemove);
        }

        /// <summary>
        /// Add a new fetch RefSpec to this remote.
        /// </summary>
        /// <param name="s">the new specification to add.</param>
        /// <returns>true if the specification was added; false if it already exists.</returns>
        public bool AddFetchRefSpec(RefSpec s)
        {
			return _config.AddFetchRefSpec (s);
        }

        /// <summary>
        /// Override existing fetch specifications with new ones.
        /// </summary>
        /// <param name="specs">
        /// list of fetch specifications to set. List is copied, it can be
        /// modified after this call.
        /// </param>
		public void SetFetchRefSpecs(List<RefSpec> specs)
		{
			_config.SetFetchRefSpecs(specs);
		}

        /// <summary>
        /// Override existing push specifications with new ones.
        /// </summary>
        /// <param name="specs">
        /// list of push specifications to set. List is copied, it can be
        /// modified after this call.
        /// </param>
		public void SetPushRefSpecs(List<RefSpec> specs)
		{
			_config.SetPushRefSpecs(specs);
		}

		/// <summary>
        /// Remove a fetch RefSpec from this remote.
		/// </summary>
        /// <param name="s">the specification to remove.</param>
        /// <returns>true if the specification existed and was removed.</returns>
        public bool RemoveFetchRefSpec(RefSpec s)
		{
			return _config.RemoveFetchRefSpec(s);
		}

        /// <summary>
        /// Add a new push RefSpec to this remote.
        /// </summary>
        /// <param name="s">the new specification to add.</param>
        /// <returns>true if the specification was added; false if it already exists.</returns>
		public bool AddPushRefSpec(RefSpec s)
		{
			return _config.AddPushRefSpec(s);
		}

        /// <summary>
        /// Remove a push RefSpec from this remote.
        /// </summary>
        /// <param name="s">the specification to remove.</param>
        /// <returns>true if the specification existed and was removed.</returns>
		public bool RemovePushRefSpec(RefSpec s)
		{
			return _config.RemovePushRefSpec(s);
		}
	}
	
	public class RemoteCollection: IEnumerable<Remote>
	{
		Repository _repo;
		
		internal RemoteCollection (Repository repo)
		{
			_repo = repo;
		}
		
		public IEnumerator<Remote> GetEnumerator ()
		{
			foreach (RemoteConfig rc in RemoteConfig.GetAllRemoteConfigs (_repo._internal_repo.Config))
				yield return new Remote (_repo, rc);
		}
		
		public Remote CreateRemote (string name)
		{
			RemoteConfig rc = new RemoteConfig (_repo._internal_repo.Config, name);
			_repo.Config.Persist ();
			return new Remote (_repo, rc);
		}
	
		public void Remove (Remote remote)
		{
			remote.Delete ();
			_repo.Config.Persist ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}

