using System;
using System.Collections.Generic;
using System.Xml;

namespace NodeFacilitator
{
    static partial class MultiNodeHelper
    {

        //########################################################
        //########################################################

        public static void OnCreate(NodeBlockBase node,
            List<Tuple<Vector3i, string, byte>> Nodes,
            ref List<PipeConnection> connections)
        {
            foreach (Tuple<Vector3i, string, byte> kv in Nodes)
            {
                BlockValue BlkNode = Block.GetBlockValue(kv.Item2);
                if (BlkNode.isair) throw new Exception(kv.Item2 + " not found");
                BlkNode.rotation = FullRotation.Rotate(node.Rotation, kv.Item3);
                var pos = node.WorldPos + FullRotation.Rotate(node.Rotation, kv.Item1);
                connections.Add(new PipeConnection(pos, BlkNode));
            }
        }

        public static void OnCreate(NodeBlockBase node,
            IMultiNodeBlock block, ref List<PipeConnection> connections)
        {
            foreach (Tuple<Vector3i, string, byte> kv in block?.Nodes)
            {
                BlockValue BlkNode = Block.GetBlockValue(kv.Item2);
                if (BlkNode.isair) throw new Exception(kv.Item2 + " not found");
                BlkNode.rotation = FullRotation.Rotate(node.Rotation, kv.Item3);
                var pos = node.WorldPos + FullRotation.Rotate(node.Rotation, kv.Item1);
                connections.Add(new PipeConnection(pos, BlkNode));
            }
        }

        //########################################################
        //########################################################

        public static void OnAfterLoad(PipeTank node,
            IMultiNodeBlock block, List<PipeConnection> connections)
        {
            if (node.Manager == null) return;
            foreach (var kv in block?.Nodes)
            {
                var pos = node.WorldPos + FullRotation.Rotate(node.Rotation, kv.Item1);
                node.Manager.TryGetNode(pos, out PipeConnection connection);
                if (connection != null) connections.Add(connection);
                else Log.Warning("No Connector at {0}", kv.Item1);
            }
        }

        //########################################################
        //########################################################

        public static void OnManagerAttached(PipeTank node, IMultiNodeBlock block,
            List<PipeConnection> connections, NodeManager manager)
        {
            if (manager == null)
            {
                foreach (var kv in block?.Nodes)
                    node.Manager?.RemoveManagedNode(node.WorldPos +
                        FullRotation.Rotate(node.Rotation, kv.Item1));
            }
            else
            {
                for (int i = 0; i < connections.Count; i++)
                    connections[i]?.AttachToManager(manager);
            }
        }

        //########################################################
        //########################################################


        public static void ParseConfig(XmlElement xml,
            List<Tuple<Vector3i, string, byte>> Nodes,
            string name = "connection-nodes")
        {
            if (Nodes.Count != 0) Log.Error("Nodes not empty");
            foreach (XmlNode child in xml.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                if (!child.Name.Equals(name)) continue;
                foreach (XmlNode node in child.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element) continue;
                    if (!node.Name.Equals("connection")) continue;
                    XmlAttribute attr_name = node.Attributes["name"];
                    XmlAttribute attr_pos = node.Attributes["position"];
                    XmlAttribute attr_rot = node.Attributes["rotation"];
                    if (attr_name == null) throw new System.Exception(
                        "Missing `name` attribute for connection node");
                    if (attr_pos == null) throw new System.Exception(
                        "Missing `position` attribute for connection node");
                    Nodes.Add(new Tuple<Vector3i, string, byte>(
                        Vector3i.Parse(attr_pos.Value), attr_name.Value,
                        attr_rot != null ? byte.Parse(attr_rot.Value) : (byte)0));
                }
            }
        }

        public static void ParseConfig(
            IMultiNodeBlock block, XmlElement xml,
            string name = "connection-nodes")
        {
            ParseConfig(xml, block.Nodes);
        }

        //########################################################
        //########################################################

    }
}
