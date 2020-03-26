using System;
using System.Collections.Generic;

public class RectangleOverlay {
    /// <summary>
    /// 定义矩形坐标结构，假定坐标都用Int32整数表示
    /// </summary>
    struct Rectangle
    {
        /// <summary>
        /// X最小值
        /// </summary>
		public int XLeft;
        /// <summary>
        /// X最大值
        /// </summary>
		public int XRight;
        /// <summary>
        /// Y最小值
        /// </summary>
		public int YBottom;
        /// <summary>
        /// Y最大值
        /// </summary>
		public int YTop;
        public Rectangle(int xL,int xR,int yB,int yT)
        {
			XLeft = xL;
			XRight = xR;
			YBottom = yB;
			YTop = yT;
        }
	}
    /// <summary>
    /// 判定两个矩形是否重叠（未排除两矩形共边的可能）
    /// </summary>
    /// <param name="rectangle1"></param>
    /// <param name="rectangle2"></param>
    /// <returns>
    ///     true:矩形重叠
    ///     false:矩形不重叠
    /// </returns>
    bool IsOverlay(Rectangle rectangle1,Rectangle rectangle2)
    {
        //若两矩形重叠，则矩形在横纵坐标轴上的投影必然存在重叠
        return Math.Min(rectangle1.XRight, rectangle2.XRight) >= Math.Max(rectangle1.XLeft, rectangle2.XLeft) &&
                Math.Min(rectangle1.YTop, rectangle2.YTop) >= Math.Max(rectangle1.YBottom, rectangle2.YBottom);
    }
    /// <summary>
    /// 统计给定矩形中重叠矩形的个数
    /// </summary>
    /// <param name="rectangles">模板矩形集合</param>
    /// <returns>
    ///     int:重叠矩形的个数
    /// </returns>
    /// <example>
    ///     eg.
    ///    Input:   rectangles = new List<Rectangle>(){
    ///                 new Rectangle(1,6,1,4),
    ///                 new Rectangle(2,7,3,9),
    ///                 new Rectangle(5,9,2,6),
    ///                 new Rectangle(8,9,10,11),
    ///                 new Rectangle(2,6,-4,-2),
    ///                 new Rectangle(3,5,-7,-3),
    ///                 new Rectangle(7,9,-7,-3),
    ///                 new Rectangle(20,35,18,26),
    ///                 new Rectangle(16,30,15,22)};
    ///                
    ///    Output:  7
    /// </example>
    int RectangleOverlayAmount(IList<Rectangle> rectangles)
    {
        //记忆回溯减少同一矩形重复的循环判断执行次数
        IList<int> overlayIndexes = new List<int>();
        for (int i = 0; i < rectangles.Count; i++)
        {
            if (!overlayIndexes.Contains(i))
            {
                for (int j =i+1; j <rectangles.Count; j++)
                {
                    if (overlayIndexes.Contains(j))
                        continue;
                    if (IsOverlay(rectangles[i],rectangles[j]))
                    {
                        if(!overlayIndexes.Contains(i))
                        {
                            overlayIndexes.Add(i);
                        }
                        if(!overlayIndexes.Contains(j))
                        {
                            overlayIndexes.Add(j);
                        }  
                    }
                }
            }
        }
		return overlayIndexes.Count;
    }
}
