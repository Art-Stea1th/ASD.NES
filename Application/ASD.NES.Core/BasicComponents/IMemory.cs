namespace ASD.NES.Core.BasicComponents {

    internal interface IMemory<T> {
        int Cells { get; }
        T this[int address] { get; set; }
    }
}