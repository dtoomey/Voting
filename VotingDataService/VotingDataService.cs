using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Data;

namespace VotingDataService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    public sealed class VotingDataService : StatefulService, IVotingDataService
    {
        IReliableDictionary<string, int> voteDictionary = null;  //NEW
        IReliableDictionary<string, long> votesCastDictionary = null;
        public const string VOTES_CAST_KEY = "TotalVotesCast";

        public VotingDataService(StatefulServiceContext context)
            : base(context)
        { }

        // NEW
        public async Task<int> AddVote(string voteItem)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.AddVote start. voteItem='{0}'", voteItem);
            int result = 0;
            long result2 = 0;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                voteDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("voteDictionary");
                votesCastDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("votesCastDictionary");
                result = await voteDictionary.AddOrUpdateAsync(tx, voteItem, 1, (key, value) => ++value);
                result2 = await votesCastDictionary.AddOrUpdateAsync(tx, VOTES_CAST_KEY, 1, (key, value) => ++value);
                await tx.CommitAsync();
            }

            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.AddVote end. Total votes: {0}", result.ToString());
            return result;
        }

        public async Task<int> DeleteVoteItem(string voteItem)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.DeleteVoteItem start.");
            ConditionalValue<int> result = new ConditionalValue<int>(false, -1);

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                if (voteDictionary != null)
                {
                    ConditionalValue<int> deleteVotes = await voteDictionary.TryGetValueAsync(tx, voteItem);
                    result = await voteDictionary.TryRemoveAsync(tx, voteItem);
                    await votesCastDictionary.AddOrUpdateAsync(tx, VOTES_CAST_KEY, -1, (key, value) => (value - deleteVotes.Value));
                    await tx.CommitAsync();
                }
            }
            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.DeleteVoteItem end.");
            return result.HasValue ? result.Value : -1;
        }

        public async Task<int> GetNumberOfVotes(string voteItem)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.GetNumberOfVotes start.");
            ConditionalValue<int> result = new ConditionalValue<int>(true, 0);

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                if (voteDictionary != null)
                {
                    result = await voteDictionary.TryGetValueAsync(tx, voteItem);
                    await tx.CommitAsync();
                }
            }

            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.GetNumberOfVotes end.");
            return result.HasValue ? result.Value : 0;
        }

        public async Task<long> GetTotalNumberOfVotes()
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.GetTotalNumberOfVotes start.");
            ConditionalValue<long> result = new ConditionalValue<long>(true, 0);

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                if (votesCastDictionary != null)
                {
                    result = await votesCastDictionary.TryGetValueAsync(tx, VOTES_CAST_KEY);
                    await tx.CommitAsync();
                }
            }

            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.GetTotalNumberOfVotes end.");
            return result.HasValue ? result.Value : 0;
        }

        public async Task<List<KeyValuePair<string, int>>> GetAllVoteCounts()
        {
            ServiceEventSource.Current.Message("VotingDataService.GetAllVoteCounts start.");
            List<KeyValuePair<string, int>> kvps = new List<KeyValuePair<string, int>>();

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                if (voteDictionary != null)
                {
                    IAsyncEnumerable<KeyValuePair<string, int>> e = await voteDictionary.CreateEnumerableAsync(tx);
                    IAsyncEnumerator<KeyValuePair<string, int>> items = e.GetAsyncEnumerator();

                    while (await items.MoveNextAsync(new CancellationToken()))
                    {
                        kvps.Add(new KeyValuePair<string, int>(items.Current.Key, items.Current.Value));
                    }

                    //kvps.Sort((x, y) => x.Value.CompareTo(y.Value) * -1);  // intentionally commented out!
                }
                await tx.CommitAsync();
            }

            ServiceEventSource.Current.Message("VotingDataService.GetAllVoteCounts end. Number of keys: {0}", kvps.Count.ToString());
            return kvps;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.CreateServiceReplicaListeners start.");
            ServiceEventSource.Current.ServiceMessage(this.Context, "VotingDataService.CreateServiceReplicaListeners end.");
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        //protected override async Task RunAsync(CancellationToken cancellationToken)
        //{
        //    // TODO: Replace the following sample code with your own logic 
        //    //       or remove this RunAsync override if it's not needed in your service.

        //    //var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("myDictionary");

        //    while (true)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();
        //        ServiceEventSource.Current.Message("I'm here!!");

        //        //using (var tx = this.StateManager.CreateTransaction())
        //        //{
        //        //    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

        //        //    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
        //        //        result.HasValue ? result.Value.ToString() : "Value does not exist.");

        //        //    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

        //        //    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
        //        //    // discarded, and nothing is saved to the secondary replicas.
        //        //    await tx.CommitAsync();
        //        //}

        //        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        //    }
        //}
    }
}
