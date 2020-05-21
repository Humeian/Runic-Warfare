using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(ScreenShakeVRRenderer), PostProcessEvent.AfterStack, "Custom/ScreenShakeVR")]
public sealed class ScreenShakeVRs : PostProcessEffectSettings
{
}

public sealed class ScreenShakeVRRenderer : PostProcessEffectRenderer<ScreenShakeVRs>
{
    public override void Render(PostProcessRenderContext context)
    {
        if (ScreenShakeVREffect.Factor != 0)
        {
            var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/ScreenShakeVR"));
            sheet.properties.SetFloat("_ShakeFac", ScreenShakeVREffect.Factor);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}