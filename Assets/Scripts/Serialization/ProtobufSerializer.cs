using UnityEngine;
using System;
using System.Collections;
using System.IO;
using ProtoBuf;

namespace com.phobos {

	public class ProtobufSerializer : ISerializer {
		public byte[] Serialize<T>(T obj) {
			using (MemoryStream ms = new MemoryStream()) {
				Serializer.Serialize(ms, obj);
				return ms.ToArray();
			}
		}

		public T Deserialize<T>(byte[] bytes) {
			int bytesLength = bytes.Length;
			return Deserialize<T> (bytes, 0, bytesLength);
		}

		public T Deserialize<T>(byte[] bytes, int startIndex, int count) {
			using (MemoryStream ms = new MemoryStream(bytes, startIndex, count)) {
				return Serializer.Deserialize<T>(ms);
			}
		}

	}
	
}
