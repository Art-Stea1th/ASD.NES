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

        // NTSC: NESDEV APU Frame Counter 4-step mode — quarter/half frame at 3728, 7456, 11185, 14914 CPU cycles
        private const int FrameCounterCycle = 14914;
        private const int Step1 = 3728;
        private const int Step2 = 7456;
        private const int Step3 = 11185;

        private int frameCounterAccumulator;
        private int frameCounterStep; // 0..3, which step we've last completed

        private const int clockSpeed = 1789773;                         // NTSC 1.789773 MHz
        private const int sampleRate = 48000;                           // 48 kHz
        private const double cyclesPerSample = (double)clockSpeed / sampleRate; // ~37.306 CPU cycles per sample
        private double sampleCycleAccumulator;

        private const int samplesPerFrame = 800;                        // ~48kHz/60Hz, one frame worth
        private int samplesBeforeFirstPlay = samplesPerFrame;           // warmup: one frame before first PlayAudio
        private int samplesSincePlay;                                    // invoke PlayAudio every frame worth

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

        /// <summary> Drive APU by CPU cycles: frame counter at 3728,7456,11185,14914 (NTSC 4-step); sample output at 48 kHz. </summary>
        /// <see href="https://www.nesdev.org/wiki/APU_Frame_Counter">NESDEV APU Frame Counter</see>
        /// <see href="https://www.nesdev.org/wiki/Cycle_reference_chart">NESDEV Cycle reference</see>
        public void StepCpuCycles(int cpuCycles) {

            frameCounterAccumulator += cpuCycles;
            while (true) {
                if (frameCounterAccumulator >= Step1 && frameCounterStep < 1) {
                    APUFrameTick(quarterOnly: true);
                    frameCounterStep = 1;
                    continue;
                }
                if (frameCounterAccumulator >= Step2 && frameCounterStep < 2) {
                    APUFrameTick(quarterOnly: false);
                    frameCounterStep = 2;
                    continue;
                }
                if (frameCounterAccumulator >= Step3 && frameCounterStep < 3) {
                    APUFrameTick(quarterOnly: true);
                    frameCounterStep = 3;
                    continue;
                }
                if (frameCounterAccumulator >= FrameCounterCycle) {
                    APUFrameTick(quarterOnly: false);
                    frameCounterStep = 0;
                    frameCounterAccumulator -= FrameCounterCycle;
                    continue;
                }
                break;
            }

            sampleCycleAccumulator += cpuCycles;
            while (sampleCycleAccumulator >= cyclesPerSample) {
                sampleCycleAccumulator -= cyclesPerSample;
                WriteOneSample();
            }
        }

        private void APUFrameTick(bool quarterOnly) {

            if (!quarterOnly) {
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
        }

        private void WriteOneSample() {

            if (samplesBeforeFirstPlay > 0) {
                samplesBeforeFirstPlay--;
                if (samplesBeforeFirstPlay == 0) {
                    PlayAudio?.Invoke();
                }
            }
            samplesSincePlay++;
            if (samplesSincePlay >= samplesPerFrame) {
                samplesSincePlay = 0;
                PlayAudio?.Invoke();
            }

            float paAudio = 0f, pbAudio = 0f, trAudio = 0f, nsAudio = 0f, dmAudio = 0f;

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
                // dmAudio = modulation.GetAudio();
            }

            (Buffer as AudioBuffer).Write(((paAudio + pbAudio + trAudio + nsAudio + dmAudio) / 5f) * 0.85f);
        }
    }
}