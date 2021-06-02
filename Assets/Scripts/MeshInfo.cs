using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace SmartTwin.Utils.Models.Geometry
{
	public class MeshInfo
	{
		private static int count = 0;

		/// <summary>
		/// Вершины меша
		/// </summary>
		public List<Vector3> vertices;

		/// <summary>
		/// Индексы вершин
		/// </summary>
		public List<int> indexes;

		/// <summary>
		/// Текстурные координаты вершин
		/// </summary>
		public List<Vector2> uv;

		/// <summary>
		/// Нормали вершин
		/// </summary>
		public List<Vector3> normals;

		/// <summary>
		/// Идентификатор меша
		/// </summary>
		public readonly int id = Interlocked.Increment(ref count);



		/// <summary>
		/// Инициализация коллекций
		/// </summary>
		public MeshInfo Init()
		{
			vertices = new List<Vector3>();
			indexes = new List<int>();
			uv = new List<Vector2>();
			normals = new List<Vector3>();

			return this;
		}

		/// <summary>
		/// Создание меша Unity
		/// </summary>
		/// <param name="yOffset">Смещение по высоте</param>
		public Mesh CreateMesh()
			=> new Mesh
			{
				vertices = vertices.ToArray(),
				triangles = indexes.ToArray(),
				uv = uv.ToArray(),
				normals = normals.ToArray(),
			};

		public override int GetHashCode() => id;
	}
}
