using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace SharpRaven.Data
{
    public class SentryStacktrace
    {
        public SentryStacktrace(Exception e)
        {
            StackTrace trace =  new StackTrace(e, true);
            this.Frames = new List<ExceptionFrame>();

            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);

                string source = "";
                int lineNo = 0;

                if (frame.GetFileLineNumber() == 0)
                {
                    //The pdb files aren't currently available
                    lineNo = frame.GetILOffset();
                    source = String.Format("{0} at IL Offset: {1}", frame.GetMethod().ToString(), lineNo);
                }
                else
                {
                    lineNo = frame.GetFileLineNumber();
                    source = String.Format("{0} at Line Number: {1}", frame.GetMethod().ToString(), lineNo);
                }

                Frames.Add(new ExceptionFrame()
                {
                    Filename = frame.GetFileName(),
                    Function = frame.GetMethod().Name,
                    Source = source,
                    LineNumber = lineNo
                });

            }
        }

        [JsonProperty(PropertyName="frames")]
        public List<ExceptionFrame> Frames;

    }
}
