using System;

namespace ASD.NES.Core.ConsoleComponents {

    using APUParts;
    using APUParts.Channels;
    using CPUParts;

    internal sealed class AudioProcessor { // HARDCODE Impl.

        private CPUAddressSpace cpuMemory = CPUAddressSpace.Instance;
        private RegistersAPU r;

        private PulseChannel pulseA;
        private PulseChannel pulseB;
        private TriangleChannel triangle;
        private NoiseChannel noise;
        private DeltaModulationChannel modulation;

        private int stepCounter;
        private bool tickLengthAndSweep;

        private const int clockSpeed = 1789773;                         // 1.79MHz
        private const int sampleRate = 48000;                           // 48kHz
        private const int samplesPerFrame = sampleRate / 60;            // 800
        private const int samplesPerAPUFrameTick = samplesPerFrame / 4; // 200

        public IAudioBuffer Buffer { get; private set; }
        public event Action PlayAudio;

        public AudioProcessor() {
            r = cpuMemory.RegistersAPU;
            Buffer = new AudioBuffer();
            InitializeChannels();
        }

        private void InitializeChannels() {
            pulseA = new PulseChannel(r.PulseA, clockSpeed, sampleRate);
            pulseB = new PulseChannel(r.PulseB, clockSpeed, sampleRate);
            triangle = new TriangleChannel(r.Triangle, clockSpeed, sampleRate);
            noise = new NoiseChannel(r.Noise, clockSpeed, sampleRate);
            modulation = new DeltaModulationChannel(r.Modulation, clockSpeed, sampleRate);
        }

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

            if (tickLengthAndSweep) {

                pulseA.TickLength();
                pulseA.TickSweep();

                pulseB.TickLength();
                pulseB.TickSweep();

                triangle.TickLength();

                noise.TickLength();
            }

            pulseA.TickEnvelope();
            pulseB.TickEnvelope();
            triangle.TickLinearCounter();
            noise.TickEnvelope();

            WriteFrameCounterAudio();
            tickLengthAndSweep = !tickLengthAndSweep;
        }

        private int apuFrameTicksBeforePlayAudio = 40; // church, delay before play

        private void WriteFrameCounterAudio() {

            if (apuFrameTicksBeforePlayAudio >= 1) {
                apuFrameTicksBeforePlayAudio--;
            }
            if (apuFrameTicksBeforePlayAudio == 0) {
                PlayAudio?.Invoke();
            }

            float paAudio = 0f, pbAudio = 0f, trAudio = 0f, nsAudio = 0f, dmAudio = 0f;

            for (var i = 0; i < samplesPerAPUFrameTick; i++) {

                if (r.Status.PulseAEnabled || pulseA.LengthCounter != 0) {
                    paAudio = pulseA.GetAudio();
                }
                if (r.Status.PulseBEnabled || pulseB.LengthCounter != 0) {
                    pbAudio = pulseB.GetAudio();
                }
                if (r.Status.TriangleEnabled && triangle.LengthCounter != 0 && triangle.LinearCounter != 0) {
                    trAudio = triangle.GetAudio();
                }
                if (r.Status.NoiseEnabled && noise.LengthCounter != 0) {
                    nsAudio = noise.GetAudio();
                }
                if (r.Status.DmcEnabled) {
                    // dmAudio = modulation.GetAudio(); // disabled, not impl. reason: No games with DMC on Mapper 0 (NROM)
                }

                // TODO: impl. APU Mixer http://wiki.nesdev.com/w/index.php/APU_Mixer instead of (n + n + n + n + n) / 5
                (Buffer as AudioBuffer).Write(((paAudio + pbAudio + trAudio + nsAudio + dmAudio) / 5f) * 0.85f);
            }
        }
    }
}