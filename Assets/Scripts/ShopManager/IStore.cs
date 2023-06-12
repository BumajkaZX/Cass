namespace Cass.StoreManager 
{
    using Cass.Items;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IStore
    {
        public abstract void Purchase(PlayerItem item);

        public abstract Task Init(CancellationToken token);
    }
}
