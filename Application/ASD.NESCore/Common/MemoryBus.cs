using System;

namespace ASD.NESCore.Common {

    internal sealed class MemoryBus {

        private static Lazy<MemoryBus> instance = new Lazy<MemoryBus>(() => new MemoryBus());

        private const int bytes = ushort.MaxValue + 1; // 64kb

        private Cell[] cpuRam = new Cell[bytes / 32].Default();   // 2 kb (mirror x4)
        private Cell[] ppuRam = new Cell[bytes / 8192].Default(); // 8  b (mirror x1024)
        private Cell[] apuRam = new Cell[bytes / 16].Default();   // 4 kb
        private Cell[] mmc5 = new Cell[bytes / 16].Default();     // 4 kb
        private Cell[] save = new Cell[bytes / 8].Default();      // 8 kb
        private Cell[] prg1 = new Cell[bytes / 4].Default();      // 16kb
        private Cell[] prg0 = new Cell[bytes / 4].Default();      // 16kb

        private static Cell[] memory = new Cell[bytes].Default();

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

        private void Bind(Cell[] bank, int start, int repeat = 1) {
            for (var i = 0; i < bank.Length; i++) {
                for (var j = 0; j < repeat; j++) {
                    var pos = start + bank.Length * j;
                    memory[pos] = bank[i];
                }
            }
        }

        private sealed class Cell { public byte Value; }
    }

    internal static class LocalExtensions {

        public static T[] Default<T>(this T[] n) where T : new() {
            for (var i = 0; i < n.Length; i++) {
                n[i] = new T();
            }
            return n;
        }
    }
}