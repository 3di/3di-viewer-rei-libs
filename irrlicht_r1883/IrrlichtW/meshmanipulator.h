#include "main.h"

extern "C"
{
	EXPORT IntPtr MeshManipulator_CreateMeshWithTangents(IntPtr mm, IntPtr mesh);
	EXPORT IntPtr MeshManipulator_CreateMeshUniquePrimitives(IntPtr mm, IntPtr mesh);
	EXPORT void MeshManipulator_MakePlanarTextureMapping(IntPtr mm, IntPtr mesh, float resolution);

	EXPORT void MeshManipulator_FlipSurfaces(IntPtr mm, IntPtr mesh);
	EXPORT void MeshManipulator_RecalculateNormals(IntPtr mm, IntPtr mesh, bool smooth);
	EXPORT void MeshManipulator_ScaleMesh(IntPtr mm, IntPtr mesh, M_VECT3DF scale);

	EXPORT void MeshManipulator_SetVertexColorAlpha(IntPtr mm, IntPtr mesh, int alpha);
	EXPORT void MeshManipulator_SetVertexColors(IntPtr mm, IntPtr mesh, M_SCOLOR alpha);

	EXPORT int MeshManipulator_GetPolyCount(IntPtr mm, IntPtr mesh);
	EXPORT int MeshManipulator_GetPolyCountA(IntPtr mm, IntPtr amesh);

	EXPORT IntPtr MeshManipulator_CreateMeshWith2TCoords (IntPtr mm, IntPtr mesh);
	EXPORT void MeshManipulator_TransformMesh (IntPtr mm, IntPtr mesh, M_MAT4 mat);
	EXPORT IntPtr MeshManipulator_CreateMeshCopy(IntPtr mm, IntPtr mesh);

}
