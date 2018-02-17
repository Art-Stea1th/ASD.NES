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

        private int stepCounter;
        private bool tickLengthCounterAndSweep;

        private const int sampleRate = 48000;                           // 48kHz
        private const int samplesPerFrame = sampleRate / 60;            // 800
        private const int samplesPerAPUFrameTick = samplesPerFrame / 4; // 200

        // PPU ticks = 260 * 341 = 88660 per frame (256 * 240 - visible pixels)
        // CPU tisks ~ 88660 / 3 ~ 29553 per frame
        // APU ticks ~ 29553 / 2 ~ 14777 ((88660/3)/2) per frame
        public void Step() {                         // TODO: impl. the higher frequency or smart clock generator?
            switch (stepCounter) {
                //case 3728:
                //case 7456:
                //case 11185:
                //case 14914: APUFrameTick(); break; // <- by spec. 4-Step mode: http://wiki.nesdev.com/w/index.php/APU_Frame_Counter
                //case 14915: stepCounter = 0; break;
                //default: break;
                case 3584:
                case 7168:
                case 10752:
                case 14336: APUFrameTick(); break;   // <- tmp. audio align: just 1024 * 14 (crutch, chosen at random)
                case 14337: stepCounter = 0; break;
                default: break;
            }
            stepCounter++;
        }

        private void APUFrameTick() {

            if (tickLengthCounterAndSweep) {
                r.PulseA.TickLengthCounter();
                r.PulseB.TickLengthCounter();
                r.Triangle.TickLengthCounter();
                r.Noise.TickLengthCounter();

                r.PulseA.TickSweep();
                r.PulseB.TickSweep();
            }

            r.PulseA.TickEnvelopeCounter();
            r.PulseB.TickEnvelopeCounter();
            r.Triangle.TickLinearCounter();

            // r.Noise.TickShiftRegister();
            r.Noise.TickEnvelopeCounter();

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

            float pulseA = 0f, pulseB = 0f, triangle = 0f, noise = 0f, dm = 0f;

            for (var i = 0; i < samplesPerAPUFrameTick; i++) {

                if (r.Status.PulseAEnabled || r.PulseA.CurrentLengthCounter != 0) {
                    pulseA = r.PulseA.GetAudio(timeInSamples, sampleRate);
                }
                if (r.Status.PulseBEnabled || r.PulseB.CurrentLengthCounter != 0) {
                    pulseB = r.PulseB.GetAudio(timeInSamples, sampleRate);
                }
                if (r.Status.TriangleEnabled && r.Triangle.CurrentLinearCounter != 0 && r.Triangle.CurrentLengthCounter != 0) {
                    triangle = r.Triangle.GetAudio(timeInSamples, sampleRate);
                }
                if (r.Status.NoiseEnabled && r.Noise.CurrentLengthCounter != 0 && r.Noise.CurrentLengthCounter != 0) {
                    // noise = r.Noise.GetAudio(timeInSamples, sampleRate);         // disabled, no pitch
                }
                if (r.Status.DmcEnabled) {
                    // dm = r.DeltaModulation.GetAudio(timeInSamples, sampleRate);  // disabled, not impl.
                }

                // TODO: impl. APU Mixer http://wiki.nesdev.com/w/index.php/APU_Mixer instead of (n + n + n + n + n) / 5
                (Buffer as AudioBuffer).Write((pulseA + pulseB + triangle + noise + dm) / 5f);

                timeInSamples++;
            }
            if (timeInSamples > sampleRate * 10) { // !!!
                timeInSamples = 0;
            }
        }
    }
}