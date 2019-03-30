using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartHotelMR
{
    public enum CollectionLayoutType
    {
        Grid,
        Stacked
    }

    public enum LayoutDirection
    {
        ColumnThenRow,
        RowThenColumn,
    }

    public enum CollectionOrientation
    {
        Horizontal,
        Vertical
    }

    public enum NodeOrientationTypeEnum
    {
        None,                   // Don't rotate at all
        FaceOrigin,             // Rotate towards the origin
        FaceOriginReversed,     // Rotate towards the origin + 180 degrees
        FaceFoward,             // Zero rotation. Aka Parent Relative Forwards 
        FaceForwardReversed,    // Zero rotation + 180 degrees. Aka Parent Relative Backwards
        FaceParentUp,           // Parent Relative Up
        FaceParentDown,         // Parent Relative Down
        FaceCenterAxis,         // Lay flat on the surface, facing in
        FaceCenterAxisReversed  // Lay flat on the surface, facing out
    }

    /// <summary>
    /// A Collection Layout is simply a set of child objects organized with some
    /// layout parameters.  The collection layout can be used to quickly create 
    /// control panels or sets of prefab/objects.
    /// </summary>
    public class CollectionLayout : MonoBehaviour
    {
        #region public members
        /// <summary>
        /// Action called when collection is updated
        /// </summary>
        public Action<CollectionLayout> OnCollectionUpdated;

        /// <summary>
        /// List of objects with generated data on the object.
        /// </summary>
        [SerializeField]
        public List<CollectionLayoutNode> NodeList = new List<CollectionLayoutNode>();

        /// <summary>
        /// Type of surface to map the collection to.
        /// </summary>
        [Tooltip("Type of layout to map the collection to")]
        public CollectionLayoutType LayoutType = CollectionLayoutType.Grid;

        /// <summary>
        /// Direction to layout nodes
        /// </summary>
        [Tooltip("Type of layout to map the collection to")]
        public CollectionOrientation Orientation = CollectionOrientation.Vertical;

        /// <summary>
        /// Whether to sort objects by row first or by column first
        /// </summary>
        [Tooltip("Whether to list objects by row first or by column first")]
        public LayoutDirection Direction = LayoutDirection.RowThenColumn;

        /// <summary>
        /// Should the objects in the collection face the origin of the collection
        /// </summary>
        [Tooltip("Should the objects in the collection be rotated / how should they be rotated")]
        public NodeOrientationTypeEnum NodeOrientation = NodeOrientationTypeEnum.FaceFoward;

        /// <summary>
        /// Width of the cell per object in the collection.
        /// </summary>
        [Tooltip("Width of cell per object")]
        public float CellWidth = 0.5f;

        /// <summary>
        /// Height of the cell per object in the collection.
        /// </summary>
        [Tooltip("Height of cell per object")]
        public float CellHeight = 0.5f;

        [SerializeField]
        [Tooltip("Margin between objects horizontally")]
        private float horizontalMargin = 0.2f;

        /// <summary>
        /// Margin between objects horizontally.
        /// </summary>
        public float HorizontalMargin
        {
            get { return horizontalMargin; }
            set { horizontalMargin = value; }
        }

        [SerializeField]
        [Tooltip("Margin between objects vertically")]
        private float verticalMargin = 0.2f;

        /// <summary>
        /// Margin between objects vertically.
        /// </summary>
        public float VerticalMargin
        {
            get { return verticalMargin; }
            set { verticalMargin = value; }
        }

        [SerializeField]
        [Tooltip("Margin between objects in depth")]
        private float depthMargin = 0.2f;

        /// <summary>
        /// Margin between objects in depth.
        /// </summary>
        public float DepthMargin
        {
            get { return depthMargin; }
            set { depthMargin = value; }
        }

        public float Width { get; private set; }

        public float Height { get; private set; }
        #endregion

        #region private variables
        private int _columns;
        private int _rows;
        private Vector2 _halfCell;
        #endregion

        /// <summary>
        /// Update collection is called from the editor button on the inspector.
        /// This function rebuilds / updates the layout.
        /// </summary>
        public void UpdateCollection()
        {
            // Check for empty nodes and remove them
            List<CollectionLayoutNode> emptyNodes = new List<CollectionLayoutNode>();

            for (int i = 0; i < NodeList.Count; i++)
            {
                if (NodeList[i].transform == null || (!NodeList[i].transform.gameObject.activeSelf) || NodeList[i].transform.parent == null || !(NodeList[i].transform.parent.gameObject == this.gameObject))
                {
                    emptyNodes.Add(NodeList[i]);
                }
            }

            // Now delete the empty nodes
            for (int i = 0; i < emptyNodes.Count; i++)
            {
                NodeList.Remove(emptyNodes[i]);
            }

            emptyNodes.Clear();

            // Check when children change and adjust
            for (int i = 0; i < this.transform.childCount; i++)
            {
                Transform child = this.transform.GetChild(i);

                if (!ContainsNode(child) && child.gameObject.activeSelf)
                {
                    CollectionLayoutNode node = new CollectionLayoutNode();

                    node.Name = child.name;
                    node.transform = child;
                    NodeList.Add(node);
                }
            }

            if (LayoutType == CollectionLayoutType.Grid)
            {
                _columns = _rows = Mathf.CeilToInt(Mathf.Sqrt((float)NodeList.Count));
            }
            else if (LayoutType == CollectionLayoutType.Stacked)
            {
                _columns = 1;
                _rows = NodeList.Count;
            }

            Width = _columns * CellWidth;
            Height = _rows * CellHeight;
            _halfCell = new Vector2(CellWidth * 0.5f, CellHeight * 0.5f);

            LayoutChildren();

            if (OnCollectionUpdated != null)
            {
                OnCollectionUpdated.Invoke(this);
            }
        }

        /// <summary>
        /// Internal function for laying out all the children when UpdateCollection is called.
        /// </summary>
        private void LayoutChildren()
        {

            int cellCounter = 0;
            float startOffsetX;
            float startOffsetY;

            Vector3[] nodeGrid = new Vector3[NodeList.Count];
            Vector3 newPos = Vector3.zero;

            // Now lets lay out the grid
            startOffsetX = (_columns * 0.5f) * CellWidth;
            startOffsetY = (_rows * 0.5f) * CellHeight;

            cellCounter = 0;

            // First start with a grid then project onto surface
            switch (Direction)
            {
                case LayoutDirection.ColumnThenRow:
                default:
                    for (int c = 0; c < _columns; c++)
                    {
                        for (int r = 0; r < _rows; r++)
                        {
                            if (cellCounter < NodeList.Count)
                            {
                                if (Orientation == CollectionOrientation.Vertical)
                                    nodeGrid[cellCounter] = new Vector3((c * CellWidth) - startOffsetX + _halfCell.x, -(r * CellHeight) + startOffsetY - _halfCell.y, 0f) + (Vector3)((NodeList[cellCounter])).Offset;
                                else
                                    nodeGrid[cellCounter] = new Vector3((c * CellWidth) - startOffsetX + _halfCell.x, 0f, -(r * CellHeight) + startOffsetY - _halfCell.y) + (Vector3)((NodeList[cellCounter])).Offset;
                            }
                            cellCounter++;
                        }
                    }
                    break;

                case LayoutDirection.RowThenColumn:
                    for (int r = 0; r < _rows; r++)
                    {
                        for (int c = 0; c < _columns; c++)
                        {
                            if (cellCounter < NodeList.Count)
                            {
                                if (Orientation == CollectionOrientation.Vertical)
                                    nodeGrid[cellCounter] = new Vector3((c * CellWidth) - startOffsetX + _halfCell.x, -(r * CellHeight) + startOffsetY - _halfCell.y, 0f) + (Vector3)((NodeList[cellCounter])).Offset;
                                else
                                    nodeGrid[cellCounter] = new Vector3((c * CellWidth) - startOffsetX + _halfCell.x, 0f, -(r * CellHeight) + startOffsetY - _halfCell.y) + (Vector3)((NodeList[cellCounter])).Offset;
                            }
                            cellCounter++;
                        }
                    }
                    break;

            }

            switch (LayoutType)
            {
                case CollectionLayoutType.Grid:
                case CollectionLayoutType.Stacked:
                    for (int i = 0; i < NodeList.Count; i++)
                    {
                        newPos = nodeGrid[i];
                        NodeList[i].transform.localPosition = newPos;
                        UpdateNodeFacing(NodeList[i], newPos);
                    }
                    break;
            }
        }

        /// <summary>
        /// Update the facing of a node given the nodes new position for facing orign with node and orientation type
        /// </summary>
        /// <param name="node"></param>
        /// <param name="orientType"></param>
        /// <param name="newPos"></param>
        private void UpdateNodeFacing(CollectionLayoutNode node, Vector3 newPos = default(Vector3))
        {
            Vector3 centerAxis;
            Vector3 pointOnAxisNearestNode;
            switch (NodeOrientation)
            {
                case NodeOrientationTypeEnum.FaceOrigin:
                    node.transform.rotation = Quaternion.LookRotation(node.transform.position - this.transform.position, this.transform.up);
                    break;

                case NodeOrientationTypeEnum.FaceOriginReversed:
                    node.transform.rotation = Quaternion.LookRotation(this.transform.position - node.transform.position, this.transform.up);
                    break;

                case NodeOrientationTypeEnum.FaceCenterAxis:
                    centerAxis = Vector3.Project(node.transform.position - this.transform.position, this.transform.up);
                    pointOnAxisNearestNode = this.transform.position + centerAxis;
                    node.transform.rotation = Quaternion.LookRotation(node.transform.position - pointOnAxisNearestNode, this.transform.up);
                    break;

                case NodeOrientationTypeEnum.FaceCenterAxisReversed:
                    centerAxis = Vector3.Project(node.transform.position - this.transform.position, this.transform.up);
                    pointOnAxisNearestNode = this.transform.position + centerAxis;
                    node.transform.rotation = Quaternion.LookRotation(pointOnAxisNearestNode - node.transform.position, this.transform.up);
                    break;

                case NodeOrientationTypeEnum.FaceFoward:
                    node.transform.forward = transform.rotation * Vector3.forward;
                    break;

                case NodeOrientationTypeEnum.FaceForwardReversed:
                    node.transform.forward = transform.rotation * Vector3.back;
                    break;

                case NodeOrientationTypeEnum.FaceParentUp:
                    node.transform.forward = transform.rotation * Vector3.up;
                    break;

                case NodeOrientationTypeEnum.FaceParentDown:
                    node.transform.forward = transform.rotation * Vector3.down;
                    break;

                case NodeOrientationTypeEnum.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Internal function to check if a node exists in the NodeList.
        /// </summary>
        /// <param name="node">A <see cref="Transform"/> of the node to see if it's in the NodeList</param>
        /// <returns></returns>
        private bool ContainsNode(Transform node)
        {
            for (int i = 0; i < NodeList.Count; i++)
            {
                if (NodeList[i] != null)
                {
                    if (NodeList[i].transform == node)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
