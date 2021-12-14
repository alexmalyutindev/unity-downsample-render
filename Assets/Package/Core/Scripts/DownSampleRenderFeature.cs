using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DownSampleRenderFeature
{
    public class DownSampleRenderFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private LayerMask _layerMask;

        [SerializeField]
        private Settings _settings;

        private Material _blitMaterial;
        private DownSampleRenderPass _pass;

        /// <inheritdoc/>
        public override void Create()
        {
            name = "Down Sample Pass";
            _blitMaterial = new Material(Shader.Find("Hidden/DownSample/ColorDepthCopy"));
            _pass = new DownSampleRenderPass(_layerMask, _blitMaterial)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _pass.Setup(renderer, _settings);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
#if UNITY_EDITOR
            DestroyImmediate(_blitMaterial);
#else
            Destroy(_blitMaterial);
#endif
        }

        [Serializable]
        public class Settings
        {
            [Range(1.0f, 8.0f)]
            public float DownSample = 2.0f;
        }
    }
}