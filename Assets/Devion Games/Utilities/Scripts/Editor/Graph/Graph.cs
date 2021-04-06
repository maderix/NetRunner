using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

namespace DevionGames
{
    [System.Serializable]
    public abstract class Graph<T> where T : IEnumerable
    {
        protected EditorWindow m_Host;
        public Rect m_GraphArea;
        public Vector2 m_GraphOffset;
        public float m_GraphZoom = 1f;
        public List<T> m_Selection = new List<T>();
        private List<int> m_PrevSelection;
        public Action<List<T>> onSelectionChange;

        protected abstract T[] inspectedNodes { get; }

        public abstract T[] nodes { get; }

        public SelectionMode m_SelectionMode;
        private Vector2 m_SelectionStartPosition;
        private bool m_DragNodes;
        private bool m_CenterGraph;
        private Styles m_Styles;

        protected ConnectionStyle m_ConnectionStyle;
        protected bool m_MoveChildrenOnDrag;
        protected bool m_ReorderChildrenByPosition;
        protected bool m_OpenInspectorOnTaskDoubleClick;
        protected bool m_OpenInspectorOnTaskClick;

        private bool IsDocked
        {
            get
            {
                BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
                return (bool)isDockedMethod.Invoke(m_Host, null);
            }
        }

        public Graph()
        {
#if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += delegate
            {
                if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    this.m_PrevSelection = inspectedNodes.Where(x => this.m_Selection.Contains(x)).Select(y => inspectedNodes.ToList().IndexOf(y)).ToList();
                }
                else
                {
                    this.m_Selection = inspectedNodes.Where(x => this.m_PrevSelection.Contains(inspectedNodes.ToList().IndexOf(x))).ToList();
                }
            };
#else
			EditorApplication.playmodeStateChanged += delegate {
				if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
					this.m_PrevSelection = inspectedNodes.Where (x => this.m_Selection.Contains (x)).Select (y => inspectedNodes.ToList ().IndexOf (y)).ToList ();	
				} else {
					this.m_Selection = inspectedNodes.Where (x => this.m_PrevSelection.Contains (inspectedNodes.ToList ().IndexOf (x))).ToList ();
				}
			};
#endif

            Undo.undoRedoPerformed += delegate ()
            {
                LoadSelection();
            };
        }

        public void SaveSelection()
        {
            this.m_PrevSelection = inspectedNodes.Where(x => this.m_Selection.Contains(x)).Select(y => inspectedNodes.ToList().IndexOf(y)).ToList();
        }

        public void LoadSelection()
        {
            this.m_Selection = inspectedNodes.Where(x => this.m_PrevSelection.Contains(inspectedNodes.ToList().IndexOf(x))).ToList();
        }

        public void OnGUI(EditorWindow host, Rect position)
        {
            if (m_Styles == null) {
                this.m_Styles = new Styles();
            }
            InitializePreferences();
            this.m_Host = host;
            this.m_GraphArea = position;
            this.DrawBackground();
            this.DrawGrid();

            ZoomableArea.Begin(this.m_GraphArea, IsDocked ? 0f : 3f, this.m_GraphZoom);
            this.DrawNodeConnections(inspectedNodes, this.m_GraphOffset);
            this.DrawNodes(inspectedNodes.Where(x => !this.m_Selection.Contains(x)).ToArray(), this.m_GraphOffset);
            this.DrawNodes(this.m_Selection.ToArray(), this.m_GraphOffset);
            this.ConnectNodes(inspectedNodes, this.m_GraphOffset);
            ZoomableArea.End();

            GUILayout.BeginArea(this.m_GraphArea);
            this.OnGUI();
            this.SelectInGraph();
            this.ContextMenuClick();
            this.DragNodes();
            this.ZoomToGraph();
            this.DragGraph();
            if(Event.current.type == EventType.KeyDown)
                this.HandleKeyEvents(Event.current.keyCode);

            this.ExecuteCommands();
            GUILayout.EndArea();
            if (this.m_CenterGraph)
            {
                CenterGraph();
            }
        }

        protected virtual void HandleKeyEvents(KeyCode key) { }

        public void ZoomField()
        {
            EditorGUIUtility.labelWidth = 40f;


            float temp = Mathf.RoundToInt(EditorGUILayout.Slider("Scale", this.m_GraphZoom * 100f, 40f, 100f)) / 100f;
            if (temp != m_GraphZoom)
            {
                Vector2 delta = this.m_GraphArea.size * 0.5f / this.m_GraphZoom;
                this.m_GraphZoom = Mathf.Clamp(temp, 0.4f, 1f);
                delta = this.m_GraphArea.size * 0.5f / this.m_GraphZoom - delta;
                this.m_GraphOffset = this.m_GraphOffset + delta;
            }
            GUILayout.Label("%");
            EditorGUIUtility.labelWidth = 0f;
        }

        public void CenterGraphView()
        {
            this.m_CenterGraph = true;
        }

        protected abstract void DrawNode(Rect rect, T node, bool selected);

        protected abstract void MoveNode(T node, Vector2 delta, bool moveChildren);

        protected abstract Rect GetNodeRect(T node, Vector2 offset);

        protected abstract void ConnectNodes(T[] nodes, Vector2 offset);

        protected virtual void OnGUI()
        {

        }

        protected virtual void NodeContextMenu(T node, Vector2 position)
        {

        }

        protected virtual void GraphContextMenu(Vector2 position)
        {

        }

        protected virtual void ExecuteCommand(string name)
        {

        }

        protected virtual void InitializePreferences() {
      
        }

        protected virtual void DrawNodeConnection(T parent, T child, Vector2 offset)
        {
            DrawConnection(GetNodeRect(parent, offset).center, GetNodeRect(child, offset).center, this.m_ConnectionStyle, Color.white);
        }

        protected virtual void DrawNodeConnections(T[] nodes, Vector2 offset)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            for (int i = 0; i < nodes.Length; i++)
            {
                T node = nodes[i];
                int index = 0;
                foreach (T child in node)
                {
                    this.DrawNodeConnection(node, child, offset);
                    ++index;
                }
            }
        }

        protected void SelectAll()
        {
            this.m_Selection.Clear();
            this.m_Selection.AddRange(inspectedNodes);
            if (onSelectionChange != null)
            {
                onSelectionChange(this.m_Selection);
            }
        }

        protected void InvertSelection()
        {
            T[] nodes = inspectedNodes.Where(x => !this.m_Selection.Contains(x)).ToArray();
            this.m_Selection.Clear();
            this.m_Selection.AddRange(nodes);
            if (onSelectionChange != null)
            {
                onSelectionChange(this.m_Selection);
            }
        }

        protected void DeselectAll()
        {
            this.m_Selection.Clear();
            if (onSelectionChange != null)
            {
                onSelectionChange(this.m_Selection);
            }
        }

        private void DrawNodes(T[] nodes, Vector2 offset)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                T node = nodes[i];
                Rect rect = GetNodeRect(node, offset);
                this.DrawNode(rect, node, this.m_Selection.Contains(node));
            }
        }

        private void ContextMenuClick()
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.ContextClick)
            {
                Vector2 mousePosition = currentEvent.mousePosition / this.m_GraphZoom - this.m_GraphOffset;
                T node = GetNodeAtPosition(mousePosition);
                if (node != null)
                {
                    this.NodeContextMenu(node, mousePosition);
                }
                else
                {
                    this.GraphContextMenu(mousePosition);
                }
            }
        }

        protected T GetNodeAtPosition(Vector2 position)
        {
            for (int i = 0; i < this.m_Selection.Count; i++)
            {
                T node = this.m_Selection[i];
                Rect rect = GetNodeRect(node, Vector2.zero);
                if (rect.Contains(position))
                {
                    return node;
                }
            }

            for (int i = 0; i < inspectedNodes.Length; i++)
            {
                T node = inspectedNodes[i];
                Rect rect = GetNodeRect(node, Vector2.zero);
                if (rect.Contains(position))
                {
                    return node;
                }
            }
            return default(T);
        }

        private List<T> GetNodesInRect(Rect rect)
        {
            List<T> nodeList = new List<T>();
            for (int i = 0; i < inspectedNodes.Length; i++)
            {
                T node = inspectedNodes[i];
                Rect position = GetNodeRect(node, Vector2.zero);
                if (position.xMax < rect.x || position.x > rect.xMax || position.yMax < rect.y || position.y > rect.yMax)
                {
                    continue;
                }
                nodeList.Add(node);
            }
            return nodeList;
        }

        private void ExecuteCommands()
        {
            Event currentEvent = Event.current;
            string[] commands = new string[] {
                "Copy",
                "Paste",
                "Cut",
                "Duplicate",
                "Delete",
                "SelectAll",
                "DeselectAll",
                "CenterGraph"
            };
            if (currentEvent.type == EventType.ValidateCommand && commands.Contains(currentEvent.commandName))
            {
                currentEvent.Use();
            }

            if (currentEvent.type == EventType.ExecuteCommand)
            {
                ExecuteCommand(currentEvent.commandName);
            }
        }

        private bool m_DragStarted;

        private void DragNodes()
        {
            Event currentEvent = Event.current;
            switch (currentEvent.rawType)
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown)
                    {
                        this.m_DragNodes = true;
                        SaveSelection();
                        currentEvent.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (this.m_DragNodes)
                    {
                        this.m_DragNodes = false;
                        if (this.m_DragStarted)
                        {
                            this.m_DragStarted = false;
                            this.m_Host.SendEvent(EditorGUIUtility.CommandEvent("OnEndDrag"));
                        }
                        currentEvent.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (this.m_DragNodes)
                    {
                        if (!m_DragStarted)
                        {
                            this.m_Host.SendEvent(EditorGUIUtility.CommandEvent("OnStartDrag"));
                            this.m_DragStarted = true;
                        }
                        for (int i = 0; i < m_Selection.Count; i++)
                        {
                            T node = m_Selection[i];
                            MoveNode(node, currentEvent.delta / this.m_GraphZoom, this.m_MoveChildrenOnDrag);
                        }
                        if (this.m_ReorderChildrenByPosition)
                        {
                            for (int i = 0; i < this.m_Selection.Count; i++)
                            {
                                T node = this.m_Selection[i];
                                ReorderNodes(node);
                            }
                        }

                        currentEvent.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (this.m_DragNodes)
                    {
                        AutoScrollNodes(2.5f);
                    }
                    break;
            }

        }

        protected virtual void ReorderNodes(T node)
        {

        }

        private void AutoScrollNodes(float speed)
        {
            Vector2 delta = Vector2.zero;
            Vector2 mousePosition = Event.current.mousePosition;
            if (mousePosition.y < 15f)
            {
                delta.y += speed;
            }
            else if (mousePosition.y > this.m_GraphArea.height - 15f)
            {
                delta.y -= speed;
            }

            if (mousePosition.x < 15f)
            {
                delta.x += speed;
            }
            else if (mousePosition.x > this.m_GraphArea.width - 15f)
            {
                delta.x -= speed;
            }
            this.m_GraphOffset = this.m_GraphOffset + (delta / this.m_GraphZoom);
            for (int i = 0; i < this.m_Selection.Count; i++)
            {
                T node = this.m_Selection[i];
                MoveNode(node, -(delta / this.m_GraphZoom), this.m_MoveChildrenOnDrag);
            }
            this.m_Host.Repaint();

        }

        private bool Select(T node)
        {
            if (node != null)
            {
                if (EditorGUI.actionKey || Event.current.shift)
                {
                    if (!m_Selection.Contains(node))
                    {
                        m_Selection.Add(node);
                    }
                    else
                    {
                        m_Selection.Remove(node);
                    }
                }
                else if (!m_Selection.Contains(node))
                {
                    m_Selection.Clear();
                    m_Selection.Add(node);
                }
                if (onSelectionChange != null)
                {
                    onSelectionChange(this.m_Selection);
                }
                return true;
            }
            if (!EditorGUI.actionKey && !Event.current.shift)
            {
                m_Selection.Clear();
                if (onSelectionChange != null)
                {
                    onSelectionChange(this.m_Selection);
                }
            }
            return false;
        }

        private void Select(Rect rect)
        {
            for (int i = 0; i < inspectedNodes.Length; i++)
            {
                T node = inspectedNodes[i];
                Rect position = GetNodeRect(node, Vector2.zero);
                if (position.xMax < rect.x || position.x > rect.xMax || position.yMax < rect.y || position.y > rect.yMax)
                {
                    m_Selection.Remove(node);
                    if (onSelectionChange != null)
                    {
                        onSelectionChange(this.m_Selection);
                    }
                    continue;
                }
                if (!m_Selection.Contains(node))
                {
                    m_Selection.Add(node);
                    if (onSelectionChange != null)
                    {
                        onSelectionChange(this.m_Selection);
                    }
                }
            }
        }

        private void SelectInGraph()
        {
            Event currentEvent = Event.current;
            switch (currentEvent.rawType)
            {
                case EventType.MouseDown:
                    if (currentEvent.type == EventType.MouseDown)
                    {

                        T node = GetNodeAtPosition(currentEvent.mousePosition / this.m_GraphZoom - this.m_GraphOffset);
                        if (node != null && Event.current.button == 1 && !this.m_Selection.Contains(node))
                        {

                            this.m_Selection.Clear();
                            this.m_Selection.Add(node);
                            if (onSelectionChange != null)
                            {
                                onSelectionChange(this.m_Selection);
                            }
                            Event.current.Use();
                            return;
                        }
                        if (currentEvent.button == 0)
                        {
                            this.m_SelectionStartPosition = currentEvent.mousePosition;
                            if (Select(node))
                            {
                                if (this.m_OpenInspectorOnTaskClick && currentEvent.clickCount == 1 || this.m_OpenInspectorOnTaskDoubleClick && currentEvent.clickCount == 2)
                                {
                                    this.m_Host.SendEvent(EditorGUIUtility.CommandEvent("SelectInspector"));
                                }
                                GUIUtility.keyboardControl = 0;
                                return;
                            }
                            this.m_SelectionMode = SelectionMode.Pick;
                            currentEvent.Use();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (this.m_SelectionMode == SelectionMode.Pick || this.m_SelectionMode == SelectionMode.Rect)
                    {
                        this.m_SelectionMode = SelectionMode.None;
                        currentEvent.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (this.m_SelectionMode == SelectionMode.Pick || this.m_SelectionMode == SelectionMode.Rect)
                    {
                        this.m_SelectionMode = SelectionMode.Rect;
                        Select(this.m_SelectionStartPosition / this.m_GraphZoom - this.m_GraphOffset, currentEvent.mousePosition / this.m_GraphZoom - this.m_GraphOffset);
                        currentEvent.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (m_SelectionMode == SelectionMode.Rect)
                    {
                        DrawSelection(this.m_SelectionStartPosition, currentEvent.mousePosition);
                        AutoScrollGraph(2.5f);
                    }

                    break;
            }
        }

        private void AutoScrollGraph(float speed)
        {
            Vector2 delta = Vector2.zero;
            Vector2 mousePosition = Event.current.mousePosition;
            if (mousePosition.y < 15f)
            {
                delta.y += speed;
            }
            else if (mousePosition.y > this.m_GraphArea.height - 15f)
            {
                delta.y -= speed;
            }

            if (mousePosition.x < 15f)
            {
                delta.x += speed;
            }
            else if (mousePosition.x > this.m_GraphArea.width - 15f)
            {
                delta.x -= speed;
            }

            delta = Vector2.ClampMagnitude(delta, 1.5f);
            this.m_GraphOffset = this.m_GraphOffset + (delta / this.m_GraphZoom);
            this.m_SelectionStartPosition += (delta / this.m_GraphZoom);
            this.m_Host.Repaint();

        }

        private void Select(Vector2 start, Vector2 end)
        {
            Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (rect.width < 0f)
            {
                rect.x = rect.x + rect.width;
                rect.width = -rect.width;
            }
            if (rect.height < 0f)
            {
                rect.y = rect.y + rect.height;
                rect.height = -rect.height;
            }
            Select(rect);
        }

        private void ZoomToGraph()
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.ScrollWheel)
            {
                GUI.FocusControl(string.Empty);
                Vector2 mousePosition = currentEvent.mousePosition / this.m_GraphZoom;
                float delta = -currentEvent.delta.y / 100f;
                this.m_GraphZoom = this.m_GraphZoom + delta;
                this.m_GraphZoom = Mathf.Clamp(this.m_GraphZoom, 0.4f, 1f);
                mousePosition = currentEvent.mousePosition / this.m_GraphZoom - mousePosition;
                this.m_GraphOffset = this.m_GraphOffset + mousePosition;
                currentEvent.Use();
            }
        }

        private bool m_DragGraph;

        private void DragGraph()
        {
            Event currentEvent = Event.current;
            //int controlID = GUIUtility.GetControlID (FocusType.Passive);
            switch (currentEvent.rawType)
            {
                case EventType.MouseDown:
                    if (currentEvent.button == 2 && currentEvent.type == EventType.MouseDown)
                    {
                        this.m_DragGraph = true;
                        //GUIUtility.hotControl = controlID;
                        currentEvent.Use();
                    }
                    break;
                case EventType.MouseUp:
                    //if (GUIUtility.hotControl == controlID) {
                    if (this.m_DragGraph)
                    {
                        this.m_DragGraph = false;
                        if (this.m_DragStarted)
                        {
                            this.m_DragStarted = false;
                            this.m_Host.SendEvent(EditorGUIUtility.CommandEvent("OnEndDrag"));
                        }
                        //	GUIUtility.hotControl = 0;
                        currentEvent.Use();
                    }
                    //}
                    break;
                case EventType.MouseDrag:
                    if (this.m_DragGraph)
                    {

                        this.m_GraphOffset = this.m_GraphOffset + (currentEvent.delta / this.m_GraphZoom);
                        if (!m_DragStarted)
                        {
                            this.m_Host.SendEvent(EditorGUIUtility.CommandEvent("OnStartDrag"));
                            this.m_DragStarted = true;
                        }
                        currentEvent.Use();

                    }
                    break;
            }
        }

        private void CenterGraph()
        {
            Vector2 center = Vector2.zero;
            if (inspectedNodes.Length > 0)
            {
                for (int i = 0; i < inspectedNodes.Length; i++)
                {
                    T node = inspectedNodes[i];
                    center += GetNodeRect(node, Vector2.zero).center;
                }
                center /= inspectedNodes.Length;

            }
            this.m_GraphOffset = -center + (this.m_GraphArea.size * 0.5f) / this.m_GraphZoom;
            this.m_Host.Repaint();
            this.m_CenterGraph = false;
        }

        private void DrawBackground()
        {
            if (Event.current.type == EventType.Repaint)
            {
                this.m_Styles.graphBackground.Draw(this.m_GraphArea, false, false, false, false);
            }
        }

        private void DrawGrid()
        {
            if (Event.current.type == EventType.Repaint)
            {
                GL.PushMatrix();
                GL.Begin(1);
                GL.Color(this.m_Styles.gridMinorColor);
                this.DrawGridLines(Styles.gridMinorSize * this.m_GraphZoom, new Vector2(this.m_GraphOffset.x % Styles.gridMinorSize * this.m_GraphZoom, this.m_GraphOffset.y % Styles.gridMinorSize * this.m_GraphZoom));
                GL.Color(this.m_Styles.gridMajorColor);
                this.DrawGridLines(Styles.gridMajorSize * this.m_GraphZoom, new Vector2(this.m_GraphOffset.x % Styles.gridMajorSize * this.m_GraphZoom, this.m_GraphOffset.y % Styles.gridMajorSize * this.m_GraphZoom));
                GL.End();
                GL.PopMatrix();
            }
        }

        private void DrawGridLines(float gridSize, Vector2 offset)
        {
            float x = this.m_GraphArea.x + offset.x;
            if (offset.x < 0f)
            {
                x = x + gridSize;
            }
            for (float i = x; i < this.m_GraphArea.x + this.m_GraphArea.width; i = i + gridSize)
            {
                this.DrawLine(new Vector2(i, this.m_GraphArea.y), new Vector2(i, this.m_GraphArea.y + this.m_GraphArea.height));
            }
            float y = this.m_GraphArea.y + offset.y;
            if (offset.y < 0f)
            {
                y = y + gridSize;
            }
            for (float j = y; j < this.m_GraphArea.y + this.m_GraphArea.height; j = j + gridSize)
            {
                this.DrawLine(new Vector2(this.m_GraphArea.x, j), new Vector2(this.m_GraphArea.x + this.m_GraphArea.width, j));
            }
        }

        private void DrawLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }

        private void DrawSelection(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                this.m_Styles.graphSelection.Draw(rect, false, false, false, false);
            }
        }

        private void DrawSelection(Vector2 start, Vector2 end)
        {
            Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (rect.width < 0f)
            {
                rect.x = rect.x + rect.width;
                rect.width = -rect.width;
            }
            if (rect.height < 0f)
            {
                rect.y = rect.y + rect.height;
                rect.height = -rect.height;
            }
            DrawSelection(rect);
        }

        protected void DrawConnection(Vector2 start, Vector2 end, ConnectionStyle style, Color color)
        {
            Vector3[] points;
            Vector3[] tangents;

            switch (style)
            {
                case ConnectionStyle.Angular:
                    GetAngularConnectorValues(start, end, out points, out tangents);
                    DrawRoundedPolyLine(points, tangents, null, color);
                    break;
                case ConnectionStyle.Curvy:
                    GetCurvyConnectorValues(start, end, out points, out tangents);
                    Handles.DrawBezier(points[0], points[1], tangents[0], tangents[1], color, null, 3f);
                    break;
                case ConnectionStyle.Line:
                    Handles.color = color;
                    float offset = 15f;
                    Handles.DrawAAPolyLine(3f, start, start - Vector2.down * offset);
                    Handles.DrawAAPolyLine(3f, start - Vector2.down * offset, end + Vector2.down * offset);
                    Handles.DrawAAPolyLine(3f, end + Vector2.down * offset, end);
                    break;
            }
        }

        private void DrawRoundedPolyLine(Vector3[] points, Vector3[] tangets, Texture2D tex, Color color)
        {
            Handles.color = color;
            for (int i = 0; i < (int)points.Length; i = i + 2)
            {
                Handles.DrawAAPolyLine(tex, 3.5f, new Vector3[] { points[i], points[i + 1] });
            }
            for (int j = 0; j < (int)tangets.Length; j = j + 2)
            {
                Handles.DrawBezier(points[j + 1], points[j + 2], tangets[j], tangets[j + 1], color, tex, 3.5f);
            }
        }

        private void GetAngularConnectorValues(Vector2 start, Vector2 end, out Vector3[] points, out Vector3[] tangents)
        {
            Vector2 vector2 = start - end;
            Vector2 vector21 = (vector2 / 2f) + end;
            Vector2 vector22 = new Vector2(Mathf.Sign(vector2.x), Mathf.Sign(vector2.y));
            Vector2 vector23 = new Vector2(Mathf.Min(Mathf.Abs(vector2.x / 2f), 5f), Mathf.Min(Mathf.Abs(vector2.y / 2f), 5f));
            points = new Vector3[] {
                start,
                new Vector3 (start.x, vector21.y + vector23.y * vector22.y),
                new Vector3 (start.x - vector23.x * vector22.x, vector21.y),
                new Vector3 (end.x + vector23.x * vector22.x, vector21.y),
                new Vector3 (end.x, vector21.y - vector23.y * vector22.y),
                end
            };
            Vector3[] vector3Array = new Vector3[4];
            Vector3 vector3 = points[1] - points[0];
            vector3Array[0] = ((vector3.normalized * vector23.x) * 0.6f) + points[1];
            Vector3 vector31 = points[2] - points[3];
            vector3Array[1] = ((vector31.normalized * vector23.y) * 0.6f) + points[2];
            Vector3 vector32 = points[3] - points[2];
            vector3Array[2] = ((vector32.normalized * vector23.y) * 0.6f) + points[3];
            Vector3 vector33 = points[4] - points[5];
            vector3Array[3] = ((vector33.normalized * vector23.x) * 0.6f) + points[4];
            tangents = vector3Array;
        }

        private void GetCurvyConnectorValues(Vector2 start, Vector2 end, out Vector3[] points, out Vector3[] tangents)
        {
            points = new Vector3[] { start, end };
            tangents = new Vector3[2];
            float single = (start.x >= end.x ? 0.7f : 0.3f);
            single = 0.5f;
            float single1 = 1f - single;
            float single2 = 0f;
            if (start.y > end.y)
            {
                float single3 = -0.25f;
                single = single3;
                single1 = single3;
                float single4 = (start.x - end.x) / (start.y - end.y);
                if (Mathf.Abs(single4) > 0.5f)
                {
                    float single5 = (Mathf.Abs(single4) - 0.5f) / 8f;
                    single5 = Mathf.Sqrt(single5);
                    single2 = Mathf.Min(single5 * 80f, 80f);
                    if (start.x > end.x)
                    {
                        single2 = -single2;
                    }
                }
            }
            Vector2 vector2 = start - end;
            float single6 = Mathf.Clamp01((vector2.magnitude - 10f) / 50f);
            tangents[0] = start + (new Vector2(single2, (end.y - start.y) * single + 30f) * single6);
            tangents[1] = end + (new Vector2(-single2, (end.y - start.y) * -single1 - 30f) * single6);
        }

        public enum SelectionMode
        {
            None,
            Pick,
            Rect,
        }

        private class Styles
        {
            public const float gridMinorSize = 12f;
            public const float gridMajorSize = 120f;
            public Color gridMinorColor;
            public Color gridMajorColor;

            public GUIStyle graphBackground;
            public GUIStyle graphSelection;

            public Styles()
            {
                gridMinorColor = EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.18f) : new Color(0f, 0f, 0f, 0.1f);
                gridMajorColor = EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.28f) : new Color(0f, 0f, 0f, 0.15f);

                graphBackground = new GUIStyle("flow background");
                graphSelection = new GUIStyle("SelectionRect");
            }
        }
    }
}