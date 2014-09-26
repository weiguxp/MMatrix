using UnityEngine;
using System;
using System.Collections;

namespace com.phobos {
	public interface ISerializer {
		byte[] Serialize<T>(T obj);
		T Deserialize<T>(byte[] bytes);
		T Deserialize<T>(byte[] bytes, int startIndex, int count);
	}
	
}
