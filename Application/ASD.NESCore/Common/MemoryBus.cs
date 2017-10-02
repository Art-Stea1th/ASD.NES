using System;

namespace ASD.NESCore.Common {

    internal sealed class MemoryBus {

        private static Lazy<MemoryBus> instance = new Lazy<MemoryBus>(() => new MemoryBus());

        private const int bytes = ushort.MaxValue + 1; // 64kb

        private RByte[] cpuRam = new RByte[bytes / 32].Default();   // 2 kb (mirror x4)
        private RByte[] ppuRam = new RByte[bytes / 8192].Default(); // 8  b (mirror x1024)
        private RByte[] apuRam = new RByte[bytes / 16].Default();   // 4 kb
        private RByte[] mmc5 = new RByte[bytes / 16].Default();     // 4 kb
        private RByte[] save = new RByte[bytes / 8].Default();      // 8 kb
        private RByte[] prg1 = new RByte[bytes / 4].Default();      // 16kb
        private RByte[] prg0 = new RByte[bytes / 4].Default();      // 16kb

        private static RByte[] memory = new RByte[bytes].Default();

        public static MemoryBus Instance => instance.Value;
        public byte this[int address] {
            get => memory[address].Value;
            set => memory[address].Value = value;
        }

        private MemoryBus() => Bind();

        private void Bind() {

            var startAddress = 0;

            Bind(cpuRam, startAddress, 4);
            Bind(ppuRam, startAddress += (cpuRam.Length * 4), 1024);
            Bind(apuRam, startAddress += (ppuRam.Length * 1024));

            Bind(mmc5, startAddress += apuRam.Length);
            Bind(save, startAddress += mmc5.Length);
            Bind(prg1, startAddress += save.Length);
            Bind(prg0, startAddress += prg1.Length);
        }

        private void Bind(RByte[] bank, int start, int repeat = 1) {
            for (var i = 0; i < bank.Length; i++) {
                for (var j = 0; j < repeat; j++) {
                    var pos = start + bank.Length * j;
                    memory[pos] = bank[i];
                }
            }
        }
    }

    internal static class LocalExtensions {

        public static RByte[] Default(this RByte[] n) {
            for (var i = 0; i < n.Length; i++) {
                n[i] = RByte.Wrap(0);
            }
            return n;
        }
    }
}