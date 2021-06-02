using SmartTwin.Utils.Models.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SmartTwin.Utils.RoadMeshGenerator
{
    public class RoadMeshGeneratorHelperModel
    {
        public MeshInfo meshInfo;

        public float width;

        public float widthCoeff;

        public float radius;

        public int iterationPointCount;

        public float precision;

        public RoadMeshGeneratorHelperModel()
        {
            precision = 0.0000001f;

            meshInfo = new MeshInfo().Init();

            widthCoeff = 1f;
        }

        public void SetWidth(float _width)
        {
            width = _width;
        }

        public void SetRadius(float _radius)
        {
            radius = _radius;
        }
    }
}