using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;

namespace AltSrc.UnityCommon.DataStructures
{
    public class QuadTreeView : MonoBehaviour
    {
        public List<LineSegment2D> LineSegments = new List<LineSegment2D>();

        /// <summary>
        ///   Draw each line segment in the scene view.
        /// </summary>
        public void Update()
        {
            foreach (LineSegment2D segment in LineSegments)
            {
                Debug.DrawLine(segment.PointA.ToVec3XZ(), segment.PointB.ToVec3XZ(), Color.cyan);
            }
        }

        /// <summary>
        ///   Retrieves the line segments from the quad tree and passes them to the view object.
        /// </summary>
        public static QuadTreeView Build<T>(QuadTree<T> model) where T : IBounds
        {
            GameObject viewObject = new GameObject("QuadTreeView");

            QuadTreeView view = viewObject.AddComponent<QuadTreeView>();
            view.LineSegments = model.GetLineSegments();

            return view;
        }
    }
}
