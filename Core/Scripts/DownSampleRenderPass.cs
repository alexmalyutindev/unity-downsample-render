using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DownSampleRenderFeature
{
	public class DownSampleRenderPass : ScriptableRenderPass
	{
		private readonly LayerMask _layerMask;
		private ScriptableRenderer _renderer;
		private readonly ProfilingSampler _profilingSampler;

		private RenderTexture _renderTexture;

		private List<ShaderTagId> _shaderTagIdList;
		private FilteringSettings _filteringSettings;
		private RenderStateBlock _renderStateBlock;
		private Material _blitMaterial;
		private DownSampleRenderFeature.Settings _settings;

		public DownSampleRenderPass(LayerMask layerMask, Material blitMaterial)
		{
			_layerMask = layerMask;
			profilingSampler = new ProfilingSampler(nameof(DownSampleRenderPass));
			_profilingSampler = new ProfilingSampler(nameof(DownSampleRenderPass));

			_blitMaterial = blitMaterial;

			_shaderTagIdList = new List<ShaderTagId>
			{
				new ShaderTagId("SRPDefaultUnlit"),
				new ShaderTagId("UniversalForward"),
				new ShaderTagId("UniversalForwardOnly"),
				new ShaderTagId("LightweightForward")
			};

			_filteringSettings = new FilteringSettings(RenderQueueRange.all, _layerMask.value);

			_renderStateBlock = new RenderStateBlock
			{
				blendState = BlendState.defaultValue,
				rasterState = RasterState.defaultValue,
				depthState = DepthState.defaultValue,
				stencilState = StencilState.defaultValue,
				stencilReference = 0,
				mask = RenderStateMask.Everything
			};
		}

		public void Setup(ScriptableRenderer renderer, DownSampleRenderFeature.Settings settings)
		{
			_renderer = renderer;
			_settings = settings;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			RenderTextureDescriptor desc = cameraTextureDescriptor;
			desc.width = (int)(desc.width / _settings.DownSample);
			desc.height = (int)(desc.width / _settings.DownSample);
			desc.colorFormat = RenderTextureFormat.ARGB2101010;

			_renderTexture = RenderTexture.GetTemporary(desc);
			_renderTexture.name = "DownSampleTexture";

			ConfigureTarget(_renderTexture.colorBuffer, _renderTexture.depthBuffer);
			ConfigureClear(ClearFlag.All, Color.clear);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var drawingSettings =
				CreateDrawingSettings(_shaderTagIdList, ref renderingData, SortingCriteria.CommonTransparent);

			var cmd = CommandBufferPool.Get();
			using (new ProfilingScope(cmd, _profilingSampler))
			{
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings,
					ref _renderStateBlock);

				_blitMaterial.SetTexture("_DownSampleTex", _renderTexture);
				cmd.Blit(_renderTexture, _renderer.cameraColorTarget, _blitMaterial);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void OnCameraCleanup(CommandBuffer cmd)
		{
			RenderTexture.ReleaseTemporary(_renderTexture);
		}
	}
}