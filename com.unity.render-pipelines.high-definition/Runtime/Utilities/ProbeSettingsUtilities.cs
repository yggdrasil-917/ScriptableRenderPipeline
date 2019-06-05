using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>Utilities for <see cref="ProbeSettings"/></summary>
    public static class ProbeSettingsUtilities
    {
        internal class VolumeBounds : IDisposable
        {
            List<Vector3> m_PointsWS;

            public List<Vector3> pointsWS => m_PointsWS;

            public VolumeBounds()
            {
                m_PointsWS = ListPool<Vector3>.Get();
            }

            public void MultiplyPoint(Matrix4x4 value)
            {
                for (var i = 0; i < m_PointsWS.Count; i++)
                    m_PointsWS[i] = value.MultiplyPoint(m_PointsWS[i]);
            }

            /// <summary>Constraint the bounds to be inside the clip space provided by the matrix.</summary>
            public void ClampWithMatrix(Matrix4x4 worldToClip)
            {
                var clipToWorld = worldToClip.inverse;

                for (var i = 0; i < m_PointsWS.Count; i++)
                {
                    var pointWS = m_PointsWS[i];
                    var pointWS4 = new Vector4(pointWS.x, pointWS.y, pointWS.z, 1.0f);
                    var pointCS = worldToClip * pointWS4;
                    pointCS.x = Mathf.Clamp(pointCS.x / pointCS.w, -1.0f, 1.0f) * pointCS.w;
                    pointCS.y = Mathf.Clamp(pointCS.y / pointCS.w, -1.0f, 1.0f) * pointCS.w;
                    pointCS.z = Mathf.Clamp(pointCS.z / pointCS.w, 0, 1.0f) * pointCS.w;
                    var clampedPointWS4 = clipToWorld * pointCS;
                    m_PointsWS[i] = new Vector3(clampedPointWS4.x, clampedPointWS4.y, clampedPointWS4.z);
                }
            }

            public void Dispose()
            {
                ListPool<Vector3>.Release(m_PointsWS);
                m_PointsWS = null;
            }
        }

        internal enum PositionMode
        {
            UseProbeTransform,
            MirrorReferenceTransformWithProbePlane
        }

        /// <summary>
        /// Apply <paramref name="settings"/> and <paramref name="probePosition"/> to
        /// <paramref name="cameraPosition"/> and <paramref name="cameraSettings"/>.
        /// </summary>
        /// <param name="settings">Settings to apply. (Read only)</param>
        /// <param name="probePosition">Position to apply. (Read only)</param>
        /// <param name="cameraSettings">Settings to update.</param>
        /// <param name="cameraPosition">Position to update.</param>
        public static void ApplySettings(
            ref ProbeSettings settings,                             // In Parameter
            ref ProbeCapturePositionSettings probePosition,         // In parameter
            ref CameraSettings cameraSettings,                      // InOut parameter
            ref CameraPositionSettings cameraPosition,              // InOut parameter
            float referenceFieldOfView = 90,
            Matrix4x4 referenceWorldToClip = default
        )
        {
            cameraSettings = settings.camera;
            // Compute the modes for each probe type
            PositionMode positionMode;
            bool useReferenceTransformAsNearClipPlane;
            VolumeBounds visibleInfluenceVolumeBounds = null;
            switch (settings.type)
            {
                case ProbeSettings.ProbeType.PlanarProbe:
                    positionMode = PositionMode.MirrorReferenceTransformWithProbePlane;
                    useReferenceTransformAsNearClipPlane = true;
                    ApplyPlanarFrustumHandling(
                        ref settings, ref probePosition,
                        ref cameraSettings, ref cameraPosition,
                        ref visibleInfluenceVolumeBounds,
                        referenceFieldOfView,
                        referenceWorldToClip
                    );
                    break;
                case ProbeSettings.ProbeType.ReflectionProbe:
                    positionMode = PositionMode.UseProbeTransform;
                    useReferenceTransformAsNearClipPlane = false;
                    cameraSettings.frustum.mode = CameraSettings.Frustum.Mode.ComputeProjectionMatrix;
                    cameraSettings.frustum.aspect = 1;
                    cameraSettings.frustum.fieldOfView = 90;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Update the position
            switch (positionMode)
            {
                case PositionMode.UseProbeTransform:
                    {
                        cameraPosition.mode = CameraPositionSettings.Mode.ComputeWorldToCameraMatrix;
                        var proxyMatrix = Matrix4x4.TRS(probePosition.proxyPosition, probePosition.proxyRotation, Vector3.one);
                        cameraPosition.position = proxyMatrix.MultiplyPoint(settings.proxySettings.capturePositionProxySpace);
                        cameraPosition.rotation = proxyMatrix.rotation * settings.proxySettings.captureRotationProxySpace;

                        // In case of probe baking, 99% of the time, orientation of the cubemap doesn't matters
                        //   so, we build one without any rotation, thus we don't have to change the basis
                        //   during sampling the cubemap.
                        if (settings.type == ProbeSettings.ProbeType.ReflectionProbe)
                            cameraPosition.rotation = Quaternion.identity;
                        break;
                    }
                case PositionMode.MirrorReferenceTransformWithProbePlane:
                    {
                        cameraPosition.mode = CameraPositionSettings.Mode.UseWorldToCameraMatrixField;
                        ComputeWorldToCameraMatrixForPlanar(
                            ref settings, ref probePosition,
                            ref cameraSettings, ref cameraPosition,
                            ref visibleInfluenceVolumeBounds,
                            referenceWorldToClip
                        );
                        break;
                    }
            }

            // Update the clip plane
            if (useReferenceTransformAsNearClipPlane)
            {
                ApplyObliqueNearClipPlane(
                    ref settings, ref probePosition,
                    ref cameraSettings, ref cameraPosition
                );
            }

            // Frame Settings Overrides
            switch (settings.mode)
            {
                default:
                case ProbeSettings.Mode.Realtime:
                    cameraSettings.defaultFrameSettings = FrameSettingsRenderType.RealtimeReflection;
                    break;
                case ProbeSettings.Mode.Baked:
                case ProbeSettings.Mode.Custom:
                    cameraSettings.defaultFrameSettings = FrameSettingsRenderType.CustomOrBakedReflection;
                    break;
            }

            switch (settings.type)
            {
                case ProbeSettings.ProbeType.ReflectionProbe:
                    cameraSettings.customRenderingSettings = true;
                    // Disable specular lighting for reflection probes, they must not have view dependent information when baking
                    cameraSettings.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SpecularLighting, false);
                    cameraSettings.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.SpecularLighting] = true;
                    break;
            }

            visibleInfluenceVolumeBounds?.Dispose();
        }

        /// <summary>
        /// Compute the world to camera matrix used to capture a planar reflection.
        ///
        /// When sampling into a planar reflection, we will sample texels that are in the direction
        /// of reflected lights by the meshes inside the influence volume of the probe.
        ///
        /// So we try to compute a direction for the capture camera that will capture as much as possible
        /// lights rays that can be reflected to the viewer's camera.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="probePosition"></param>
        /// <param name="cameraSettings"></param>
        /// <param name="cameraPosition"></param>
        internal static void ComputeWorldToCameraMatrixForPlanar(
            ref ProbeSettings settings,                             // In Parameter
            ref ProbeCapturePositionSettings probePosition,         // In parameter
            ref CameraSettings cameraSettings,                      // InOut parameter
            ref CameraPositionSettings cameraPosition,              // InOut parameter
            ref VolumeBounds visibleInfluenceVolumeBounds,          // InOut parameter
            Matrix4x4 referenceWorldToClip = default
        )
        {
            // The capture camera should capture all light rays incoming on the visible influence volume.
            // The visible influence volume is the intersection of the influence volume of the probe with the
            // viewer's camera frustum

            // To approximate the visible volume bounds, we do:
            //   1. Get several points on the surface of the influence volume (ex: 6 points on sphere, 8 vertices of a box)
            //   2. Compute the clip space coordinate with viewer's matrices
            //   3. Clamp the clip space coordinates in range ([-1..1]x[-1..1]x[0..1])
            //   4. Convert back to the world space coordinates
            // This way, we "project" the influence volume bounds vertices on the frustum of the viewer's camera.

            // Then we can compute the center of mass of those bounds.
            // This will be the direction we will be looking at when capturing the planar probe

            // Calculate mirror position and forward world space
            var proxyMatrix = Matrix4x4.TRS(probePosition.proxyPosition, probePosition.proxyRotation, Vector3.one);
            var mirrorPosition = proxyMatrix.MultiplyPoint(settings.proxySettings.mirrorPositionProxySpace);
            var mirrorForward = proxyMatrix.MultiplyVector(settings.proxySettings.mirrorRotationProxySpace * Vector3.forward);

            var debug = (UnityEditor.SceneView.sceneViews[0] as UnityEditor.SceneView) ==
                        UnityEditor.SceneView.currentDrawingSceneView;
            if (debug)
                UnityEditor.CameraEditorUtils.DrawFrustumGizmo(UnityEditor.SceneView.currentDrawingSceneView.camera);
            if (visibleInfluenceVolumeBounds == null)
            {
                visibleInfluenceVolumeBounds = new VolumeBounds();
                settings.influence.GetBoundsPoints(visibleInfluenceVolumeBounds.pointsWS);
                visibleInfluenceVolumeBounds.MultiplyPoint(probePosition.influenceToWorld);
                if (debug)
                {
                    for (var i = 0; i < visibleInfluenceVolumeBounds.pointsWS.Count; i++)
                        Debug.DrawLine(mirrorPosition, visibleInfluenceVolumeBounds.pointsWS[i], Color.red, 1);
                }

                visibleInfluenceVolumeBounds.ClampWithMatrix(referenceWorldToClip);
            }

            var visibleInfluenceVolumeCenterWS = Vector3.zero;
            if (visibleInfluenceVolumeBounds.pointsWS.Count > 0)
            {
                for (var i = 0; i < visibleInfluenceVolumeBounds.pointsWS.Count; i++)
                {
                    if (debug)
                        Debug.DrawLine(mirrorPosition, visibleInfluenceVolumeBounds.pointsWS[i], Color.blue, 1);
                    visibleInfluenceVolumeCenterWS += visibleInfluenceVolumeBounds.pointsWS[i];
                }

                visibleInfluenceVolumeCenterWS /= visibleInfluenceVolumeBounds.pointsWS.Count;
            }

            var worldToCameraRHS = GeometryUtils.CalculateWorldToCameraMatrixRHS(
                probePosition.referencePosition,
                // The capture always look at the center of the visible probe influence volume
                Quaternion.LookRotation(visibleInfluenceVolumeCenterWS - probePosition.referencePosition, Vector3.up)
            );
            var reflectionMatrix = GeometryUtils.CalculateReflectionMatrix(mirrorPosition, mirrorForward);
            cameraPosition.worldToCameraMatrix = worldToCameraRHS * reflectionMatrix;
            // We must invert the culling because we performed a plane reflection
            cameraSettings.invertFaceCulling = true;

            // Calculate capture position and rotation
            cameraPosition.position = reflectionMatrix.MultiplyPoint(probePosition.referencePosition);
            var forward = reflectionMatrix.MultiplyVector(probePosition.referenceRotation * Vector3.forward);
            var up = reflectionMatrix.MultiplyVector(probePosition.referenceRotation * Vector3.up);
            cameraPosition.rotation = Quaternion.LookRotation(forward, up);
        }

        internal static void ApplyPlanarFrustumHandling(
            ref ProbeSettings settings,                             // In Parameter
            ref ProbeCapturePositionSettings probePosition,         // In parameter
            ref CameraSettings cameraSettings,                      // InOut parameter
            ref CameraPositionSettings cameraPosition,              // InOut parameter
            ref VolumeBounds visibleInfluenceBounds,                // InOut parameter
            float referenceFieldOfView,
            Matrix4x4 referenceWorldToClip
        )
        {
            const float k_MaxFieldOfView = 170;

            var proxyMatrix = Matrix4x4.TRS(probePosition.proxyPosition, probePosition.proxyRotation, Vector3.one);
            var mirrorPosition = proxyMatrix.MultiplyPoint(settings.proxySettings.mirrorPositionProxySpace);

            switch (settings.frustum.fieldOfViewMode)
            {
                case ProbeSettings.Frustum.FOVMode.Fixed:
                    cameraSettings.frustum.fieldOfView = settings.frustum.fixedValue;
                    break;
                case ProbeSettings.Frustum.FOVMode.Viewer:
                    cameraSettings.frustum.fieldOfView = Mathf.Min(
                        referenceFieldOfView * settings.frustum.viewerScale,
                        k_MaxFieldOfView
                    );
                    break;
                case ProbeSettings.Frustum.FOVMode.Automatic:
                    if (visibleInfluenceBounds == null)
                    {
                        // See ComputeWorldToCameraMatrixForPlanar
                        // for an explanation of the visible influence bounds
                        visibleInfluenceBounds = new VolumeBounds();
                        settings.influence.GetBoundsPoints(visibleInfluenceBounds.pointsWS);
                        visibleInfluenceBounds.MultiplyPoint(probePosition.influenceToWorld);
                        visibleInfluenceBounds.ClampWithMatrix(referenceWorldToClip);
                    }

                    // Dynamic FOV tries to adapt the FOV to have maximum usage of the target render texture
                    //     (A lot of pixel can be discarded in the render texture). This way we can have a greater
                    //     resolution for the planar with the same cost.
                    cameraSettings.frustum.fieldOfView = Mathf.Min(
                            InfluenceVolume.ComputeFOVAt(
                            probePosition.referencePosition, mirrorPosition, visibleInfluenceBounds.pointsWS
                        ) * settings.frustum.automaticScale,
                            k_MaxFieldOfView
                    );
                    break;
            }
        }

        internal static void ApplyObliqueNearClipPlane(
            ref ProbeSettings settings,                             // In Parameter
            ref ProbeCapturePositionSettings probePosition,         // In parameter
            ref CameraSettings cameraSettings,                      // InOut parameter
            ref CameraPositionSettings cameraPosition               // InOut parameter
        )
        {
            var proxyMatrix = Matrix4x4.TRS(probePosition.proxyPosition, probePosition.proxyRotation, Vector3.one);
            var mirrorPosition = proxyMatrix.MultiplyPoint(settings.proxySettings.mirrorPositionProxySpace);
            var mirrorForward = proxyMatrix.MultiplyVector(settings.proxySettings.mirrorRotationProxySpace * Vector3.forward);

            var clipPlaneCameraSpace = GeometryUtils.CameraSpacePlane(
                cameraPosition.worldToCameraMatrix,
                mirrorPosition,
                mirrorForward
            );

            var sourceProjection = Matrix4x4.Perspective(
                cameraSettings.frustum.fieldOfView,
                cameraSettings.frustum.aspect,
                cameraSettings.frustum.nearClipPlane,
                cameraSettings.frustum.farClipPlane
            );
            var obliqueProjection = GeometryUtils.CalculateObliqueMatrix(
                sourceProjection, clipPlaneCameraSpace
            );
            cameraSettings.frustum.mode = CameraSettings.Frustum.Mode.UseProjectionMatrixField;
            cameraSettings.frustum.projectionMatrix = obliqueProjection;
        }
    }
}
