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
                r.PulseA.TickSweep();
                r.PulseB.TickSweep();
            }

            r.PulseA.TickEnvelopeCounter();
            r.PulseB.TickEnvelopeCounter();

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

            for (var i = 0; i < samplesPerAPUFrameTick; i++) {
                var pulseA = r.PulseA.GetPulseAudio(timeInSamples, sampleRate);
                var pulseB = r.PulseB.GetPulseAudio(timeInSamples, sampleRate);

                // TODO: impl. APU Mixer http://wiki.nesdev.com/w/index.php/APU_Mixer
                (Buffer as AudioBuffer).Write(pulseA + pulseB);
                timeInSamples++;
            }

            if (timeInSamples > sampleRate) { // !!!
                timeInSamples = 0;
            }
        }
    }
}