﻿using System;
using System.Diagnostics;

namespace Lucene.Net.Util.Fst
{
    public class CharSequenceOutputs : Outputs<CharsRef>
    {
        private static readonly CharsRef NO_OUTPUT;
        private static readonly CharSequenceOutputs _singleton;

        static CharSequenceOutputs()
        {
            NO_OUTPUT = new CharsRef();
            _singleton = new CharSequenceOutputs();
        }

        private CharSequenceOutputs() {} // can't construct

        public static CharSequenceOutputs GetSingleton() { return _singleton; }

        public override CharsRef Add(CharsRef prefix, CharsRef output)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            if (output == null) throw new ArgumentNullException("output");

            if (prefix == NO_OUTPUT) return output;
            if (output == NO_OUTPUT) return prefix;

            // TODO: assert correct here?
            Debug.Assert(prefix.Length > 0);
            Debug.Assert(output.Length > 0);
            var result = new CharsRef(prefix.Length + output.Length);
            Array.Copy(prefix.chars, prefix.offset, result.chars, 0, prefix.Length);
            Array.Copy(output.chars, output.offset, result.chars, prefix.Length, output.Length);
            result.Length = prefix.Length + output.Length;
            return result;
        }

        public override CharsRef Common(CharsRef output1, CharsRef output2)
        {
            if (output1 == null) throw new ArgumentNullException("output1");
            if (output2 == null) throw new ArgumentNullException("output2");

            var pos1 = output1.offset;
            var pos2 = output1.offset;
            var stopAt1 = pos1 + Math.Min(output1.Length, output2.Length);
            while (pos1 < stopAt1)
            {
                if (output1.chars[pos1] != output2.chars[pos2])
                    break;
                pos1++;
                pos2++;
            }

            if (pos1 == output1.offset) return NO_OUTPUT;
            if (pos1 == output1.offset + output1.Length) return output1;
            if (pos2 == output2.offset + output2.Length) return output2;
            return new CharsRef(output1.chars, output1.offset, pos1-output1.offset);
        }

        public override CharsRef GetNoOutput()
        {
            return NO_OUTPUT;
        }

        public override String OutputToString(CharsRef output)
        {
            return output.ToString();
        }

        public override CharsRef Read(DataInput dataInput)
        {
            var len = dataInput.ReadVInt();
            
            if (len == 0) return NO_OUTPUT;

            var output = new CharsRef(len);
            for (var idx = 0; idx < len; idx++)
                output.chars[idx] = (Char) dataInput.ReadVInt();

            output.length = len;
            return output;
        }

        public override CharsRef Subtract(CharsRef output, CharsRef inc)
        {
            if (output == null) throw new ArgumentNullException("output");
            if (inc == null) throw new ArgumentNullException("inc");

            if (inc == NO_OUTPUT) return output;
            if (inc.Length == output.Length) return NO_OUTPUT;

            // TODO: debug appropriate here?
            Debug.Assert(inc.Length < output.Length, "inc.length=" + inc.Length + " vs output.length=" + output.Length);
            Debug.Assert(inc.Length > 0);
            return  new CharsRef(output.chars, output.offset + inc.Length, output.Length - inc.Length);
        }

        public override void Write(CharsRef prefix, DataOutput dataOutput)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            dataOutput.WriteVInt(prefix.Length);

            for (var idx = 0; idx < prefix.Length; idx++)
                dataOutput.WriteVInt(prefix.chars[prefix.offset + idx]);
        }
    }
}