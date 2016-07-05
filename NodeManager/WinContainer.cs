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
    /// <summary>
    /// A container for windows application (.exe)
    /// </summary>
    class WinContainer : Container
    {

        #region Private Field
        /// <summary>
        /// Root process of this container's inner application
        /// </summary>
        private Process _root;

        private readonly ProcessStartInfo _startInfo;
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
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments
            };
        }
        #endregion

        #region Private Method
        /// <summary>
        /// Kill entire process tree
        /// </summary>
        private void KillProcessTree()
        {
            foreach(var process in TraverseProcessTree())
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
        private IEnumerable<Process> TraverseProcessTree()
        {
            if (_root == null)
                yield break;

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
        public override void Run()
        {
            // TODO: Should start this process in another user group
            // Start a new process to run the given WinApplication
            try
            {
                _root = Process.Start(_startInfo);
            }
            catch (InvalidOperationException)
            {
                // fileName or arguments is null
            }
            catch (Win32Exception)
            {
                // Can't lunch the process
            }
            catch (FileNotFoundException)
            {
                // WinApplication was not found
            }
        }

        private bool disposedValue = false;

        /// <summary>
        /// Release resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                KillProcessTree();
                Dispose();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Get current memory usage of this container
        /// </summary>
        /// <returns>Memory usage (MB)</returns>
        public override long GetMemoryUsage()
        {
            long usage = 0;
            foreach (var process in TraverseProcessTree())
            {
                usage += process.WorkingSet64;
            }
            // Convert (byte) to (MB)
            return usage >> 20;
        }
        #endregion
    }
}
