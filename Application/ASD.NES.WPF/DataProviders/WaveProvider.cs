using NAudio.Wave;

namespace ASD.NES.WPF.DataProviders {

    using Kernel;

    // NAudio
    internal sealed class WaveProvider : IWaveProvider {

        private WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 1);
        private IAudioBuffer audioBuffer;

        public WaveFormat WaveFormat => waveFormat;

        public WaveProvider(IAudioBuffer buffer) {
            audioBuffer = buffer;
        }

        public int Read(byte[] buffer, int offset, int count) {
            var waveBuffer = new WaveBuffer(buffer);
            var samplesRequired = count / 4;
            var samplesRead = audioBuffer.CopyToArray(waveBuffer.FloatBuffer, offset / 4, samplesRequired);
            return samplesRead * 4;
        }        
    }
}