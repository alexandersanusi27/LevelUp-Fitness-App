using Plugin.Maui.Audio;

namespace LevelUp;

// handles playing sound effects throughout the app
// using Plugin.Maui.Audio for this because MAUI doesnt have built in audio that works well
public static class SoundService
{
    // the audio manager gets injected in MauiProgram and stored here
    private static IAudioManager? _audio;

    // called once at startup to give this class the audio manager it needs
    public static void Init(IAudioManager audio) => _audio = audio;

    // plays a sound file from the app bundle by name (e.g. "rank_up.wav")
    // wrapped in try/catch so the app doesnt crash if a file is missing
    public static async Task PlayAsync(string fileName)
    {
        if (_audio == null) return;
        try
        {
            // grab the file from the app package and stream it straight to the player
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            var player = _audio.CreateAsyncPlayer(stream);
            await player.PlayAsync(CancellationToken.None);
        }
        catch { /* ignore if file is missing */ }
    }
}
