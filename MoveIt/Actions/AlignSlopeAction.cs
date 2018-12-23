﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoveIt
{
    class AlignSlopeAction : Action
    {
        protected static Building[] buildingBuffer = BuildingManager.instance.m_buildings.m_buffer;
        protected static PropInstance[] propBuffer = PropManager.instance.m_props.m_buffer;
        protected static TreeInstance[] treeBuffer = TreeManager.instance.m_trees.m_buffer;
        protected static NetSegment[] segmentBuffer = NetManager.instance.m_segments.m_buffer;
        protected static NetNode[] nodeBuffer = NetManager.instance.m_nodes.m_buffer;

        public HashSet<InstanceState> m_states = new HashSet<InstanceState>();

        private Instance[] keyInstance = new Instance[2];

        public Instance PointA
        {
            get
            {
                return keyInstance[0];
            }
            set
            {
                keyInstance[0] = value;
            }
        }
        public Instance PointB
        {
            get
            {
                return keyInstance[1];
            }
            set
            {
                keyInstance[1] = value;
            }
        }

        public bool followTerrain;

        public AlignSlopeAction()
        {
            foreach (Instance instance in selection)
            {
                if (instance.isValid)
                {
                    m_states.Add(instance.GetState());
                }
            }
        }


        public override void Do()
        {
            float angleDelta;
            float heightDelta;
            float distance;
            Matrix4x4 matrix = default(Matrix4x4);

            angleDelta = 0 - (float)Math.Atan2(PointB.position.z - PointA.position.z, PointB.position.x - PointA.position.x);
            heightDelta = PointB.position.y - PointA.position.y;
            distance = (float)Math.Sqrt(Math.Pow(PointB.position.z - PointA.position.z, 2) + Math.Pow(PointB.position.x - PointA.position.x, 2));

            string msg = $"\nA:{PointA.position}, B:{PointB.position}\nAng:{angleDelta} ({angleDelta * Mathf.Rad2Deg}) - H:{heightDelta} - D:{distance}";
            foreach (InstanceState state in m_states)
            {
                string name = state.instance.info.name.Length > 15 ? state.instance.info.name.Substring(0, 15) : state.instance.info.name.PadLeft(15);
                msg += $"\n{name} <{state.instance.GetType().ToString().Substring(15)}> {state.position}: ";

                float distanceOffset, heightOffset;

                matrix.SetTRS(PointA.position, Quaternion.AngleAxis(angleDelta * Mathf.Rad2Deg, Vector3.down), Vector3.one);
                distanceOffset = (matrix.MultiplyPoint(state.position - PointA.position) - PointA.position).x;
                heightOffset = distanceOffset / distance * heightDelta;
                state.instance.SetHeight(Mathf.Clamp(PointA.position.y + heightOffset, 0f, 4000f));

                msg += $"offset:{distanceOffset} hOffset:{heightOffset}";
            }
            Debug.Log(msg);
        }


        public override void Undo()
        {
            foreach (InstanceState state in m_states)
            {
                state.instance.SetState(state);
            }

            UpdateArea(GetTotalBounds(false));
        }


        public override void ReplaceInstances(Dictionary<Instance, Instance> toReplace)
        {
            foreach (InstanceState state in m_states)
            {
                if (toReplace.ContainsKey(state.instance))
                {
                    DebugUtils.Log("AlignSlopeAction Replacing: " + state.instance.id.RawData + " -> " + toReplace[state.instance].id.RawData);
                    state.ReplaceInstance(toReplace[state.instance]);
                }
            }
        }
    }
}
