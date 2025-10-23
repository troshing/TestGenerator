using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace ConvertObject
{
	public class ConvObject
	{
		public byte[] StructToByte<T>(T any)
		{
			//Возвращает массив байт длинной, равной длинне структуры any типа T
			//передаем данные из  структуры  any  в   массив байт 
			byte[] Bdata = new byte[Marshal.SizeOf(typeof(T))];      //байтовый буфер размером  //во что кладем

			GCHandle handle = GCHandle.Alloc(Bdata, GCHandleType.Pinned);

			Marshal.StructureToPtr(any, handle.AddrOfPinnedObject(), false);
			handle.Free();
			return Bdata;
		}

		public T ByteToStruct<T>(object any)
		{
			//возвращает структуру типа T
			//T тип структуры, any - набор байт
			GCHandle handle = GCHandle.Alloc(any, GCHandleType.Pinned);
			T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return temp;
		}

		public T ReadStructFromStream<T>(Stream fs)
        {
			//возвращает структуру типа T
			//читает структуру типа Т из потока FileStream

			byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
			fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));
			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return temp;
        }

		public int WriteStructToStream<T>(Stream fs,T any)
		{
			//возвращает int - размер any
			//пишет структуру типа Т в поток FileStream

			byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];

			GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			Marshal.StructureToPtr(any, handle.AddrOfPinnedObject(), false);

			fs.Write(buffer, 0, Marshal.SizeOf(typeof(T)));

			handle.Free();

			return Marshal.SizeOf(typeof(T));
		}
	}
}
