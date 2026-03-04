using System;

namespace ASD.NES.Core {

    /// <summary>
    /// Region-dependent timing constants: PPU scanlines, frame interval, CPU/APU clock, APU frame counter.
    /// Single source of truth for NTSC vs PAL so all components stay in sync.
    /// </summary>
    public static class TvRegionProfile {

        /// <summary> NTSC: 262 scanlines (0..260), ~60.1 Hz, CPU 1.789773 MHz. </summary>
        public static TvRegionProfileData For(TvRegion region) {
            return region == TvRegion.PAL ? Pal : Ntsc;
        }

        public static readonly TvRegionProfileData Ntsc = new TvRegionProfileData(
            lastScanline: 260,
            frameIntervalMs: 1000.0 / 60.0988,
            cpuClockHz: 1_789_773,
            apuFrameStep1: 3728,
            apuFrameStep2: 7456,
            apuFrameStep3: 11185,
            apuFrameCycle: 14914,
            samplesPerFrame: 800
        );

        /// <summary> PAL: 312 scanlines (0..311), ~50 Hz, CPU 1.662607 MHz. NESDEV APU Frame Counter (4-step). </summary>
        public static readonly TvRegionProfileData Pal = new TvRegionProfileData(
            lastScanline: 311,
            frameIntervalMs: 1000.0 / 50.0,
            cpuClockHz: 1_662_607,
            apuFrameStep1: 8314,
            apuFrameStep2: 16629,
            apuFrameStep3: 24943,
            apuFrameCycle: 33247,
            samplesPerFrame: 960
        );
    }

    public sealed class TvRegionProfileData {

        public int LastScanline { get; }
        public TimeSpan FrameInterval { get; }
        public int CpuClockHz { get; }
        public int ApuFrameStep1 { get; }
        public int ApuFrameStep2 { get; }
        public int ApuFrameStep3 { get; }
        public int ApuFrameCycle { get; }
        /// <summary> Approximate samples per frame at 48 kHz (NTSC ~800, PAL 960). </summary>
        public int SamplesPerFrame { get; }

        public TvRegionProfileData(
            int lastScanline,
            double frameIntervalMs,
            int cpuClockHz,
            int apuFrameStep1,
            int apuFrameStep2,
            int apuFrameStep3,
            int apuFrameCycle,
            int samplesPerFrame) {
            LastScanline = lastScanline;
            FrameInterval = TimeSpan.FromMilliseconds(frameIntervalMs);
            CpuClockHz = cpuClockHz;
            ApuFrameStep1 = apuFrameStep1;
            ApuFrameStep2 = apuFrameStep2;
            ApuFrameStep3 = apuFrameStep3;
            ApuFrameCycle = apuFrameCycle;
            SamplesPerFrame = samplesPerFrame;
        }
    }
}
