namespace ASD.NES.Core {

    public interface IAudioBuffer {
        int CopyToArray(float[] buffer, int offset, int samplesCount);
    }
}