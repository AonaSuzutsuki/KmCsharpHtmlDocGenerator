using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace XmlDocumentParser.EasyCs
{
	public class ArraySplitter : IEnumerable
	{

		private int copiedCount = 0;
		private int copiedCount2 = 0;
		private int threshold;
		private string[] array;

		public ArraySplitter(string[] array)
		{
			this.array = array;
			this.threshold = 2;
		}

		public ArraySplitter(string[] array, int threshold)
		{         
            this.array = array;
			this.threshold = threshold;
		}
        
		public IEnumerator<string> GetEnumerator()
		{
			var length = array.Length / threshold;
			length = (array.Length - copiedCount) > length ? length : array.Length - copiedCount;
			while (length > 1)
			{
				var copiedArray = new string[length];
                for (int i = 0; i < copiedArray.Length; i++)
                {
					copiedArray[i] = array[copiedCount++];
				}
                copiedCount--;
				length = (array.Length - copiedCount) > length ? length : array.Length - copiedCount;
				yield return ArrayToString(copiedArray);
			}

		}

		private static string ArrayToString(string[] array)
		{
			var sb = new StringBuilder(array.Length);
			foreach (var elem in array)
				sb.AppendLine(elem);
			return sb.ToString();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
