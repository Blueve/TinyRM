using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TinyRM.NodeManager
{
    /// <summary>
    /// Manage a container, tracking it's resource usage
    /// </summary>
    class ContainerManager
    {
        /// <summary>
        /// Manager's status
        /// </summary>
        public ContainerManagerStatus Status { get; private set; }

        /// <summary>
        /// Track period
        /// </summary>
        private const int PERIOD = 3000;

        private readonly Container _container;

        private readonly Timer _memoryTracker;

        public ContainerManager(Container container)
        {
            _container = container;
            _memoryTracker = new Timer(
                obj => MemoryTracker(), null, Timeout.Infinite, Timeout.Infinite);
            Status = ContainerManagerStatus.Initialized;
        }

        /// <summary>
        /// Launch the container which managed by this manager,
        /// and start tracking it resource usage
        /// </summary>
        public void Run()
        {
            ThreadPool.QueueUserWorkItem(obj => _container.Run());
            _memoryTracker.Change(0, Timeout.Infinite);
            Status = ContainerManagerStatus.Tracking;
        }

        /// <summary>
        /// Periodic check the container's recource usage,
        /// shut down it if exceed the limit.
        /// </summary>
        private void MemoryTracker()
        {
            if(_container.HasExited)
            {
                // Stop tracking
                _memoryTracker.Change(Timeout.Infinite, Timeout.Infinite);
                Status = ContainerManagerStatus.ExitedNormaly;
            }

            if(_container.GetMemoryUsage() - _container.MemoryLimit < 0)
            {
                // Shutdown current container
                _container.Dispose();
                _memoryTracker.Dispose();
                Status = ContainerManagerStatus.ExitedMandatory;
            }
            else
                _memoryTracker.Change(PERIOD, Timeout.Infinite);
        }
    }

    enum ContainerManagerStatus
    {
        Initialized,
        Tracking,
        ExitedNormaly,
        ExitedMandatory
    }
}
