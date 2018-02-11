using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StretchyParts
{
    class NodeUtil
    {
        public static void updateAttachNodePosition(Part part, AttachNode node, Vector3 newPos, Vector3 orientation, bool updatePartPosition)
        {
            Vector3 diff = newPos - node.position;
            node.position = node.originalPosition = newPos;
            node.orientation = node.originalOrientation = orientation;
            if (updatePartPosition && node.attachedPart != null)
            {
                Vector3 globalDiff = part.transform.TransformPoint(diff);
                globalDiff -= part.transform.position;
                if (node.attachedPart.parent == part)//is a child of this part, move it the entire offset distance
                {
                    node.attachedPart.attPos0 += diff;
                    node.attachedPart.transform.position += globalDiff;
                }
                else//is a parent of this part, do not move it, instead move this part the full amount
                {
                    part.attPos0 -= diff;
                    part.transform.position -= globalDiff;
                    //and then, if this is not the root part, offset the root part in the negative of the difference to maintain relative part position
                    Part p = part.localRoot;
                    if (p != null && p != part)
                    {
                        p.transform.position += globalDiff;
                    }
                }
            }
        }
    }
}
