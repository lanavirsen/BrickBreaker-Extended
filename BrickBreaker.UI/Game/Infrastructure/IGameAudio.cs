
namespace BrickBreaker.UI.Game.Infrastructure
{
    public interface IGameAudio // Interface for game audio management
    {
        void StartMusic(); // Starts playing music
        void StopMusic(); // Stops all music

        void Next(); // Jumps to the next track in the playlist
        void Pause(); // Pauses or resumes the current track

    }
}
