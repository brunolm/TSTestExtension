using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using TSTestAdapter.Helpers;

namespace TSTestAdapter
{
    [Export(typeof(ITestContainerDiscoverer))]
    public class TSTestContainerDiscoverer : ITestContainerDiscoverer
    {
        #region ITestContainerDiscoverer members

        public Uri ExecutorUri
        {
            get { return TSTestExecutor.ExecutorUri; }
        }

        public IEnumerable<ITestContainer> TestContainers
        {
            get { return GetContainers(); }
        }

        public event EventHandler TestContainersUpdated;

        #endregion


        private IServiceProvider serviceProvider;

        [ImportingConstructor]
        public TSTestContainerDiscoverer([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private IEnumerable<ITestContainer> GetContainers()
        {
            var containers = new List<ITestContainer>();

            Parallel.ForEach(GetTestFiles(), filePath =>
            {
                containers.Add(new TSTestContainer(this, filePath));
            });

            return containers;
        }

        private IEnumerable<string> GetTestFiles()
        {
            var solution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
            var loadedProjects = solution.EnumerateLoadedProjects(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION).OfType<IVsProject>();

            return loadedProjects.SelectMany(GetTestFiles).ToList();
        }

        private IEnumerable<string> GetTestFiles(IVsProject project)
        {
            return VsSolutionHelper.GetProjectItems(project).Where(o => IsTestFile(o));
        }

        private bool IsTestFile(string filePath)
        {
            return filePath.EndsWith(".test.ts");
        }
    }
}
