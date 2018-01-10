using System;

namespace ASD.NES.Kernel.ConsoleComponents.APUParts {

    internal sealed class AudioBuffer : IAudioBuffer {

        private float[] circularBuffer = new float[1024 * 64];
        private ushort headPtr = 0;
        private ushort nextPtr = 0;

        public void Write(float value) {
            circularBuffer[nextPtr] = value;
            nextPtr++;
        }

        public int CopyToArray(float[] buffer, int offset, int samplesCount) {

            if (headPtr < nextPtr) {

                var amount = (ushort)Math.Min(nextPtr - headPtr, samplesCount);

                Copy(circularBuffer, headPtr, buffer, offset, amount);

                headPtr += amount;
                return amount;
            }
            else if (nextPtr < headPtr) {

                var amountAfter = Math.Min(circularBuffer.Length - headPtr, samplesCount);

                Copy(circularBuffer, headPtr, buffer, offset, amountAfter);

                samplesCount -= amountAfter;

                var amountBefore = Math.Min(nextPtr, samplesCount);

                Copy(circularBuffer, 0, buffer, offset + amountAfter, amountBefore);

                var floatsCopied = amountAfter + amountBefore;
                headPtr += (ushort)floatsCopied;
                return floatsCopied;
            }
            else {
                return 0;
            }
        }

        private void Copy(float[] src, int srcOffset, float[] dest, int destOffset, int length) {
            for (var i = 0; i < length; i++) {
                dest[destOffset + i] = src[srcOffset + i];
            }
        }
    }
}