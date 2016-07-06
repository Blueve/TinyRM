using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyRM.NodeManager
{
    abstract class Container : IDisposable
    {
        #region Public Property
        /// <summary>
        /// Memory limit of this container
        /// </summary>
        public int MemoryLimit { get; private set; }

        /// <summary>
        /// Has current container exited
        /// </summary>
        public abstract bool HasExited { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Init a container with a memory limit
        /// </summary>
        /// <param name="memory">Memory limit (MB)</param>
        protected Container(int memory)
        {
            MemoryLimit = memory;
        }
        #endregion

        #region Public Abstract Method
        /// <summary>
        /// Get current memory usage of this container
        /// </summary>
        /// <returns>Memory usage (MB)</returns>
        public abstract long GetMemoryUsage();

        /// <summary>
        /// Launch this container
        /// </summary>
        public abstract void Run();
        #endregion

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }
        
        /// <summary>
        /// Release resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
