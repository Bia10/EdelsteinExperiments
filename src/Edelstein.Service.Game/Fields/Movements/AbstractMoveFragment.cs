using Edelstein.Network.Packets;

namespace Edelstein.Service.Game.Fields.Movements
{
    public abstract class AbstractMoveFragment : IMoveFragment
    {
        public MoveFragmentAttribute Attribute { get; }

        protected AbstractMoveFragment(MoveFragmentAttribute attribute, IPacket packet)
        {
            Attribute = attribute;
            Decode(packet);
        }

        public abstract void Apply(IMoveContext context);

        public void Decode(IPacket packet)
            => DecodeData(packet);

        public void Encode(IPacket packet)
        {
            packet.EncodeByte((byte) Attribute);
            EncodeData(packet);
        }

        public abstract void DecodeData(IPacket packet);
        public abstract void EncodeData(IPacket packet);
    }
}