using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class PipeConnectTree<T>
    {
        private PipeConnectTree<T> parent;

        /// <summary>
        /// 父结点
        /// </summary>
        public PipeConnectTree<T> Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// 结点数据
        /// </summary>
        public T Data { get; set; }
        private List<PipeConnectTree<T>> nodes;
        /// <summary>
        /// 是否为需要伸缩的管道
        /// </summary>
        public bool isTargetPipe = false;
        /// <summary>
        /// 初始值为-1，同向为1，反向为2
        /// </summary>
        public int direction = -1;
        /// <summary>
        /// 管道的流向，同向为1，反向为2
        /// </summary>
        public int fluidDir = -1;
        /// <summary>
        /// 该节点的父节点是否有过同向的管道节点，为 true 时，反向的管道不再认为是可伸长的管道
        /// </summary>
        public bool hasParallel = false;
        public bool isOutrange = false;
        public PipeConnectTree()
        {
            nodes = new List<PipeConnectTree<T>>();
        }

        public PipeConnectTree(T data)
        {
            this.Data = data;
            nodes = new List<PipeConnectTree<T>>();
        }

        /// <summary>
        /// 子结点
        /// </summary>
        public List<PipeConnectTree<T>> Nodes
        {
            get { return nodes; }
        }

        /// <summary>
        /// 添加结点
        /// </summary>
        /// <param name="node">结点</param>
        public void AddNode(PipeConnectTree<T> node)
        {
            if(!nodes.Contains(node))
            {
                node.parent = this;
                nodes.Add(node);
            }
        }

        /// <summary>
        /// 添加结点
        /// </summary>
        /// <param name="nodes">结点集合</param>
        public void AddNode(List<PipeConnectTree<T>> nodes)
        {
            foreach(var node in nodes)
            {
                if(!nodes.Contains(node))
                {
                    node.parent = this;
                    nodes.Add(node);
                }
            }
        }

        /// <summary>
        /// 移除结点
        /// </summary>
        /// <param name="node"></param>
        public void Remove(PipeConnectTree<T> node)
        {
            if (nodes.Contains(node))
                nodes.Remove(node);
        }

        /// <summary>
        /// 清空结点集合
        /// </summary>
        public void RemoveAll()
        {
            nodes.Clear();
        }
    }
}
