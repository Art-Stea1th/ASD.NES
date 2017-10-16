using System.Runtime.InteropServices;

// Yes, I know about extension methods, but in this case I preferred this way :)

namespace ASD.NES.Core.Shared {

    /// <summary> Represents a 32-bit unsigned integer </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct Quadlet {

        [FieldOffset(0)] private uint quadlet;

        [FieldOffset(0)] private ushort hextetL;
        [FieldOffset(2)] private ushort hextetH;

        /// <summary> Get or set High-Hextet (16 bits) </summary>
        public Hextet H { get => hextetH; set => hextetH = value; }

        /// <summary> Get or set Low-Hextet (16 bits) </summary>
        public Hextet L { get => hextetL; set => hextetL = value; }

        /// <summary> Get or set specified bit </summary>
        public bool this[int bit] {
            get => ((quadlet >> bit) & 1) == 1;
            set => quadlet = (uint)(value ? (int)quadlet | (1 << bit) : quadlet & ~(1 << bit));
        }

        public static implicit operator uint(Quadlet value) => value.quadlet;
        public static implicit operator Quadlet(uint value) => new Quadlet(value);

        public static Quadlet Make(ushort highHextet, ushort lowHextet)
            => new Quadlet(highHextet, lowHextet);

        private Quadlet(uint value) : this() => this.quadlet = value;
        private Quadlet(ushort highHextet, ushort lowHextet) : this() {
            hextetH = highHextet; hextetL = lowHextet;
        }
    }

    /// <summary> Represents a 16-bit unsigned integer </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct Hextet {

        [FieldOffset(0)] private ushort hextet;

        [FieldOffset(0)] private byte octetL;
        [FieldOffset(1)] private byte octetH;

        /// <summary> Get or set High-Octet (8 bits) </summary>
        public Octet H { get => octetH; set => octetH = value; }

        /// <summary> Get or set Low-Octet (8 bits) </summary>
        public Octet L { get => octetL; set => octetL = value; }

        /// <summary> Get or set specified bit </summary>
        public bool this[int bit] {
            get => ((hextet >> bit) & 1) == 1;
            set => hextet = (ushort)(value ? hextet | (1 << bit) : hextet & ~(1 << bit));
        }

        public static implicit operator ushort(Hextet value) => value.hextet;
        public static implicit operator Hextet(ushort value) => new Hextet(value);

        public static Hextet Make(byte highOctet, byte lowOctet) => new Hextet(highOctet, lowOctet);
        private Hextet(ushort value) : this() => this.hextet = value;
        private Hextet(byte highOctet, byte lowOctet) : this() {
            octetH = highOctet; octetL = lowOctet;
        }
    }

    /// <summary> Represents an 8-bit unsigned integer </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct Octet {

        [FieldOffset(0)] private byte octet;

        /// <summary> Get or set High-Nybble (4 bits) </summary>
        public byte H {
            get => (byte)(octet >> 4);
            set => octet = (byte)((octet & 0x0F) | (value << 4));
        }

        /// <summary> Get or set Low-Nybble (4 bits) </summary>
        public byte L {
            get => (byte)(octet & 0x0F);
            set => octet = (byte)((octet & 0xF0) | (value & 0x0F));
        }

        /// <summary> Get or set specified bit </summary>
        public bool this[int bit] {
            get => ((octet >> bit) & 1) == 1;
            set => octet = (byte)(value ? octet | (1 << bit) : octet & ~(1 << bit));
        }

        public static implicit operator byte(Octet value) => value.octet;
        public static implicit operator Octet(byte value) => new Octet(value);

        public static Octet Make(byte highNybble, byte lowNybble) => new Octet(highNybble, lowNybble);
        private Octet(byte value) : this() => octet = value;
        private Octet(byte highNybble, byte lowNybble) : this() {
            H = highNybble; L = lowNybble;
        }
    }
}