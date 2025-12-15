using System.Collections.Generic;
using UnityEngine;

namespace Connect.Generator
{
    public class NodeRenderer : MonoBehaviour
    {
        [SerializeField] private List<Color> NodeColors;

        [SerializeField] private GameObject _point;
        


        public void Init()
        {
            _point.SetActive(false);
           
        }

        public void SetEdge(int colorId, Point direction)
        {
            GameObject connectedNode = _point;
            connectedNode.SetActive(true);
            connectedNode.GetComponent<SpriteRenderer>().color = NodeColors[colorId % NodeColors.Count];
        }
    } 
}
