/*
 * Copyright (C) 2008, Shawn O. Pearce <spearce@spearce.org>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using GitSharp.Core.Exceptions;
using GitSharp.Core.Util;
using Tamir.SharpSsh.jsch;

namespace GitSharp.Core.Transport
{
    public class TransportSftp : SshTransport, IWalkTransport
    {
        public static bool canHandle(URIish uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            return uri.IsRemote && "sftp".Equals(uri.Scheme);
        }

        public TransportSftp(Repository local, URIish uri)
            : base(local, uri)
        {
        }

        public override IFetchConnection openFetch()
        {
            var c = new SftpObjectDatabase(Uri.Path, this);
            var r = new WalkFetchConnection(this, c);
            r.available(c.ReadAdvertisedRefs());
            return r;
        }

        public override IPushConnection openPush()
        {
            var c = new SftpObjectDatabase(Uri.Path, this);
            var r = new WalkPushConnection(this, c);
            r.available(c.ReadAdvertisedRefs());
            return r;
        }

        private ChannelSftp NewSftp()
        {
            InitSession();

            // No timeout support in our JSch
            try
            {
                Channel channel = Sock.openChannel("sftp");
                channel.connect();
                return (ChannelSftp)channel;
            }
            catch (JSchException je)
            {
                throw new TransportException(Uri, je.Message, je);
            }
        }

        #region Nested Types

        private class SftpObjectDatabase : WalkRemoteObjectDatabase
        {
            private readonly TransportSftp _instance;
            private readonly string _objectsPath;
            private ChannelSftp _ftp;

            public SftpObjectDatabase(string path, TransportSftp instance)
            {
                this._instance = instance;

                if (path.StartsWith("/~"))
                {
                    path = path.Substring(1);
                }

                if (path.StartsWith("~/"))
                {
                    path = path.Substring(2);
                }

                try
                {
                    _ftp = instance.NewSftp();
                    _ftp.cd(path);
                    _ftp.cd("objects");
                    _objectsPath = _ftp.pwd();
                }
                catch (TransportException)
                {
                    CleanUp();
                    throw;
                }
                catch (SftpException je)
                {
                    throw new TransportException("Can't enter " + path + "/objects: " + je.message, je);
                }
            }

            private SftpObjectDatabase(SftpObjectDatabase parent, string p, TransportSftp instance)
            {
                this._instance = instance;
                try
                {
                    _ftp = instance.NewSftp();
                    _ftp.cd(parent._objectsPath);
                    _ftp.cd(p);
                    _objectsPath = _ftp.pwd();
                }
                catch (TransportException)
                {
                    CleanUp();
                    throw;
                }
                catch (SftpException je)
                {
                    throw new TransportException("Can't enter " + p + " from " + parent._objectsPath + ": " + je.message, je);
                }
            }

            public override ICollection<string> getPackNames()
            {
                var packs = new List<string>();
                try
                {
                    var list = new List<ChannelSftp.LsEntry>();
                    foreach (object o in _ftp.ls("pack")) list.Add((ChannelSftp.LsEntry)o);

                    var files = new Dictionary<string, ChannelSftp.LsEntry>();
                    var mtimes = new Dictionary<string, int>();

                    foreach (ChannelSftp.LsEntry ent in list)
                    {
                        files.Add(ent.getFilename(), ent);
                    }

                    foreach (ChannelSftp.LsEntry ent in list)
                    {
                        string n = ent.getFilename();
                        if (!n.StartsWith("pack-") || n.EndsWith(IndexPack.PackSuffix)) continue;

                        string @in = IndexPack.GetIndexFileName(n.Slice(0, n.Length - 5));
                        if (!files.ContainsKey(@in)) continue;

                        mtimes.Add(n, ent.getAttrs().getMTime());
                        packs.Add(n);
                    }

                    packs.Sort((a, b) => mtimes[a] - mtimes[b]);
                }
                catch (SftpException je)
                {
                    throw new TransportException("Can't ls " + _objectsPath + "/pack: " + je.message, je);
                }
                return packs;
            }

            public override Stream open(string path)
            {
                try
                {
                    return _ftp.get(path);
                }
                catch (SftpException je)
                {
                    if (je.id == ChannelSftp.SSH_FX_NO_SUCH_FILE)
                    {
                        throw new FileNotFoundException(path);
                    }
                    throw new TransportException("Can't get " + _objectsPath + "/" + path + ": " + je.message, je);
                }
            }

            public override Stream writeFile(string path, ProgressMonitor monitor, string monitorTask)
            {
                try
                {
                    return _ftp.put(path);
                }
                catch (SftpException je)
                {
                    if (je.id == ChannelSftp.SSH_FX_NO_SUCH_FILE)
                    {
                        MkdirP(path);
                        try
                        {
                            return _ftp.put(path);
                        }
                        catch (SftpException je2)
                        {
                            je = je2;
                        }
                    }

                    throw new TransportException("Can't write " + _objectsPath + "/" + path + ": " + je.message, je);
                }
            }

            public override void writeFile(string path, byte[] data)
            {
                string @lock = path + ".lock";
                try
                {
                    base.writeFile(@lock, data);
                    try
                    {
                        _ftp.rename(@lock, path);
                    }
                    catch (SftpException je)
                    {
                        throw new TransportException("Can't write " + _objectsPath + "/" + path + ": " + je.message, je);
                    }
                }
                catch (IOException)
                {
                    try
                    {
                        _ftp.rm(@lock);
                    }
                    catch (SftpException)
                    {
                    }
                    throw;
                }
            }

            public override void deleteFile(string path)
            {
                try
                {
                    _ftp.rm(path);
                }
                catch (SftpException je)
                {
                    if (je.id == ChannelSftp.SSH_FX_NO_SUCH_FILE)
                    {
                        throw new FileNotFoundException(path);
                    }
                    throw new TransportException("Can't delete " + _objectsPath + "/" + path + ": " + je.message, je);
                }

                string dir = path;
                int s = dir.LastIndexOf('/');
                while (s > 0)
                {
                    try
                    {
                        dir = dir.Slice(0, s);
                        _ftp.rmdir(dir);
                        s = dir.LastIndexOf('/');
                    }
                    catch (SftpException)
                    {
                        break;
                    }
                }
            }

            private void MkdirP(string path)
            {
                int s = path.LastIndexOf('/');
                if (s <= 0) return;

                path = path.Slice(0, s);
                try
                {
                    _ftp.mkdir(path);
                }
                catch (SftpException je)
                {
                    if (je.id == ChannelSftp.SSH_FX_NO_SUCH_FILE)
                    {
                        MkdirP(path);
                        try
                        {
                            _ftp.mkdir(path);
                            return;
                        }
                        catch (SftpException je2)
                        {
                            je = je2;
                        }
                    }

                    throw new TransportException("Can't mkdir " + _objectsPath + "/" + path + ": " + je.message, je);
                }
            }

            public override URIish getURI()
            {
                return _instance.Uri.SetPath(_objectsPath);
            }

            public Dictionary<string, Ref> ReadAdvertisedRefs()
            {
                var avail = new Dictionary<string, Ref>();
                readPackedRefs(avail);
                ReadRef(avail, ROOT_DIR + Constants.HEAD, Constants.HEAD);
                ReadLooseRefs(avail, ROOT_DIR + "refs", "refs/");
                return avail;
            }

            public override ICollection<WalkRemoteObjectDatabase> getAlternates()
            {
                try
                {
                    return readAlternates(INFO_ALTERNATES);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }

            public override WalkRemoteObjectDatabase openAlternate(string location)
            {
                return new SftpObjectDatabase(this, location, _instance);
            }

            private static Storage Loose(Ref r)
            {
                if (r != null && r.StorageFormat == Storage.Packed)
                    return Storage.LoosePacked;
                return Storage.Loose;
            }

            private void ReadLooseRefs(IDictionary<string, Ref> avail, string dir, string prefix)
            {
                var list = new List<ChannelSftp.LsEntry>();
                try
                {
                    foreach (object o in _ftp.ls(dir))
                    {
                        list.Add((ChannelSftp.LsEntry)o);
                    }
                }
                catch (SftpException je)
                {
                    throw new TransportException("Can't ls " + _objectsPath + "/" + dir + ": " + je.message, je);
                }

                foreach (ChannelSftp.LsEntry ent in list)
                {
                    string n = ent.getFilename();
                    if (".".Equals(n) || "..".Equals(n)) continue;

                    string nPath = dir + "/" + n;
                    if (ent.getAttrs().isDir())
                    {
                        ReadLooseRefs(avail, nPath, prefix + n + "/");
                    }
                    else
                    {
                        ReadRef(avail, nPath, prefix + n);
                    }
                }
            }

            private Ref ReadRef(IDictionary<string, Ref> avail, string path, string name)
            {
                string line;
                try
                {
                    using (StreamReader br = openReader(path))
                    {
                        line = br.ReadLine();
                    }
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
                catch (IOException err)
                {
                    throw new TransportException("Cannot Read " + _objectsPath + "/" + path + ": " + err.Message, err);
                }

                if (line == null)
                    throw new TransportException("Empty ref: " + name);

                if (line.StartsWith("ref: "))
                {
                    string target = line.Substring("ref: ".Length);
                    Ref r = avail.GetValue(target);
                    if (r == null)
                        r = ReadRef(avail, ROOT_DIR + target, target);
                    if (r == null)
                        r = new Unpeeled(Storage.New, target, null);
                    r = new SymbolicRef(name, r);
                    avail.put(r.getName(), r);
                    return r;
                }

                if (ObjectId.IsId(line))
                {
                    Ref r = new Unpeeled(Loose(avail.GetValue(name)),
                            name, ObjectId.FromString(line));
                    avail.put(r.getName(), r);
                    return r;
                }

                throw new TransportException("Bad ref: " + name + ": " + line);
            }

            private void CleanUp()
            {
                if (_ftp != null)
                {
                    try
                    {
                        if (_ftp.isConnected())
                            _ftp.disconnect();
                    }
                    finally
                    {
                        _ftp = null;
                    }
                }
#if DEBUG
                GC.SuppressFinalize(this); // Disarm lock-release checker
#endif
            }
            public override void close()
            {
                CleanUp();
            }

#if DEBUG
            // A debug mode warning if the type has not been disposed properly
            ~SftpObjectDatabase()
            {
                Console.Error.WriteLine(GetType().Name + " has not been properly disposed: " + this.getURI());
            }
#endif
        }

        #endregion
    }
}