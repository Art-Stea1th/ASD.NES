namespace ASD.NES.Kernel {

    public interface IAudioBuffer {
        int CopyToArray(float[] buffer, int offset, int samplesCount);
    }
}