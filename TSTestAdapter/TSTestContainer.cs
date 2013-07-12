using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TSTestAdapter
{
    public class TSTestContainer : ITestContainer
    {
        private readonly string source;
        private readonly ITestContainerDiscoverer discoverer;
        private readonly DateTime timeStamp;

        public TSTestContainer(ITestContainerDiscoverer discoverer, string source)
        {
            this.discoverer = discoverer;
            this.source = source;
            this.timeStamp = GetTimeStamp();
        }

        private TSTestContainer(TSTestContainer copy)
            : this(copy.discoverer, copy.Source)
        {
        }

        private DateTime GetTimeStamp()
        {
            if (!String.IsNullOrEmpty(this.Source) && File.Exists(this.Source))
            {
                return File.GetLastWriteTime(this.Source);
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        #region ITestContainer members

        public int CompareTo(ITestContainer other)
        {
            var testContainer = other as TSTestContainer;
            if (testContainer == null)
            {
                return -1;
            }

            var result = String.Compare(this.Source, testContainer.Source, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
            {
                return result;
            }

            return this.timeStamp.CompareTo(testContainer.timeStamp);
        }

        public IEnumerable<Guid> DebugEngines
        {
            get { return Enumerable.Empty<Guid>(); }
        }

        public Microsoft.VisualStudio.TestWindow.Extensibility.Model.IDeploymentData DeployAppContainer()
        {
            return null;
        }

        public ITestContainerDiscoverer Discoverer
        {
            get { return this.discoverer; }
        }

        public bool IsAppContainerTestContainer
        {
            get { return false; }
        }

        public ITestContainer Snapshot()
        {
            return new TSTestContainer(this);
        }

        public string Source
        {
            get { return this.source; }
        }

        public Microsoft.VisualStudio.TestPlatform.ObjectModel.FrameworkVersion TargetFramework
        {
            get { return Microsoft.VisualStudio.TestPlatform.ObjectModel.FrameworkVersion.None; }
        }

        public Microsoft.VisualStudio.TestPlatform.ObjectModel.Architecture TargetPlatform
        {
            get { return Microsoft.VisualStudio.TestPlatform.ObjectModel.Architecture.AnyCPU; }
        }

        #endregion
    }
}
