namespace ASD.NES.Core.Shared {

    internal class Reference<T> where T : struct {

        public T Value;

        protected Reference(T value = default(T))
            => Value = value;
    }
}