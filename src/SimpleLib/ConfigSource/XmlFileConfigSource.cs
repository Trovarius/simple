﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.ComponentModel;
using Simple.Logging;
using System.Xml;

namespace Simple.ConfigSource
{
    public class XmlFileConfigSource<T> :
        XmlConfigSource<T>,
        IFileConfigSource<T>,
        IConfigSource<T, XPathParameter<FileInfo>>
    {
        public XPathParameter<FileInfo> XmlFile { get; set; }
        protected FileSystemWatcher Watcher { get; set; }

        public bool Active { get; protected set; }
        public DateTime LastModification { get; protected set; }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock (this)
            {
                if (Active)
                {
                    SimpleLogger.Get(this).DebugFormat("The watch in file {0} has raised.", XmlFile.Parameter.Name);
                    InvokeReload();
                }
            }
        }

        protected virtual void SetXmlFileInfo(XPathParameter<FileInfo> info)
        {
            XmlFile = info;

            if (Watcher != null) Watcher.Dispose();

            Watcher = new FileSystemWatcher(XmlFile.Parameter.DirectoryName, XmlFile.Parameter.Name);
            Watcher.Changed += Watcher_Changed;
            Watcher.Created += Watcher_Changed;
            Watcher.Deleted += Watcher_Changed;
            Watcher.EnableRaisingEvents = true;

            Active = true;
        }

        public virtual IConfigSource<T> LoadFile(string fileName)
        {
            return Load(new FileInfo(fileName));
        }

        public override bool Reload()
        {
            lock (this)
            {
                if (XmlFile.Parameter == null) throw new InvalidOperationException("Cannot reload a non-loaded source");

                SimpleLogger.Get(this).DebugFormat("Reloading file {0}...", XmlFile.Parameter.Name);

                try
                {
                    Load(XmlFile);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
            }
        }

        public override void Dispose()
        {
            lock (this)
            {
                SimpleLogger.Get(this).DebugFormat("Disposing configurator for {0}...", typeof(T));
                Active = false;
                Watcher.Dispose();
            }
        }

        #region IConfigSource<T,XPathParameter<FileInfo>> Members

        public IConfigSource<T> Load(XPathParameter<FileInfo> input)
        {
            lock (this)
            {
                SimpleLogger.Get(this).DebugFormat("Loading XMLConfig for class {0}...", typeof(T).Name);

                SetXmlFileInfo(input.Parameter);


                using (Stream s = XmlFile.Parameter.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var ret = Load(XmlFile.ToOther(s));
                    s.Close();
                    return ret;
                }
            }
        }

        #endregion
    }
}
