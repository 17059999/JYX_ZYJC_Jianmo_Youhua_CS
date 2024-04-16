using Bentley.OpenPlant.Modeler.Api;
using Bentley.GeometryNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class Segment
    {
        /// <summary>
        /// 交点到父亲线段的startpoint的距离
        /// </summary>
        public double Distence { get; set; }
        /// <summary>
        /// 是否是三通
        /// </summary>
        public bool IsSanTong { get; set; }
        /// <summary>
        /// 交点是否是父亲的端点
        /// </summary>
        public bool OtherDpoint { get; set; }
        /// <summary>
        /// 交点
        /// </summary>
        public DPoint3d Lianjiedian { get; set; }
        /// <summary>
        /// 线段
        /// </summary>
        public DSegment3d Xianduan { get; set; }
        /// <summary>
        /// 父亲端点处相交线段个数
        /// </summary>
        public int Xianduanshu { get; set; }
        /// <summary>
        /// 生成的管道
        /// </summary>
        public List<BMECObject> scBmec { get; set; }
        public Segment(double distence,bool isSanTong,bool otherDpoint,DPoint3d lianjiedian,DSegment3d xianduan,int xianduanshu)
        {
            this.Distence = distence;
            this.IsSanTong = isSanTong;
            this.OtherDpoint = otherDpoint;
            this.Lianjiedian = lianjiedian;
            this.Xianduan = xianduan;
            this.Xianduanshu = xianduanshu;
            this.scBmec = new List<BMECObject>();
        }
    }
}
