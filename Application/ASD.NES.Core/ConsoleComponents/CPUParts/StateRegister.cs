namespace ASD.NES.Core.ConsoleComponents.CPUParts {

    using Shared;

    /// <summary> Processor State flag </summary>
    internal sealed class StateRegister {

        private readonly RInt8 register = RInt8.Wrap(0x00);
        private readonly StateFlag s, v, u, b, d, i, z, c;

        /// <summary> "Signed" (bit 7) - set when the previous operation resulted in a negative value. </summary>
        public StateFlag S => s;

        /// <summary> "Overflow" (bit 6) - set when the previous caused a signed overflow. </summary>
        public StateFlag V => v;

        /// <summary> "Unused" (bit 5) - not used. Supposed to be logical 1 at all times. </summary>
        public StateFlag U => u;

        /// <summary> "Break" (bit 4) - set when a software interrupt (BRK instruction) is executed. </summary>
        public StateFlag B => b;

        /// <summary> "Decimal" (bit 3) - set when the Decimal Mode is enabled. </summary>
        public StateFlag D => d;

        /// <summary> "Interrupt disable" (bit 2) - set: only NMI interrupts will get through (unset: IRQ and NMI will get through) </summary>
        public StateFlag I => i;

        /// <summary> "Zero" (bit 1) - set when the last operation resulted in a zero. </summary>
        public StateFlag Z => z;

        /// <summary> "Carry" (bit 0) - set when the last addition or shift resulted in a carry, or last subtraction resulted in no borrow. </summary>
        public StateFlag C => c;

        public StateRegister() {
            s = new StateFlag(register, 7); d = new StateFlag(register, 3);
            v = new StateFlag(register, 6); i = new StateFlag(register, 2);
            u = new StateFlag(register, 5); z = new StateFlag(register, 1);
            b = new StateFlag(register, 4); c = new StateFlag(register, 0);
        }

        public void UpdateSigned(int value) => s.Set(((sbyte)value) < 0);
        public void UpdateOverflow(int value) => v.Set(value > 255);
        public void UpdateZero(int value) => z.Set(value == 0);
        public void UpdateCarry(int value) => c.Set((value >> 8) != 0);

        public void SetNew(byte vatue) => this.register.Value = vatue;
        public static implicit operator byte(StateRegister state) => state.register;
    }
}