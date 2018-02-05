using System.Collections.Generic;
using UnityEngine;
using AltSrc.UnityCommon.Math;
using AltSrc.UnityCommon.Debugging;
using System.Linq;

namespace AltSrc.UnityCommon.DataStructures
{
    public class QuadTreeView : MonoBehaviour
    {
        public List<Rect> Bounds = new List<Rect>();

        protected float yOffset = 0.1f;

        /// <summary>
        ///   Draw each line segment in the scene view.
        /// </summary>
        public void Update()
        {
            foreach (Rect b in Bounds)
            {
                DebugUtils.DrawRect(b.width, b.height, b.center.ToVec3XZ(yOffset), Vector3.up, Color.yellow);
            }
        }

        /// <summary>
        ///   Retrieves the line segments from the quad tree and passes them to the view object.
        /// </summary>
        public static QuadTreeView Build<T>(QuadTree<T> model) where T : IBounds
        {
            GameObject viewObject = new GameObject("QuadTreeView");
            QuadTreeView view = viewObject.AddComponent<QuadTreeView>();

            view.Bounds = 
                model
                .GetAllNodes()
                .Select(n => n.Bounds)
                .ToList();

            return view;
        }
    }
}
