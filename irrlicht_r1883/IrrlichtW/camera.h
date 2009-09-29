#include "main.h"

extern "C"
{
    EXPORT float Camera_GetAspectRation(IntPtr camera);
    EXPORT float Camera_GetFarValue(IntPtr camera);
    EXPORT float Camera_GetFOV(IntPtr camera);
    EXPORT float Camera_GetNearValue(IntPtr camera);
    EXPORT void Camera_GetProjectionMatrix(IntPtr camera, M_MAT4 toR);
    EXPORT void Camera_GetTarget(IntPtr camera, M_VECT3DF toR);
    EXPORT void Camera_GetUpVector(IntPtr camera, M_VECT3DF toR);
    EXPORT void Camera_GetViewMatrix(IntPtr camera, M_MAT4 toR);
    EXPORT IntPtr Camera_GetViewFrustrum(IntPtr camera);
    EXPORT bool Camera_IsInputReceiverEnabled(IntPtr camera);
    EXPORT bool Camera_IsOrthogonal(IntPtr camera);
    EXPORT bool Camera_OnEvent(IntPtr camera, IntPtr event);
    EXPORT void Camera_SetAspectRatio(IntPtr camera, float aspect);
    EXPORT void Camera_SetFarValue(IntPtr camera, float far);
    EXPORT void Camera_SetFOV(IntPtr camera, float FOV);
    EXPORT void Camera_SetInputReceiverEnabled(IntPtr camera, bool enabled);
    // Removed in Irrlicht moved to parameter of projection matrix
	// EXPORT void Camera_SetIsOrthogonal(IntPtr camera, bool orthogonal);
    EXPORT void Camera_SetNearValue(IntPtr camera, float near);
    
	// orthogonal moved here
	EXPORT void Camera_SetProjectionMatrix(IntPtr camera, M_MAT4 projection, bool orthogonal);
    EXPORT void Camera_SetTarget(IntPtr camera, M_VECT3DF target);
    EXPORT void Camera_SetUpVector(IntPtr camera, M_VECT3DF upvector);

    EXPORT void VF_GetBoundingBox(IntPtr vf, M_BOX3D box);
    EXPORT void VF_GetFarLeftUp(IntPtr vf, M_VECT3DF pf);
    EXPORT void VF_GetFarLeftDown(IntPtr vf, M_VECT3DF pf);
    EXPORT void VF_GetFarRightDown(IntPtr vf, M_VECT3DF pf);
    EXPORT void VF_GetFarRightUp(IntPtr vf, M_VECT3DF pf);
    EXPORT void VF_RecalculateBoundingBox(IntPtr v);
    EXPORT void VF_Transform(IntPtr vf, M_MAT4 mat);
    EXPORT void VF_GetPlane(IntPtr vf, int idx, M_PLANE3DF plane);

    EXPORT void CameraFPS_SetRotateSpeed(IntPtr camera, float speed);
    EXPORT void CameraFPS_SetMoveSpeed(IntPtr camera, float speed);
    EXPORT float CameraFPS_GetRotateSpeed(IntPtr camera);
    EXPORT float CameraFPS_GetMoveSpeed(IntPtr camera);

}
