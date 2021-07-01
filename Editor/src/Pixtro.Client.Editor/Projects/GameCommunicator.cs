using Pixtro.Emulation.Common;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixtro.Client.Editor.Projects
{
	public sealed class GameCommunicator
	{
		private const bool BigEndian = false;

		public sealed class MemoryMap
		{
			public MemoryDomain domain;
			public int address { get; private set; }
			public int size { get; private set; }

			private byte[] state;

			public byte this[int index]
			{
				get
				{
					return GetByte(index);
				}
				set
				{
					SetByte(index, value);
				}
			}

			public byte GetByte(int index)
			{
				if (size > 0 && index > size || index < 0)
					throw new IndexOutOfRangeException();

				return domain.PeekByte(index + address);
			}
			public void SetByte(int index, byte val)
			{
				if (size > 0 && index > size || index < 0)
					throw new IndexOutOfRangeException();

				domain.PokeByte(index + address, val);
			}
			public ushort GetUshort(int index)
			{
				index >>= 1;
				if (size > 0 && index > size || index < 0)
					throw new IndexOutOfRangeException();

				return domain.PeekUshort(index + address, BigEndian);
			}
			public void SetUshort(int index, ushort val)
			{
				index >>= 1;
				if (size > 0 && index > size || index < 0)
					throw new IndexOutOfRangeException();

				domain.PokeUshort(index + address, val, BigEndian);
			}
			public uint GetUint(int index)
			{
				index >>= 2;
				if (size > 0 && index > size || index < 0)
					throw new IndexOutOfRangeException();

				return domain.PeekUint(index + address, BigEndian);
			}
			public void SetUint(int index, uint val)
			{
				index >>= 2;
				if (size > 0 && index > size || index < 0)
					throw new IndexOutOfRangeException();

				domain.PokeUint(index + address, val, BigEndian);
			}

			public bool GetFlag(int flag, int offset = 0)
			{
				uint val = GetUint(offset);

				return (val & (1 << flag)) > 0;
			}
			public void SetFlag(int flag, bool value, int offset = 0)
			{
				uint val = GetUint(offset);
				val &= (uint)~(1 << flag);
				if (value)
					val |= (uint)(1 << flag);

				SetUint(offset, val);
			}

			public void SaveState()
			{
				for (int i = 0; i < size; ++i)
				{
					state[i] = GetByte(i);
				}
			}
			public byte[] LoadState()
			{
				return state.ToArray(); // Just so you can't dig in and mess with the values
			}
			public byte[] GetState()
			{
				byte[] retval = new byte[size];
				for (int i = 0; i < size; ++i)
				{
					retval[i] = GetByte(i);
				}

				return retval;
			}
			public void SetState(byte[] bytes)
			{
				for (int i = 0; i < size; ++i)
				{
					SetByte(i, bytes[i]);
				}
			}

			public MemoryMap(MemoryDomain _domain, int _addr, int _size)
			{
				domain = _domain;
				address = _addr;
				size = Math.Max(_size, 1);

				state = new byte[size];
			}
		}
		public static GameCommunicator Instance { get; private set; }

		private IEmulator _emulator;

		[RequiredService]
		private IMemoryDomains MemoryDomains { get; set; }

		private MemoryDomain IWRam, EWRam, PalRam;

		public MemoryMap debug_engine_flags { get; private set; }
		private readonly MemoryMap[] maps;
		
		

		public GameCommunicator(IEmulator emulator, StreamReader memoryMap)
		{
			_emulator = emulator;
			Instance = this;

			while (!memoryMap.ReadLine().StartsWith("Allocating common symbols")) ;

			string nextLine;

			List<MemoryMap> mapList = new List<MemoryMap>();

			do
			{
				nextLine = memoryMap.ReadLine();

				if (string.IsNullOrWhiteSpace(nextLine))
					continue;

				string[] split = nextLine.Split(new char[]{ ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (!split[0].StartsWith("0x0") || split.Length > 3)
					continue;

				MemoryDomain domain;
				switch (split[0][11])
				{
					default:
						continue;
					case '2':
						domain = EWRam;
						break;
					case '3':
						domain = IWRam;
						break;
				}

				int parsed = Convert.ToInt32(split[0].Substring(12, 6), 16);

				switch (split[1])
				{
					case "debug_engine_flags":
						mapList.Add(debug_engine_flags = new MemoryMap(domain, parsed, 1));

						break;
				}

			} while (!nextLine.Trim().StartsWith(".comment"));

			maps = mapList.ToArray();
		}
		public GameCommunicator(IEmulator emulator, StreamReader memoryMap, GameCommunicator previousState)
			:this(emulator, memoryMap)
		{
			GetStateFrom(previousState);
		}

		public void SaveState()
		{
			foreach (var map in maps)
			{
				map.SaveState();
			}
		}
		public void GetStateFrom(GameCommunicator other)
		{
			for (int i = 0; i < maps.Length; ++i)
			{
				maps[i].SetState(other.maps[i].GetState());
			}
		}

		public void RomLoaded()
		{
			IWRam = MemoryDomains["IWRAM"];
			EWRam = MemoryDomains["EWRAM"];
			PalRam = MemoryDomains["PALRAM"];
		}
	}
}
