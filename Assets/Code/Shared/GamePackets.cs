using System;
using Code.Server;
using LiteNetLib.Utils;
using UnityEngine;

namespace Code.Shared
{
    public enum PacketType : byte
    {
        Movement,
        Spawn,
        ServerState,
        Serialized
    }
    
    //Auto serializable packets
    public class JoinPacket
    {
        public string UserName { get; set; }
    }

    public class JoinAcceptPacket
    {
        public long Id { get; set; }
    }

    public class PlayerJoinedPacket
    {
        public string UserName { get; set; }
        public bool NewPlayer { get; set; }

        public PlayerState InitialPlayerState { get; set; }
    }

    public class PlayerLeavedPacket
    {
        public byte Id { get; set; }
    }

    //Manual serializable packets
    public struct SpawnPacket : INetSerializable
    {
        public long PlayerId;
        public Vector2 Position;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(Position);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetLong();
            Position = reader.GetVector2();
        }
    }

    [Flags]
    public enum MovementKeys : byte
    {
        Left = 1 << 1,
        Right = 1 << 2,
        Up = 1 << 3,
        Down = 1 << 4,
        Fire = 1 << 5
    }
    
    public struct PlayerInputPacket : INetSerializable
    {
        public ushort Id;
        public MovementKeys Keys;
        public float Rotation;
        public ushort ServerTick;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put((byte)Keys);
            writer.Put(Rotation);
            writer.Put(ServerTick);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetUShort();
            Keys = (MovementKeys)reader.GetByte();
            Rotation = reader.GetFloat();
            ServerTick = reader.GetUShort();
        }
    }
    
    public struct PlayerState : INetSerializable
    {
        public byte Id;
        public Vector2 Position;
        public float Rotation;
        public byte Health;
        public ushort ProcessedCommandId;

        public const int Size = 1 + 8 + 4 + 1 + 2;
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(Position);
            writer.Put(Rotation);
            writer.Put(Health);
            writer.Put(ProcessedCommandId);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetByte();
            Position = reader.GetVector2();
            Rotation = reader.GetFloat();
            Health = reader.GetByte();
            ProcessedCommandId = reader.GetUShort();
        }
    }

    public struct ServerState : INetSerializable
    {
        public ushort Tick;
        public int PlayerStatesCount;
        public int StartState; //server only
        public PlayerState[] PlayerStates;
        
        //tick
        public const int HeaderSize = sizeof(ushort);
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Tick);
            
            for (int i = 0; i < PlayerStatesCount; i++)
                PlayerStates[StartState + i].Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetUShort();
            
            PlayerStatesCount = reader.AvailableBytes / PlayerState.Size;
            if (PlayerStates == null || PlayerStates.Length < PlayerStatesCount)
                PlayerStates = new PlayerState[PlayerStatesCount];
            for (int i = 0; i < PlayerStates.Length; i++)
                PlayerStates[i].Deserialize(reader);
        }
    }
}