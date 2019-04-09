using System.IO;
using System.Text;

namespace SoftwarePioniere.Util
{
    public class StringWriterWithEncoding : StringWriter
    {
        public StringWriterWithEncoding(StringBuilder builder, Encoding encoding)
            : base(builder)
        {
            Encoding = encoding;
        }
        public override Encoding Encoding { get; }
    }
}
