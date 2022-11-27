using UnityEngine;

namespace ComputeShaderReferences
{
	public static class MatchAlembicSubmeshWithFbxSubmesh
	{
		public static int FbxVertexBuffer;
		public static int FbxIndexBuffer;
		public static int AlembicVertexBuffer;
		public static int AlembicIndexBuffer;
		public static int FbxIndexStart;
		public static int FbxIndexCount;
		public static int AlembicIndexStart;
		public static int AlembicIndexCount;
		public static int FbxBoundsCenter;
		public static int FbxBoundsExtents;
		public static int Scale;
		public static int MatchCount;
		public static int RWMatchCount;

		public static int SubmeshMatch { get; set; }

		public static void Setup(ComputeShader cs)
		{
			foreach (var info in typeof(MatchAlembicSubmeshWithFbxSubmesh).GetFields())
			{
				var index = Shader.PropertyToID(info.Name);
				info.SetValue(null, index);
			}
			foreach (var info in typeof(MatchAlembicSubmeshWithFbxSubmesh).GetProperties())
			{
				try
				{
					var index = cs.FindKernel(info.Name);
					info.SetValue(null, index);
				}
				catch
				{
					continue;
				}
			}
		}
	}
}