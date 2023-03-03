using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PathCreation {
    public class PathCreator : MonoBehaviour {

        /// This class stores data for the path editor, and provides accessors to get the current vertex and bezier path.
        /// Attach to a GameObject to create a new path editor.

        public event System.Action pathUpdated;

        [SerializeField, HideInInspector]
        PathCreatorData editorData;
        [SerializeField, HideInInspector]
        bool initialized;

        GlobalDisplaySettings globalEditorDisplaySettings;

        // Vertex path created from the current bezier path
        public VertexPath path {
            get {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                return editorData.GetVertexPath(transform);
            }
        }

        // The bezier path created in the editor
        public BezierPath bezierPath {
            get {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                return editorData.bezierPath;
            }
            set {
                if (!initialized) {
                    InitializeEditorData (false);
                }
                editorData.bezierPath = value;
            }
        }

        [ShowInInspector]
        public List<PathActionArea> ActionArea;
        
        [ShowIf("@EditorActionArea.BezierIndex % 3 == 0")]
        public PathActionArea EditorActionArea;

        
        [ShowIf("GetAction")]
        [Button("增加新路径数据")]
        public void AddNewArea()
        {
            SetActionArea(EditorActionArea);
        }

        public void GetActionArea(int index)
        {
            foreach (var pathActionArea in ActionArea)
            {
                if (pathActionArea.BezierIndex == index)
                {
                    EditorActionArea = pathActionArea;
                    return;
                }
            }

            EditorActionArea = new PathActionArea
            {
                BezierIndex = index
            };
        }

        public bool GetAction()
        {
            foreach (var pathActionArea in ActionArea)
            {
                if (pathActionArea.BezierIndex == EditorActionArea.BezierIndex)
                {
                    return false;
                }
            }

            if (EditorActionArea.BezierIndex % 3 != 0)
            {
                return false;
            }

            return true;
        }
        
        public void SetActionArea(PathActionArea info)
        {
            for (var index = 0; index < ActionArea.Count; index++)
            {
                var pathActionArea = ActionArea[index];
                if (pathActionArea.BezierIndex == info.BezierIndex)
                {
                    ActionArea[index] = info;
                    return;
                }
            }
            ActionArea.Add(info);
        }

        #region Internal methods

        /// Used by the path editor to initialise some data
        public void InitializeEditorData (bool in2DMode) {
            if (editorData == null) {
                editorData = new PathCreatorData ();
            }
            editorData.bezierOrVertexPathModified -= TriggerPathUpdate;
            editorData.bezierOrVertexPathModified += TriggerPathUpdate;

            editorData.Initialize (in2DMode);
            initialized = true;
        }

        public PathCreatorData EditorData {
            get {
                return editorData;
            }

        }

        public void TriggerPathUpdate () {
            if (pathUpdated != null) {
                pathUpdated ();
            }
        }

#if UNITY_EDITOR

        // Draw the path when path objected is not selected (if enabled in settings)
        void OnDrawGizmos () {

            // Only draw path gizmo if the path object is not selected
            // (editor script is resposible for drawing when selected)
            GameObject selectedObj = UnityEditor.Selection.activeGameObject;
            if (selectedObj != gameObject) {

                if (path != null) {
                    path.UpdateTransform (transform);

                    if (globalEditorDisplaySettings == null) {
                        globalEditorDisplaySettings = GlobalDisplaySettings.Load ();
                    }

                    if (globalEditorDisplaySettings.visibleWhenNotSelected) {

                        Gizmos.color = globalEditorDisplaySettings.bezierPath;

                        for (int i = 0; i < path.NumPoints; i++) {
                            int nextI = i + 1;
                            if (nextI >= path.NumPoints) {
                                if (path.isClosedLoop) {
                                    nextI %= path.NumPoints;
                                } else {
                                    break;
                                }
                            }
                            Gizmos.DrawLine (path.GetPoint (i), path.GetPoint (nextI));
                        }
                    }
                }
            }
            else
            {
                foreach (var path in ActionArea)
                {
                    Gizmos.color = Color.gray;
                        
                    Gizmos.DrawWireSphere(bezierPath.GetPoint(path.BezierIndex),path.ActionRadius);
                }
            }
        }
#endif

        #endregion
    }
}