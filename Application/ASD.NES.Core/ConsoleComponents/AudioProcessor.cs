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

        private TvRegionProfileData _profile;
        private int frameCounterAccumulator;
        private int frameCounterStep; // 0..3, which step we've last completed

        private const int sampleRate = 48000;                           // 48 kHz
        private double cyclesPerSample;                                 // depends on region CPU clock
        private double sampleCycleAccumulator;

        private int samplesPerFrame;                                    // from profile (NTSC ~800, PAL 960)
        private int samplesBeforeFirstPlay;
        private int samplesSincePlay;

        public IAudioBuffer Buffer { get; private set; }
        public event Action PlayAudio;

        public AudioProcessor() {
            r = cpuMemory.RegistersAPU;
            Buffer = new AudioBuffer();
            _profile = TvRegionProfile.Ntsc;
            cyclesPerSample = (double)_profile.CpuClockHz / sampleRate;
            samplesPerFrame = _profile.SamplesPerFrame;
            samplesBeforeFirstPlay = samplesPerFrame;
            InitializeChannels();
        }

        /// <summary> Set TV region (NTSC/PAL). Uses TvRegionProfile for frame counter and CPU clock; re-inits channels. </summary>
        internal void SetRegion(TvRegion region) {
            _profile = TvRegionProfile.For(region);
            cyclesPerSample = (double)_profile.CpuClockHz / sampleRate;
            samplesPerFrame = _profile.SamplesPerFrame;
            samplesBeforeFirstPlay = samplesPerFrame;
            frameCounterAccumulator = 0;
            frameCounterStep = 0;
            sampleCycleAccumulator = 0;
            samplesSincePlay = 0;
            InitializeChannels();
        }

        private void InitializeChannels() {
            var clock = _profile.CpuClockHz;
            pulseA = new PulseChannel(r.PulseA, clock, sampleRate);
            pulseB = new PulseChannel(r.PulseB, clock, sampleRate);
            triangle = new TriangleChannel(r.Triangle, clock, sampleRate);
            noise = new NoiseChannel(r.Noise, clock, sampleRate);
            modulation = new DeltaModulationChannel(r.Modulation, clock, sampleRate);
        }

        /// <summary> Drive APU by CPU cycles: frame counter from TvRegionProfile (NTSC/PAL); sample output at 48 kHz. </summary>
        /// <see href="https://www.nesdev.org/wiki/APU_Frame_Counter">NESDEV APU Frame Counter</see>
        /// <see href="https://www.nesdev.org/wiki/Cycle_reference_chart">NESDEV Cycle reference</see>
        public void StepCpuCycles(int cpuCycles) {

            frameCounterAccumulator += cpuCycles;
            while (true) {
                if (frameCounterAccumulator >= _profile.ApuFrameStep1 && frameCounterStep < 1) {
                    APUFrameTick(quarterOnly: true);
                    frameCounterStep = 1;
                    continue;
                }
                if (frameCounterAccumulator >= _profile.ApuFrameStep2 && frameCounterStep < 2) {
                    APUFrameTick(quarterOnly: false);
                    frameCounterStep = 2;
                    continue;
                }
                if (frameCounterAccumulator >= _profile.ApuFrameStep3 && frameCounterStep < 3) {
                    APUFrameTick(quarterOnly: true);
                    frameCounterStep = 3;
                    continue;
                }
                if (frameCounterAccumulator >= _profile.ApuFrameCycle) {
                    APUFrameTick(quarterOnly: false);
                    frameCounterStep = 0;
                    frameCounterAccumulator -= _profile.ApuFrameCycle;
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