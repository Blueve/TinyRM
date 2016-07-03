using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace TinyRM.NodeManager
{
    class WinContainer : Container
    {

        #region Private Field
        /// <summary>
        /// Root process of this container's inner application
        /// </summary>
        private readonly Process _root;
        #endregion

        #region Constructor
        /// <summary>
        /// Init a WinApplication container
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="fileName">Executable file name</param>
        /// <param name="arguments">Application's arguments</param>
        public WinContainer(int memory, string fileName, string arguments = "") : base(memory)
        {
            // TODO: Should start this process in another user group
            // Start a new process to run the given WinApplication
            try
            {
                _root = Process.Start(fileName, arguments);
            }
            catch(InvalidOperationException)
            {
                // fileName or arguments is null
            }
            catch(Win32Exception)
            {
                // Can't lunch the process
            }
            catch(FileNotFoundException)
            {
                // WinApplication was not found
            }
        }
        #endregion

        #region Private Method
        /// <summary>
        /// Kill entire process tree
        /// </summary>
        private void KillProcessTree()
        {
            foreach(var process in TraversalProcessTree())
            {
                KillProcess(process);
            }
        }

        /// <summary>
        /// Kill given process
        /// </summary>
        /// <param name="process"></param>
        private static void KillProcess(Process process)
        {
            try
            {
                process.Kill();
            }
            catch (Win32Exception)
            {
                // Can't kill the process
            }
            catch (InvalidOperationException)
            {
                // Process was exited
            }
        }

        /// <summary>
        /// Traversal the process tree
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Process> TraversalProcessTree()
        {
            var bfs = new Queue<Process>();
            bfs.Enqueue(_root);
            while (bfs.Count > 0)
            {
                // Kill front process
                var process = bfs.Dequeue();
                yield return process;

                // Add child process to bfs queue
                var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_Process WHERE ParentProcessID=" + process.Id);
                foreach (var managementObject in searcher.Get())
                {
                    try
                    {
                        var p = Process.GetProcessById(
                                Convert.ToInt32(managementObject["ProcessID"]));
                        bfs.Enqueue(p);
                    }
                    catch(ArgumentException)
                    {
                        // Process was exited
                    }
                    catch(InvalidOperationException)
                    {
                        // Illegal operation
                    }
                }
            }
        }
        #endregion

        #region Implement Continer's Abstract Method
        /// <summary>
        /// Get current memory usage of this container
        /// </summary>
        /// <returns>Memory usage (MB)</returns>
        public override void Dispose()
        {
            KillProcessTree();
        }

        /// <summary>
        /// Release resources
        /// </summary>
        public override long GetMemoryUsage()
        {
            long usage = 0;
            foreach (var process in TraversalProcessTree())
            {
                usage += process.WorkingSet64;
            }
            // Convert (byte) to (MB)
            return usage >> 20;
        }
        #endregion
    }
}
