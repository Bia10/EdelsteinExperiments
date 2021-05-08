using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Edelstein.Core.Entities;
using Edelstein.Core.Entities.Characters;
using Edelstein.Core.Gameplay.Extensions.Packets;
using Edelstein.Core.Gameplay.Migrations.States;
using Edelstein.Core.Network.Packets;
using Edelstein.Core.Utils.Packets;
using Edelstein.Service.Login.Types;
using MoreLinq;

namespace Edelstein.Service.Login.Handlers
{
    public class SelectWorldHandler : AbstractPacketHandler<LoginServiceAdapter>
    {
        protected override async Task Handle(
            LoginServiceAdapter adapter,
            RecvPacketOperations operation,
            IPacketDecoder packet
        )
        {
            packet.DecodeByte();

            var worldID = packet.DecodeByte();
            var channelID = packet.DecodeByte();

            if (adapter.Account == null) return;

            try
            {
                using var p = new OutPacket(SendPacketOperations.SelectWorldResult);
                using var store = adapter.Service.DataStore.StartSession();
                var result = LoginResultCode.Success;

                var peers = await adapter.Service.GetPeers();
                var service = peers
                    .Select(n => n.State)
                    .OfType<GameNodeState>()
                    .Where(s => s.Worlds.Contains(worldID))
                    .OrderBy(s => s.ChannelID)
                    .ToImmutableList()[channelID];

                if (service == null) result = LoginResultCode.NotConnectableWorld;

                p.EncodeByte((byte) result);

                if (result == LoginResultCode.Success)
                {
                    var accountWorld = store
                        .Query<AccountWorld>()
                        .Where(a =>
                            a.AccountID == adapter.Account.ID &&
                            a.WorldID == worldID
                        )
                        .FirstOrDefault();

                    if (accountWorld == null)
                    {
                        accountWorld = new AccountWorld
                        {
                            AccountID = adapter.Account.ID,
                            WorldID = worldID
                        };
                        await store.InsertAsync(accountWorld);
                    }

                    adapter.SelectedNode = service;
                    adapter.AccountWorld = accountWorld;
                    if (adapter.Account.LatestConnectedWorld != worldID)
                        adapter.Account.LatestConnectedWorld = worldID;

                    var characters = store
                        .Query<Character>()
                        .Where(c => c.AccountWorldID == accountWorld.ID)
                        .ToImmutableList();

                    p.EncodeByte((byte) characters.Count);
                    characters.ForEach(c =>
                    {
                        c.EncodeStats(p);
                        c.EncodeLook(p);

                        p.EncodeBool(false);
                        p.EncodeBool(false);
                    });

                    //p.EncodeBool(!string.IsNullOrEmpty(adapter.Account.SPW)); // bLoginOpt TODO: proper bLoginOpt stuff
                    //2,3 select character
                    p.EncodeByte(0); //0 register pic, 1 require pic
                    p.EncodeInt(accountWorld.SlotCount); // m_nSlotCount
                    p.EncodeInt(0); // m_nBuyCharCount
                }

                await adapter.SendPacket(p);
            }
            catch
            {
                using var p = new OutPacket(SendPacketOperations.SelectWorldResult);

                p.EncodeByte((byte) LoginResultCode.DBFail);

                await adapter.SendPacket(p);
            }
        }
    }
}