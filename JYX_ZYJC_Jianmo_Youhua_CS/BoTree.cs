using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class BoTree<T>
    {
        public BoTree()
        {
            nodes = new List<BoTree<T>>();
        }

        public BoTree(T data)
        {
            this.Data = data;
            nodes = new List<BoTree<T>>();
        }

        private BoTree<T> parent;

        /// <summary>
        /// 父结点
        /// </summary>
        public BoTree<T> Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// 结点数据
        /// </summary>
        public T Data { get; set; }

        private List<BoTree<T>> nodes;


        /// <summary>
        /// 子结点
        /// </summary>
        public List<BoTree<T>> Nodes
        {
            get { return nodes; }
        }

        /// <summary>
        /// 添加结点
        /// </summary>
        /// <param name="node">结点</param>
        public void AddNode(BoTree<T> node)
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
        public void AddNode(List<BoTree<T>> nodes)
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
        public void Remove(BoTree<T> node)
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
