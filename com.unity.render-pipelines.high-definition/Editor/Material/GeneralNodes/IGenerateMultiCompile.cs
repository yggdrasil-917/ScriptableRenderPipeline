using System.Collections.Generic;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    public delegate HashSet<string> HashSetAllocator();
    public interface IGenerateMultiCompile
    {
        void GetMultiCompiles(HashSetAllocator setAllocator, List<HashSet<string>> values);
        void GetInputSlotsFor(HashSet<string> keywordSet, List<int> slotIds);
    }

    public static class IGenerateMultiCompileExtensions
    {
        public static List<int> GetInputSlotsFor(this IGenerateMultiCompile self, HashSet<string> keywordSet)
        {
            var result = new List<int>();
            self.GetInputSlotsFor(keywordSet, result);
            return result;
        }
    }
}
