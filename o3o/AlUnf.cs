using System;
using System.IO;
using OpenAL;

namespace Unf
{
    internal static class FileWAV : object
    {
        private class HeaderWAV : object
        {
            internal byte[] RIFFHeader = new byte[4];
            internal int FileSize;
            internal byte[] WAVEHeader = new byte[4];
            internal byte[] FormatHeader = new byte[4];
            internal int FormatHeaderSize;
            internal short FormatCode;
            internal short ChannelNumber;
            internal int SampleRate;
            internal int BytesPerSecond;
            internal short BytesPerSample;
            internal short BitsPerSample;
        }

        private const int HeaderSize = 36;
        private const int ChunkHeaderSize = 4;

        private static Stream FStream;
        private static BinaryReader FReader;

        private static HeaderWAV FHeader = new HeaderWAV();

        private static byte[] FDataBuffer;

        private static string ReadANSIString(int Count)
        {
            byte[] Buffer = new byte[Count];
            FStream.Read(Buffer, 0, Count);
            string Result = System.Text.Encoding.ASCII.GetString(Buffer);

            int Index = Result.IndexOf('\0');
            if (Index == -1)
                return Result;

            return Result.Substring(0, Index);
        }

        private static void RaiseUnknownFormatException()
        {
            new Exception("format not supported");
        }


        private static void ReadHeader()
        {
            FReader.Read(FHeader.RIFFHeader, 0, 4);
            FHeader.FileSize = FReader.ReadInt32();
            FReader.Read(FHeader.WAVEHeader, 0, 4);
            FReader.Read(FHeader.FormatHeader, 0, 4);
            FHeader.FormatHeaderSize = FReader.ReadInt32();
            FHeader.FormatCode = FReader.ReadInt16();
            FHeader.ChannelNumber = FReader.ReadInt16();
            FHeader.SampleRate = FReader.ReadInt32();
            FHeader.BytesPerSecond = FReader.ReadInt32();
            FHeader.BytesPerSample = FReader.ReadInt16();
            FHeader.BitsPerSample = FReader.ReadInt16();

            FStream.Seek(FHeader.FormatHeaderSize - 16, SeekOrigin.Current);
        }

        private static void ReadDataChunk()
        {
            int BufferSize;
            BufferSize = FReader.ReadInt32();

            FDataBuffer = new byte[BufferSize];
            FStream.Read(FDataBuffer, 0, BufferSize);
        }

        private static void ReadData()
        {
            do
            {
                switch (ReadANSIString(ChunkHeaderSize))
                {
                    case "data":
                        ReadDataChunk();
                        break;
                    default:
                        //ignore unknown chunks
                        FStream.Seek(FReader.ReadInt32(), SeekOrigin.Current);
                        break;
                }
            }
            while (FStream.Position < FStream.Length);
        }

        private static int GenerateSoundBuffer()
        {
            int Result = al.GenBuffer();

            if (FHeader.ChannelNumber == 1)
                switch (FHeader.BitsPerSample)
                {
                    case 8:
                        al.BufferData(Result, al.FORMAT_MONO8, FDataBuffer, FDataBuffer.Length, FHeader.SampleRate);
                        break;
                    case 16:
                        al.BufferData(Result, al.FORMAT_MONO16, FDataBuffer, FDataBuffer.Length, FHeader.SampleRate);
                        break;
                    default:
                        RaiseUnknownFormatException();
                        break;
                }
            else if (FHeader.ChannelNumber == 2)
                switch (FHeader.BitsPerSample)
                {
                    case 8:
                        al.BufferData(Result, al.FORMAT_STEREO8, FDataBuffer, FDataBuffer.Length, FHeader.SampleRate);
                        break;
                    case 16:
                        al.BufferData(Result, al.FORMAT_STEREO16, FDataBuffer, FDataBuffer.Length, FHeader.SampleRate);
                        break;
                    default:
                        RaiseUnknownFormatException();
                        break;
                }
            else
                RaiseUnknownFormatException();

            return Result;
        }

        public static int LoadFromFile(string FileName)
        {
            FStream = new FileStream(FileName, FileMode.Open);
            FReader = new BinaryReader(FStream);
            try
            {
                ReadHeader();
                ReadData();
            }
            finally
            {
                FReader.Close();
            }
            int Result = GenerateSoundBuffer();
            FDataBuffer = null;
            return Result;
        }
    }
}
