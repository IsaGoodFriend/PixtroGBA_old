#nullable enable

using Pixtro.Emulation.Common;

namespace Pixtro.Client.Common
{
	public readonly struct FirmwareEventArgs
	{
		public readonly string? Hash;

		public readonly FirmwareID ID;

		public readonly long Size;

		public FirmwareEventArgs(FirmwareID id, string? hash, long size)
		{
			Hash = hash;
			ID = id;
			Size = size;
		}
	}
}
