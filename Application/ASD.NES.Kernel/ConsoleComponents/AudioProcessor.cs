using System;

namespace ASD.NES.Kernel.ConsoleComponents {

    using APUParts;
    using CPUParts;

    internal sealed class AudioProcessor { // HARDCODE Impl.

        private CPUAddressSpace cpuMemory = CPUAddressSpace.Instance;
        private RegistersAPU r;

        public IAudioBuffer Buffer { get; private set; }
        public event Action PlayAudio;

        public AudioProcessor() {
            r = cpuMemory.RegistersAPU;
            Buffer = new AudioBuffer();
        }

        private int apuStepCounter;
        private bool tickLengthCounterAndSweep;        

        private const int sampleRate = 48000;                           // 48kHz
        private const int samplesPerFrame = sampleRate / 60;            // 800
        private const int samplesPerAPUFrameTick = samplesPerFrame / 4; // 200

        // Mode 0: 4-Step Sequence
        // http://wiki.nesdev.com/w/index.php/APU_Frame_Counter
        public void Step() {
            apuStepCounter++;
            switch (apuStepCounter) {
                case 3728:
                case 7456:
                case 11185:
                    APUFrameTick();
                    break;
                case 14914:
                    APUFrameTick();
                    apuStepCounter = 0;
                    break;
                default:
                    break;
            }
        }

        private void APUFrameTick() {

            if (tickLengthCounterAndSweep) {
                r.PulseA.TickLengthCounter();
                r.PulseB.TickLengthCounter();
                r.Triangle.TickLengthCounter();
                r.PulseA.TickSweep();
                r.PulseB.TickSweep();
            }

            r.PulseA.TickEnvelopeCounter();
            r.PulseB.TickEnvelopeCounter();
            r.Triangle.TickLinearCounter();

            WriteFrameCounterAudio();
            tickLengthCounterAndSweep = !tickLengthCounterAndSweep;
        }

        private int apuFrameTicksFillAudio = 40; // !!!
        private int timeInSamples = 0;           // !!!

        private void WriteFrameCounterAudio() {

            if (apuFrameTicksFillAudio > -1) { // !!!
                apuFrameTicksFillAudio--;
            }
            if (apuFrameTicksFillAudio == 0) { // !!!
                PlayAudio.Invoke();
            }

            float pulseA = 0f, pulseB = 0f, triangle = 0f;

            for (var i = 0; i < samplesPerAPUFrameTick; i++) {

                if (r.Status.PulseAEnabled || r.PulseA.CurrentLengthCounter != 0) {
                    pulseA = r.PulseA.GetPulseAudio(timeInSamples, sampleRate);
                }
                if (r.Status.PulseBEnabled || r.PulseB.CurrentLengthCounter != 0) {
                    pulseB = r.PulseB.GetPulseAudio(timeInSamples, sampleRate);
                }
                if (r.Status.TriangleEnabled && r.Triangle.CurrentLinearCounter != 0 && r.Triangle.CurrentLengthCounter != 0) {
                    triangle = r.Triangle.GetTriangleAudio(timeInSamples, sampleRate);
                }                

                // TODO: impl. APU Mixer http://wiki.nesdev.com/w/index.php/APU_Mixer
                (Buffer as AudioBuffer).Write(pulseA + pulseB + triangle);
                timeInSamples++;
            }
            if (timeInSamples > ushort.MaxValue) { // !!!
                timeInSamples = 0;
            }
        }
    }
}