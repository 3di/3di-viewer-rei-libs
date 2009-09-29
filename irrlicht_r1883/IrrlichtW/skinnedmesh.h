#include "main.h"

extern "C"
{
    EXPORT E_BONE_ANIMATION_MODE BoneSceneNode_GetAnimationMode(IntPtr bone);
    EXPORT u32 BoneSceneNode_GetBoneIndex(IntPtr bone);
    EXPORT M_STRING BoneSceneNode_GetBoneName(IntPtr bone);
    EXPORT void BoneSceneNode_SetAnimationMode(IntPtr bone, E_BONE_ANIMATION_MODE mode);
    EXPORT u32 BoneSceneNode_GetSkinningSpace(IntPtr bone);
    EXPORT void BoneSceneNode_SetSkinningSpace (IntPtr bone, E_BONE_SKINNING_SPACE space);
    EXPORT s32 BoneSceneNode_GetScaleHint (IntPtr bone);
    EXPORT s32 BoneSceneNode_GetRotationHint (IntPtr bone);
    EXPORT s32 BoneSceneNode_GetPositionHint (IntPtr bone);
    EXPORT void BoneSceneNode_SetScaleHint (IntPtr bone, s32 hint);
    EXPORT void BoneSceneNode_SetRotationHint (IntPtr bone, s32 hint);
    EXPORT void BoneSceneNode_SetPositionHint (IntPtr bone, s32 hint);
    EXPORT void BoneSceneNode_UAPOAC(IntPtr bone);


    EXPORT void SkinnedMesh_AnimateMesh(IntPtr mesh, f32 frame, f32 blend);
    EXPORT void SkinnedMesh_ConvertMeshToTangents(IntPtr mesh);
	EXPORT void SkinnedMesh_SkinMesh(IntPtr mesh);
}
