using Microsoft.Maui.Handlers;

namespace Ahorita.App.Theming;

/// <summary>Ajustes visuales de controles nativos que XAML no expone.</summary>
internal static class PlatformTweaks
{
    public static void Apply()
    {
#if ANDROID
        // Sin subrayado nativo: los campos ya tienen su propio borde redondeado (InputField).
        var transparent = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        EntryHandler.Mapper.AppendToMapping("Ahorita.NoUnderline", (handler, _) => handler.PlatformView.BackgroundTintList = transparent);
        TimePickerHandler.Mapper.AppendToMapping("Ahorita.NoUnderline", (handler, _) => handler.PlatformView.BackgroundTintList = transparent);
#endif
    }
}
