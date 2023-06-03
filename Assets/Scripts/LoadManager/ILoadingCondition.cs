namespace Cass.LoadManager
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILoadingCondition
    {
        public int Order { get; }
        public string Name { get; }
        public bool IsInited { get; }
        public abstract Task<Action> Initialization(CancellationToken token);
    }
}
