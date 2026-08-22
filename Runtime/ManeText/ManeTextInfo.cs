using System.Collections.Generic;
using System.Linq;

namespace Mane.Unity.TextMesh
{
    public class ManeTextInfo
    {
        public readonly List<string> String = new();
        public readonly List<float> Length = new();

        public int TotalCount => String.Sum(s => s.Length);

        public float MaxLength => Length.Max(s => s);

        public void Append(string str, float length)
        {
            String.Add(str);
            Length.Add(length);
        }
    }
}