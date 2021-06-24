//using System;
//using System.IO;
//using System.Linq;

//using Pixtro.Common;
//using Pixtro.Emulation.Common;

//namespace Pixtro.Client.Common
//{
//	public static class MovieConversionExtensions
//	{
//		// TODO: This doesn't really belong here, but not sure where to put it
//		public static void PopulateWithDefaultHeaderValues(
//			this IMovie movie,
//			IEmulator emulator,
//			IGameInfo game,
//			FirmwareManager firmwareManager,
//			string author)
//		{
//			movie.Author = author;
//			movie.EmulatorVersion = VersionInfo.GetEmuVersion();
//			movie.OriginalEmulatorVersion = VersionInfo.GetEmuVersion();
//			movie.SystemID = emulator.SystemId;

//			var settable = new SettingsAdapter(emulator);
//			if (settable.HasSyncSettings)
//			{
//				movie.SyncSettingsJson = ConfigService.SaveWithType(settable.GetSyncSettings());
//			}

//			if (game.IsNullInstance())
//			{
//				movie.GameName = "NULL";
//			}
//			else
//			{
//				movie.GameName = game.FilesystemSafeName();
//				movie.Hash = game.Hash;
//				if (game.FirmwareHash != null)
//				{
//					movie.FirmwareHash = game.FirmwareHash;
//				}
//			}

//			if (emulator.HasBoardInfo())
//			{
//				movie.BoardName = emulator.AsBoardInfo().BoardName;
//			}

//			if (emulator.HasRegions())
//			{
//				var region = emulator.AsRegionable().Region;
//				if (region == DisplayType.PAL)
//				{
//					movie.HeaderEntries.Add(HeaderKeys.Pal, "1");
//				}
//			}

//			if (firmwareManager.RecentlyServed.Count != 0)
//			{
//				foreach (var firmware in firmwareManager.RecentlyServed)
//				{
//					var key = firmware.ID.MovieHeaderKey;
//					if (!movie.HeaderEntries.ContainsKey(key))
//					{
//						movie.HeaderEntries.Add(key, firmware.Hash);
//					}
//				}
//			}

//			if (emulator is GBHawk gbHawk && gbHawk.IsCGBMode())
//			{
//				movie.HeaderEntries.Add("IsCGBMode", "1");
//			}

//			if (emulator is SubGBHawk subgbHawk)
//			{
//				if (subgbHawk._GBCore.IsCGBMode())
//				{
//					movie.HeaderEntries.Add("IsCGBMode", "1");
//				}

//				movie.HeaderEntries.Add(HeaderKeys.CycleCount, "0");
//			}

//			if (emulator is Gameboy gb)
//			{
//				if (gb.IsCGBMode())	
//				{
//					movie.HeaderEntries.Add(gb.IsCGBDMGMode() ? "IsCGBDMGMode" : "IsCGBMode", "1");
//				}

//				movie.HeaderEntries.Add(HeaderKeys.CycleCount, "0");
//			}

//			if (emulator is SMS sms)
//			{
//				if (sms.IsSG1000)
//				{
//					movie.HeaderEntries.Add("IsSGMode", "1");
//				}

//				if (sms.IsGameGear)
//				{
//					movie.HeaderEntries.Add("IsGGMode", "1");
//				}
//			}

//			if (emulator is GPGX gpgx && gpgx.IsMegaCD)
//			{
//				movie.HeaderEntries.Add("IsSegaCDMode", "1");
//			}

//			if (emulator is PicoDrive pico && pico.Is32XActive)
//			{
//				movie.HeaderEntries.Add("Is32X", "1");
//			}

//			if (emulator is SubNESHawk)
//			{
//				movie.HeaderEntries.Add(HeaderKeys.VBlankCount, "0");
//			}

//			movie.Core = ((CoreAttribute)Attribute
//				.GetCustomAttribute(emulator.GetType(), typeof(CoreAttribute)))
//				.CoreName;
//		}

//		internal static string ConvertFileNameToTasMovie(string oldFileName)
//		{
//			string newFileName = Path.ChangeExtension(oldFileName, $".{TasMovie.Extension}");
//			int fileSuffix = 0;
//			while (File.Exists(newFileName))
//			{
//				// Using this should hopefully be system agnostic
//				var temp_path = Path.Combine(Path.GetDirectoryName(oldFileName), Path.GetFileNameWithoutExtension(oldFileName));
//				newFileName = $"{temp_path} {++fileSuffix}.{TasMovie.Extension}";
//			}

//			return newFileName;
//		}
//	}
//}
