namespace ASD.NES.Core {

    /// <summary> Options for hardware-accurate behaviour (originals vs pirates). NESDEV: UxROM/CNROM bus conflicts. </summary>
    public static class EmulationOptions {

        /// <summary> UxROM (mapper 2): when true, latch = value AND PRG byte at write address (original Nintendo boards). Default false = no conflict (pirates, most dumps). </summary>
        public static bool UxROMBusConflict { get; set; }

        /// <summary> CNROM (mapper 3): when true, bank select = value AND PRG byte at write address (AND-type bus conflict). Default false = no conflict. </summary>
        public static bool CNROMBusConflict { get; set; }
    }
}
