using System.Collections.Generic;
using System.Linq;
using System;
namespace Core.Sorting
{
    public static class SortWithUnknowns
    {
        public static SortWithUnknownsNode<TPayload>[] Sort<TPayload>(SortWithUnknownsNode<TPayload>[] sortWithUnknownNodes)
        {
            CheckAllRelationshipsAreBidirectional(sortWithUnknownNodes);
            DoDepthFirstSearchToFindCircularReferences(sortWithUnknownNodes);
            return FindNodesWithNoOtherUninsertedNodesAfterThemAndInsertThemRecursively(sortWithUnknownNodes.ToList());
        }
        private static void CheckAllRelationshipsAreBidirectional<TPayload>(
            SortWithUnknownsNode<TPayload>[] nodes) {
            foreach (SortWithUnknownsNode<TPayload> node in nodes) {
                foreach (SortWithUnknownsNode<TPayload> toNode in node.ToNodes)
                {
                    if (!toNode.FromNodes.Contains(node)) throw new ArgumentException($"The node \"{node.GetName()}\" contained \"{toNode.GetName()}\" in its {nameof(node.ToNodes)} but \"{toNode.GetName()}\" did not contain \"{node.GetName()}\" in its {nameof(toNode.FromNodes)}");
                }
                foreach (SortWithUnknownsNode<TPayload> fromNode in node.FromNodes)
                {
                    if (!fromNode.ToNodes.Contains(node)) throw new ArgumentException($"The node \"{node.GetName()}\" contained \"{fromNode.GetName()}\" in its {nameof(node.FromNodes)} but \"{fromNode.GetName()}\" did not contain \"{node.GetName()}\" in its {nameof(fromNode.ToNodes)}");
                }
            }
        }
        private static SortWithUnknownsNode<TPayload>[] FindNodesWithNoOtherUninsertedNodesAfterThemAndInsertThemRecursively<TPayload>(
            List<SortWithUnknownsNode<TPayload>> nodes) {
            List<SortWithUnknownsNode<TPayload>> sortedNodes = new List<SortWithUnknownsNode<TPayload>>();
            int initialNodesLength;
            while ((initialNodesLength=nodes.Count )> 0)
            {
                foreach (SortWithUnknownsNode<TPayload> node in nodes.ToArray())
                {
                    foreach (SortWithUnknownsNode<TPayload> nodeThatCameAfter in node.ToNodes) {
                        if (nodes.Contains(nodeThatCameAfter)) continue;    
                    }
                    sortedNodes.Add(node);
                    nodes.Remove(node);
                    break;
                }
                if (nodes.Count >= initialNodesLength) throw new Exception("This should never happen. Something went wrong");
            }
            return sortedNodes.ToArray();
        }
        private static void DoDepthFirstSearchToFindCircularReferences<TPayload>( 
            SortWithUnknownsNode<TPayload>[] nodes)
        {
            foreach (SortWithUnknownsNode<TPayload> node in nodes)
            {
                DoDepthFirstSearchToFindCircularReferencesRecursive(new SortWithUnknownsNode<TPayload>[] { node }, node);
            }
        }
        private static void DoDepthFirstSearchToFindCircularReferencesRecursive<TPayload>(
            SortWithUnknownsNode<TPayload>[] seenNodes, SortWithUnknownsNode<TPayload> node){
            foreach (SortWithUnknownsNode<TPayload> nodeThatCameAfter in node.ToNodes) {
                if (seenNodes.Contains(nodeThatCameAfter))
                    throw new SortWithUnknownsCircularException<TPayload>(ExtractCircularReferencePathRFromSeen(seenNodes, nodeThatCameAfter));
                DoDepthFirstSearchToFindCircularReferencesRecursive(seenNodes.Concat(new SortWithUnknownsNode<TPayload>[] { nodeThatCameAfter }).ToArray(), nodeThatCameAfter);
            }
        }
        private static SortWithUnknownsNode<TPayload>[] ExtractCircularReferencePathRFromSeen<TPayload>(SortWithUnknownsNode<TPayload>[] seenNodes, SortWithUnknownsNode<TPayload> nodeThatWasAlreadySeen) {
            List<SortWithUnknownsNode<TPayload>> path = new List<SortWithUnknownsNode<TPayload>>();
            foreach(SortWithUnknownsNode<TPayload> seenNode in seenNodes.Reverse())
            {
                if (seenNode == nodeThatWasAlreadySeen)
                {
                    path.Insert(0,nodeThatWasAlreadySeen);
                    return path.ToArray();
                }
                path.Insert(0, seenNode);
            }
            throw new Exception("Something went wrong. This should never happen");
        }

        /*
        private static void MoveAnyNodesThatAreAfterInListButBelongBeforeBackwardsRecursively<TPayload>(List<SortWithUnknownsNode<TPayload>> nodesThatHaveBeenInserted,
            SortWithUnknownsNode<TPayload> insertedNode)
        {
            MoveAnyNodesThatAreAfterInListButBelongBeforeBackwardsRecursively(nodesThatHaveBeenInserted, insertedNode, new List<SortWithUnknownsNode<TPayload>> { insertedNode});
        }
        private static void MoveAnyNodesThatAreAfterInListButBelongBeforeBackwardsRecursively<TPayload>(List<SortWithUnknownsNode<TPayload>> nodesThatHaveBeenInserted,
            SortWithUnknownsNode<TPayload> insertedNode, List<SortWithUnknownsNode<TPayload>> nodesThatHaveMovedAlready)
        {
            int indexOfInsertedNode = nodesThatHaveBeenInserted.IndexOf(insertedNode);
            SortWithUnknownsNode<TPayload>[] nodesThatComeAfterInsertedNode =
                nodesThatHaveBeenInserted.Skip(indexOfInsertedNode + 1).ToArray();
            foreach (SortWithUnknownsNode<TPayload> nodeThatCameAfterInsertedNode in nodesThatComeAfterInsertedNode)
            {
                bool nodeIsStillAfterInsertedNode = nodesThatHaveBeenInserted.IndexOf(insertedNode) < nodesThatHaveBeenInserted.IndexOf(nodeThatCameAfterInsertedNode);
                if (!nodeIsStillAfterInsertedNode) continue;
                if (!nodeThatCameAfterInsertedNode.ToNodes.Contains(insertedNode)) continue;
                if (nodesThatHaveMovedAlready.Contains(nodeThatCameAfterInsertedNode))
                    throw new SortWithUnknownsCircularException<TPayload>(nodesThatHaveMovedAlready.ToArray());
                nodesThatHaveBeenInserted.Remove(nodeThatCameAfterInsertedNode);
                nodesThatHaveBeenInserted.Insert(indexOfInsertedNode, nodeThatCameAfterInsertedNode);
                nodesThatHaveMovedAlready.Add(nodeThatCameAfterInsertedNode);      
                MoveAnyNodesThatAreAfterInListButBelongBeforeBackwardsRecursively(nodesThatHaveBeenInserted, insertedNode, nodesThatHaveMovedAlready);
            }
        }
        private static void FindMinimumIndexCanInsertNodeAndInsert<TPayload>(List<SortWithUnknownsNode<TPayload>> nodesThatHaveBeenInserted,
            SortWithUnknownsNode<TPayload> nodeToInsert)
        {
            List<SortWithUnknownsNode<TPayload>> nodesLeftToOvertake = nodeToInsert.FromNodes.ToList();
            for (int i = 0; i < nodesThatHaveBeenInserted.Count; i++)
            {
                if (nodesLeftToOvertake.Count < 1)
                {
                    nodesThatHaveBeenInserted.Insert(i, nodeToInsert);
                    return;
                }
                SortWithUnknownsNode<TPayload> nodeAtIndex = nodesThatHaveBeenInserted[i];
                if (nodesLeftToOvertake.Contains(nodeAtIndex))
                    nodesLeftToOvertake.Remove(nodeAtIndex);
            }
            nodesThatHaveBeenInserted.Add(nodeToInsert);
        }*/
    }
}