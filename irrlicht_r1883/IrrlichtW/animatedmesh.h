#include "main.h"

extern "C"
{
    typedef void (STDCALL ANIMATIONENDCALLBACK)(IntPtr);

    //Shadow Volume Scene Node
    EXPORT void ShadowVolume_SetMeshToRenderFrom(IntPtr shadow, IntPtr mesh);
    //Animated Mesh Scene Node
    EXPORT IntPtr AnimatedMeshSceneNode_AddShadowVolumeSceneNode(IntPtr node, IntPtr mesh, int ID, bool zfail, float infinity);
    EXPORT f32 AnimatedMeshSceneNode_GetFrameNr(IntPtr node);
    EXPORT IntPtr AnimatedMeshSceneNode_GetMS3DJointNode(IntPtr node, M_STRING jointName);
    EXPORT IntPtr AnimatedMeshSceneNode_GetXJointNode(IntPtr node, M_STRING jointName);
    EXPORT void AnimatedMeshSceneNode_SetAnimationEndCallback(IntPtr node, ANIMATIONENDCALLBACK callback);
    EXPORT void AnimatedMeshSceneNode_SetAnimationSpeed(IntPtr node, int framePS);
    EXPORT void AnimatedMeshSceneNode_SetCurrentFrame(IntPtr node, int cf);
    EXPORT void AnimatedMeshSceneNode_SetFrameLoop(IntPtr node, int start, int end);
    EXPORT void AnimatedMeshSceneNode_SetLoopMode(IntPtr node, bool animationLooped);
    EXPORT void AnimatedMeshSceneNode_SetMD2Animation(IntPtr node, M_STRING animationname);
    EXPORT void AnimatedMeshSceneNode_SetMD2AnimationA(IntPtr node, EMD2_ANIMATION_TYPE anim);
    EXPORT IntPtr AnimatedMeshSceneNode_GetMesh(IntPtr node);
    EXPORT IntPtr AnimatedMeshSceneNode_GetJointNode(IntPtr node, u32 jointn);
    EXPORT IntPtr AnimatedMeshSceneNode_GetJointNodeA(IntPtr node, M_STRING name);
    EXPORT void AnimatedMeshSceneNode_SetJointMode(IntPtr node, E_JOINT_UPDATE_ON_RENDER mode);
    EXPORT void AnimatedMeshSceneNode_AnimateJoints (IntPtr node, bool calc);
}
