using System;
using System.Media;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace BlueBerryDictionary.ApiClient.Client
{
    /// <summary>
    /// Service xử lý phát âm thanh phát âm từ vựng
    /// </summary>
    public class Audio
    {
        private readonly HttpClient _httpClient;
        private SoundPlayer _soundPlayer;

        public Audio()
        {
            _httpClient = new HttpClient();
            _soundPlayer = new SoundPlayer();
        }

        /// <summary>
        /// Phát âm thanh từ URL
        /// </summary>
        /// <param name="audioUrl">URL file âm thanh (.mp3, .wav)</param>
        public async Task PlayAudioAsync(string audioUrl)
        {
            if (string.IsNullOrEmpty(audioUrl))
            {
                MessageBox.Show("No audio file available", "Notification",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Tải file âm thanh về
                byte[] audioData = await _httpClient.GetByteArrayAsync(audioUrl);

                // Lưu tạm vào bộ nhớ và phát
                string tempPath = System.IO.Path.GetTempFileName();
                await System.IO.File.WriteAllBytesAsync(tempPath, audioData);

                _soundPlayer.SoundLocation = tempPath;
                _soundPlayer.Load();
                _soundPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing audio: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Giải phóng tài nguyên
        /// </summary>
        public void Dispose()
        {
            _soundPlayer?.Dispose();
        }
    }
}