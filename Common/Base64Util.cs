﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace KHFC {
	public static class Base64Util {
		public static string Encoding(string text, System.Text.Encoding encodeType = null) {
			if (encodeType == null)
				encodeType = System.Text.Encoding.UTF8;

			byte[] bytes = encodeType.GetBytes(text);
			return System.Convert.ToBase64String(bytes);
		}

		public static string Decoding(string text, System.Text.Encoding encodeType = null) {
			if (encodeType == null)
				encodeType = System.Text.Encoding.UTF8;

			byte[] bytes = System.Convert.FromBase64String(text);
			return encodeType.GetString(bytes);
		}
	}
}