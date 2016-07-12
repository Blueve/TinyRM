using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyRM.NodeManager
{
    class RecourseManager
    {
        private static Lazy<RecourseManager> _instance = new Lazy<RecourseManager>(() => new RecourseManager());

        private readonly HashSet<ContainerManager> _containers = new HashSet<ContainerManager>();

        public RecourseManager Instance
        {
            get { return _instance.Value; }
        }
    }
}
