﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class elbowOrBend
    {
        /// <summary>
        /// elbow或者是bend
        /// </summary>
        public string elbowOrBendName { get; set; }

        /// <summary>
        /// elbow弯曲半径
        /// </summary>
        public string elbowRadius { get; set; }

        /// <summary>
        /// elbow弯曲角度
        /// </summary>
        public string elbowAngle { get; set; }

        /// <summary>
        /// elbow类型elbow或者虾米弯
        /// </summary>
        public string elbowType { get; set; }

        /// <summary>
        /// bend弯曲半径比例
        /// </summary>
        public double bendRadiusRatio { get; set; }

        /// <summary>
        /// bend弯曲半径
        /// </summary>
        public double bendRadius { get; set; }

        /// <summary>
        /// bend增加部分的前段长
        /// </summary>
        public double bendFrontLong { get; set; }

        /// <summary>
        /// bend增加部分的后段长
        /// </summary>
        public double bendAfterLong { get; set; }

        /// <summary>
        /// 虾米弯节数
        /// </summary>
        public int pitchNumber { get; set; }

        /// <summary>
        /// 判断第几次生成管道
        /// </summary>
        public int typeNumber { get; set; }

        /// <summary>
        /// 布置切割弯头
        /// </summary>
        public bool isBzQgWt { get; set; }

        /// <summary>
        /// 一根弯头切割
        /// </summary>
        public bool isYgWt { get; set; }

        /// <summary>
        /// 两端切
        /// </summary>
        public bool isLDQ { get; set; }
    }
}
