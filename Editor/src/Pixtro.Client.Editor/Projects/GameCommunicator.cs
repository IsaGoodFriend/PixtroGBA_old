using Pixtro.Emulation.Common;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pixtro.Client.Editor.Projects
{
	public sealed class GameCommunicator
	{
		private const bool BigEndian = false;

		public sealed class RomMapping
		{
			public int Address { get; private set; }
			public int Size { get; private set; }

			public RomMapping(int addr, int size)
			{
				Address = addr;
				Size = size;
			}
		}
		public sealed class MemoryMap
		{
			public MemoryDomain domain;
			public int domainIndex { get; private set; }
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
				if (index > size || index < 0)
					throw new IndexOutOfRangeException();

				domain.PokeUint(index + address, val, BigEndian);

				uint value = domain.PeekUint(index + address, BigEndian);
			}

			public bool GetFlag(int flag, int offset = 0)
			{
				uint val = GetUint(offset);

				return (val & (1 << flag)) > 0;
			}
			public bool GetFlag(Enum value, int offset = 0)
			{
				int parsed = Convert.ToInt32(value);

				for (int i = 0; i < 32; ++i)
				{
					if ((parsed & 0x1) > 0)
					{
						if (parsed == 1)
						{
							return GetFlag(i, offset);
						}

						throw new ArgumentException();
					}
					parsed >>= 1;
				}
				throw new Exception();
			}
			public void SetFlag(int flag, bool value, int offset = 0)
			{
				uint val = GetUint(offset);
				val &= (uint)~(1 << flag);
				if (value)
					val |= (uint)(1 << flag);

				SetUint(offset, val);
			}
			public void EnableFlags(Enum values, int offset = 0)
			{
				uint parsedValues = Convert.ToUInt32(values);

				for (int i = 0; i < 32; ++i)
				{
					if ((parsedValues & 1) != 0)
						SetFlag(i, true, offset);

					parsedValues >>= 1;
				}

			}
			public void DisableFlags(Enum values, int offset = 0)
			{
				uint parsedValues = Convert.ToUInt32(values);

				for (int i = 0; i < 32; ++i)
				{
					if ((parsedValues & 1) != 0)
						SetFlag(i, false, offset);

					parsedValues >>= 1;
				}


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

			public MemoryMap(int _domainIndex, int _addr, int _size)
			{
				domainIndex = _domainIndex;
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

		public IReadOnlyDictionary<string, RomMapping> RomMap => romMap;

		public MemoryMap debug_engine_flags { get; private set; }
		public MemoryMap debug_game_flags { get; private set; }
		public MemoryMap entities { get; private set; }

		private MemoryMap[] maps;

		private Dictionary<string, RomMapping> romMap = new Dictionary<string, RomMapping>();
		

		public GameCommunicator(IEmulator emulator, StreamReader memoryMap)
		{
			_emulator = emulator;
			Instance = this;

			while (!memoryMap.ReadLine().StartsWith("Allocating common symbols")) ;

			string currentLine, nextLine = memoryMap.ReadLine();

			List<MemoryMap> mapList = new List<MemoryMap>();

			do
			{
				currentLine = nextLine;
				nextLine = memoryMap.ReadLine();

				if (string.IsNullOrWhiteSpace(currentLine))
					continue;

				string[] split = currentLine.Split(new char[]{ ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (!split[0].StartsWith("0x0") || split.Length > 3 || split[1].StartsWith("0x"))
					continue;

				int domain = int.Parse(split[0].Substring(11, 1));
				int parsed = Convert.ToInt32(split[0].Substring(12, 6), 16);

				switch (split[0][11])
				{
					case '8':
						string name = split[1];

						try
						{
							int otherAddr = Convert.ToInt32(nextLine.Substring(28, 6), 16);
							
							romMap.Add(name, new RomMapping(parsed, otherAddr - parsed));
						}
						catch { }
						continue;
				}
				

				var propertyInfo = GetType().GetProperty(split[1], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

				if (propertyInfo != null)
				{
					split = nextLine.Split(new char[]{ ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

					int otherAddr = Convert.ToInt32(split[0].Substring(12, 6), 16);

					var mapped = new MemoryMap(domain, parsed, otherAddr - parsed);

					mapList.Add(mapped);

					propertyInfo.GetSetMethod(true).Invoke(this, new object[] { mapped });

				}

			} while (!currentLine.Trim().StartsWith(".comment"));

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
			
			foreach (var map in maps)
			{
				switch (map.domainIndex)
				{
					case 2:
						map.domain = EWRam;
						break;
					case 3:
						map.domain = IWRam;
						break;
				}
			}
		}
	}
}
